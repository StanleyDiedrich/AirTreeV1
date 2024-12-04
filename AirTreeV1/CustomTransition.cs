using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;
using System.Windows.Media.Animation;
using Autodesk.Revit.DB.Architecture;

namespace AirTreeV1
{
    public class CustomTransition
    {
        public Document Document { get; set; }
        public CustomElement Element { get; set; }
        public ElementId ElementId { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public double Diameter { get; set; }
        public double ElbowRadius { get; set; }
        public CustomConnector InletConnector { get; set; }
        public CustomConnector OutletConnector { get; set; }
        public DuctSystemType SystemType { get; set; }
        public double LocRes { get; set; }
        public ConnectorProfileType ProfileType { get; set; }
        public double RelA { get; set; }
        public double Angle { get; set; }

        public double Velocity { get; set; }
        public CustomTransition(Autodesk.Revit.DB.Document document, CustomElement element)
        {
            Document = document;
            Element = element;
            ElementId = element.ElementId;
            SystemType = element.SystemType;

            bool IsWidth1 = false;
            bool IsWidth2 = false;
            bool IsWidth = false;
            bool IsHeight1 = false;
            bool IsHeight2 = false;
            bool IsHeight = false;
            bool IsDiameter1 = false;
            bool IsDiameter2 = false;
            bool IsDiameter = false;

            var parameter = Element.Element.LookupParameter("Ширина воздуховода 1");
            if (parameter != null && parameter.HasValue)
            {
                IsWidth1 = true;
                
            }
            parameter = Element.Element.LookupParameter("Ширина воздуховода 2");
            if (parameter != null && parameter.HasValue)
            {
                IsWidth2 = true;

            }
            parameter = Element.Element.LookupParameter("Ширина воздуховода");
            if (parameter != null && parameter.HasValue)
            {
                IsWidth = true;

            }
            parameter = Element.Element.LookupParameter("Высота воздуховода 1");
            if (parameter != null && parameter.HasValue)
            {
                IsHeight1 = true;

            }
            parameter = Element.Element.LookupParameter("Высота воздуховода 2");
            if (parameter != null && parameter.HasValue)
            {
                IsHeight2 = true;

            }
            parameter = Element.Element.LookupParameter("Высота воздуховода");
            if (parameter != null && parameter.HasValue)
            {
                IsHeight = true;

            }
            parameter = Element.Element.LookupParameter("Диаметр воздуховода 1");
            if (parameter != null && parameter.HasValue)
            {
                IsDiameter1 = true;

            }
            parameter = Element.Element.LookupParameter("Диаметр воздуховода 2");
            if (parameter != null && parameter.HasValue)
            {
                IsDiameter2 = true;

            }
            parameter = Element.Element.LookupParameter("Диаметр воздуховода");
            if (parameter != null && parameter.HasValue)
            {
                IsDiameter = true;

            }

            double width1 = 0;
            double width2 = 0;
            double height1 = 0;
            double height2 = 0;
            double length = 0;
            double diameter1 = 0;
            double diameter2 = 0;

            if (IsWidth1 == true && IsWidth2 == true && IsHeight1 == true && IsHeight2 == true)
            {
                width1 = Convert.ToDouble(Element.Element.LookupParameter("Ширина воздуховода 1").AsValueString());
                width2 = Convert.ToDouble(Element.Element.LookupParameter("Ширина воздуховода 2").AsValueString());
                length = Convert.ToDouble(Element.Element.LookupParameter("Длина воздуховода").AsValueString());
                if (width1 != width2)
                {
                    double d = Math.Abs((2 * length / (width1 - width2)));
                    double angle = Acot(d);
                    Angle = 2 * angle;
                }
                else
                {
                    height1 = Convert.ToDouble(Element.Element.LookupParameter("Высота воздуховода 1").AsValueString());
                    height2 = Convert.ToDouble(Element.Element.LookupParameter("Высота воздуховода 2").AsValueString());

                    if (height1 == height2)
                    {
                        element.DetailType = CustomElement.Detail.RectangularDuct;
                        return;
                    }
                    double d = Math.Abs((2 * length / (height1 - height2)));
                    double angle = Acot(d);
                    Angle = 2 * angle;
                }
            }
            else if (IsDiameter1 == true && IsDiameter2 == true)
            {
                length = Convert.ToDouble(Element.Element.LookupParameter("L").AsValueString());
                 diameter1 = Convert.ToDouble(Element.Element.LookupParameter("Диаметр воздуховода 1").AsValueString());
                 diameter2 = Convert.ToDouble(Element.Element.LookupParameter("Диаметр воздуховода 2").AsValueString());

                if (diameter1 == diameter2)
                {
                    element.DetailType = CustomElement.Detail.RoundDuct;
                    return;
                }
                else
                {
                    double d = Math.Abs((2 * length / (diameter1 - diameter2)));
                    double angle = Acot(d);
                    Angle = 2 * angle;
                }
            }
            else if (IsWidth == true && IsHeight == true && IsDiameter == true)
            {
                width1 = Convert.ToDouble(Element.Element.LookupParameter("Ширина воздуховода").AsValueString());
                height1 = Convert.ToDouble(Element.Element.LookupParameter("Высота воздуховода").AsValueString());
                diameter1 = Convert.ToDouble(Element.Element.LookupParameter("Диаметр воздуховода").AsValueString());
                length = Convert.ToDouble(Element.Element.LookupParameter("Длина воздуховода").AsValueString());
                double d = (1.13 * Math.Sqrt(width1 * height1) - diameter1) / (2 * length);
                double angle = Math.Abs(2 * Math.Atan(d))*57.3;
                Angle = d;
            }



            /*try
            {
                double width1 = Convert.ToDouble(Element.Element.LookupParameter("Ширина воздуховода 1").AsValueString());
                double width2 = Convert.ToDouble(Element.Element.LookupParameter("Ширина воздуховода 2").AsValueString());
                double length = Convert.ToDouble(Element.Element.LookupParameter("Длина воздуховода").AsValueString());
                if (width1 != width2)
                {
                    double d = Math.Abs((2 * length / (width1 - width2)));
                    double angle = Acot(d);
                    Angle = 2 * angle;
                }
                else
                {
                    double height1 = Convert.ToDouble(Element.Element.LookupParameter("Высота воздуховода 1").AsValueString());
                    double height2 = Convert.ToDouble(Element.Element.LookupParameter("Высота воздуховода 2").AsValueString());

                    if (height1 == height2)
                    {
                        element.DetailType = CustomElement.Detail.RectangularDuct;
                        return;
                    }
                    double d = Math.Abs((2 * length / (height1 - height2)));
                    double angle = Acot(d);
                    Angle = 2 * angle;


                }
            }
            catch
            {
                double length = Convert.ToDouble(Element.Element.LookupParameter("L").AsValueString());
                double diameter1 = Convert.ToDouble(Element.Element.LookupParameter("Диаметр воздуховода 1").AsValueString());
                double diameter2 = Convert.ToDouble(Element.Element.LookupParameter("Диаметр воздуховода 2").AsValueString());

                if (diameter1==diameter2)
                {
                    element.DetailType = CustomElement.Detail.RoundDuct;
                    return;
                }
                else
                {
                    double d = Math.Abs((2 * length / (diameter1 - diameter2)));
                    double angle = Acot(d);
                    Angle = 2 * angle;
                }
                
            }*/


            if (Angle<10)
            {
                Angle = 10;
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
                                                custom.Diameter = connect.Radius * 2*304.8/1000;
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600* custom.Area);
                                                
                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width*304.8/1000;
                                                custom.Height = connect.Height*304.8/1000;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;

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
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);

                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;
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
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);

                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;

                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            //SecondaryConnectors.Add(custom);
                                        }
                                        else
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
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);

                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;
                                        }
                                    }

                                }

                            }

                        }
                    }

                }
                if (InletConnector.Shape==ConnectorProfileType.Rectangular && OutletConnector.Shape==ConnectorProfileType.Rectangular)
                {
                    if (SystemType == DuctSystemType.ExhaustAir)
                    {
                        RelA = OutletConnector.AOutlet / InletConnector.AInlet;
                        if (RelA > 1)
                        {
                            element.DetailType = CustomElement.Detail.RectExpansion;
                        }
                        else
                        {
                            element.DetailType = CustomElement.Detail.RectContraction;
                        }
                        RectTransitionData elbowdata = new RectTransitionData(SystemType, RelA, Angle);
                        elbowdata.Interpolation(100000, RelA, Angle);
                        LocRes = elbowdata.LocRes;
                        

                    }
                    else
                    {
                        RelA = InletConnector.AInlet / OutletConnector.AOutlet;
                        if (RelA > 1)
                        {
                            element.DetailType = CustomElement.Detail.RectExpansion;
                        }
                        else
                        {
                            element.DetailType = CustomElement.Detail.RectContraction;
                        }
                        RectTransitionData elbowdata = new RectTransitionData(SystemType, RelA, Angle);
                        elbowdata.Interpolation(100000, RelA, Angle);
                        LocRes = elbowdata.LocRes;
                        
                    }
                }
                else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector.Shape == ConnectorProfileType.Round)
                {
                    if (SystemType == DuctSystemType.ExhaustAir)
                    {
                        RelA = OutletConnector.AOutlet / InletConnector.AInlet;
                        if (RelA > 1)
                        {
                            element.DetailType = CustomElement.Detail.RoundExpansion;
                        }
                        else
                        {
                            element.DetailType = CustomElement.Detail.RoundContraction;
                        }
                        
                        RoundTransitionData elbowdata = new RoundTransitionData(SystemType, RelA, Angle);
                        elbowdata.Interpolation(100000, RelA, Angle);
                        LocRes = elbowdata.LocRes;
                        

                    }
                    else
                    {
                        RelA = InletConnector.AInlet / OutletConnector.AOutlet;
                        if (RelA > 1)
                        {
                            element.DetailType = CustomElement.Detail.RoundExpansion;
                        }
                        else
                        {
                            element.DetailType = CustomElement.Detail.RoundContraction;
                        }
                        RoundTransitionData elbowdata = new RoundTransitionData(SystemType, RelA, Angle);
                        elbowdata.Interpolation(100000, RelA, Angle);
                        LocRes = elbowdata.LocRes;
                        
                    }

                }
                else if ((InletConnector.Shape==ConnectorProfileType.Round&&OutletConnector.Shape==ConnectorProfileType.Rectangular)||(InletConnector.Shape == ConnectorProfileType.Rectangular && OutletConnector.Shape == ConnectorProfileType.Round) )
                {
                    RelA = OutletConnector.AOutlet / InletConnector.AInlet;
                    if (SystemType == DuctSystemType.ExhaustAir)
                    {

                        if (OutletConnector.Shape == ConnectorProfileType.Rectangular && InletConnector.Shape==ConnectorProfileType.Round && RelA>1)
                        {
                            element.DetailType = CustomElement.Detail.RoundRectExpansion;
                        }
                        else if (OutletConnector.Shape == ConnectorProfileType.Round && InletConnector.Shape == ConnectorProfileType.Rectangular && RelA>1)
                        {
                            element.DetailType = CustomElement.Detail.RectRoundExpansion;
                        }
                        else if (OutletConnector.Shape == ConnectorProfileType.Rectangular && InletConnector.Shape == ConnectorProfileType.Round && RelA < 1)
                        {
                            element.DetailType = CustomElement.Detail.RoundRectContraction;
                        }
                        else if (OutletConnector.Shape == ConnectorProfileType.Round && InletConnector.Shape == ConnectorProfileType.Rectangular && RelA < 1)
                        {
                            element.DetailType = CustomElement.Detail.RectRoundContraction;
                        }

                       /* RelA = OutletConnector.AOutlet / InletConnector.AInlet;
                        if (RelA > 1)
                        {
                            element.DetailType = CustomElement.Detail.RoundExpansion;
                        }
                        else
                        {
                            element.DetailType = CustomElement.Detail.RoundContraction;
                        }*/

                        MixedTransitionData elbowdata = new MixedTransitionData(SystemType, RelA, Angle);
                        elbowdata.Interpolation(100000, RelA, Angle);
                        LocRes = elbowdata.LocRes;


                    }
                   else if (SystemType == DuctSystemType.SupplyAir)
                    {
                        RelA = InletConnector.AInlet/OutletConnector.AOutlet;
                        if (OutletConnector.Shape == ConnectorProfileType.Rectangular && InletConnector.Shape == ConnectorProfileType.Round && RelA > 1)
                        {
                            element.DetailType = CustomElement.Detail.RectRoundExpansion;
                        }
                        else if (OutletConnector.Shape == ConnectorProfileType.Round && InletConnector.Shape == ConnectorProfileType.Rectangular && RelA > 1)
                        {
                            element.DetailType = CustomElement.Detail.RoundRectExpansion;
                        }
                        else if (OutletConnector.Shape == ConnectorProfileType.Rectangular && InletConnector.Shape == ConnectorProfileType.Round && RelA < 1)
                        {
                            element.DetailType = CustomElement.Detail.RoundRectContraction;
                        }
                        else if (OutletConnector.Shape == ConnectorProfileType.Round && InletConnector.Shape == ConnectorProfileType.Rectangular && RelA < 1)
                        {
                            element.DetailType = CustomElement.Detail.RectRoundContraction;
                        }

                        /* RelA = OutletConnector.AOutlet / InletConnector.AInlet;
                         if (RelA > 1)
                         {
                             element.DetailType = CustomElement.Detail.RoundExpansion;
                         }
                         else
                         {
                             element.DetailType = CustomElement.Detail.RoundContraction;
                         }*/

                        MixedTransitionData elbowdata = new MixedTransitionData(SystemType, RelA, Angle);
                        elbowdata.Interpolation(100000, RelA, Angle);
                        LocRes = elbowdata.LocRes;


                    }

                    /*else
                    {
                        RelA = InletConnector.AInlet / OutletConnector.AOutlet;
                        if (RelA > 1)
                        {
                            element.DetailType = CustomElement.Detail.RoundExpansion;
                        }
                        else
                        {
                            element.DetailType = CustomElement.Detail.RoundContraction;
                        }
                        MixedTransitionData elbowdata = new MixedTransitionData(SystemType, RelA, Angle);
                        elbowdata.Interpolation(100000, RelA, Angle);
                        LocRes = elbowdata.LocRes;

                    }*/

                }

                
                
                
            }
           
            if (InletConnector.Velocity>OutletConnector.Velocity)
            {
                Velocity = InletConnector.Velocity;
            }
            else
            {
                Velocity = OutletConnector.Velocity;
            }
        }
        public static double Acot(double d)
        {
            if (d < 0) return (Math.PI - Math.Atan(1 / -d)*180/3.14);
            return Math.Atan(1.0 / d)*180/3.14;
        }
    }
    

}
