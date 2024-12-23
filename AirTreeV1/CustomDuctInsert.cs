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
        public double Velocity { get; set; }

        public double IA { get; set; }
        public double IQ { get; set; }
        public double IC { get; set; }
        public double O1A { get; set; }
        public double O1Q { get; set; }
        public double O1C { get; set; }
        public double O2A { get; set; }
        public double O2Q { get; set; }
        public double O2C { get; set; }
        public double RA { get; set; }
        public double RQ { get; set; }
        public double RC { get; set; }

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
                                            custom.Flow = connect.Flow * 101.947308132875143184421534937;
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
                                                custom.Diameter = connect.Radius * 2 * 304.8 / 1000;
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (custom.Area * 3600);

                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            custom.PressureDrop = connect.PressureDrop;
                                            custom.AInlet = custom.Area;
                                            custom.AOutlet = custom.Area;
                                            // Вот это добавлено в версии 4.1
                                            /*InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;*/

                                            //SecondaryConnectors.Add(custom);
                                        }
                                        if (connect.Direction == FlowDirectionType.In || connect.Direction == FlowDirectionType.Bidirectional)
                                        {
                                            custom.Flow = connect.Flow * 101.947308132875143184421534937;
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
                                                custom.Diameter = connect.Radius * 2 * 304.8 / 1000;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;

                                                custom.Velocity = custom.Flow / (3600 * custom.Area);

                                            }
                                            else
                                            {
                                                ProfileType = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop;
                                            custom.Origin = connect.Origin;
                                            custom.AInlet = custom.Area;
                                            custom.AOutlet = custom.Area;
                                            /*OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);*/
                                        }



                                    }
                                    else if (SystemType == DuctSystemType.ExhaustAir )
                                    {
                                        if (connect.Direction == FlowDirectionType.In || connect.Direction == FlowDirectionType.Bidirectional)
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
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);

                                            }
                                            else
                                            {
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            OutletConnector = custom;
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnector.AInlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);
                                            /*InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;*/
                                            //SecondaryConnectors.Add(custom);
                                        }
                                        else if (connect.Direction == FlowDirectionType.Out || connect.Direction == FlowDirectionType.Bidirectional)
                                        {
                                            custom.Flow = connect.Flow * 101.947308132875143184421534937;
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
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2 * 304.8 / 1000;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);

                                            }
                                            else
                                            {
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnector.AInlet = custom.Area;
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
                                            custom.Flow = connect.Flow * 101.947308132875143184421534937;
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
                                                custom.Diameter = connect.Radius * 2 * 304.8 / 1000;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            else
                                            {
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;
                                            InletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(InletConnector);
                                            //DuctConnectors.Add(InletConnector);
                                            //SecondaryConnectors.Add(custom);
                                        }
                                        if (connect.Direction == FlowDirectionType.In || connect.Direction == FlowDirectionType.Bidirectional)
                                        {
                                            custom.Flow = connect.Flow * 101.947308132875143184421534937;
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
                                                custom.Diameter = connect.Radius * 2 * 304.8 / 1000;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);

                                            }
                                            else
                                            {
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop;
                                            custom.Origin = connect.Origin;
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnector.AInlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);
                                            //DuctConnectors.Add(OutletConnector);
                                        }



                                    }
                                    else if (SystemType == DuctSystemType.ExhaustAir)
                                    {
                                        if (connect.Direction == FlowDirectionType.In|| connect.Direction == FlowDirectionType.Out)
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
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);

                                            }
                                            else if( connect.Direction == FlowDirectionType.Bidirectional || connect.Direction == FlowDirectionType.Out)
                                            {
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnector.AInlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);
                                            //DuctConnectors.Add(InletConnector);
                                            //SecondaryConnectors.Add(custom);
                                        }
                                        else 
                                        {
                                            custom.Flow = connect.Flow * 101.947308132875143184421534937;
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
                                                ProfileType = ConnectorProfileType.Round;
                                                custom.Diameter = connect.Radius * 2 * 304.8 / 1000;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);

                                            }
                                            else
                                            {
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;
                                            InletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(InletConnector);
                                            
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

            InletConnector = OutletConnectors.OrderByDescending(x => x.Flow).FirstOrDefault();
            OutletConnector1 = OutletConnectors.OrderByDescending(x=>x.Flow).Skip(1).FirstOrDefault();
            OutletConnector2 = OutletConnectors.OrderByDescending(x => x.Flow).LastOrDefault();
            if (OutletConnector1.Flow == OutletConnector2.Flow)
            {
                InletConnector.Vector = GetVector(InletConnector.Origin, LocPoint);
                OutletConnector1.Vector = GetVector(OutletConnector1.Origin, LocPoint);
                OutletConnector2.Vector = GetVector(OutletConnector2.Origin, LocPoint);
                // After identifying the main connector, set its IsMainConnector property
                StraightTee(InletConnector, OutletConnector1, OutletConnector2, curvepoints);
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
                StraightTee(InletConnector, OutletConnector1, OutletConnector2,curvepoints);
            }

            IA = InletConnector.AInlet * 0.09;
            IQ = InletConnector.Flow * 102;
            IC = InletConnector.Velocity;
            O1A = OutletConnector1.AInlet * 0.09;
            O1Q = OutletConnector1.Flow * 102;
            O1C = OutletConnector1.Velocity;
            O2A = OutletConnector2.AInlet * 0.09;
            O2Q = OutletConnector2.Flow * 102;
            O2C = OutletConnector2.Velocity;

            Velocity = InletConnector.Velocity;
           
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


            if (inletRound ==true )
            {
                if (outlet2Round==true)
                {
                    if (OutletConnector1.IsStraight == true)
                    {
                        relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                        relQ = OutletConnector1.Flow / InletConnector.Flow;
                        RA = relA;
                        RQ = relQ;
                        RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                        element.DetailType = CustomElement.Detail.RoundInRoundDuctInsertStraight;
                        LocRes = roundTeeData.Interpolation(100000, relA, relQ);
                    }
                    else
                    {
                        relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                        relQ = OutletConnector1.Flow / InletConnector.Flow;
                        RA = relA;
                        RQ = relQ;
                        RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                        element.DetailType = CustomElement.Detail.RoundInRoundDuctInsertBranch;
                        LocRes = roundTeeData.Interpolation(100000, relA, relQ);
                    }
                }
                else
                {
                    if (OutletConnector1.IsStraight == true)
                    {
                        relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                        relQ = OutletConnector1.Flow / InletConnector.Flow;
                        relC = OutletConnector1.Velocity / InletConnector.Velocity;
                        RA = relA;
                        RQ = relQ;
                        RC = relC;
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
                        RA = relA;
                        RQ = relQ;
                        RC = relC;
                        MixedTeeData roundTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                        if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                        {
                            element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                            LocRes = roundTeeData.Interpolation(100000);
                        }

                    }
                }
            }
            else
            {
                if (outlet2Round==true)
                {
                    if (OutletConnector1.IsStraight == true)
                    {
                        relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                        relQ = OutletConnector1.Flow / InletConnector.Flow;
                        relC = OutletConnector1.Velocity / InletConnector.Velocity;
                        RA = relA;
                        RQ = relQ;
                        RC = relC;
                        MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                        if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                        {
                            element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                            LocRes = rectTeeData.Interpolation(100000);
                        }

                    }
                    else
                    {
                        relA = OutletConnector2.AOutlet / InletConnector.AInlet;
                        relQ = OutletConnector2.Flow / InletConnector.Flow;
                        relC = OutletConnector2.Velocity / InletConnector.Velocity;
                        RA = relA;
                        RQ = relQ;
                        RC = relC;
                        MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                        if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                        {
                            element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                            LocRes = rectTeeData.Interpolation(100000);
                        }

                    }
                }
                else
                {
                    if (OutletConnector1.IsStraight == true)
                    {
                        relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                        relQ = OutletConnector1.Flow / InletConnector.Flow;
                        relC = OutletConnector1.Velocity / InletConnector.Velocity;
                        RA = relA;
                        RQ = relQ;
                        RC = relC;
                        RectTeeData roundTeeData = new RectTeeData(Element.SystemType, true, relA, relQ, relC,InletConnector);
                        if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                        {
                            element.DetailType = CustomElement.Detail.RectInRectDuctInsertStraight;
                            LocRes = roundTeeData.Interpolation(100000,relA,relQ);
                        }

                    }
                     else
                       {
                           relA = OutletConnector2.AOutlet / InletConnector.AInlet;
                           relQ = OutletConnector2.Flow / InletConnector.Flow;
                           relC = OutletConnector2.Velocity / InletConnector.Velocity;
                           RA = relA;
                           RQ = relQ;
                           RC = relC;
                           RectTeeData roundTeeData = new RectTeeData(Element.SystemType, true, relA, relQ, relC,InletConnector);
                           if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                           {
                               element.DetailType = CustomElement.Detail.RectInRectDuctInsertBranch;
                               LocRes = roundTeeData.Interpolation(100000,relA,relQ);
                           }

                       }
                }
            }

           /* if (inletRound == true && outlet1Round==true && outlet2Round == true)
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
            else if (inletRound==true && outlet1Round==true && outlet2Round==false)
            {
                if (OutletConnector1.IsStraight == true)
                {
                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                    relC = OutletConnector1.Velocity / InletConnector.Velocity;
                    MixedTeeData roundTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                    if (element.DetailType!=CustomElement.Detail.AirTerminalConnection)
                    { element.DetailType = CustomElement.Detail.RectInRoundDuctInsertStraight;
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
            else if (inletRound == false && outlet1Round == false && outlet2Round == false)
            {
                if (OutletConnector1.IsStraight == true)
                {
                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                    relC = OutletConnector1.Velocity / InletConnector.Velocity;
                    RectTeeData roundTeeData = new RectTeeData(Element.SystemType, true, relA, relQ, relC, InletConnector);
                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                    {
                        element.DetailType = CustomElement.Detail.RectInRoundDuctInsertStraight;
                        LocRes = roundTeeData.Interpolation(100000,relA,relC);
                    }
                        
                }
                else
                {
                    relA = OutletConnector2.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector2.Flow / InletConnector.Flow;
                    relC = OutletConnector2.Velocity / InletConnector.Velocity;
                    RectTeeData roundTeeData = new RectTeeData(Element.SystemType, true, relA, relQ, relC, InletConnector);
                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                    {
                        element.DetailType = CustomElement.Detail.RectInRoundDuctInsertBranch;
                        LocRes = roundTeeData.Interpolation(100000,relA,relC);
                    }
                        
                }
            }
            else if (inletRound==false && outlet1Round==false && outlet2Round==true)
            {
                if (OutletConnector1.IsStraight == true)
                {
                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                    relC = OutletConnector1.Velocity / InletConnector.Velocity;
                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                    {
                        element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                        LocRes = rectTeeData.Interpolation(100000);
                    }
                       
                }
                else
                {
                    relA = OutletConnector2.AOutlet / InletConnector.AInlet;
                    relQ = OutletConnector2.Flow / InletConnector.Flow;
                    relC = OutletConnector2.Velocity / InletConnector.Velocity;
                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                    {
                        element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                        LocRes = rectTeeData.Interpolation(100000);
                    }
                       
                }
            }*/



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
        public void StraightTee(CustomConnector inlet, CustomConnector outlet, CustomConnector outlet2, IList<XYZ> curvepoints)
        {

            foreach (XYZ pt in curvepoints)
            {
                if (outlet.Origin ==pt)
                {
                    outlet.IsStraight=true;
                }
                else
                {
                    outlet2.IsStraight = true;
                }
            }
            /*XYZ inletvector = inlet.Vector;
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
            }*/

        }
    }
}
