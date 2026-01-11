using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;

namespace AirTreeV1
{
    public class CustomValve
    {
        public Document Document { get; set; }
        public CustomElement Element { get; set; }
        public ElementId ElementId { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Angle { get; set; }
        public double Radius { get; set; }
        public double Diameter { get; set; }
        public double ElbowRadius { get; set; }
        public double Velocity { get; set; }
        public double HydraulicDiameter { get; set; }
        public double HydraulicArea { get; set; }
        public CustomConnector InletConnector { get; set; }
        public CustomConnector OutletConnector { get; set; }
        public DuctSystemType SystemType { get; set; }
        public double LocRes { get; set; }
        public double AirTree_Area { get; set; }
        public ConnectorProfileType ProfileType { get; set; }
        public enum Rotation
        {
            Vertical,
            Horizontal
        }

        public CustomValve(Autodesk.Revit.DB.Document document, CustomElement element)
        {
            Document = document;
            Element = element;
            ElementId = element.ElementId;
            SystemType = element.SystemType;

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
                                            custom.Flow = connect.Flow * 101.947308132875143184421534937;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2 * 304.8 / 1000;                                               
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;

                                                element.ModelDiameter = Convert.ToString(custom.Diameter);
                                                
                                                custom.EquiDiameter = custom.Diameter;
                                                try
                                                {
                                                    ElbowRadius = document.GetElement(ElementId).LookupParameter("u").AsDouble() * 304.8 / 1000;
                                                    custom.Velocity = custom.Flow / (custom.Area * 3600);
                                                }
                                                catch { }
                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                element.ModelWidth = Convert.ToString(custom.Width * 1000);
                                                element.ModelHeight = Convert.ToString(custom.Height * 1000);
                                                
                                                //custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                //custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop;
                                            custom.Origin = connect.Origin;
                                            OutletConnector = custom;


                                            //SecondaryConnectors.Add(custom);
                                        }
                                        if (connect.Direction == FlowDirectionType.In)
                                        {
                                            custom.Flow = connect.Flow * 101.947308132875143184421534937;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2 * 304.8 / 1000;
                                                element.ModelDiameter = Convert.ToString(custom.Diameter);
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.EquiDiameter = custom.Diameter;
                                                try
                                                {
                                                    ElbowRadius = document.GetElement(ElementId).LookupParameter("u").AsDouble() * 304.8 / 1000;
                                                    custom.Velocity = custom.Flow / (custom.Area * 3600);
                                                }
                                                catch { }
                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                element.ModelWidth = Convert.ToString(custom.Width * 1000);
                                                element.ModelHeight = Convert.ToString(custom.Height * 1000);
                                                //custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                // custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop;
                                            custom.Origin = connect.Origin;
                                            InletConnector = custom;


                                            //SecondaryConnectors.Add(custom);
                                        }

                                    }
                                    else if (SystemType == DuctSystemType.ExhaustAir)
                                    {
                                        if (connect.Direction == FlowDirectionType.In)
                                        {
                                            custom.Flow = connect.Flow * 101.947308132875143184421534937;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.In;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2 * 304.8 / 1000;

                                                Diameter = custom.Diameter;
                                                element.ModelDiameter = Convert.ToString(custom.Diameter);
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                try
                                                {
                                                    ElbowRadius = document.GetElement(ElementId).LookupParameter("u").AsDouble() * 304.8 / 1000;
                                                    custom.Velocity = custom.Flow / (custom.Area * 3600);
                                                }
                                                catch { }

                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                element.ModelWidth = Convert.ToString(custom.Width * 1000);
                                                element.ModelHeight = Convert.ToString(custom.Height * 1000);
                                                //custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                //custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            OutletConnector = custom;
                                            //SecondaryConnectors.Add(custom);
                                        }
                                        if (connect.Direction == FlowDirectionType.Out)
                                        {
                                            custom.Flow = connect.Flow * 101.947308132875143184421534937;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.In;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2 * 304.8 / 1000;
                                                Diameter = custom.Diameter;
                                                element.ModelDiameter = Convert.ToString(custom.Diameter);
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                try
                                                {
                                                    ElbowRadius = document.GetElement(ElementId).LookupParameter("u").AsDouble() * 304.8 / 1000;
                                                    custom.Velocity = custom.Flow / (custom.Area * 3600);
                                                }
                                                catch { }

                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                element.ModelWidth = Convert.ToString(custom.Width * 1000);
                                                element.ModelHeight = Convert.ToString(custom.Height * 1000);
                                                //custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                //custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            InletConnector = custom;
                                            //SecondaryConnectors.Add(custom);
                                        }
                                    }

                                }

                            }

                        }
                    }

                }
            }


            if (dP==true)
            {
                element.PDyn = Element.Element.LookupParameter("AirTree_dP").AsDouble();
                element.ModelHydraulicArea = Convert.ToString(InletConnector.Area);
            }

            if (kMS == true && dP == false)
            {
                element.LocRes = Element.Element.LookupParameter("AirTree_КМС").AsDouble();
                Velocity = InletConnector.Velocity;
                element.ModelVelocity = Convert.ToString(Velocity);
                HydraulicArea = InletConnector.Area;
                element.ModelHydraulicArea = Convert.ToString(HydraulicArea);
                element.PDyn = element.LocRes * 0.6 * Velocity * Velocity;
            }
           /* if (kMS == true && dP == false)
            {

                element.ModelHydraulicArea = Convert.ToString(custom.Area);
                custom.Velocity = custom.Flow / (custom.Area * 3600);
                element.ModelVelocity = Convert.ToString(custom.Velocity);
                element.LocRes = Element.Element.LookupParameter("AirTree_КМС").AsDouble();

                PDyn = 0.6 * Velocity * Velocity * element.LocRes;
            }*/

            if (KZHS == true && dP == false)
            {
                double Koeffizient = Element.Element.LookupParameter("AirTree_КЖС").AsDouble();
                HydraulicArea = InletConnector.Area * Koeffizient;
                element.ModelHydraulicArea = Convert.ToString(HydraulicArea);
                Velocity = InletConnector.Flow / (HydraulicArea * 3600);
                element.ModelVelocity = Convert.ToString(Velocity);
                element.PDyn = 0.6 * Velocity * Velocity;
            }

            /*if (KZHS == true && dP == false)
            {
                custom.Area = custom.Width * custom.Height;
                Koeffizient = Element.Element.LookupParameter("AirTree_КЖС").AsDouble();
                custom.Area = custom.Area * Koeffizient;
                element.ModelHydraulicArea = Convert.ToString(custom.Area);
                custom.Velocity = custom.Flow / (custom.Area * 3600);
                element.ModelVelocity = Convert.ToString(custom.Velocity);
                PDyn = 0.6 * custom.Velocity * custom.Velocity;
            }*/

            if (FKMS == true && dP == false)
            {
                
                InletConnector.Area = Element.Element.LookupParameter("AirTree_F(КМС)").AsDouble();
                element.ModelHydraulicArea = Convert.ToString(InletConnector.Area);
                InletConnector.Velocity = InletConnector.Flow / (3600 * InletConnector.Area);
                element.ModelVelocity = Convert.ToString(InletConnector.Velocity);
                element.PDyn = InletConnector.Velocity * InletConnector.Velocity * 0.6;
            }

            if (FKMS == true && KZHS == true)
            {
                InletConnector.Area = InletConnector.Width * InletConnector.Height;


                double koeff = Element.Element.LookupParameter("AirTree_КЖС").AsDouble();



                double kmsArea = Element.Element.LookupParameter("AirTree_F(КМС)").AsDouble();

                InletConnector.Area = kmsArea * koeff;
                element.ModelHydraulicArea = Convert.ToString(InletConnector.Area);
                Velocity = InletConnector.Flow / (3600 * InletConnector.Area);
                element.ModelVelocity = Convert.ToString(Velocity);

                if (Element.Element.LookupParameter("AirTree_КМС").AsDouble() != 0)
                {
                    element.LocRes = Element.Element.LookupParameter("AirTree_КМС").AsDouble();
                    element.PDyn = element.LocRes * Velocity * Velocity * 0.6;
                }
                else
                {
                   element.PDyn = Velocity * Velocity * 0.6;
                }


            }


            if (FKMS == true && kMS == true && dP == false)
            {
                HydraulicArea = Element.Element.LookupParameter("AirTree_F(КМС)").AsDouble();
                element.ModelHydraulicArea = Convert.ToString(HydraulicArea);
                element.LocRes = Element.Element.LookupParameter("AirTree_КМС").AsDouble();
                Velocity = InletConnector.Flow / (3600 * HydraulicArea);
                element.ModelVelocity = Convert.ToString(Velocity);
                element.PDyn = element.LocRes * Velocity * Velocity * 0.6;
            }
            if (KZHS == true && kMS == true && dP == false)
            {
                double Koeffizient = Element.Element.LookupParameter("AirTree_КЖС").AsDouble();
                HydraulicArea =InletConnector.Area * Koeffizient;
                element.ModelHydraulicArea = Convert.ToString(HydraulicArea);
                Velocity =InletConnector.Flow / (3600 * HydraulicArea);
                element.ModelVelocity = Convert.ToString(Velocity);
                element.LocRes = Element.Element.LookupParameter("AirTree_КМС").AsDouble();
                element.PDyn = element.LocRes * Velocity *Velocity * 0.6;
            }
            if (KZHS ==true && FKMS==true && kMS==true)
            {
                double Koeffizient = Element.Element.LookupParameter("AirTree_КЖС").AsDouble();
                HydraulicArea = Element.Element.LookupParameter("AirTree_F(КМС)").AsDouble();
                element.LocRes = Element.Element.LookupParameter("AirTree_КМС").AsDouble();

                double area = HydraulicArea * Koeffizient;
                double flow = InletConnector.Flow;
                double velocity = flow / (3600 * area);
                element.ModelVelocity = Convert.ToString(velocity);
                element.ModelHydraulicArea = Convert.ToString(area);
                element.PDyn = element.LocRes * 0.6 * velocity * velocity;

            }

            if (kMS==false && dP==false && FKMS==false && KZHS == false )
            {
                element.ModelHydraulicArea = Convert.ToString(InletConnector.Area);
                element.ModelVelocity = Convert.ToString(InletConnector.Velocity);
                element.PDyn = 0.6 * InletConnector.Velocity * InletConnector.Velocity;

            }
            /*else
            {
                Velocity = InletConnector.Flow / (InletConnector.Area * 3600);
                element.Volume = Convert.ToString(Math.Round(InletConnector.Flow, 0));
                element.ModelVelocity = Convert.ToString(Math.Round(Velocity, 2));
                element.PDyn = 0.6 * Velocity * Velocity;
                
            }*/

           /* LocRes = element.Element.LookupParameter("AirTree_КМС").AsDouble();
            AirTree_Area = element.Element.LookupParameter("AirTree_F(КМС)").AsDouble();

            if (LocRes ==0)
            {
                LocRes = 0;
            }
            else
            {
                LocRes = element.Element.LookupParameter("AirTree_КМС").AsDouble();
            }
            if (AirTree_Area==0)
            {
                Velocity = InletConnector.Velocity;
                AirTree_Area = InletConnector.Area;
            }
            else
            {
                Velocity = InletConnector.Flow / (3600 * AirTree_Area);
            }*/

           

        }
    }
}
