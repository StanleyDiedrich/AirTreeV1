﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;
using System.Runtime.Remoting.Contexts;
using System.Windows.Media.Media3D;
using Autodesk.Revit.Creation;
using System.Text.RegularExpressions;
using System.Security.Policy;
using System.Windows.Controls;

namespace AirTreeV1
{

    
    
    public class CustomElement
    {
        static int _id = 0;
        public int PluginId { get; set; }
        public Element Element { get; set; }
        public ElementId ElementId { get; set; }
        public ElementId NextElementId { get; set; }
        public MEPSystem MSystem { get; set; }
        public MEPModel Model { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string ShortSystemName { get; set; }
        public string Lvl { get; set; }
        public DuctSystemType SystemType { get; set; }

        public CustomConnector SelectedConnector { get; set; }
        public List<CustomConnector> SecondaryConnectors { get; set; }

        public ConnectorSet OwnConnectors { get; set; }
        public string Volume { get; set; }
        public string ModelWidth { get; set; }
        public string ModelHeight { get; set; }

        public string NewModelWidth { get; set; }
        public string NewModelHeight { get; set; }
        public string ModelLength { get; set; }
        public string ModelDiameter { get; set; }
        public string ModelVelocity { get; set; }
        public string ModelHydraulicDiameter { get; set; }
        public double EquiDiameter { get; set; }
        public string ModelHydraulicArea { get; set; }
        public double LocRes { get; set; }
        public double PDyn { get; set; }
        public double PStat { get; set; }
        public double Ptot { get; set; }
        public double AirTree_Area { get; set; }
        public bool IsReversed { get; set; } 

        public enum Detail
        {
            RectangularDuct,
            RoundDuct,
            Tee,
            Elbow,
            Silencer,
            FireProtectValve,
            AirTerminal,
            Drossel,
            Cap,
            TapAdjustable,
            Multiport,
            RoundElbow,
            RectElbow,


            Transition,
            RectTransition,
            RectExpansion,
            RectContraction,
            RoundTransition,


            RoundExpansion,
            RoundContraction,
            RectRoundExpansion,
            RoundRectExpansion,
            RectRoundContraction,
            RoundRectContraction,



            


            RoundFlexDuct,
            RectFlexDuct,

            RoundTeeBranch,
            RoundTeeStraight,
            RectTeeBranch,
            RectTeeStraight,
            RectRoundTeeBranch,
            RectRoundTeeStraight,

            RoundInRoundDuctInsertStraight,
            RoundInRoundDuctInsertBranch,
            RoundInRectDuctInsertStraight,
            RoundInRectDuctInsertBranch,
            RectInRectDuctInsertStraight,
            RectInRectDuctInsertBranch,
            RectInRoundDuctInsertStraight,
            RectInRoundDuctInsertBranch,

            AirTerminalConnection,
            Union,
            Equipment

            
        }
        public bool IsTee { get; set; }
        public bool IsTapAdjustable { get; set; }
        public Detail DetailType { get;  set; }
        public int TrackNumber { get; set; }
        public int BranchNumber { get; set; }
        public bool MainTrack { get; set; }
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
        public bool IsNonPrinted { get; set; }

        
        private string GetValue(string primaryvolume)
        {
            // Используем регулярное выражение, чтобы найти и вернуть только числовую часть
            var match = System.Text.RegularExpressions.Regex.Match(primaryvolume, @"\d+(\.\d+)?");
            return match.Success ? match.Value : string.Empty; // Вернуть число или пустую строку, если числ
        }
        public CustomElement(Autodesk.Revit.DB.Document doc, ElementId elementId)
        {
            PluginId = _id;
            _id++;
            if (elementId == null)
            {
                return;
            }
            ElementId = elementId;
            Element = doc.GetElement(ElementId);
            SystemName = Element.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();

            if (Element.LookupParameter("Базовый уровень") != null)
            {
                Lvl = Element.LookupParameter("Базовый уровень").AsValueString();
            }

            else
            {
                Lvl = Element.LookupParameter("Уровень").AsValueString();
            }





            if (Element is Duct)
            {
                MSystem = (Element as MEPCurve).MEPSystem;
                SystemType = (MSystem as MechanicalSystem).SystemType;
                ShortSystemName = Element.LookupParameter("Сокращение для системы").AsString();
                Name = Element.Name;//Добавил в патче
                OwnConnectors = ((Element as Duct) as MEPCurve).ConnectorManager.Connectors;
                string primaryvolume = Element.get_Parameter(BuiltInParameter.RBS_DUCT_FLOW_PARAM).AsValueString();
                Volume = GetValue(primaryvolume);
                string primarylength = Element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsValueString();
                ModelLength = primarylength;
                string primaryvelocity = Convert.ToString(Math.Round(Element.get_Parameter(BuiltInParameter.RBS_VELOCITY).AsDouble()/3.25,2));
                //string primaryvelocity = Element.get_Parameter(BuiltInParameter.RBS_VELOCITY).AsValueString();
                ModelVelocity = primaryvelocity;
                //ModelVelocity = GetValue(primaryvelocity);
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
                                                    DetailType = Detail.RoundDuct;
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    try
                                                    {
                                                        string primarydiameter = Element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsValueString();
                                                        ModelDiameter = primarydiameter;
                                                        ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                        ModelHydraulicArea = Math.Round(((Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4) / 1000000), 5).ToString();
                                                    }
                                                    catch
                                                    {
                                                        string primarywidth = Element.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsValueString();
                                                        ModelWidth = primarywidth;
                                                        string primaryheight = Element.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsValueString();
                                                        ModelHeight = primaryheight;
                                                        ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                        ModelHydraulicArea = Math.Round(((Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4) / 1000000), 5).ToString();
                                                        DetailType = Detail.RectangularDuct;

                                                        //ModelDiameter = "Not defined";
                                                    }

                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    ModelHydraulicArea = Math.Round(((Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4) / 1000000), 5).ToString();
                                                    //ModelHydraulicArea = ((Math.PI*Math.Pow(Convert.ToDouble(ModelHydraulicDiameter),2) / 4)/1000000).ToString();
                                                }
                                                else
                                                {
                                                    DetailType = Detail.RectangularDuct;
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                    string primarywidth = Element.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsValueString();
                                                    ModelWidth = primarywidth;
                                                    string primaryheight = Element.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsValueString();
                                                    ModelHeight = primaryheight;
                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    ModelHydraulicArea = Math.Round(((Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4) / 1000000),5).ToString();
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter * 304.8;
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
                                                    DetailType = Detail.RoundDuct;
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    try
                                                    {
                                                        string primarydiameter = Element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsValueString();
                                                        ModelDiameter = primarydiameter;
                                                    }
                                                    catch
                                                    {
                                                        double d = custom.Diameter * 304.8;
                                                        ModelDiameter = d.ToString();
                                                    }
                                                   
                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    ModelHydraulicArea = Math.Round(((Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4) / 1000000), 5).ToString();
                                                    //ModelHydraulicArea = ((Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4) / 1000000).ToString();
                                                }
                                                else
                                                {
                                                    DetailType = Detail.RectangularDuct;
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                    try
                                                    {
                                                        string primarywidth = Element.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsValueString();
                                                        ModelWidth = primarywidth;
                                                    }
                                                    catch
                                                    {
                                                        double w = custom.Width * 304.8;
                                                        ModelWidth = w.ToString();
                                                    }

                                                    try
                                                    {
                                                        string primaryheight = Element.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsValueString();
                                                        ModelHeight = primaryheight;
                                                    }
                                                    catch
                                                    {
                                                        double h = custom.Height * 304.8;
                                                        ModelHeight = h.ToString();
                                                    }
                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    ModelHydraulicArea = Math.Round(((Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4) / 1000000), 5).ToString();
                                                    //ModelHydraulicArea = ((Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4) / 1000000).ToString();
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter * 304.8;
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

            if (Element is FlexDuct)
            {
                MSystem = (Element as MEPCurve).MEPSystem;
                SystemType = (MSystem as MechanicalSystem).SystemType;
                ShortSystemName = Element.LookupParameter("Сокращение для системы").AsString();
                Name = Element.Name;//Добавил в патче
                OwnConnectors = ((Element as FlexDuct) as MEPCurve).ConnectorManager.Connectors;
                string primaryvolume = Element.get_Parameter(BuiltInParameter.RBS_DUCT_FLOW_PARAM).AsValueString();
                Volume = GetValue(primaryvolume);
                string primarylength = Element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsValueString();
                ModelLength = primarylength;
                string primaryvelocity = Convert.ToString(Math.Round(Element.get_Parameter(BuiltInParameter.RBS_VELOCITY).AsDouble() / 3.25, 2));
                //string primaryvelocity = Element.get_Parameter(BuiltInParameter.RBS_VELOCITY).AsValueString();
                ModelVelocity = primaryvelocity;
                //ModelVelocity = GetValue(primaryvelocity);
                
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
                                                    DetailType = Detail.RoundDuct;
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    string primarydiameter = Element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsValueString();
                                                    ModelDiameter = primarydiameter;

                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    ModelHydraulicArea = ((Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4) / 1000000).ToString();
                                                    DetailType = Detail.RoundFlexDuct;
                                                }
                                                else
                                                {
                                                    DetailType = Detail.RectangularDuct;
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                    string primarywidth = Element.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsValueString();
                                                    ModelWidth = primarywidth;
                                                    string primaryheight = Element.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsValueString();
                                                    ModelHeight = primaryheight;
                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    ModelHydraulicArea = (Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4).ToString();
                                                    DetailType = Detail.RectFlexDuct;
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter * 304.8;
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
                                                    DetailType = Detail.RoundDuct;
                                                    custom.Diameter = connect.Radius * 2;
                                                    custom.EquiDiameter = custom.Diameter;
                                                    string primarydiameter = Element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsValueString();
                                                    ModelDiameter = primarydiameter;
                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    DetailType = Detail.RoundFlexDuct;
                                                }
                                                else
                                                {
                                                    DetailType = Detail.RectangularDuct;
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                    string primarywidth = Element.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsValueString();
                                                    ModelWidth = primarywidth;
                                                    string primaryheight = Element.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsValueString();
                                                    ModelHeight = primaryheight;
                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    DetailType = Detail.RectFlexDuct;
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter * 304.8;
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
                Name = Element.Name;//Добавил в патче
                if ((Model as MechanicalFitting) != null)
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
                    else if ((Model as MechanicalFitting).PartType == PartType.TapAdjustable)
                    {
                        DetailType = Detail.TapAdjustable;
                    }
                    else if ((Model as MechanicalFitting).PartType == PartType.Transition)
                    {
                        DetailType = Detail.Transition;
                    }
                    else if ((Model as MechanicalFitting).PartType == PartType.Union)
                    {
                        DetailType = Detail.Union;
                    }

                    // Добавил 18.12.24
                    else if ((Model as MechanicalFitting).PartType==PartType.MultiPort)
                    {
                        DetailType = Detail.Multiport; 
                    }    

                }
                else if (Element.Category.Id.IntegerValue == -2008016)
                {
                    DetailType = Detail.FireProtectValve;
                   

                }
                else if (Element.Category.Id.IntegerValue == -2008013)
                {
                    DetailType = Detail.AirTerminal;
                }
                else if (Element.Category.Id.IntegerValue == -2001140)
                { 
                    DetailType = Detail.Equipment;
                }
                /*else if (Element.LookupParameter("ТипДетали").AsString())
                {
                    DetailType = Detail.FireProtectValve;
                }*/

                OwnConnectors = (Element as FamilyInstance).MEPModel.ConnectorManager.Connectors;

                foreach (Connector connector in OwnConnectors)
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
                                                    ModelDiameter = Math.Round(custom.Diameter * 304.8, 0).ToString();
                                                }
                                                else
                                                {
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                    try
                                                    {
                                                        ModelWidth = doc.GetElement(ElementId).LookupParameter("Ширина воздуховода").AsValueString();
                                                        ModelHeight = doc.GetElement(ElementId).LookupParameter("Высота воздуховода").AsValueString();
                                                        double mwidth = Convert.ToDouble(ModelWidth);
                                                        double mheight = Convert.ToDouble(ModelHeight);
                                                        ModelHydraulicDiameter = Convert.ToInt32(2 * mwidth * mheight / (mwidth + mheight)).ToString();
                                                    }
                                                    catch
                                                    {

                                                    }
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter;

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
                                                    ModelDiameter = Math.Round(custom.Diameter * 304.8, 0).ToString();
                                                }
                                                else
                                                {
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                    try
                                                    {
                                                        ModelWidth = doc.GetElement(ElementId).LookupParameter("Ширина воздуховода").AsValueString();
                                                        ModelHeight = doc.GetElement(ElementId).LookupParameter("Высота воздуховода").AsValueString();
                                                        double mwidth = Convert.ToDouble(ModelWidth);
                                                        double mheight = Convert.ToDouble(ModelHeight);
                                                        ModelHydraulicDiameter = Convert.ToInt32(2 * mwidth * mheight / (mwidth + mheight)).ToString();
                                                    }
                                                    catch
                                                    {

                                                    }
                                                }
                                                custom.Coefficient = connect.Coefficient;
                                                custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                                custom.NextOwnerId = custom.NextOwnerId;
                                                NextElementId = custom.NextOwnerId;
                                                EquiDiameter = custom.EquiDiameter;
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
