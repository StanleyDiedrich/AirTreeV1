using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;

namespace AirTreeV1
{
    public class CustomTee2
    {
        public Document Document { get; set; }
        public CustomElement Element { get; set; }
        public CustomElement NextElement { get; set; }
        public CustomElement PreviousElement { get; set; }
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

        public List<ElementId> PreviousElements { get; set; } = new List<ElementId>();
        public List<CustomConnector> OutletConnectors { get; set; } = new List<CustomConnector>();
        public List<CustomConnector> DuctConnectors { get; set; } = new List<CustomConnector>();

        public DuctSystemType SystemType { get; set; }
        public double LocRes { get; set; }
        public ConnectorProfileType ProfileType { get; set; }
        public ConnectorType ConnectorType { get; set; }
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
        public CustomTee2(Autodesk.Revit.DB.Document document, CustomElement element, List<CustomBranch> collection, bool isReversed)
        {
            Document = document;
            Element = element;
            ElementId = element.ElementId;
            SystemType = element.SystemType;
            LocPoint = ((element.Element.Location) as LocationPoint).Point;

            if (Element.Element is FamilyInstance)
            {

                //ModelVelocity = GetValue(primaryvelocity);
                foreach (Connector connector in Element.OwnConnectors)
                {
                    CustomConnector custom = new CustomConnector(Document, ElementId, SystemType);
                    SystemType = connector.DuctSystemType;
                    if (SystemType == DuctSystemType.SupplyAir)
                    {
                        custom.Flow = connector.Flow * 101.947308132875143184421534937;
                        custom.Domain = Domain.DomainHvac;
                        //custom.DirectionType = FlowDirectionType.Out;
                        custom.OwnerId = connector.Owner.Id;
                        custom.ConnectorType = connector.ConnectorType;
                        custom.Shape = connector.Shape;
                        custom.Origin = connector.Origin;
                        if (custom.Shape == ConnectorProfileType.Round)
                        {
                            ProfileType = ConnectorProfileType.Round;
                            custom.Diameter = connector.Radius * 2 * 304.8 / 1000;
                            custom.EquiDiameter = custom.Diameter;
                            custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                            custom.Velocity = custom.Flow / (3600 * custom.Area);

                        }
                        else
                        {
                            ProfileType = ConnectorProfileType.Rectangular;
                            custom.Width = connector.Width * 304.8 / 1000;
                            custom.Height = connector.Height * 304.8 / 1000;
                            //custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                            //custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                            custom.Area = custom.Width * custom.Height;
                            custom.Velocity = custom.Flow / (3600 * custom.Area);
                        }
                        custom.Coefficient = connector.Coefficient;
                        custom.Origin = connector.Origin;
                        custom.PressureDrop = connector.PressureDrop;


                        ConnectorSet nextconnectors = connector.AllRefs;

                        foreach (Connector connect in nextconnectors)
                        {
                            if (connect.Domain != Domain.DomainHvac)
                            {
                                continue;
                            }
                            else
                            {

                                if (Document.GetElement(connect.Owner.Id) is MechanicalSystem || Document.GetElement(connect.Owner.Id) is DuctInsulation)
                                {
                                    continue;
                                }
                                else if (connect.Owner.Id == NextElementId)
                                {
                                    continue;
                                }
                                if (connect.Domain == Autodesk.Revit.DB.Domain.DomainHvac || connect.Domain == Autodesk.Revit.DB.Domain.DomainPiping)
                                {
                                    if (SystemType == DuctSystemType.SupplyAir)
                                    {

                                        if (connect.Direction == FlowDirectionType.In || connect.Direction == FlowDirectionType.Bidirectional)
                                        {

                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;

                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);

                                        }
                                        if (connect.Direction == FlowDirectionType.Out)
                                        {
                                            custom.NextOwnerId = connect.Owner.Id;
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;
                                        }

                                    }

                                }
                            }
                        }
                    }
                    else if (SystemType == DuctSystemType.ExhaustAir)
                    {
                        custom.Flow = connector.Flow * 101.947308132875143184421534937;
                        custom.Domain = Domain.DomainHvac;
                        //custom.DirectionType = FlowDirectionType.Out;
                        custom.OwnerId = connector.Owner.Id;
                        custom.ConnectorType = connector.ConnectorType;
                        custom.Shape = connector.Shape;
                        custom.Origin = connector.Origin;
                        if (custom.Shape == ConnectorProfileType.Round)
                        {
                            ProfileType = ConnectorProfileType.Round;
                            custom.Diameter = connector.Radius * 2 * 304.8 / 1000;
                            custom.EquiDiameter = custom.Diameter;
                            custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                            custom.Velocity = custom.Flow / (3600 * custom.Area);

                        }
                        else
                        {
                            ProfileType = ConnectorProfileType.Rectangular;
                            custom.Width = connector.Width * 304.8 / 1000;
                            custom.Height = connector.Height * 304.8 / 1000;
                            /*custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                            custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;*/
                            custom.Area = custom.Width * custom.Height;
                            custom.Velocity = custom.Flow / (3600 * custom.Area);
                        }
                        custom.Coefficient = connector.Coefficient;
                        custom.Origin = connector.Origin;
                        custom.PressureDrop = connector.PressureDrop;


                        ConnectorSet nextconnectors = connector.AllRefs;

                        foreach (Connector connect in nextconnectors)
                        {
                            if (connect.Domain != Domain.DomainHvac)
                            {
                                continue;
                            }
                            else
                            {

                                if (Document.GetElement(connect.Owner.Id) is MechanicalSystem || Document.GetElement(connect.Owner.Id) is DuctInsulation)
                                {
                                    continue;
                                }
                                else if (connect.Owner.Id == NextElementId)
                                {
                                    continue;
                                }
                                if (connect.Domain == Autodesk.Revit.DB.Domain.DomainHvac || connect.Domain == Autodesk.Revit.DB.Domain.DomainPiping)
                                {
                                    if (SystemType == DuctSystemType.ExhaustAir)
                                    {

                                        if (connect.Direction == FlowDirectionType.Out || connect.Direction == FlowDirectionType.Bidirectional)
                                        {

                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;

                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);

                                        }
                                        if (connect.Direction == FlowDirectionType.In)
                                        {
                                            custom.NextOwnerId = connect.Owner.Id;
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;
                                        }

                                    }

                                }
                            }
                        }
                    }


                }



                double Ptot1 = 0;
                double Ptot2 = 0;
               
                OutletConnector1 = OutletConnectors.OrderByDescending(x => x.Flow).FirstOrDefault();
                OutletConnector2 = OutletConnectors.OrderByDescending(x => x.Flow).LastOrDefault();
                try
                {
                    CustomElement neighbour1 = GetNeighbour(OutletConnector1.NextOwnerId, collection);
                    CustomElement neighbour2 = GetNeighbour(OutletConnector2.NextOwnerId, collection);
                    Ptot1 = neighbour1.Ptot;
                    Ptot2 = neighbour2.Ptot;
                }

                catch
                {

                }
                CustomConnector selectedConnector;
                if (Ptot1 > Ptot2)
                {
                    selectedConnector = OutletConnector1;
                }
                else
                {
                    selectedConnector = OutletConnector2;
                }

                InletConnector.Vector = GetVector(InletConnector.Origin, LocPoint);
                selectedConnector.Vector = GetVector(selectedConnector.Origin, LocPoint);

                bool isStraight = StraightTee(InletConnector, selectedConnector);

                double relA;
                double relQ;
                double relC;

                if (SystemType == DuctSystemType.SupplyAir)
                {
                    if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
                    {
                        if (isStraight ==true)
                        {
                            if (isReversed==false)
                            {
                                relA = selectedConnector.Area / InletConnector.Area;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                RectTeeData roundTeeData = new RectTeeData(Element.SystemType, true, relA, relQ, relC, InletConnector);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectTeeStraight;
                                    /* LocRes = roundTeeData.Interpolation(100000, relA, relQ);*/
                                    LocRes = roundTeeData.Interpolation2(relA, relQ);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.Area / InletConnector.Area;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                RectTeeData roundTeeData = new RectTeeData(Element.SystemType, false, relA, relQ, relC, InletConnector);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectTeeBranch;
                                    /*LocRes = roundTeeData.Interpolation(100000, relA, relC);*/
                                    LocRes = roundTeeData.Interpolation2(relA, relC);
                                }
                            }
                            

                        }
                        else
                        {
                            if (isReversed == false)
                            {
                                relA = selectedConnector.Area / InletConnector.Area;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                RectTeeData roundTeeData = new RectTeeData(Element.SystemType, false, relA, relQ, relC, InletConnector);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectTeeBranch;
                                    /*LocRes = roundTeeData.Interpolation(100000, relA, relC);*/
                                    LocRes = roundTeeData.Interpolation2(relA, relC);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.Area / InletConnector.Area;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                RectTeeData roundTeeData = new RectTeeData(Element.SystemType, true, relA, relQ, relC, InletConnector);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectTeeStraight;
                                    /* LocRes = roundTeeData.Interpolation(100000, relA, relQ);*/
                                    LocRes = roundTeeData.Interpolation2(relA, relQ);
                                }
                            }
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletConnector1.Shape == ConnectorProfileType.Round && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
                    {
                        // Смешанный случай
                        if (isStraight == true)
                        {
                            if (isReversed==false)
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                            
                        }
                        else
                        {
                            if (isReversed==false)
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                            
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Round)
                    {
                        // Смешанный случай
                        if (isStraight == true)
                        {
                            if (isReversed ==false)
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                           
                        }
                        else
                        {
                            if (isReversed ==false)
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
                    {
                        // Смешанный случай
                        if (isStraight == true)
                        {
                            if (isReversed==false)
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                            
                        }
                        else
                        {
                            if(isReversed ==false)
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Round && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
                    {
                        // Смешанный случай
                        if (isStraight == true)
                        {
                            if (isReversed == false)
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }

                            
                        }
                        else
                        {
                            if (isReversed==false)
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                            
                        }

                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Round)
                    {
                        // Смешанный случай
                        if (isStraight == true)
                        {
                            if (isReversed ==false)
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                            
                        }
                        else
                        {
                            if (isReversed == false)
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Round && OutletConnector2.Shape == ConnectorProfileType.Round)
                    {
                        // Все коннекторы круглые
                        if (isStraight == true)
                        {
                            if (isReversed ==false)
                            {
                                // Тройник прямой
                                relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                relQ = OutletConnector1.Flow / InletConnector.Flow;
                                RA = relA;
                                RQ = relQ;
                                RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                element.DetailType = CustomElement.Detail.RoundTeeStraight;
                                LocRes = roundTeeData.Interpolation(100000, relA, relQ);
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                relQ = OutletConnector1.Flow / InletConnector.Flow;
                                RA = relA;
                                RQ = relQ;
                                RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                element.DetailType = CustomElement.Detail.RoundTeeBranch;
                                LocRes = roundTeeData.Interpolation(100000, relA, relQ);
                            }
                           
                        }
                        else
                        {
                            if (isReversed==false)
                            {
                                // Тройник на ответвление
                                relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                relQ = OutletConnector1.Flow / InletConnector.Flow;
                                RA = relA;
                                RQ = relQ;
                                RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                element.DetailType = CustomElement.Detail.RoundTeeBranch;
                                LocRes = roundTeeData.Interpolation(100000, relA, relQ);
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                relQ = OutletConnector1.Flow / InletConnector.Flow;
                                RA = relA;
                                RQ = relQ;
                                RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                element.DetailType = CustomElement.Detail.RoundTeeStraight;
                                LocRes = roundTeeData.Interpolation(100000, relA, relQ);
                            }
                            
                        }


                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletConnector1.Shape == ConnectorProfileType.Round && OutletConnector2.Shape == ConnectorProfileType.Round)
                    {
                        // Все коннекторы по одному случаю смешанные (прямоугольный — круглый — круглый)
                        if (isStraight == true)
                        {
                            if (isReversed ==false)
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
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
                            if (isReversed ==false)
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
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
                           
                        }
                    }
                }

                else
                {
                    if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
                    {
                        if (isStraight == true)
                        {
                            if (isReversed==false)
                            {
                                relA = selectedConnector.Area / InletConnector.Area;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                RectTeeData roundTeeData = new RectTeeData(Element.SystemType, true, relA, relQ, relC, InletConnector);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectTeeStraight;
                                    LocRes = roundTeeData.Interpolation2(relA, relQ);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.Area / InletConnector.Area;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                RectTeeData roundTeeData = new RectTeeData(Element.SystemType, false, relA, relQ, relC, InletConnector);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectTeeBranch;
                                    LocRes = roundTeeData.Interpolation2(relA, relC);
                                }
                            }
                           
                        }
                        else
                        {
                            if (isReversed ==false)
                            {
                                relA = selectedConnector.Area / InletConnector.Area;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                RectTeeData roundTeeData = new RectTeeData(Element.SystemType, false, relA, relQ, relC, InletConnector);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectTeeBranch;
                                    LocRes = roundTeeData.Interpolation2(relA, relC);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.Area / InletConnector.Area;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                RectTeeData roundTeeData = new RectTeeData(Element.SystemType, true, relA, relQ, relC, InletConnector);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectTeeStraight;
                                    LocRes = roundTeeData.Interpolation2(relA, relQ);
                                }
                            }
                            
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletConnector1.Shape == ConnectorProfileType.Round && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
                    {
                        // Смешанный случай
                        if (isStraight == true)
                        {
                            if (isReversed ==false)
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                            
                        }
                        else
                        {
                            if (isReversed ==false)
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                            
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Round)
                    {
                        // Смешанный случай
                        if (isStraight == true)
                        {
                            if (isReversed ==false)
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                            
                        }
                        else
                        {
                            if (isReversed ==false)
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
                    {
                        // Смешанный случай
                        if (isStraight == true)
                        {
                            if (isReversed ==false)
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                            
                        }
                        else
                        {
                            if (isReversed == false)
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                            
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Round && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
                    {
                        // Смешанный случай
                        if (isStraight == true)
                        {
                            if (isReversed ==false)
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                            
                        }
                        else
                        {
                            if (isReversed ==false)
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                          
                        }

                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Round)
                    {
                        // Смешанный случай
                        if (isStraight == true)
                        {
                            if (isReversed ==false)
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                           
                        }
                        else
                        {
                            if (isReversed ==true)
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectRoundTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                           
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Round && OutletConnector2.Shape == ConnectorProfileType.Round)
                    {
                        // Все коннекторы круглые
                        if (isStraight == true)
                        {
                            if (isReversed ==false)
                            {
                                // Тройник прямой
                                relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                relQ = OutletConnector1.Flow / InletConnector.Flow;
                                RA = relA;
                                RQ = relQ;
                                RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                element.DetailType = CustomElement.Detail.RoundInRoundDuctInsertStraight;
                                LocRes = roundTeeData.Interpolation(100000, relA, relQ);
                                element.IsReversed = true;
                            }
                            else
                            {
                                // Тройник на ответвление
                                relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                relQ = OutletConnector1.Flow / InletConnector.Flow;
                                RA = relA;
                                RQ = relQ;
                                RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                LocRes = roundTeeData.Interpolation(100000, relA, relQ);
                            }
                            
                        }
                        else
                        {
                            if (isReversed ==true)
                            {
                                // Тройник на ответвление
                                relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                relQ = OutletConnector1.Flow / InletConnector.Flow;
                                RA = relA;
                                RQ = relQ;
                                RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                element.DetailType = CustomElement.Detail.RectRoundTeeBranch;
                                LocRes = roundTeeData.Interpolation(100000, relA, relQ);
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                relQ = OutletConnector1.Flow / InletConnector.Flow;
                                RA = relA;
                                RQ = relQ;
                                RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                element.DetailType = CustomElement.Detail.RoundInRoundDuctInsertStraight;
                                LocRes = roundTeeData.Interpolation(100000, relA, relQ);
                            }
                            
                        }


                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletConnector1.Shape == ConnectorProfileType.Round && OutletConnector2.Shape == ConnectorProfileType.Round)
                    {
                        // Все коннекторы по одному случаю смешанные (прямоугольный — круглый — круглый)
                        if (isStraight == true)
                        {
                            if (isReversed == true)
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                        }
                        else
                        {
                            if (isReversed ==false)
                            {
                                // Тройник на ответвление
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectTeeBranch;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                                element.IsReversed = true;
                            }
                            else
                            {
                                relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                relQ = selectedConnector.Flow / InletConnector.Flow;
                                relC = selectedConnector.Velocity / InletConnector.Velocity;
                                RA = relA;
                                RQ = relQ;
                                RC = relC;
                                MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, true, relA, relQ, relC);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectTeeStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }    
                           
                        }
                    }
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
                //Допиши сюда что может быть на отвод
                
                

                Velocity = InletConnector.Velocity;

            }


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
        public bool StraightTee(CustomConnector inlet, CustomConnector selectedConnector)
        {
            bool isStraight = true;
            XYZ inletvector = inlet.Vector;
            XYZ outletvector = selectedConnector.Vector;

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
            if (angle == 180 || angle ==0)
            {

                isStraight = true;
                return isStraight;
            }
            else
            {
                isStraight = false;
                return isStraight;
            }
            
        }

        private CustomElement GetNeighbour(ElementId neighbourg, List<CustomBranch> collection)
        {
            foreach (var branch in collection)
            {
                foreach (var element in branch.Elements)
                {
                    if (element.ElementId == neighbourg)
                    {
                        return element; // возвращаем элемент сразу после нахождения
                    }
                }
            }
            return null; // если не найден, просто возвращаем null
        }
    }
}
