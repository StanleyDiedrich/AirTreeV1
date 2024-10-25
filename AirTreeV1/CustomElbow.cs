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
        public double Radius { get; set; }
        public CustomConnector InletConnector { get; set; }
        public CustomConnector OutletConnector { get; set; }
        public DuctSystemType SystemType { get; set; }
        public double LocRes { get; set; }

        public CustomElbow(Autodesk.Revit.DB.Document document, CustomElement element)
        {
            Document = document;
            Element = element;
            ElementId = element.ElementId;
            SystemType = element.SystemType;
            
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
                                        OutletConnector = custom;


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

                                        OutletConnector = custom;
                                        //SecondaryConnectors.Add(custom);
                                    }
                                }

                            }

                        }

                    }
                }

            }
            Width = OutletConnector.Width;
            Height = OutletConnector.Height;
            Radius = Document.GetElement(ElementId).LookupParameter("Центр и радиус").AsDouble();
            ElbowData elbowdata = new ElbowData();
            double hw = Height / Radius;
            double rw = Radius / Height;
            LocRes = elbowdata.Interpolation(hw, rw);

        }
    }
}
