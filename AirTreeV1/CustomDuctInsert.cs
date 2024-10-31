using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;

namespace AirTreeV1
{
    public class CustomDuctInsert
    {
        public Document Document { get; set; }
        public CustomElement Element { get; set; }
        public CustomElement NextElement { get; set; }
        public ElementId ElementId { get; set; }
        public ElementId NextElementId { get; set; }
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
        public List<CustomConnector> DuctConnectors { get; set; } = new List<CustomConnector>();

        public DuctSystemType SystemType { get; set; }
        public double LocRes { get; set; }
        public ConnectorProfileType ProfileType { get; set; }
        public double RelA { get; set; }
        public double Angle { get; set; }


        public CustomDuctInsert(Autodesk.Revit.DB.Document document, CustomElement element)
        {
            Document = document;
            Element = element;
            ElementId = element.ElementId;
            
            NextElementId = element.NextElementId;
            Element NextElement = document.GetElement(element.NextElementId);
            


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

                                        if (connect.Direction == FlowDirectionType.Out || connect.Direction == FlowDirectionType.Bidirectional)
                                        {
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            if (Document.GetElement(custom.NextOwnerId).Category.Id.IntegerValue == -2008013)
                                            {
                                                Element.DetailType = CustomElement.Detail.AirTerminalConnection;

                                            }
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
                                                custom.Velocity = custom.Flow / (3.3 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                            /*InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;*/

                                            //SecondaryConnectors.Add(custom);
                                        }
                                        if (connect.Direction == FlowDirectionType.In || connect.Direction == FlowDirectionType.Bidirectional)
                                        {
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;

                                            if (Document.GetElement(NextElementId).Category.Id.IntegerValue == -2008013)
                                            {
                                                Element.DetailType = CustomElement.Detail.AirTerminalConnection;
                                                break;
                                            }
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
                                                custom.Velocity = custom.Flow / (3.3 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop;
                                            custom.Origin = connect.Origin;
                                            /*OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);*/
                                        }



                                    }
                                    else if (SystemType == DuctSystemType.ExhaustAir )
                                    {
                                        if (connect.Direction == FlowDirectionType.In || connect.Direction == FlowDirectionType.Bidirectional)
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
                                                custom.Velocity = custom.Flow / (3.3 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            /*InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;*/
                                            //SecondaryConnectors.Add(custom);
                                        }
                                        else if (connect.Direction == FlowDirectionType.Out || connect.Direction == FlowDirectionType.Bidirectional)
                                        {
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.In;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            if (Document.GetElement(custom.NextOwnerId).Category.Id.IntegerValue == -2008013)
                                            {
                                                Element.DetailType = CustomElement.Detail.AirTerminalConnection;

                                            }
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
                                                custom.Velocity = custom.Flow / (3.3 * custom.Area);
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
            
            if (NextElement is Duct)
            {
                foreach (Connector connector in (NextElement as MEPCurve).ConnectorManager.Connectors)
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
                                CustomConnector custom = new CustomConnector(Document, NextElement.Id, SystemType);

                                if (Document.GetElement(connect.Owner.Id) is MechanicalSystem || Document.GetElement(connect.Owner.Id) is DuctInsulation)
                                {
                                    continue;
                                }

                                else if (connect.Owner.Id == NextElement.Id)
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

                                        if (connect.Direction == FlowDirectionType.Out || connect.Direction == FlowDirectionType.Bidirectional)
                                        {
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            /*if (Document.GetElement(NextElementId).Category.Id.IntegerValue == -2008013)
                                            {
                                                Element.DetailType = CustomElement.Detail.AirTerminalConnection;

                                            }*/
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
                                                custom.Velocity = custom.Flow / (3.3 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;
                                            OutletConnectors.Add(InletConnector);
                                            //DuctConnectors.Add(InletConnector);
                                            //SecondaryConnectors.Add(custom);
                                        }
                                        if (connect.Direction == FlowDirectionType.In || connect.Direction == FlowDirectionType.Bidirectional)
                                        {
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;

                                            if (Document.GetElement(NextElementId).Category.Id.IntegerValue == -2008013)
                                            {
                                                Element.DetailType = CustomElement.Detail.AirTerminalConnection;

                                            }
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
                                                custom.Velocity = custom.Flow / (3.3 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop;
                                            custom.Origin = connect.Origin;
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);
                                            //DuctConnectors.Add(OutletConnector);
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
                                            else if( connect.Direction == FlowDirectionType.Bidirectional || connect.Direction == FlowDirectionType.Out)
                                            {
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3.3 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);
                                            //DuctConnectors.Add(InletConnector);
                                            //SecondaryConnectors.Add(custom);
                                        }
                                        else 
                                        {
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            //custom.DirectionType = FlowDirectionType.In;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            if (Document.GetElement(NextElementId).Category.Id.IntegerValue == -2008013)
                                            {
                                                Element.DetailType = CustomElement.Detail.AirTerminalConnection;
                                                
                                            }
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
                                                custom.Velocity = custom.Flow / (3.3 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;
                                            
                                           // DuctConnectors.Add(OutletConnector);

                                        }
                                    }

                                }

                            }

                        }
                    }

                }
            }

            IList<XYZ> curvepoints = (NextElement.Location as LocationCurve).Curve.Tessellate();
            LocPoint = GetCenter(curvepoints);

            double maxflow = double.MinValue;
            CustomConnector selectedconnector = null; // Initialize to null

            InletConnector = OutletConnectors.OrderByDescending(x => x.Coefficient).FirstOrDefault();
            OutletConnector1 = OutletConnectors.OrderByDescending(x=>x.Coefficient).Skip(1).FirstOrDefault();
            OutletConnector2 = OutletConnectors.OrderByDescending(x => x.Coefficient).LastOrDefault();
            if (OutletConnector1.Flow == OutletConnector2.Flow)
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

           
            bool inletRound = false;
            
            bool outlet1Round = false;
            
            bool outlet2Round = false;

            if (InletConnector.Shape==ConnectorProfileType.Round)
            {
                inletRound = true;
            }
           
            if (OutletConnector1.Shape==ConnectorProfileType.Round)
            {
                outlet1Round = true;
            }
            
           
            if (OutletConnector2.Shape == ConnectorProfileType.Round)
            {
                outlet2Round = true;
            }
           

            double relA;
            double relQ;
            double relC;

            if (inletRound == true && outlet1Round==true && outlet2Round == true)
            {
                if (OutletConnector1.IsStraight==true)
                {
                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                    RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                    element.DetailType = CustomElement.Detail.RoundInRoundDuctInsertStraight;
                    LocRes = roundTeeData.Interpolation(100000, relA, relQ);
                }
                else
                {
                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                    RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                    element.DetailType = CustomElement.Detail.RoundInRoundDuctInsertBranch;
                    LocRes = roundTeeData.Interpolation(100000, relA, relQ);
                }
            }
            else if (inletRound==true && outlet1Round==false && outlet2Round==false)
            {
                if (OutletConnector1.IsStraight == true)
                {
                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                    relC = OutletConnector1.Velocity / InletConnector.Velocity;
                    MixedTeeData roundTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                    if (element.DetailType!=CustomElement.Detail.AirTerminalConnection)
                    { element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                        LocRes = roundTeeData.Interpolation(100000);
                    }
                    
                    
                }
                else
                {
                    relA = OutletConnector2.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector2.Flow / InletConnector.Flow;
                    relC = OutletConnector2.Velocity / InletConnector.Velocity;
                    MixedTeeData roundTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                    {
                        element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                        LocRes = roundTeeData.Interpolation(100000);
                    }
                        
                }
            }
            else if (inletRound == true && outlet1Round == false && outlet2Round == false)
            {
                if (OutletConnector1.IsStraight == true)
                {
                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                    relC = OutletConnector1.Velocity / InletConnector.Velocity;
                    MixedTeeData roundTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                    {
                        element.DetailType = CustomElement.Detail.RectInRoundDuctInsertStraight;
                        LocRes = roundTeeData.Interpolation(100000);
                    }
                        
                }
                else
                {
                    relA = OutletConnector2.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector2.Flow / InletConnector.Flow;
                    relC = OutletConnector2.Velocity / InletConnector.Velocity;
                    MixedTeeData roundTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                    {
                        element.DetailType = CustomElement.Detail.RectInRoundDuctInsertBranch;
                        LocRes = roundTeeData.Interpolation(100000);
                    }
                        
                }
            }
            else if (inletRound==false && outlet1Round==false && outlet2Round==false)
            {
                if (OutletConnector1.IsStraight == true)
                {
                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                    relC = OutletConnector1.Velocity / InletConnector.Velocity;
                    RectTeeData rectTeeData = new RectTeeData(Element.SystemType, true, relA, relQ, relC, InletConnector);
                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                    {
                        element.DetailType = CustomElement.Detail.RectInRectDuctInsertStraight;
                        LocRes = rectTeeData.Interpolation(100000, relA, relQ);
                    }
                       
                }
                else
                {
                    relA = OutletConnector2.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector2.Flow / InletConnector.Flow;
                    relC = OutletConnector2.Velocity / InletConnector.Velocity;
                    RectTeeData rectTeeData = new RectTeeData(Element.SystemType, false, relA, relQ, relC, InletConnector);
                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                    {
                        element.DetailType = CustomElement.Detail.RectInRectDuctInsertBranch;
                        LocRes = rectTeeData.Interpolation(100000, relA, relQ);
                    }
                       
                }
            }



        }

        private XYZ GetCenter(IList<XYZ> curvepoints)
        {
            XYZ startpoint = curvepoints.First();
            XYZ endpoint = curvepoints.Last();

            double spX = startpoint.X;
            double spY = startpoint.Y;
            double spZ = startpoint.Z;

            double epX = endpoint.X;
            double epY = endpoint.Y;
            double epZ = endpoint.Z;

            double pX = (epX + spX) / 2;
            double pY = (epX + spY) / 2;
            double pZ = (epZ + spZ) / 2;

            XYZ res = new XYZ(pX, pY, pZ);
            return res;

        }

        private XYZ GetVector(XYZ origin, XYZ locPoint)
        {
            XYZ result = null;
            double xorigin = origin.X;
            double yorigin = origin.Y;
            double zorigin = origin.Z;
            double xlocpoint = locPoint.X;
            double ylocpoint = locPoint.Y;
            double zlocpoint = locPoint.Z;

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
            double angle = Math.Round(Math.Acos(radian) * 57.3, 0);
            if (angle<20 || angle >160)
            {
                angle = 0;
            }
            
            if (angle == 180 || angle ==0)
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
