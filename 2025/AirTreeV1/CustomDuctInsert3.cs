using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

namespace AirTreeV1
{
    public class CustomDuctInsert3
    {
        public Autodesk.Revit.DB.Document Document { get; set; }
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

        public CustomConnector OutletStraight { get; set; }
        public CustomConnector OutletBranch { get; set; }

        public List<ElementId> PreviousElements { get; set; } = new List<ElementId>();
        public List<CustomConnector> Connectors { get; set; } = new List<CustomConnector>();
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
        public CustomElement.Detail Detail { get; set; }
        public CustomDuctInsert3(Autodesk.Revit.DB.Document document, CustomElement element, List<CustomBranch> collection, bool isReversed)
        {
            Document = document;
            Element = element;
            Element.IsStartPart = true;
            ElementId = element.ElementId;

            NextElementId = element.NextElementId;
            CustomElement NextElement = GetNextElement(Element, collection);
            //Element NextElement = document.GetElement(element.NextElementId);




            //NextElement = TryGetNextElement(Element, collection);
            DuctSystemType systemType = Element.SystemType;
            (OutletConnectors, InletConnector) = GetConnectors(Document,Element, NextElement, collection, systemType );
            if (OutletBranch != null || OutletStraight != null)
            {


                double Ptot1 = 0;
                double Ptot2 = 0;
                CustomConnector selectedConnector = null;
                double relA;
                double relQ;
                double relC;
                CustomElement neighbour1 = null;
                CustomElement neighbour2 = null;
                try
                {
                     neighbour1= GetNeighbourInBranch(Element, OutletStraight.NextOwnerId, collection);
                     neighbour2 = GetNeighbourInBranch(Element, OutletBranch.NextOwnerId, collection);
                   /* try
                    {
                       
                    }
                     
                    catch
                    {
                        neighbour2 = GetNextNeighbourInBranch(Element, InletConnector.NextOwnerId, collection);
                    }*/
                }
                catch
                {
                    OutletStraight = OutletConnectors.OrderByDescending(x => x.Flow).First();
                    OutletBranch = OutletConnectors.OrderByDescending(x => x.Flow).Last();
                     neighbour1 = GetNeighbourInBranch(Element, OutletStraight.NextOwnerId, collection);
                     neighbour2 = GetNeighbourInBranch(Element, OutletBranch.NextOwnerId, collection);
                }
                try
                {


                    if (Element.BranchNumber == neighbour1.BranchNumber && neighbour1!=null )
                    {
                        selectedConnector = OutletStraight;
                    }
                    if(neighbour2!=null)
                    {
                        selectedConnector = OutletBranch;
                    }

                    if (SystemType == DuctSystemType.SupplyAir)
                    {
                        if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletStraight.Shape == ConnectorProfileType.Rectangular && OutletBranch.Shape == ConnectorProfileType.Rectangular)
                        {
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
                            {
                                if (isReversed == false)
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
                                        Detail = CustomElement.Detail.RectInRectDuctInsertStraight;

                                        LocRes = roundTeeData.Interpolation2(relA, relQ,relC);
                                    }

                                }
                                else
                                {
                                    relA = selectedConnector.Area / InletConnector.Area;
                                    relQ = selectedConnector.Flow / InletConnector.Flow;
                                    relC = selectedConnector.Velocity / InletConnector.Velocity;
                                    RA = relA;
                                    RQ = relQ;
                                    RC = relC;
                                    if (relC > 2)
                                    { relC = 2; }
                                    RectTeeData roundTeeData = new RectTeeData(Element.SystemType, false, relA, relQ, relC, InletConnector);
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RectInRectDuctInsertBranch;

                                        LocRes = roundTeeData.Interpolation2(relA, relC,relC);
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
                                    if (relC > 2)
                                    { relC = 2; }
                                    RectTeeData roundTeeData = new RectTeeData(Element.SystemType, false, relA, relQ, relC, InletConnector);
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RectInRectDuctInsertBranch;

                                        LocRes = roundTeeData.Interpolation2(relA, relC,relC);
                                    }

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
                                        Detail = CustomElement.Detail.RectInRectDuctInsertStraight;

                                        LocRes = roundTeeData.Interpolation2(relA, relQ,relC);
                                    }
                                }
                            }
                        }
                        else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletStraight.Shape == ConnectorProfileType.Round && OutletBranch.Shape == ConnectorProfileType.Rectangular)
                        {
                            // Смешанный случай
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;

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
                                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;

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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;

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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;

                                        LocRes = rectTeeData.Interpolation(100000);

                                    }
                                }
                            }
                        }
                        else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletStraight.Shape == ConnectorProfileType.Rectangular && OutletBranch.Shape == ConnectorProfileType.Round)
                        {
                            // Смешанный случай
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;

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
                                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;

                                        LocRes = rectTeeData.Interpolation(100000);
                                    }
                                }

                            }
                            else
                            {
                                if (isReversed == false)
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;

                                        LocRes = rectTeeData.Interpolation(100000);
                                    }

                                }
                                // Тройник на ответвление
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;

                                        LocRes = rectTeeData.Interpolation(100000);
                                    }
                                }
                            }
                        }
                        else if (InletConnector.Shape == ConnectorProfileType.Round && OutletStraight.Shape == ConnectorProfileType.Rectangular && OutletBranch.Shape == ConnectorProfileType.Rectangular)
                        {
                            // Смешанный случай
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;

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
                                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;

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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;

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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;

                                        LocRes = rectTeeData.Interpolation(100000);
                                    }
                                }
                            }
                        }
                        else if (InletConnector.Shape == ConnectorProfileType.Round && OutletStraight.Shape == ConnectorProfileType.Round && OutletBranch.Shape == ConnectorProfileType.Rectangular)
                        {
                            // Смешанный случай
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;

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
                                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;

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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;

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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;

                                        LocRes = rectTeeData.Interpolation(100000);
                                    }
                                }
                            }

                        }
                        else if (InletConnector.Shape == ConnectorProfileType.Round && OutletStraight.Shape == ConnectorProfileType.Rectangular && OutletBranch.Shape == ConnectorProfileType.Round)
                        {
                            // Смешанный случай
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;

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
                                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;

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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;

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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;

                                        LocRes = rectTeeData.Interpolation(100000);
                                    }
                                }
                            }
                        }
                        else if (InletConnector.Shape == ConnectorProfileType.Round && OutletStraight.Shape == ConnectorProfileType.Round && OutletBranch.Shape == ConnectorProfileType.Round)
                        {
                            // Все коннекторы круглые
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
                            {
                                if (isReversed == false)
                                {
                                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                                    RA = relA;
                                    RQ = relQ;
                                    RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                    Detail = CustomElement.Detail.RoundInRoundDuctInsertStraight;

                                    LocRes = roundTeeData.Interpolation2(relA, relQ);

                                }
                                else
                                {
                                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                                    RA = relA;
                                    RQ = relQ;
                                    RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                    Detail = CustomElement.Detail.RoundInRoundDuctInsertBranch;

                                    LocRes = roundTeeData.Interpolation2(relA, relQ);
                                }
                                // Тройник прямой

                            }
                            else
                            {
                                if (isReversed == false)
                                {
                                    // Тройник на ответвление
                                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                                    RA = relA;
                                    RQ = relQ;
                                    RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                    Detail = CustomElement.Detail.RoundInRoundDuctInsertBranch;

                                    LocRes = roundTeeData.Interpolation2(relA, relQ);

                                }

                                else
                                {
                                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                                    RA = relA;
                                    RQ = relQ;
                                    RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                    Detail = CustomElement.Detail.RoundInRoundDuctInsertStraight;

                                    LocRes = roundTeeData.Interpolation2(relA, relQ);
                                }

                            }


                        }
                        else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletStraight.Shape == ConnectorProfileType.Round && OutletBranch.Shape == ConnectorProfileType.Round)
                        {
                            // Все коннекторы по одному случаю смешанные (прямоугольный — круглый — круглый)
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        Detail = element.DetailType;*/
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
                                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /* element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                         Detail = element.DetailType;*/
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        Detail = element.DetailType;*/
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        Detail = element.DetailType;*/
                                        LocRes = rectTeeData.Interpolation(100000);
                                    }
                                }
                            }
                        }
                    }

                    else
                    {
                        if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletStraight.Shape == ConnectorProfileType.Rectangular && OutletBranch.Shape == ConnectorProfileType.Rectangular)
                        {
                            if (InletConnector.ConnectorType == OutletStraight.ConnectorType)
                            {
                                if (isReversed == false)
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
                                        Detail = CustomElement.Detail.RectInRectDuctInsertStraight;
                                        /*element.DetailType = CustomElement.Detail.RectInRectDuctInsertStraight;
                                        Detail = element.DetailType;*/
                                        LocRes = roundTeeData.Interpolation2(relA, relQ,relC);
                                    }

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
                                        Detail = CustomElement.Detail.RectInRectDuctInsertBranch;
                                        /*element.DetailType = CustomElement.Detail.RectInRectDuctInsertBranch;
                                        Detail = element.DetailType;*/
                                        LocRes = roundTeeData.Interpolation2(relA, relQ,relC);
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
                                        Detail = CustomElement.Detail.RectInRectDuctInsertBranch;
                                        /* element.DetailType = CustomElement.Detail.RectInRectDuctInsertBranch;
                                         Detail = element.DetailType;*/
                                        LocRes = roundTeeData.Interpolation2(relC, relQ,relC);
                                    }

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
                                        Detail = CustomElement.Detail.RectInRectDuctInsertStraight;
                                        /* element.DetailType = CustomElement.Detail.RectInRectDuctInsertStraight;
                                         Detail = element.DetailType;*/
                                        LocRes = roundTeeData.Interpolation2(relA, relQ,relC);
                                    }
                                }

                            }
                        }
                        else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletStraight.Shape == ConnectorProfileType.Round && OutletBranch.Shape == ConnectorProfileType.Rectangular)
                        {
                            // Смешанный случай
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        Detail = element.DetailType;*/
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
                                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        Detail = element.DetailType;*/
                                        LocRes = rectTeeData.Interpolation(100000);
                                    }
                                }

                            }
                            else
                            {
                                if (isReversed == false)
                                {
                                    relA = selectedConnector.AOutlet / InletConnector.AInlet;
                                    relQ = selectedConnector.Flow / InletConnector.Flow;
                                    relC = selectedConnector.Velocity / InletConnector.Velocity;
                                    RA = relA;
                                    RQ = relQ;
                                    RC = relC;
                                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);////
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        Detail = element.DetailType;*/
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        /* element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                         Detail = element.DetailType;*/
                                        LocRes = rectTeeData.Interpolation(100000);
                                    }
                                }
                            }
                            // Тройник на ответвление


                        }
                        else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletStraight.Shape == ConnectorProfileType.Rectangular && OutletBranch.Shape == ConnectorProfileType.Round)
                        {
                            // Смешанный случай
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        Detail = element.DetailType;*/
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
                                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        Detail = element.DetailType;*/
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /* element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                         Detail = element.DetailType;*/
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        Detail = element.DetailType;*/
                                        LocRes = rectTeeData.Interpolation(100000);
                                    }
                                }
                            }
                        }
                        else if (InletConnector.Shape == ConnectorProfileType.Round && OutletStraight.Shape == ConnectorProfileType.Rectangular && OutletBranch.Shape == ConnectorProfileType.Rectangular)
                        {
                            // Смешанный случай
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                    Detail = element.DetailType;*/
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
                                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                    Detail = element.DetailType;*/
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        Detail = element.DetailType;*/
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        Detail = element.DetailType;*/
                                        LocRes = rectTeeData.Interpolation(100000);
                                    }

                                }
                            }
                        }
                        else if (InletConnector.Shape == ConnectorProfileType.Round && OutletStraight.Shape == ConnectorProfileType.Round && OutletBranch.Shape == ConnectorProfileType.Rectangular)
                        {
                            // Смешанный случай
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        /*  element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                          Detail = element.DetailType;*/
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
                                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /* element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                         Detail = element.DetailType;*/
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /* element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                         Detail = element.DetailType;*/
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /* element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                         Detail = element.DetailType;*/
                                        LocRes = rectTeeData.Interpolation(100000);
                                    }
                                }
                            }

                        }
                        else if (InletConnector.Shape == ConnectorProfileType.Round && OutletStraight.Shape == ConnectorProfileType.Rectangular && OutletBranch.Shape == ConnectorProfileType.Round)
                        {
                            // Смешанный случай
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        /* element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                     Detail = element.DetailType;*/
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
                                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /* element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                         Detail = element.DetailType;*/
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /* element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                         Detail = element.DetailType;*/
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        /*  element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                          Detail = element.DetailType;*/
                                        LocRes = rectTeeData.Interpolation(100000);
                                    }

                                }
                            }
                        }
                        else if (InletConnector.Shape == ConnectorProfileType.Round && OutletStraight.Shape == ConnectorProfileType.Round && OutletBranch.Shape == ConnectorProfileType.Round)
                        {
                            // Все коннекторы круглые
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
                            {
                                if (isReversed == false)
                                {
                                    // Тройник прямой
                                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                                    RA = relA;
                                    RQ = relQ;
                                    RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                    Detail = CustomElement.Detail.RoundInRoundDuctInsertStraight;
                                    /*element.DetailType = CustomElement.Detail.RoundInRoundDuctInsertStraight;
                                    Detail = element.DetailType;*/
                                    LocRes = roundTeeData.Interpolation2(relA, relQ);


                                }
                                else
                                {
                                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                                    RA = relA;
                                    RQ = relQ;
                                    RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                    Detail = CustomElement.Detail.RoundInRoundDuctInsertBranch;
                                    /* element.DetailType = CustomElement.Detail.RoundInRoundDuctInsertBranch;
                                     Detail = element.DetailType;*/
                                    LocRes = roundTeeData.Interpolation2(relA, relQ);
                                }
                            }
                            else
                            {
                                if (isReversed == false)
                                {
                                    // Тройник на ответвление
                                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                                    RA = relA;
                                    RQ = relQ;
                                    RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                    Detail = CustomElement.Detail.RoundInRoundDuctInsertBranch;
                                    /* element.DetailType = CustomElement.Detail.RoundInRoundDuctInsertBranch;
                                     Detail = element.DetailType;*/
                                    LocRes = roundTeeData.Interpolation2(relA, relQ);

                                }
                                else
                                {
                                    relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                    relQ = OutletConnector1.Flow / InletConnector.Flow;
                                    RA = relA;
                                    RQ = relQ;
                                    RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                    Detail = CustomElement.Detail.RoundInRoundDuctInsertStraight;
                                    /* element.DetailType = CustomElement.Detail.RoundInRoundDuctInsertStraight;
                                     Detail = element.DetailType;*/
                                    LocRes = roundTeeData.Interpolation2(relA, relQ);
                                }
                            }


                        }
                        else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletStraight.Shape == ConnectorProfileType.Round && OutletBranch.Shape == ConnectorProfileType.Round)
                        {
                            // Все коннекторы по одному случаю смешанные (прямоугольный — круглый — круглый)
                            if (InletConnector.ConnectorType == selectedConnector.ConnectorType)
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        Detail = element.DetailType;*/
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
                                    MixedTeeData rectTeeData = new MixedTeeData(Element.SystemType, false, relA, relQ, relC);
                                    if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                    {
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        Detail = element.DetailType;*/
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        /*element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
                                        Detail = element.DetailType;*/
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
                                        Detail = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                        /* element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                             Detail = element.DetailType;*/
                                        LocRes = rectTeeData.Interpolation(100000);
                                    }
                                }
                            }
                        }
                    }

                    IA = InletConnector.AInlet;
                    IQ = InletConnector.Flow;
                    IC = InletConnector.Velocity;
                    O1A = OutletStraight.AInlet;
                    O1Q = OutletStraight.Flow;
                    O1C = OutletStraight.Velocity;
                    O2A = OutletBranch.AInlet;
                    O2Q = OutletBranch.Flow;
                    O2C = OutletBranch.Velocity;
                    Velocity = InletConnector.Velocity;
                }
                catch
                {

                }
            }


        }

        private (List<CustomConnector>, CustomConnector) GetConnectors(Autodesk.Revit.DB.Document document, CustomElement element, CustomElement nextElement, List<CustomBranch> collection, DuctSystemType systemType)
        {
            CustomElement Element = element;
            ElementId elementId = element.ElementId;
            CustomConnector OutletConnector = null;
            CustomConnector InletConnector = null;
            List<CustomConnector> OutletConnectors = new List<CustomConnector>();

            if (Element.DetailType == CustomElement.Detail.DuctTap || (Element.DetailType.ToString().Contains("Duct") && Element.OwnConnectors.Size==3))
           
            {

                //ModelVelocity = GetValue(primaryvelocity);
                foreach (Connector connector in (Element.Element as MEPCurve).ConnectorManager.Connectors)
                {

                    CustomConnector custom = new CustomConnector(document, elementId, systemType);
                    systemType = connector.DuctSystemType;
                    if (systemType == DuctSystemType.SupplyAir)
                    {
                        custom.Flow = connector.Flow * 101.947308132875143184421534937;
                        custom.Domain = Domain.DomainHvac;
                        //custom.DirectionType = FlowDirectionType.Out;
                        custom.OwnerId = connector.Owner.Id;



                        ConnectorSet nextconnectors = connector.AllRefs;

                        foreach (Connector connect in nextconnectors)
                        {
                            if (connect.Domain != Domain.DomainHvac)
                            {
                                continue;
                            }
                            else
                            {

                                if (document.GetElement(connect.Owner.Id) is MechanicalSystem || document.GetElement(connect.Owner.Id) is DuctInsulation)
                                {
                                    continue;
                                }
                                /*else if (connect.Owner.Id == NextElementId)
                                {
                                    continue;
                                }*/
                                if (connect.Domain == Autodesk.Revit.DB.Domain.DomainHvac || connect.Domain == Autodesk.Revit.DB.Domain.DomainPiping)
                                {
                                    if (systemType == DuctSystemType.SupplyAir)
                                    {

                                        if (connect.Direction == FlowDirectionType.In /*|| connect.Direction == FlowDirectionType.Bidirectional*/)
                                        {

                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.ConnectorType = connector.ConnectorType;
                                            try
                                            {
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                /* custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                 custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;*/
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                            }
                                            catch
                                            {
                                                custom.Diameter = connect.Radius * 2 * 304.8 / 1000;
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                custom.Shape = ConnectorProfileType.Round;
                                            }

                                           


                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            custom.PressureDrop = connect.PressureDrop;
                                            OutletConnector = custom;

                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);
                                            if (custom.ConnectorType == ConnectorType.End)
                                            {
                                                OutletStraight = custom;
                                            }
                                            if (custom.ConnectorType == ConnectorType.Curve)
                                            {
                                                OutletBranch = custom;
                                            }

                                        }
                                        if (connect.Direction == FlowDirectionType.Out)
                                        {
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.ConnectorType = connector.ConnectorType;

                                            try
                                            {
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                /* custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                 custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;*/
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                            }
                                            catch
                                            {
                                                custom.Diameter = connect.Radius * 2 * 304.8 / 1000;
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                custom.Shape = ConnectorProfileType.Round;
                                            }








                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            custom.PressureDrop = connect.PressureDrop;
                                            InletConnector = custom;
                                            InletConnector.AInlet = custom.Area;
                                        }

                                    }

                                }
                            }
                        }
                    }
                    else if (systemType == DuctSystemType.ExhaustAir)
                    {
                        custom.Flow = connector.Flow * 101.947308132875143184421534937;
                        custom.Domain = Domain.DomainHvac;
                        //custom.DirectionType = FlowDirectionType.Out;
                        custom.OwnerId = connector.Owner.Id;
                        custom.ConnectorType = connector.ConnectorType;



                        ConnectorSet nextconnectors = connector.AllRefs;

                        foreach (Connector connect in nextconnectors)
                        {
                            if (connect.Domain != Domain.DomainHvac)
                            {
                                continue;
                            }
                            if (connect.Owner.Id.Value==element.ElementId.Value)
                            {
                                continue;
                            }
                            else
                            {

                                if (document.GetElement(connect.Owner.Id) is MechanicalSystem || document.GetElement(connect.Owner.Id) is DuctInsulation)
                                {
                                    continue;
                                }
                               /* else if (connect.Owner.Id == NextElementId)
                                {
                                    continue;
                                }*/
                                if (connect.Domain == Autodesk.Revit.DB.Domain.DomainHvac || connect.Domain == Autodesk.Revit.DB.Domain.DomainPiping)
                                {
                                    if (systemType == DuctSystemType.ExhaustAir)
                                    {

                                        if (connect.Direction == FlowDirectionType.Out /*|| connect.Direction == FlowDirectionType.Bidirectional*/)
                                        {

                                            //custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.ConnectorType = connector.ConnectorType;
                                            //custom.Shape = connector.Shape;
                                            //bool isRound = false;

                                            try
                                            {
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                custom.Width = connect.Width * 304.8 / 1000;
                                                custom.Height = connect.Height * 304.8 / 1000;
                                                /* custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                 custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;*/
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                custom.Shape = ConnectorProfileType.Rectangular;
                                            }
                                            catch
                                            {
                                                custom.Diameter = connect.Radius * 2 * 304.8 / 1000;
                                                custom.EquiDiameter = custom.Diameter;
                                                custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                custom.Shape = ConnectorProfileType.Round;
                                            }





                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            custom.PressureDrop = connect.PressureDrop;
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);

                                            if (custom.ConnectorType == ConnectorType.End)
                                            {
                                                OutletStraight = custom;
                                            }
                                            if (custom.ConnectorType == ConnectorType.Curve)
                                            {
                                                OutletBranch = custom;
                                            }

                                        }
                                        if (connect.Direction == FlowDirectionType.In)
                                        {
                                            
                                                custom.NextOwnerId = connect.Owner.Id;
                                                custom.ConnectorType = connector.ConnectorType;

                                                try
                                                {
                                                    custom.Width = connect.Width * 304.8 / 1000;
                                                    custom.Height = connect.Height * 304.8 / 1000;
                                                    custom.Width = connect.Width * 304.8 / 1000;
                                                    custom.Height = connect.Height * 304.8 / 1000;

                                                    custom.Area = custom.Width * custom.Height;
                                                    custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                    custom.Shape = ConnectorProfileType.Rectangular;
                                                }
                                                catch
                                                {
                                                    custom.Diameter = connect.Radius * 2 * 304.8 / 1000;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    custom.Area = Math.PI * Math.Pow(custom.Diameter, 2) / 4;
                                                    custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                    custom.Shape = ConnectorProfileType.Round;
                                                }

                                                custom.Coefficient = connect.Coefficient;
                                                custom.Origin = connect.Origin;
                                                custom.PressureDrop = connect.PressureDrop;
                                                InletConnector = custom;
                                                InletConnector.AInlet = custom.Area;
                                            
                                            
                                           

                                        }

                                    }

                                }
                            }
                        }
                    }

                   
                }
            }
            return (OutletConnectors, InletConnector);
        }


        private CustomElement TryGetNextElement(CustomElement element, List<CustomBranch> collection)
        {
            CustomElement nextElement = null;
            if (element.DetailType.ToString().Contains("Insert"))
            {
                //nextElement = GetNextElement(element, collection);

                return element;
            }
            if (element.DetailType == CustomElement.Detail.TapAdjustable)
            {
                nextElement = GetNextElement(element, collection);
                return nextElement;
            }
            if (element.DetailType.ToString().Contains("Duct") || element.DetailType == CustomElement.Detail.Union)
            {
                return element;
            }
            return nextElement;
        }

        private CustomElement GetNextElement(CustomElement element, List<CustomBranch> collection)
        {
            foreach (var branch in collection)
            {
                if (branch.Elements.First().BranchNumber == element.BranchNumber)
                {
                    for (int i = 0; i < branch.Elements.Count; i++)
                    {
                        if (branch.Elements[i].ElementId.Value == element.ElementId.Value)
                        {
                            // дополнительная проверка на одинаковость
                            if (branch.Elements[i+1].ElementId==element.NextElementId)
                            {
                                return branch.Elements[i+1];
                            }
                            return branch.Elements[i + 1];
                        }
                    }
                }
            }
            return null;
        }

        private CustomElement GetPrevious(ElementId elementId, List<CustomBranch> collection)
        {
            foreach (var branch in collection)
            {
                var elements = branch.Elements;
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].ElementId == elementId)
                    {
                        // Возвращаем предыдущий элемент, если он существует
                        if (i > 0)
                        {
                            return elements[i - 1];
                        }
                        // Если предыдущего элемента нет, можно вернуть null или бросить исключение
                        return null;
                    }
                }
            }
            return null; // Если элемент не найден
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

        private CustomElement GetNeighbourInBranch(CustomElement element, ElementId owner, List<CustomBranch> collection)
        {
            int branchNumber = element.BranchNumber;


            foreach (var branch in collection)
            {
                if (branch.Elements.First().BranchNumber == branchNumber)
                {
                    for (int i =1; i<branch.Elements.Count;i++)
                    {
                        if (branch.Elements[i].ElementId == owner)
                        {
                           return  branch.Elements[i - 1];
                        }
                    }
                }
                else
                {
                    for (int i = 1; i < branch.Elements.Count; i++)
                    {
                        if (branch.Elements[i].ElementId == owner)
                        {
                            return branch.Elements[i - 1];
                        }
                    }
                }
                
            }
            return null; // если не найден, просто возвращаем null
        }
        private CustomElement GetNextNeighbourInBranch(CustomElement element, ElementId owner, List<CustomBranch> collection)
        {
            int branchNumber = element.BranchNumber;


            foreach (var branch in collection)
            {
                if (branch.Elements.First().BranchNumber == branchNumber)
                {
                    for (int i = 1; i < branch.Elements.Count; i++)
                    {
                        if (branch.Elements[i].ElementId == owner)
                        {
                            return branch.Elements[i + 1];
                        }
                    }
                }
                else
                {
                    for (int i = 1; i < branch.Elements.Count; i++)
                    {
                        if (branch.Elements[i].ElementId == owner)
                        {
                            return branch.Elements[i + 1];
                        }
                    }
                }

            }
            return null; // если не найден, просто возвращаем null
        }
    }


    
    



}



