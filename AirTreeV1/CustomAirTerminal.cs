using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

namespace AirTreeV1
{
    public class CustomAirTerminal
    {
        public Document Document { get; set; }
        public CustomElement Element { get; set; }
        public ElementId ElementId { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public double Diameter { get; set; }
        public double Koeffizient { get; set; }
        public double Velocity { get; set; }
        public double Volume { get; set; }
        public double HArea { get; set; }
        public CustomConnector InletConnector { get; set; }
        public CustomConnector OutletConnector { get; set; }
        public DuctSystemType SystemType { get; set; }
        public double LocRes { get; set; }
        public double PDyn { get; set; }
        public bool IsSpecial { get; set; }
        public ConnectorProfileType ProfileType { get; set; }
        public CustomAirTerminal(Autodesk.Revit.DB.Document document, CustomElement element)
        {
            Document = document;
            Element = element;
            ElementId = element.ElementId;
            SystemType = element.SystemType;
            IsSpecial = false;
            //Koeffizient = Element.Element.LookupParameter("AirTree_КЖС").AsDouble();


            bool KZHS = false;
            bool FKMS = false;
            bool kMS = false;
            bool dP = false;

            if (Element.Element.LookupParameter("AirTree_КЖС").AsDouble() != 0)
            {
                KZHS = true;
            }
            if (Element.Element.LookupParameter("AirTree_F(КМС)").AsDouble() != 0)
            {
                FKMS = true;
            }
            if (Element.Element.LookupParameter("AirTree_КМС").AsDouble() != 0)
            {
                kMS = true;
            }
            if (Element.Element.LookupParameter("AirTree_dP").AsDouble() != 0)
            {
                dP = true;
            }






            /*if (Koeffizient ==0)
            {
                Koeffizient = 1;
            }*/
            if (document.GetElement(ElementId) is FamilyInstance)
            {

                foreach (Connector connector in Element.OwnConnectors)
                {
                    if (connector.Domain != Domain.DomainHvac)
                    {
                        continue;
                    }
                    else
                    {
                        ConnectorSet nextconnectors = connector.AllRefs;

                        foreach (Connector connect in nextconnectors)
                        {
                            if (connect.Domain != Domain.DomainHvac)
                            {
                                continue;
                            }
                            else
                            {
                                SystemType = connect.DuctSystemType;
                                CustomConnector custom = new CustomConnector(Document, ElementId, SystemType);

                                if (Document.GetElement(connect.Owner.Id) is MechanicalSystem || Document.GetElement(connect.Owner.Id) is DuctInsulation)
                                {
                                    continue;
                                }

                                else if (connect.Owner.Id == ElementId)
                                {
                                    continue; // Игнорируем те же элементы
                                }
                                /*else if (connect.Owner.Id == NextElementId)
                                {
                                    continue;
                                }*/


                                if (connect.Domain == Autodesk.Revit.DB.Domain.DomainHvac || connect.Domain == Autodesk.Revit.DB.Domain.DomainPiping)
                                {

                                    if (SystemType == DuctSystemType.SupplyAir)
                                    {

                                        if (connect.Direction == FlowDirectionType.Out)
                                        {
                                            custom.Flow = connect.Flow * 102;
                                            Volume = Math.Round(custom.Flow, 0);
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2 * 0.3125;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                HArea = custom.Area;
                                                custom.EquiDiameter = custom.Diameter;
                                                element.ModelDiameter = custom.Diameter.ToString();
                                                element.ModelHydraulicArea = Convert.ToString(Math.Round(custom.Area, 2));
                                                try
                                                {
                                                    if (element.Element.LookupParameter("AirTree_Спецрешетка").AsInteger() == 0)
                                                    {
                                                        custom.Velocity = custom.Flow / (custom.Area * 3600);
                                                        
                                                        element.Volume = Convert.ToString(Math.Round(custom.Flow, 0));
                                                        element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                                                        element.ModelHydraulicArea = Convert.ToString(Math.Round(custom.Area, 2));
                                                        PDyn = 0.6 * Velocity * Velocity;
                                                        element.PDyn = PDyn;

                                                        if (dP == true)
                                                        {

                                                            PDyn = Element.Element.LookupParameter("AirTree_dP").AsDouble();
                                                        }

                                                        if (KZHS == true && dP == false)
                                                        {
                                                            Koeffizient = Element.Element.LookupParameter("AirTree_КЖС").AsDouble();
                                                            custom.Area = custom.Area * Koeffizient;
                                                            element.ModelHydraulicArea = Convert.ToString(custom.Area);
                                                            Velocity = custom.Flow / (custom.Area * 3600);
                                                            element.ModelVelocity = Convert.ToString(custom.Velocity);
                                                            PDyn = 0.6 * Velocity * Velocity;
                                                        }

                                                        if (FKMS == true && kMS == true && dP == false)
                                                        {
                                                            custom.Area = Element.Element.LookupParameter("AirTree_F(КМС)").AsDouble();
                                                            element.ModelHydraulicArea = Convert.ToString(custom.Area);
                                                            element.LocRes = Element.Element.LookupParameter("AirTree_КМС").AsDouble();
                                                            custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                            element.ModelVelocity = Convert.ToString(custom.Velocity);
                                                            PDyn = element.LocRes * custom.Velocity * custom.Velocity * 0.6;
                                                        }

                                                        if (KZHS == true && kMS == true && dP == false)
                                                        {
                                                            Koeffizient = Element.Element.LookupParameter("AirTree_КЖС").AsDouble();
                                                            custom.Area = custom.Area * Koeffizient;
                                                            element.ModelHydraulicArea = Convert.ToString(custom.Area);
                                                            custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                            element.ModelVelocity = Convert.ToString(custom.Velocity);
                                                            element.LocRes = Element.Element.LookupParameter("AirTree_КМС").AsDouble();
                                                            PDyn = element.LocRes * custom.Velocity * custom.Velocity;
                                                        }

                                                        /*if (element.Element.LookupParameter("AirTree_F(КМС)").AsDouble() != 0)
                                                        {
                                                            custom.Area = element.Element.LookupParameter("AirTree_F(КМС)").AsDouble();
                                                            element.ModelHydraulicArea = Convert.ToString(Math.Round(custom.Area, 2));
                                                            Velocity = custom.Flow / (3600 * custom.Area);
                                                            if (element.Element.LookupParameter("AirTree_КМС").AsDouble() != 0)
                                                            {
                                                                element.LocRes = element.Element.LookupParameter("AirTree_КМС").AsDouble();
                                                                PDyn = element.LocRes * Velocity * Velocity * 0.6;
                                                                element.PDyn = PDyn;
                                                            }
                                                            
                                                        }*/

                                                    }
                                                    else
                                                    {
                                                        custom.Velocity = custom.Flow / (custom.Area  * 3600);
                                                        element.Volume = Convert.ToString(Math.Round(custom.Flow, 0));
                                                        element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                                                        PDyn = 0;
                                                        element.PDyn = PDyn;
                                                    }

                                                }
                                                catch { }
                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8;
                                                custom.Height = connect.Height * 304.8;
                                                element.ModelWidth = custom.Width.ToString();
                                                element.ModelHeight = custom.Height.ToString();
                                                
                                                    if (element.Element.LookupParameter("AirTree_Спецрешетка").AsInteger() == 0)
                                                    {
                                                        custom.Area = custom.Width / 1000 * custom.Height / 1000;
                                                        custom.Velocity = custom.Flow / (custom.Area * 3600);
                                                        
                                                        element.Volume = Convert.ToString(Math.Round(custom.Flow, 0));
                                                        element.ModelVelocity = Convert.ToString(Math.Round(custom.Velocity, 2));
                                                        element.ModelHydraulicArea = Convert.ToString(Math.Round(custom.Area, 2));
                                                        PDyn = 0.6 * custom.Velocity * custom.Velocity;
                                                        element.PDyn = PDyn;

                                                        if (dP == true)
                                                        {

                                                            PDyn = Element.Element.LookupParameter("AirTree_dP").AsDouble();
                                                        }

                                                        if (KZHS == true && dP == false)
                                                        {
                                                            Koeffizient = Element.Element.LookupParameter("AirTree_КЖС").AsDouble();
                                                            custom.Area = custom.Area * Koeffizient;
                                                            element.ModelHydraulicArea = Convert.ToString(custom.Area);
                                                            custom.Velocity = custom.Flow / (custom.Area * 3600);
                                                            element.ModelVelocity = Convert.ToString(custom.Velocity);
                                                            PDyn = 0.6 * custom.Velocity * custom.Velocity;
                                                        }

                                                        if (FKMS == true && kMS == true && dP == false)
                                                        {
                                                            custom.Area = Element.Element.LookupParameter("AirTree_F(КМС)").AsDouble();
                                                            element.ModelHydraulicArea = Convert.ToString(custom.Area);
                                                            element.LocRes = Element.Element.LookupParameter("AirTree_КМС").AsDouble();
                                                            custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                            element.ModelVelocity = Convert.ToString(custom.Velocity);
                                                            PDyn = element.LocRes * custom.Velocity * custom.Velocity * 0.6;
                                                        }

                                                        if (KZHS == true && kMS == true && dP == false)
                                                        {
                                                            Koeffizient = Element.Element.LookupParameter("AirTree_КЖС").AsDouble();
                                                            custom.Area = custom.Area * Koeffizient;
                                                            element.ModelHydraulicArea = Convert.ToString(custom.Area);
                                                            custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                            element.ModelVelocity = Convert.ToString(custom.Velocity);
                                                            element.LocRes = Element.Element.LookupParameter("AirTree_КМС").AsDouble();
                                                            PDyn = element.LocRes * custom.Velocity * custom.Velocity;
                                                        }

                                                        /*if (element.Element.LookupParameter("AirTree_F(КМС)").AsDouble() != 0)
                                                        {
                                                            custom.Area = element.Element.LookupParameter("AirTree_F(КМС)").AsDouble();
                                                            element.ModelHydraulicArea = Convert.ToString(Math.Round(custom.Area, 2));
                                                            Velocity = custom.Flow / (3600 * custom.Area);
                                                            if (element.Element.LookupParameter("AirTree_КМС").AsDouble() != 0)
                                                            {
                                                                element.LocRes = element.Element.LookupParameter("AirTree_КМС").AsDouble();
                                                                PDyn = element.LocRes * Velocity * Velocity * 0.6;
                                                                element.PDyn = PDyn;
                                                            }
                                                            
                                                        }*/

                                                    }
                                                    else
                                                    {
                                                        custom.Velocity = custom.Flow / (custom.Area  * 3600);
                                                        element.Volume = Convert.ToString(Math.Round(custom.Flow, 0));
                                                        element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                                                        PDyn = 0;
                                                        element.PDyn = PDyn;
                                                    }

                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                OutletConnector = custom;


                                                //SecondaryConnectors.Add(custom);
                                            }


                                        }
                                        else if (SystemType == DuctSystemType.ExhaustAir)
                                        {
                                            if (connect.Direction == FlowDirectionType.In)
                                            {
                                                custom.Flow = connect.Flow * 102;
                                                Volume = Math.Round(custom.Flow, 0);
                                                custom.Domain = Domain.DomainHvac;
                                                //custom.DirectionType = FlowDirectionType.In;
                                                custom.NextOwnerId = connect.Owner.Id;
                                                custom.Shape = connect.Shape;
                                                custom.Type = connect.ConnectorType;
                                                if (custom.Shape == ConnectorProfileType.Round)
                                                {
                                                    ProfileType = ConnectorProfileType.Round;

                                                    custom.Diameter = connect.Radius * 2 * 0.3125;
                                                    custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                    element.ModelDiameter = custom.Diameter.ToString();
                                                    element.ModelHydraulicArea = Convert.ToString(Math.Round(custom.Area, 2));
                                                    try
                                                    {

                                                        if (element.Element.LookupParameter("AirTree_Спецрешетка").AsInteger() == 0)
                                                        {
                                                            custom.Velocity = custom.Flow / (custom.Area * 3600);
                                                            Velocity = OutletConnector.Velocity;
                                                            element.Volume = Convert.ToString(Math.Round(custom.Flow, 0));
                                                            element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                                                            element.ModelHydraulicArea = Convert.ToString(Math.Round(custom.Area, 2));
                                                            PDyn = 0.6 * Velocity * Velocity;
                                                            element.PDyn = PDyn;

                                                            if (dP == true)
                                                            {

                                                                PDyn = Element.Element.LookupParameter("AirTree_dP").AsDouble();
                                                            }

                                                            if (KZHS == true && dP == false)
                                                            {
                                                                Koeffizient = Element.Element.LookupParameter("AirTree_КЖС").AsDouble();
                                                                custom.Area = custom.Area * Koeffizient;
                                                                element.ModelHydraulicArea = Convert.ToString(custom.Area);
                                                                Velocity = custom.Flow / (custom.Area * 3600);
                                                                element.ModelVelocity = Convert.ToString(custom.Velocity);
                                                                PDyn = 0.6 * Velocity * Velocity;
                                                            }

                                                            if (FKMS == true && kMS == true && dP == false)
                                                            {
                                                                custom.Area = Element.Element.LookupParameter("AirTree_F(КМС)").AsDouble();
                                                                element.ModelHydraulicArea = Convert.ToString(custom.Area);
                                                                element.LocRes = Element.Element.LookupParameter("AirTree_КМС").AsDouble();
                                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                                element.ModelVelocity = Convert.ToString(custom.Velocity);
                                                                PDyn = element.LocRes * custom.Velocity * custom.Velocity * 0.6;
                                                            }

                                                            if (KZHS == true && kMS == true && dP == false)
                                                            {
                                                                Koeffizient = Element.Element.LookupParameter("AirTree_КЖС").AsDouble();
                                                                custom.Area = custom.Area * Koeffizient;
                                                                element.ModelHydraulicArea = Convert.ToString(custom.Area);
                                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                                element.ModelVelocity = Convert.ToString(custom.Velocity);
                                                                element.LocRes = Element.Element.LookupParameter("AirTree_КМС").AsDouble();
                                                                PDyn = element.LocRes * custom.Velocity * custom.Velocity;
                                                            }

                                                            /*if (element.Element.LookupParameter("AirTree_F(КМС)").AsDouble() != 0)
                                                            {
                                                                custom.Area = element.Element.LookupParameter("AirTree_F(КМС)").AsDouble();
                                                                element.ModelHydraulicArea = Convert.ToString(Math.Round(custom.Area, 2));
                                                                Velocity = custom.Flow / (3600 * custom.Area);
                                                                if (element.Element.LookupParameter("AirTree_КМС").AsDouble() != 0)
                                                                {
                                                                    element.LocRes = element.Element.LookupParameter("AirTree_КМС").AsDouble();
                                                                    PDyn = element.LocRes * Velocity * Velocity * 0.6;
                                                                    element.PDyn = PDyn;
                                                                }

                                                            }*/

                                                        }
                                                        else
                                                        {
                                                            custom.Velocity = custom.Flow / (custom.Area  * 3600);
                                                            element.Volume = Convert.ToString(Math.Round(custom.Flow, 0));
                                                            element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                                                            PDyn = 0;
                                                            element.PDyn = PDyn;
                                                        }
                                                    }
                                                    catch { }

                                                }
                                                else
                                                {
                                                    ProfileType = ConnectorProfileType.Rectangular;
                                                    custom.Width = connect.Width * 304.8 / 1000;
                                                    custom.Height = connect.Height * 304.8 / 1000;
                                                    element.ModelWidth = custom.Width.ToString();
                                                    element.ModelHeight = custom.Height.ToString();
                                                    if (element.Element.LookupParameter("AirTree_Спецрешетка").AsInteger() == 0)
                                                    {
                                                        custom.Velocity = custom.Flow / (custom.Area * 3600);
                                                        Velocity = OutletConnector.Velocity;
                                                        element.Volume = Convert.ToString(Math.Round(custom.Flow, 0));
                                                        element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                                                        element.ModelHydraulicArea = Convert.ToString(Math.Round(custom.Area, 2));
                                                        PDyn = 0.6 * Velocity * Velocity;
                                                        element.PDyn = PDyn;

                                                        if (dP == true)
                                                        {

                                                            PDyn = Element.Element.LookupParameter("AirTree_dP").AsDouble();
                                                        }

                                                        if (KZHS == true && dP == false)
                                                        {
                                                            Koeffizient = Element.Element.LookupParameter("AirTree_КЖС").AsDouble();
                                                            custom.Area = custom.Area * Koeffizient;
                                                            element.ModelHydraulicArea = Convert.ToString(custom.Area);
                                                            Velocity = custom.Flow / (custom.Area * 3600);
                                                            element.ModelVelocity = Convert.ToString(custom.Velocity);
                                                            PDyn = 0.6 * Velocity * Velocity;
                                                        }

                                                        if (FKMS == true && kMS == true && dP == false)
                                                        {
                                                            custom.Area = Element.Element.LookupParameter("AirTree_F(КМС)").AsDouble();
                                                            element.ModelHydraulicArea = Convert.ToString(custom.Area);
                                                            element.LocRes = Element.Element.LookupParameter("AirTree_КМС").AsDouble();
                                                            custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                            element.ModelVelocity = Convert.ToString(custom.Velocity);
                                                            PDyn = element.LocRes * custom.Velocity * custom.Velocity * 0.6;
                                                        }

                                                        if (KZHS == true && kMS == true && dP == false)
                                                        {
                                                            Koeffizient = Element.Element.LookupParameter("AirTree_КЖС").AsDouble();
                                                            custom.Area = custom.Area * Koeffizient;
                                                            element.ModelHydraulicArea = Convert.ToString(custom.Area);
                                                            custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                            element.ModelVelocity = Convert.ToString(custom.Velocity);
                                                            element.LocRes = Element.Element.LookupParameter("AirTree_КМС").AsDouble();
                                                            PDyn = element.LocRes * custom.Velocity * custom.Velocity;
                                                        }

                                                        /*if (element.Element.LookupParameter("AirTree_F(КМС)").AsDouble() != 0)
                                                        {
                                                            custom.Area = element.Element.LookupParameter("AirTree_F(КМС)").AsDouble();
                                                            element.ModelHydraulicArea = Convert.ToString(Math.Round(custom.Area, 2));
                                                            Velocity = custom.Flow / (3600 * custom.Area);
                                                            if (element.Element.LookupParameter("AirTree_КМС").AsDouble() != 0)
                                                            {
                                                                element.LocRes = element.Element.LookupParameter("AirTree_КМС").AsDouble();
                                                                PDyn = element.LocRes * Velocity * Velocity * 0.6;
                                                                element.PDyn = PDyn;
                                                            }
                                                            
                                                        }*/

                                                    }
                                                    else
                                                    {
                                                        custom.Velocity = custom.Flow / (custom.Area  * 3600);
                                                        element.Volume = Convert.ToString(Math.Round(custom.Flow, 0));
                                                        element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                                                        PDyn = 0;
                                                        element.PDyn = PDyn;
                                                    }

                                                }
                                                custom.Coefficient = connect.Coefficient;

                                                OutletConnector = custom;
                                                //SecondaryConnectors.Add(custom);
                                            }
                                        }

                                    }

                                }

                            }
                        }

                    }
                    /* Width = OutletConnector.Width;
                     Height = OutletConnector.Height;
                     Radius = Document.GetElement(ElementId).LookupParameter("Центр и радиус").AsDouble();
                     Diameter = Document.GetElement(ElementId).LookupParameter("Центр и радиус").AsDouble();*/
                    /* Velocity = OutletConnector.Velocity;
                     PDyn = 0.6 * Velocity * Velocity;*/
                    /*if (Element.Element.LookupParameter("AirTree_dP").AsDouble() != 0)
                    { PDyn = Element.Element.LookupParameter("AirTree_dP").AsDouble(); }*/
                }


            }

        }
    }


