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
    public class CustomElbow
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
        public ConnectorProfileType ProfileType { get; set; }
        public enum Rotation
        {
            Vertical,
            Horizontal
        }

        public CustomElbow(Autodesk.Revit.DB.Document document, CustomElement element)
        {
            Document = document;
            Element = element;
            ElementId = element.ElementId;
            SystemType = element.SystemType;
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
                                            custom.Flow = connect.Flow* 101.947308132875143184421534937;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            custom.Type = connect.ConnectorType;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2*304.8/1000;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.EquiDiameter = custom.Diameter;
                                                try
                                                {
                                                    ElbowRadius = document.GetElement(ElementId).LookupParameter("u").AsDouble()*304.8/1000;
                                                    custom.Velocity = custom.Flow /( custom.Area  *3600);
                                                }
                                                catch { }
                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width*304.8/1000;
                                                custom.Height = connect.Height*304.8/1000;
                                                //custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                //custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3600*custom.Area );
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
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.EquiDiameter = custom.Diameter;
                                                try
                                                {
                                                    ElbowRadius = document.GetElement(ElementId).LookupParameter("u").AsDouble()*304.8/1000;
                                                    custom.Velocity = custom.Flow / (custom.Area * 3600);
                                                }
                                                catch { }
                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
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
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                try
                                                {
                                                    ElbowRadius = document.GetElement(ElementId).LookupParameter("u").AsDouble()*304.8/1000;
                                                    custom.Velocity = custom.Flow / (custom.Area * 3600);
                                                }
                                                catch { }
                                                
                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
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
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                try
                                                {
                                                    ElbowRadius = document.GetElement(ElementId).LookupParameter("u").AsDouble()*304.8/1000;
                                                    custom.Velocity = custom.Flow / (custom.Area * 3600);
                                                }
                                                catch { }

                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
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

                if (GetRotation(OutletConnector, InletConnector)==Rotation.Horizontal)
                {
                    Width = OutletConnector.Width;
                    Height = OutletConnector.Height;
                   
                   
                }
                else
                {
                    Width = OutletConnector.Height;
                    Height = OutletConnector.Width;
                }

                try
                {
                    Angle = GetAngle(Document, ElementId);
                }
                catch
                {
                    Angle = 90;
                }

                Radius = Document.GetElement(ElementId).LookupParameter("Центр и радиус").AsDouble()*304.8/1000;
                Diameter = Document.GetElement(ElementId).LookupParameter("Центр и радиус").AsDouble()*304.8/1000;
                Velocity = OutletConnector.Velocity;
                ElbowData elbowdata = new ElbowData(ProfileType);
                if (ProfileType == ConnectorProfileType.Rectangular)
                {
                    double hw = 0;
                    double rw = 0;
                   /* double hw = Height / Width;
                    double rw = Radius / Width;*/
                    if (GetRotation(OutletConnector, InletConnector) == Rotation.Horizontal)
                    {
                        Width = OutletConnector.Width;
                        Height = OutletConnector.Height;
                         hw = Height / Width;
                         rw = Radius / Width;

                    }
                    else
                    {
                        Width = OutletConnector.Height;
                        Height = OutletConnector.Width;
                        hw = Height / Width;
                        rw = Radius / Width;
                    }




                    if (Angle<90)
                    {
                        LocRes = elbowdata.Interpolation(hw, rw)/2;

                    }
                    else
                    {
                        LocRes = elbowdata.Interpolation(hw, rw);
                    }
                   
                    element.DetailType = CustomElement.Detail.RectElbow;
                }
                else
                {
                    double rd = ElbowRadius / Diameter;
                    if (Angle <90)
                    {
                        LocRes = elbowdata.Interpolation(rd)/2;
                    }
                    else
                    {
                        LocRes = elbowdata.Interpolation(rd);
                    }
                   
                    element.DetailType = CustomElement.Detail.RoundElbow;
                }
            }
            

        }
        public double GetAngle (Autodesk.Revit.DB.Document document, ElementId elementId)
        {
            double res = 90;
            string value = document.GetElement(elementId).LookupParameter("Угол").AsValueString();
            string cleandedvalue = value.Replace("°", string.Empty).Replace(",",".");
            if (double.TryParse(cleandedvalue, out double result))
            {
                res = result;
            }
              
            return res;

        }
        public Rotation GetRotation (CustomConnector outletconnector, CustomConnector inletconnector)
        {
            if (outletconnector.Origin.Z == inletconnector.Origin.Z)
            {
                return Rotation.Horizontal;
            }
            else
            {
                return Rotation.Vertical;
            }
        }
    }
}
