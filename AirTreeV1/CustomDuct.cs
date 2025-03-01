using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Visual;

namespace AirTreeV1
{
    public  class CustomDuct
    {
        public Autodesk.Revit.DB.Document Document { get; set; }
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
        public CustomElement.Detail DetailType { get; set; }

        public CustomConnector SelectedConnector { get; set; }
        public List<CustomConnector> SecondaryConnectors { get; set; }

        public ConnectorSet OwnConnectors { get; set; }

        public string Volume { get; set; } = "0";
        public string ModelWidth { get; set; } = "0";
        public string ModelHeight { get; set; } = "0";

        public string NewModelWidth { get; set; } = "0";
        public string NewModelHeight { get; set; } = "0";
        public string ModelLength { get; set; } = "0";
        public string ModelDiameter { get; set; } = "0";
        public string ModelVelocity { get; set; } = "0";
        public string ModelHydraulicDiameter { get; set; } = "0";
        public double EquiDiameter { get; set; }
        public string ModelHydraulicArea { get; set; } = "0";
        public double LocRes { get; set; }
        public double PDyn { get; set; }
        public double PStat { get; set; }
        public double Ptot { get; set; }

        private string GetValue(string primaryvolume)
        {
            // Используем регулярное выражение, чтобы найти и вернуть только числовую часть
            var match = System.Text.RegularExpressions.Regex.Match(primaryvolume, @"\d+(\.\d+)?");
            return match.Success ? match.Value : string.Empty; // Вернуть число или пустую строку, если числ
        }
        public CustomDuct (Autodesk.Revit.DB.Document doc, CustomElement customElement)
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
                    if (connector.ConnectorType == ConnectorType.End)
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
                                                    DetailType = CustomElement.Detail.RoundDuct;
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
                                                        DetailType = CustomElement.Detail.RectangularDuct;

                                                        //ModelDiameter = "Not defined";
                                                    }

                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    ModelHydraulicArea = Math.Round(((Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4) / 1000000), 5).ToString();
                                                    //ModelHydraulicArea = ((Math.PI*Math.Pow(Convert.ToDouble(ModelHydraulicDiameter),2) / 4)/1000000).ToString();
                                                }
                                                else
                                                {
                                                    DetailType = CustomElement.Detail.RectangularDuct;
                                                    custom.Width = connect.Width;
                                                    custom.Height = connect.Height;
                                                    custom.EquiDiameter = 2 * custom.Width * custom.Height / (custom.Width + custom.Height);
                                                    string primarywidth = Element.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsValueString();
                                                    ModelWidth = primarywidth;
                                                    string primaryheight = Element.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsValueString();
                                                    ModelHeight = primaryheight;
                                                    ModelHydraulicDiameter = Element.get_Parameter(BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM).AsValueString();
                                                    ModelHydraulicArea = Math.Round(((Math.PI * Math.Pow(Convert.ToDouble(ModelHydraulicDiameter), 2) / 4) / 1000000), 5).ToString();
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
                                                    DetailType = CustomElement.Detail.RoundDuct;
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
                                                    DetailType = CustomElement.Detail.RectangularDuct;
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
                    if (connector.ConnectorType == ConnectorType.Curve)
                    {
                        DetailType = CustomElement.Detail.DuctTap;
                        foreach (Connector nextconnector in connector.AllRefs)
                        {
                            if (nextconnector.Owner.Id!= connector.Owner.Id)
                            {
                                ElementId = nextconnector.Owner.Id;
                                //customElement.IsReversed = true;

                            }
                        }
                    }
                }
               
                
            }
        }
    }
}
