using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography.X509Certificates;
using Autodesk.Revit.DB.Visual;
using System.Windows.Controls;
using Autodesk.Revit.UI;
using System.Xml.Linq;

namespace AirTreeV1
{
    public class CustomDuctInsert3
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

        public CustomDuctInsert3(Autodesk.Revit.DB.Document document, CustomElement element, CustomBranch branch, bool isReversed)
        {
            Document = document;
            Element = element;
            ElementId = element.ElementId;

            NextElementId = element.NextElementId;
            Element NextElement = document.GetElement(element.NextElementId);

            if (NextElement is Duct)
            {

                //ModelVelocity = GetValue(primaryvelocity);
                foreach (Connector connector in (NextElement as MEPCurve).ConnectorManager.Connectors)
                {
                    CustomConnector custom = new CustomConnector(Document, ElementId, SystemType);
                    SystemType = connector.DuctSystemType;
                    if (SystemType == DuctSystemType.SupplyAir)
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



                                            /* custom.Shape = connector.Shape;
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
                                                 *//*custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                 custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;*//*
                                                 custom.Area = custom.Width * custom.Height;
                                                 custom.Velocity = custom.Flow / (3600 * custom.Area);
                                             }*/
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            custom.PressureDrop = connect.PressureDrop;
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);

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







                                            /*custom.Shape = connect.Shape;
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
                                                *//* custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                 custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;*//*
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }*/
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
                    else if (SystemType == DuctSystemType.ExhaustAir)
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




                                            /* if (custom.Shape == ConnectorProfileType.Round)
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
                                                 *//* custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                  custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;*//*
                                                 custom.Area = custom.Width * custom.Height;
                                                 custom.Velocity = custom.Flow / (3600 * custom.Area);
                                             }*/
                                            custom.Coefficient = connect.Coefficient;
                                            custom.Origin = connect.Origin;
                                            custom.PressureDrop = connect.PressureDrop;
                                            OutletConnector = custom;
                                            OutletConnector.AOutlet = custom.Area;
                                            OutletConnectors.Add(OutletConnector);

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

                                            /*custom.Shape = connector.Shape;
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
                                                *//*custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;*//*
                                                custom.Area = custom.Width * custom.Height;
                                                custom.Velocity = custom.Flow / (3600 * custom.Area);
                                            }*/
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







                //OutletConnector1 = OutletConnectors.Where(x => x.ConnectorType == ConnectorType.End).FirstOrDefault();

                double Ptot1 = 0;
                double Ptot2 = 0;
                CustomConnector selectedConnector = null;






                if (OutletConnectors.Count == 1)
                {
                    InletConnector = null;
                    OutletConnectors.Clear();

                    Element = FindPrevious(Element, branch, isReversed);
                    //Element = GetPrevious(ElementId, collection);

                    if (Element.Element is Duct)
                    {

                        //ModelVelocity = GetValue(primaryvelocity);
                        foreach (Connector connector in (Element.Element as MEPCurve).ConnectorManager.Connectors)
                        {
                            CustomConnector custom = new CustomConnector(Document, ElementId, SystemType);
                            SystemType = connector.DuctSystemType;
                            if (SystemType == DuctSystemType.SupplyAir)
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

                                        if (Document.GetElement(connect.Owner.Id) is MechanicalSystem || Document.GetElement(connect.Owner.Id) is DuctInsulation)
                                        {
                                            continue;
                                        }
                                        else if (connect.Owner.Id == Element.ElementId)
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
                                                    custom.ConnectorType = connector.ConnectorType;
                                                    try
                                                    {
                                                        custom.Width = connect.Width * 304.8 / 1000;
                                                        custom.Height = connect.Height * 304.8 / 1000;
                                                        custom.Width = connect.Width * 304.8 / 1000;
                                                        custom.Height = connect.Height * 304.8 / 1000;
                                                        custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                        custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
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



                                                    custom.Shape = connector.Shape;
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
                                                        custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                        custom.Area = custom.Width * custom.Height;
                                                        custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                    }
                                                    custom.Coefficient = connect.Coefficient;
                                                    custom.Origin = connect.Origin;
                                                    custom.PressureDrop = connect.PressureDrop;
                                                    OutletConnector = custom;
                                                    OutletConnector.AOutlet = custom.Area;
                                                    OutletConnectors.Add(OutletConnector);

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
                                                        custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                        custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
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







                                                    custom.Shape = connect.Shape;
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
                                                        custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                        custom.Area = custom.Width * custom.Height;
                                                        custom.Velocity = custom.Flow / (3600 * custom.Area);
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
                            else if (SystemType == DuctSystemType.ExhaustAir)
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
                                    else
                                    {

                                        if (Document.GetElement(connect.Owner.Id) is MechanicalSystem || Document.GetElement(connect.Owner.Id) is DuctInsulation)
                                        {
                                            continue;
                                        }
                                        else if (connect.Owner.Id == Element.ElementId)
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
                                                    custom.ConnectorType = connector.ConnectorType;
                                                    //custom.Shape = connector.Shape;
                                                    //bool isRound = false;

                                                    try
                                                    {
                                                        custom.Width = connect.Width * 304.8 / 1000;
                                                        custom.Height = connect.Height * 304.8 / 1000;
                                                        custom.Width = connect.Width * 304.8 / 1000;
                                                        custom.Height = connect.Height * 304.8 / 1000;
                                                        custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                        custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
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
                                                        custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                        custom.Area = custom.Width * custom.Height;
                                                        custom.Velocity = custom.Flow / (3600 * custom.Area);
                                                    }
                                                    custom.Coefficient = connect.Coefficient;
                                                    custom.Origin = connect.Origin;
                                                    custom.PressureDrop = connect.PressureDrop;
                                                    OutletConnector = custom;
                                                    OutletConnector.AOutlet = custom.Area;
                                                    OutletConnectors.Add(OutletConnector);

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
                                                        custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                        custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
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

                                                    custom.Shape = connector.Shape;
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
                                                        custom.Area = Math.PI * Math.Pow(custom.EquiDiameter, 2) / 4;
                                                        custom.Area = custom.Width * custom.Height;
                                                        custom.Velocity = custom.Flow / (3600 * custom.Area);
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



                }

                
                if (OutletConnectors.Count == 3)
                {
                    /*foreach (var connector in OutletConnectors)
                    {
                        if (connector.NextOwnerId == Element.ElementId)
                        {
                            InletConnector = connector;
                        }
                    }*/


                    InletConnector = OutletConnectors.OrderByDescending(x => x.Flow).FirstOrDefault();
                    //OutletConnector1 = OutletConnectors.Select(x => x).Where(x => x.NextOwnerId == previouselement.ElementId).First();
                    //OutletConnector2 = OutletConnectors.Select(x => x).Where(x => x.NextOwnerId != previouselement.ElementId).First();
                    OutletConnector1 = OutletConnectors.Skip(1).OrderByDescending(x => x.Flow).FirstOrDefault();
                    OutletConnector2 = OutletConnectors.Skip(1).OrderByDescending(x => x.Flow).LastOrDefault();
                }


                else
                {
                    //OutletConnector1 = OutletConnectors.Select(x => x).Where(x => x.NextOwnerId == previouselement.ElementId).First();
                    //OutletConnector2 = OutletConnectors.Select(x => x).Where(x => x.NextOwnerId != previouselement.ElementId).First();
                    OutletConnector1 = OutletConnectors.OrderByDescending(x => x.Flow).FirstOrDefault();
                    OutletConnector2 = OutletConnectors.OrderByDescending(x => x.Flow).LastOrDefault();
                }



               /* CustomElement neighbour1 = GetNeighbour(OutletConnector1.NextOwnerId, collection);
                CustomElement neighbour2 = GetNeighbour(OutletConnector2.NextOwnerId, collection);*/

               /* if (neighbour1 != null && neighbour2 != null)
                {
                    Ptot1 = neighbour1.Ptot;
                    Ptot2 = neighbour2.Ptot;

                    if (Ptot1 > Ptot2)
                    {
                        selectedConnector = OutletConnector1;
                    }
                    else
                    {
                        selectedConnector = OutletConnector2;
                    }
                }
                else if (neighbour1 != null && neighbour2 == null)
                {
                    selectedConnector = OutletConnector1;
                }
                else if (neighbour1 == null && neighbour2 != null)
                {
                    selectedConnector = OutletConnector2;
                }*/

                /* Ptot1 = neighbour1.Ptot;
                 Ptot2 = neighbour2.Ptot;*/
















                //CustomElement selectedNeigbour;

                /*if (Ptot1 > Ptot2)
                {
                    selectedConnector = OutletConnector1;
                }
                else
                {
                    selectedConnector = OutletConnector2;
                }*/

                double relA;
                double relQ;
                double relC;
                //var previouselement = FindPrevious(element, branch, isReversed);

                /* var elementId = OutletConnectors.Select(x => x).Where(x => x.NextOwnerId == previouselement.ElementId).First();*/



                 selectedConnector = OutletConnectors.Select(x => x).Where(x => x.NextOwnerId == element.ElementId).First();

                if (SystemType == DuctSystemType.SupplyAir)
                {
                    if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
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
                                    element.DetailType = CustomElement.Detail.RectInRectDuctInsertStraight;
                                    /* LocRes = roundTeeData.Interpolation(100000, relA, relQ);*/
                                    LocRes = roundTeeData.Interpolation2(relA, relQ);
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
                                    element.DetailType = CustomElement.Detail.RectInRectDuctInsertBranch;
                                    /* LocRes = roundTeeData.Interpolation(100000, relA, relC);*/
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
                                if (relC > 2)
                                { relC = 2; }
                                RectTeeData roundTeeData = new RectTeeData(Element.SystemType, false, relA, relQ, relC, InletConnector);
                                if (element.DetailType != CustomElement.Detail.AirTerminalConnection)
                                {
                                    element.DetailType = CustomElement.Detail.RectInRectDuctInsertBranch;
                                    /* LocRes = roundTeeData.Interpolation(100000, relA, relC);*/
                                    LocRes = roundTeeData.Interpolation2(relA, relC);
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
                                    element.DetailType = CustomElement.Detail.RectInRectDuctInsertStraight;
                                    /* LocRes = roundTeeData.Interpolation(100000, relA, relQ);*/
                                    LocRes = roundTeeData.Interpolation2(relA, relQ);
                                }
                            }
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletConnector1.Shape == ConnectorProfileType.Round && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                    LocRes = rectTeeData.Interpolation(100000);

                                }
                            }
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Round)
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Round && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                        }

                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Round)
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Round && OutletConnector2.Shape == ConnectorProfileType.Round)
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
                                element.DetailType = CustomElement.Detail.RoundInRoundDuctInsertBranch;
                                LocRes = roundTeeData.Interpolation(100000, relA, relQ);

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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RectInRectDuctInsertStraight;
                                    LocRes = roundTeeData.Interpolation2(relA, relQ);
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
                                    element.DetailType = CustomElement.Detail.RectInRectDuctInsertBranch;
                                    LocRes = roundTeeData.Interpolation2(relC, relQ);
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
                                    element.DetailType = CustomElement.Detail.RectInRectDuctInsertBranch;
                                    LocRes = roundTeeData.Interpolation2(relC, relQ);
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
                                    element.DetailType = CustomElement.Detail.RectInRectDuctInsertStraight;
                                    LocRes = roundTeeData.Interpolation2(relA, relQ);
                                }
                            }

                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletConnector1.Shape == ConnectorProfileType.Round && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                        }
                        // Тройник на ответвление


                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Rectangular && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Round)
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }

                            }
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Round && OutletConnector2.Shape == ConnectorProfileType.Rectangular)
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                        }

                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Rectangular && OutletConnector2.Shape == ConnectorProfileType.Round)
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }

                            }
                        }
                    }
                    else if (InletConnector.Shape == ConnectorProfileType.Round && OutletConnector1.Shape == ConnectorProfileType.Round && OutletConnector2.Shape == ConnectorProfileType.Round)
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
                            if (isReversed == false)
                            {
                                // Тройник на ответвление
                                relA = OutletConnector1.AOutlet / InletConnector.AInlet;
                                relQ = OutletConnector1.Flow / InletConnector.Flow;
                                RA = relA;
                                RQ = relQ;
                                RoundTeeData roundTeeData = new RoundTeeData(Element.SystemType, true, relA, relQ);
                                element.DetailType = CustomElement.Detail.RoundInRoundDuctInsertBranch;
                                LocRes = roundTeeData.Interpolation(100000, relA, relQ);

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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertBranch;
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
                                    element.DetailType = CustomElement.Detail.RoundInRectDuctInsertStraight;
                                    LocRes = rectTeeData.Interpolation(100000);
                                }
                            }
                        }
                    }
                }

                IA = InletConnector.AInlet;
                IQ = InletConnector.Flow;
                IC = InletConnector.Velocity;
                O1A = OutletConnector1.AInlet;
                O1Q = OutletConnector1.Flow;
                O1C = OutletConnector1.Velocity;
                O2A = OutletConnector2.AInlet;
                O2Q = OutletConnector2.Flow;
                O2C = OutletConnector2.Velocity;
                Velocity = InletConnector.Velocity;


            }
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

        private CustomElement FindPrevious(CustomElement element, CustomBranch branch, bool isReversed)
        {
            int index = branch.Elements.IndexOf(element);

            // Проверяем, найден ли элемент и есть ли предыдущий элемент
            if (index > 0)
            {
                if (isReversed == false)
                {
                    var previousElement = branch.Elements[index - 1];
                    return previousElement;
                }
                else
                {
                    var previousElement = branch.Elements[index - 2];
                    return previousElement;
                }


            }

            return null;
        }

    }


}

