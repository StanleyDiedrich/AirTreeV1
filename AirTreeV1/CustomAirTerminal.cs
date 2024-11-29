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
        
        public double Velocity { get; set; }
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
                                            custom.Flow = connect.Flow*102;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2*0.3125;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.EquiDiameter = custom.Diameter;
                                                try
                                                {
                                                    if (element.Element.LookupParameter("AirTree_Спецрешетка").AsInteger() == 0)
                                                    {
                                                        custom.Velocity = custom.Flow / (custom.Area * 3600);
                                                        Velocity = OutletConnector.Velocity;
                                                        element.Volume = Convert.ToString(Math.Round(custom.Flow, 0));
                                                        element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                                                        PDyn = 0.6 * Velocity * Velocity;
                                                    }
                                                    else
                                                    {
                                                        custom.Velocity = custom.Flow / (custom.Area*0.7 * 3600);
                                                        element.Volume = Convert.ToString(Math.Round(custom.Flow, 0));
                                                        element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                                                        PDyn = 0;
                                                    }
                                                   
                                                }
                                                catch { }
                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width*304.8;
                                                custom.Height = connect.Height*304.8;
                                                if (element.Element.LookupParameter("AirTree_Спецрешетка").AsInteger() == 0)
                                                {
                                                    custom.Area = custom.Height / 1000 * custom.Height / 1000 * 0.7;
                                                    custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                    Velocity = custom.Velocity;
                                                    element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                                                    PDyn = 0.6 * Velocity * Velocity;
                                                }
                                                else
                                                {
                                                    custom.Area = custom.Height / 1000 * custom.Height / 1000 ;
                                                    custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                    element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                                                    PDyn = 0;
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
                                            custom.Flow = connect.Flow*102;
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
                                                try
                                                {

                                                    if (element.Element.LookupParameter("AirTree_Спецрешетка").AsInteger() == 0)
                                                    {
                                                        custom.Velocity = custom.Flow / (custom.Area*0.7 * 3600);
                                                        Velocity = custom.Velocity;
                                                        element.Volume = Convert.ToString(Math.Round(custom.Flow, 0));
                                                        element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                                                        PDyn = 0.6 * Velocity * Velocity;
                                                    }
                                                    else
                                                    {
                                                        custom.Velocity = custom.Flow / (custom.Area  * 3600);
                                                        element.Volume = Convert.ToString(Math.Round(custom.Flow,0));
                                                        element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                                                        PDyn = 0;
                                                    }
                                                }
                                                catch { }

                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8/1000;
                                                custom.Height = connect.Height * 304.8/1000;
                                                if (element.Element.LookupParameter("AirTree_Спецрешетка").AsInteger() == 0)
                                                {
                                                    custom.Area = custom.Height  * custom.Height  * 0.7;
                                                    custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                    element.Volume = Convert.ToString(Math.Round(custom.Flow, 0));
                                                    element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                                                    Velocity = custom.Velocity;
                                                    PDyn = 0.6 * Velocity * Velocity;
                                                }
                                                else
                                                {
                                                    custom.Area = custom.Height  * custom.Height ;
                                                    custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                    element.Volume = Convert.ToString(Math.Round(custom.Flow, 0));
                                                    element.ModelVelocity = Convert.ToString(Math.Round(Velocity,2));
                                                    Velocity = custom.Velocity;
                                                    PDyn = 0;
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
               
            }


        }

    }
}
    

