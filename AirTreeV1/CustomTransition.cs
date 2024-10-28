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
        public CustomTransition(Autodesk.Revit.DB.Document document, CustomElement element)
        {
            Document = document;
            Element = element;
            ElementId = element.ElementId;
            SystemType = element.SystemType;

            
            try
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
                                                
                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;

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

                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
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
                                                

                                            }
                                            else
                                            {
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                            }
                                            custom.Coefficient = connect.Coefficient;

                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
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


                                            }
                                            else
                                            {
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
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
                    if (SystemType == DuctSystemType.ExhaustAir)
                    {

                        if (OutletConnector.Shape == ConnectorProfileType.Rectangular && InletConnector.Shape==ConnectorProfileType.Round && RelA>1)
                        {
                            element.DetailType = CustomElement.Detail.RectRoundExpansion;
                        }
                        else if (OutletConnector.Shape == ConnectorProfileType.Round && InletConnector.Shape == ConnectorProfileType.Rectangular && RelA>1)
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
                        MixedTransitionData elbowdata = new MixedTransitionData(SystemType, RelA, Angle);
                        elbowdata.Interpolation(100000, RelA, Angle);
                        LocRes = elbowdata.LocRes;

                    }

                }

                
                
                
            }
           

        }
        public static double Acot(double d)
        {
            if (d < 0) return (Math.PI - Math.Atan(1 / -d)*180/3.14);
            return Math.Atan(1.0 / d)*180/3.14;
        }
    }
    

}
