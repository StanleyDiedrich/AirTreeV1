using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;

namespace AirTreeV1
{
    public class CustomTee
    {
        public Document Document { get; set; }
        public CustomElement Element { get; set; }
        public ElementId ElementId { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public double Diameter { get; set; }
        public double ElbowRadius { get; set; }
        public XYZ LocPoint { get; set; }
        public CustomConnector InletConnector { get; set; }
        public CustomConnector OutletConnector { get; set; }
        public CustomConnector OutletConnector1 { get; set; }
        public CustomConnector OutletConnector2 { get; set; }

        public List<CustomConnector> OutletConnectors { get; set; } = new List<CustomConnector>();
        public DuctSystemType SystemType { get; set; }
        public double LocRes { get; set; }
        public ConnectorProfileType ProfileType { get; set; }
        public double RelA { get; set; }
        public double Angle { get; set; }
        public double Velocity { get; set; }

        public CustomTee(Autodesk.Revit.DB.Document document, CustomElement element)
        {
            Document = document;
            Element = element;
            ElementId = element.ElementId;
            SystemType = element.SystemType;
            LocPoint = ((element.Element.Location) as LocationPoint).Point;
            bool IsWidth1 = false;
            bool IsWidth3 = false;
            bool IsWidth = false;
            bool IsHeight1 = false;
            bool IsHeight3 = false;
            bool IsHeight = false;
            bool IsDiameter1 = false;
            bool IsDiameter3 = false;
            bool IsDiameter = false;


            var parameter = Element.Element.LookupParameter("Ширина воздуховода 1");
            if (parameter != null && parameter.HasValue)
            {
                IsWidth1 = true;

            }
            parameter = Element.Element.LookupParameter("Ширина воздуховода 3");
            if (parameter != null && parameter.HasValue)
            {
                IsWidth3 = true;

            }
            parameter = Element.Element.LookupParameter("Ширина воздуховода");
            if (parameter != null && parameter.HasValue)
            {
                IsWidth3 = true;
                IsWidth = true;
            }

            parameter = Element.Element.LookupParameter("Высота воздуховода");
            if (parameter != null && parameter.HasValue)
            {
                IsHeight1 = true;
                IsHeight = true;
            }
            parameter = Element.Element.LookupParameter("Высота воздуховода 3");
            if (parameter != null && parameter.HasValue)
            {
                IsHeight3 = true;

            }

            parameter = Element.Element.LookupParameter("Радиус воздуховода 1");
            if (parameter != null && parameter.HasValue)
            {
                IsDiameter1 = true;

            }
            parameter = Element.Element.LookupParameter("Радиус воздуховода 3");
            if (parameter != null && parameter.HasValue)
            {
                IsDiameter3 = true;

            }

            parameter = Element.Element.LookupParameter("Диаметр воздуховода");
            if (parameter != null && parameter.HasValue)
            {
                IsDiameter = true;

            }


            double width1 = 0;
            double width3 = 0;
            double width = 0;
            double height1 = 0;
            double height3 = 0;
            double height = 0;
            //double length = 0;
            double diameter1 = 0;
            double diameter3 = 0;
            double diameter = 0;

            if (IsWidth1 == true && IsWidth3 == true && IsHeight1 == true && IsHeight3 == true)
            {
                width1 = Convert.ToDouble(Element.Element.LookupParameter("Ширина воздуховода 1").AsValueString());
                width3 = Convert.ToDouble(Element.Element.LookupParameter("Ширина воздуховода 3").AsValueString());
                height1= Convert.ToDouble(Element.Element.LookupParameter("Высота воздуховода").AsValueString());
                height3 = Convert.ToDouble(Element.Element.LookupParameter("Высота воздуховода 3").AsValueString());
            }
            else if (IsDiameter1==true&&IsDiameter3==true)
            {
                diameter1 = 2*Convert.ToDouble(Element.Element.LookupParameter("Радиус воздуховода 1").AsValueString());
                diameter3 = 2*Convert.ToDouble(Element.Element.LookupParameter("Радиус воздуховода 3").AsValueString());
            }
            else if (IsWidth==true && IsWidth==true&& IsDiameter==true)
            {
                width = Convert.ToDouble(Element.Element.LookupParameter("Ширина воздуховода").AsValueString());
                height = Convert.ToDouble(Element.Element.LookupParameter("Высота воздуховода").AsValueString());
                diameter = Convert.ToDouble(Element.Element.LookupParameter("Диаметр воздуховода").AsValueString());

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
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2;
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (custom.Area * 6.68);
                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (6.68* custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;

                                            //SecondaryConnectors.Add(custom);
                                        }
                                        if (connect.Direction == FlowDirectionType.In)
                                        {
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2;
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (custom.Area * 6.68);

                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (6.68 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop;
                                            custom.Origin = connect.Origin;
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);
                                        }



                                    }
                                    else if (SystemType == DuctSystemType.ExhaustAir)
                                    {
                                        if (connect.Direction == FlowDirectionType.In)
                                        {
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.In;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                custom.Diameter = connect.Radius * 2;
                                                Diameter = custom.Diameter;
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (custom.Area * 6.68);

                                            }
                                            else
                                            {
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (6.68 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;
                                            //SecondaryConnectors.Add(custom);
                                        }
                                        else
                                        {
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.In;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                custom.Diameter = connect.Radius * 2;
                                                Diameter = custom.Diameter;
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (custom.Area * 6.68);

                                            }
                                            else
                                            {
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (6.68 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);

                                        }
                                    }

                                }

                            }

                        }
                    }

                }
            }
            double maxflow = double.MinValue;
            CustomConnector selectedconnector = null; // Initialize to null

            OutletConnector1 = OutletConnectors.OrderByDescending(x => x.Flow).FirstOrDefault();
            OutletConnector2 = OutletConnectors.OrderByDescending(x => x.Flow).LastOrDefault();
            if (OutletConnector1.Flow==OutletConnector2.Flow)
            {
                InletConnector.Vector = GetVector(InletConnector.Origin, LocPoint);
                OutletConnector1.Vector = GetVector(OutletConnector1.Origin, LocPoint);
                OutletConnector2.Vector = GetVector(OutletConnector2.Origin, LocPoint);
                // After identifying the main connector, set its IsMainConnector property
                StraightTee(InletConnector, OutletConnector1, OutletConnector2);
                CustomConnector swapconnector = null;
                if (OutletConnector2.IsStraight)
                {
                    swapconnector = OutletConnector1;
                    OutletConnector1 = OutletConnector2;
                    OutletConnector2 = swapconnector;
                }
            }

            else
            {
                InletConnector.Vector = GetVector(InletConnector.Origin, LocPoint);
                OutletConnector1.Vector = GetVector(OutletConnector1.Origin, LocPoint);
                OutletConnector2.Vector = GetVector(OutletConnector2.Origin, LocPoint);
                // After identifying the main connector, set its IsMainConnector property
                StraightTee(InletConnector, OutletConnector1, OutletConnector2);
            }
            //Допиши сюда что может быть на отвод
            double relA;
            double relQ;
            double relC;
            if (IsDiameter1 = true && IsDiameter3==true)
            {
                if (OutletConnector1.IsStraight == true)
                {
                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                    RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                    element.DetailType = CustomElement.Detail.RoundTeeStraight;
                    LocRes = roundTeeData.Interpolation(100000, relA, relQ);
                }
                else
                {
                    relA = OutletConnector2.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector2.Flow / InletConnector.Flow;
                    RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, false, relA, relQ);
                    element.DetailType = CustomElement.Detail.RoundTeeBranch;
                    LocRes = roundTeeData.Interpolation(100000, relA, relQ);
                }
            }
            else if (IsWidth1 == true && IsWidth3 == true && IsHeight1 == true && IsHeight3 == true)
            {
                if(OutletConnector1.IsStraight == true)
                {
                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                    relC = OutletConnector1.Velocity / InletConnector.Velocity;
                    RectTeeData rectTeeData = new RectTeeData(Element.SystemType, true, relA, relQ, relC, InletConnector);
                    element.DetailType = CustomElement.Detail.RectTeeStraight;
                    LocRes = rectTeeData.Interpolation(100000, relA, relQ);
                }
                else
                {
                    relA = OutletConnector2.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector2.Flow / InletConnector.Flow;
                    relC = OutletConnector2.Velocity / InletConnector.Velocity;
                    if (relQ ==0)
                    {
                        ElbowData elbowData = new ElbowData(InletConnector.Shape);
                        Width = InletConnector.Width;
                        Height = InletConnector.Height;
                        Radius = Document.GetElement(ElementId).LookupParameter("Длина воздуховода 1").AsDouble();
                        Diameter = Document.GetElement(ElementId).LookupParameter("Длина воздуховода 1").AsDouble();
                        Velocity = InletConnector.Velocity;
                        if (InletConnector.Shape == ConnectorProfileType.Rectangular)
                        {
                            double hw = Height / Radius;
                            double rw = Radius / Height;
                            LocRes = elbowData.Interpolation(hw, rw);
                            element.DetailType = CustomElement.Detail.RectElbow;
                        }
                        else
                        {
                            double rd = ElbowRadius / Diameter;
                            LocRes = elbowData.Interpolation(rd);
                            element.DetailType = CustomElement.Detail.RoundElbow;
                        }
                    }
                    else
                    {
                        RectTeeData rectTeeData = new RectTeeData(Element.SystemType, false, relA, relQ, relC, InletConnector);
                        element.DetailType = CustomElement.Detail.RectTeeBranch;
                        LocRes = rectTeeData.Interpolation(100000, relA, relQ);
                    }
                    
                }
            }
            else if (IsWidth==true&&IsHeight==true && IsDiameter==true)
            {
                if (OutletConnector1.IsStraight == true)
                {
                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                    relC = OutletConnector1.Velocity / InletConnector.Velocity;
                    MixedTeeData roundTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ,relC);
                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                    LocRes = roundTeeData.Interpolation(100000);
                }
                else
                {
                    relA = OutletConnector2.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector2.Flow / InletConnector.Flow;
                    relC = OutletConnector2.Velocity / InletConnector.Velocity;
                    MixedTeeData roundTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ,relC);
                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                    LocRes = roundTeeData.Interpolation(100000);
                }
            }

            Velocity = InletConnector.Velocity;

        }

        private XYZ GetVector(XYZ origin, XYZ locPoint)
        {
            XYZ result = null;
            double xorigin=origin.X;
            double yorigin=origin.Y;
            double zorigin=origin.Z;
            double xlocpoint= locPoint.X;
            double ylocpoint=locPoint.Y;
            double zlocpoint=locPoint.Z;

            result = new XYZ(xorigin - xlocpoint, yorigin - ylocpoint, zorigin - zlocpoint);
            return result;

        }
        public void StraightTee(CustomConnector inlet, CustomConnector outlet, CustomConnector outlet2)
        {
            XYZ inletvector = inlet.Vector;
            XYZ outletvector = outlet.Vector;

            double ivX = inletvector.X;
            double ivY = inletvector.Y;
            double ivZ = inletvector.Z;
            double ovX = outletvector.X;
            double ovY = outletvector.Y;
            double ovZ = outletvector.Z;

            double scalar = ivX * ovX + ivY * ovY + ivZ * ovZ;
            double vectorA = Math.Sqrt(Math.Pow(ivX, 2) + Math.Pow(ivY, 2) + Math.Pow(ivZ, 2));
            double vectorB = Math.Sqrt(Math.Pow(ovX, 2) + Math.Pow(ovY, 2) + Math.Pow(ovZ, 2));

            double radian = scalar / (vectorA * vectorB);
            double angle = Math.Round(Math.Acos(radian) * 57.3,0);
            if (angle ==180)
            {
                outlet.IsStraight = true;
            }
            else
            {
                outlet2.IsStraight = true;
            }
            
        }
    }
}
