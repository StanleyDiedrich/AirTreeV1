using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;
using System.Runtime.Remoting.Contexts;
using System.Windows.Media.Media3D;

namespace AirTreeV1
{
   public  class CustomElement
    {
        public Element Element { get; set; }
        public ElementId ElementId { get; set; }
        public ElementId NextElementId { get; set; }
        public MEPSystem System { get; set; }
        public MEPModel Model { get; set; }
        public string SystemName { get; set; }
        public string ShortSystemName { get; set; }
        public DuctSystemType SystemType { get; set; }

        public CustomConnector SelectedConnector { get; set; }
        public List<CustomConnector> SecondaryConnectors { get; set; }

        public ConnectorSet OwnConnectors { get; set; }

        public enum Detail
        {
            Duct,
            Tee,
            Elbow,
            Silencer,
            FireProtectValve,
            AirTerminal,
            Drossel,
            Cap,
            TapAdjustable,
            Transition

        }

        public Detail DetailType { get; private set; }
        public int TrackNumber { get; set; }
        public int BranchNumber { get; set; }
        public bool MainTrack { get; set; }

        public CustomElement (Autodesk.Revit.DB.Document doc, ElementId elementId)
        {
            if (elementId==null)
            {
                return;
            }
            ElementId = elementId;
            Element = doc.GetElement(ElementId);
            if (ElementId.IntegerValue==4657641)
            {
                Element = Element;
            }
            if (Element is Duct)
            {
                System = (Element as MEPCurve).MEPSystem;
                SystemType = (System as MechanicalSystem).SystemType;
                ShortSystemName = Element.LookupParameter("Сокращение для системы").AsString();
                DetailType = Detail.Duct;
                OwnConnectors = ((Element as Duct) as MEPCurve).ConnectorManager.Connectors;
                foreach (Connector connector in OwnConnectors)
                {
                    ConnectorSet nextconnectors = connector.AllRefs;

                    if (connector.Domain != Domain.DomainHvac)
                    {
                        continue;
                    }
                    else
                    {
                        foreach (Connector connect in nextconnectors)
                        {
                            if (connect.Domain != Domain.DomainHvac)
                            {
                                continue;
                            }
                            else
                            {
                                CustomConnector custom = new CustomConnector(doc, ElementId, SystemType);
                                try
                                {
                                    ShortSystemName = doc.GetElement(connect.Owner.Id).get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString();
                                }
                                catch
                                {

                                }
                                if (ShortSystemName == null || ShortSystemName == string.Empty)
                                {
                                    continue;
                                }

                                if (doc.GetElement(connect.Owner.Id) is MechanicalSystem || doc.GetElement(connect.Owner.Id) is DuctInsulation)
                                {
                                    continue;
                                }

                                else if (connect.Owner.Id == ElementId)
                                {
                                    continue; // Игнорируем те же элементы
                                }
                                else if (connect.Owner.Id == NextElementId)
                                {
                                    continue;
                                }

                                else if (ShortSystemName.Contains(ShortSystemName))
                                {
                                    if (connect.Domain == Autodesk.Revit.DB.Domain.DomainHvac || connect.Domain == Autodesk.Revit.DB.Domain.DomainPiping)
                                    {

                                        if (SystemType == DuctSystemType.SupplyAir)
                                        {

                                            if (connect.Direction == FlowDirectionType.Out)
                                            {
                                                custom.Flow = connect.Flow;
                                                custom.Domain = Domain.DomainHvac;
                                                custom.DirectionType = FlowDirectionType.Out;
                                                custom.NextOwnerId = connect.Owner.Id;
                                                custom.Shape = connect.Shape;
                                                custom.Type = connect.ConnectorType;
                                                if (custom.Shape == ConnectorProfileType.Round)
                                                {
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                }
                                                else
                                                {
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                //SecondaryConnectors.Add(custom);
                                            }

                                        }
                                        else if (SystemType == DuctSystemType.ExhaustAir)
                                        {
                                            if (connect.Direction == FlowDirectionType.In)
                                            {
                                                custom.Flow = connect.Flow;
                                                custom.Domain = Domain.DomainHvac;
                                                custom.DirectionType = FlowDirectionType.In;
                                                custom.NextOwnerId = connect.Owner.Id;
                                                custom.Shape = connect.Shape;
                                                custom.Type = connect.ConnectorType;
                                                if (custom.Shape == ConnectorProfileType.Round)
                                                {
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                }
                                                else
                                                {
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                //SecondaryConnectors.Add(custom);
                                            }
                                        }

                                    }
                                }
                            }
                            
                        }

                    


                    }
                }

            

        }
            if (Element is FamilyInstance)
            {
                Model = (Element as FamilyInstance).MEPModel;
                if ((Model as MechanicalFitting)!=null)
                {
                    if ((Model as MechanicalFitting).PartType == PartType.Cap)
                    {
                        DetailType = Detail.Cap;
                    }
                    else if ((Model as MechanicalFitting).PartType == PartType.Elbow)
                    {
                        DetailType = Detail.Elbow;
                    }
                    else if ((Model as MechanicalFitting).PartType == PartType.Tee)
                    {
                        DetailType = Detail.Tee;
                    }
                    else if ((Model as MechanicalFitting).PartType==PartType.TapAdjustable)
                    {
                        DetailType = Detail.TapAdjustable;
                    }
                    else if ((Model as MechanicalFitting).PartType == PartType.Transition)
                    {
                        DetailType = Detail.Transition;
                    }
                    

                }
                else if (Element.Category.Id.IntegerValue == -2008016)
                {
                    DetailType = Detail.FireProtectValve;
                }
                else if (Element.Category.Id.IntegerValue==-2008013)
                {
                    DetailType = Detail.AirTerminal;
                }
                /*else if (Element.LookupParameter("ТипДетали").AsString())
                {
                    DetailType = Detail.FireProtectValve;
                }*/

                OwnConnectors = (Element as FamilyInstance).MEPModel.ConnectorManager.Connectors;
                
                foreach (Connector connector in OwnConnectors)
                {

                    if (connector.Domain!=Domain.DomainHvac)
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
                                CustomConnector custom = new CustomConnector(doc, ElementId, SystemType);
                                try
                                {
                                    ShortSystemName = doc.GetElement(connect.Owner.Id).get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString();
                                }
                                catch
                                {

                                }
                                if (ShortSystemName == null || ShortSystemName == string.Empty)
                                {
                                    continue;
                                }

                                if (doc.GetElement(connect.Owner.Id) is MechanicalSystem || doc.GetElement(connect.Owner.Id) is DuctInsulation)
                                {
                                    continue;
                                }

                                else if (connect.Owner.Id == ElementId)
                                {
                                    continue; // Игнорируем те же элементы
                                }
                                else if (connect.Owner.Id == NextElementId)
                                {
                                    continue;
                                }

                                else if (ShortSystemName.Contains(ShortSystemName))
                                {
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
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                }
                                                else
                                                {
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                //SecondaryConnectors.Add(custom);
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
                                                    custom.EquiDiameter = custom.Diameter;
                                                }
                                                else
                                                {
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                //SecondaryConnectors.Add(custom);
                                            }
                                        }

                                    }
                                }
                            }
                            
                        }
                    


                        }
                    }

                }
               

            }
           
        }
    }
