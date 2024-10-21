using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using System.Collections.Generic;
﻿using System;

using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Creation;
using Autodesk.Revit.UI;

namespace AirTreeV1
{
    public class Node
    {
        public Element Element { get; set; }
        public ElementId ElementId { get; set; }
        public string SystemName { get; set; }
        public string ShortSystemName { get; set; }
        public DuctSystemType DuctSystemType { get; set; }
        

        public List<CustomConnector> Connectors { get; set; } = new List<CustomConnector>();
        public List<CustomConnector> ConnectorList { get; set; } = new List<CustomConnector>();

        public ElementId NextOwnerId { get; set; }
        public ElementId Neighbourg { get; set; }
        public bool IsManifold { get; set; }
        public bool IsTee { get; set; }
        public bool IsElbow { get; set; }
        public bool IsSelected { get; set; }
        public bool IsChecked { get; set; }
        public bool IsSplitter { get; set; }
       
        public bool IsOCK { get; set; }
        public bool Reverse { get; set; }
        public int TeeNumber { get; set; }
        public int BranchNumber { get; set; }


        public Node(Autodesk.Revit.DB.Document doc, Element element, DuctSystemType ductSystemType, string shortsystemName, bool reverse)
        {
            Element = element;
            ElementId = Element.Id;
            ShortSystemName = shortsystemName;
            DuctSystemType = ductSystemType;
            Reverse = reverse;



            ConnectorSet connectorSet = null;
            try
            {
                if (element is Autodesk.Revit.DB.Mechanical.DuctInsulation)
                {
                    return;
                }
                if (element is Duct)
                {
                    Duct duct = Element as Duct;
                    SystemName = duct.LookupParameter("Имя системы").AsString();
                    connectorSet = duct.ConnectorManager.Connectors;
                }
                if (element is FamilyInstance)
                {
                    FamilyInstance familyInstance = element as FamilyInstance;
                    SystemName = familyInstance.LookupParameter("Имя системы").AsString();
                    MEPModel mepModel = familyInstance.MEPModel;
                    connectorSet = mepModel.ConnectorManager.Connectors;
                    try
                    {
                        if ((mepModel as MechanicalFitting) == null)
                        {

                        }
                        else
                        {
                            if ((mepModel as MechanicalFitting).PartType == PartType.Elbow && (mepModel as MechanicalFitting) != null)
                            {
                                IsElbow = true;
                            }
                            else if ((mepModel as MechanicalFitting).PartType == PartType.Tee && (mepModel as MechanicalFitting) != null)
                            {
                                IsTee = true;
                            }
                        }

                    }
                    catch
                    {

                    }

                }
                if (ElementId.IntegerValue==1561539)
                {
                    element = element;
                }
                List<List<CustomConnector>> branches = new List<List<CustomConnector>>();
                if (connectorSet.Size >= 2)
                {
                    // IsManifold = true;

                    List<CustomConnector> customConnectors = new List<CustomConnector>();

                    CustomConnector custom = null;
                    ConnectorSet nextconnectors = null;
                    foreach (Connector connector in connectorSet)
                    {
                        if (connector.DuctSystemType == DuctSystemType.SupplyAir)
                        {
                            if (connector.Direction == FlowDirectionType.In)
                            {
                                continue;
                            }
                            else
                            {
                                custom = new CustomConnector(doc, ElementId, DuctSystemType);
                                nextconnectors = connector.AllRefs;
                            }

                        }
                        else if (connector.DuctSystemType == DuctSystemType.ExhaustAir)
                        {
                            if (connector.Direction == FlowDirectionType.Out)
                            {
                                continue;
                            }
                            else
                            {
                                custom = new CustomConnector(doc, ElementId, DuctSystemType);
                                nextconnectors = connector.AllRefs;
                            }
                        }

                        /* CustomConnector custom = new CustomConnector(doc, ElementId, DuctSystemType);
                          ConnectorSet nextconnectors = connector.AllRefs;*/
                        foreach (Connector connect in nextconnectors)
                        {
                            

                            string sysname = doc.GetElement(connect.Owner.Id).LookupParameter("Сокращение для системы").AsString();
                            if (sysname==null || sysname==string.Empty)
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
                            else if (connect.Owner.Id == NextOwnerId)
                            {
                                continue;
                            }
                          
                            else if (sysname.Contains(ShortSystemName))
                            {
                                if (connect.Domain == Autodesk.Revit.DB.Domain.DomainHvac || connect.Domain == Autodesk.Revit.DB.Domain.DomainPiping)
                                {

                                    if (ductSystemType == DuctSystemType.SupplyAir)
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
                                            }
                                            else
                                            {
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                            NextOwnerId = custom.NextOwnerId;

                                            customConnectors.Add(custom);
                                        }



                                    }
                                    else if (ductSystemType == DuctSystemType.ExhaustAir)
                                    {

                                        if (connect.Direction == FlowDirectionType.Out)
                                        {
                                            custom.Flow = connect.Flow;
                                            custom.Domain = Domain.DomainHvac;
                                            custom.DirectionType = FlowDirectionType.Out;
                                            custom.NextOwnerId = connect.Owner.Id;
                                            custom.Shape = connect.Shape;
                                            NextOwnerId = custom.NextOwnerId;
                                            custom.Type = connect.ConnectorType;
                                            custom.Shape = connect.Shape;
                                            if (custom.Shape == ConnectorProfileType.Round)
                                            {
                                                custom.Diameter = connect.Radius * 2;
                                            }
                                            else
                                            {
                                                custom.Width = connect.Width;
                                                custom.Height = connect.Height;
                                            }
                                            custom.Coefficient = connect.Coefficient;
                                            custom.PressureDrop = connect.PressureDrop; // Вот это добавлено в версии 4.1
                                            customConnectors.Add(custom);
                                        }

                                    }
                                   
                                }
                            }


                        }
                        


                    }
                    Connectors = customConnectors;
                    double maxVolume = double.MinValue;
                    double maxCoefficient = double.MinValue;
                    CustomConnector selectedConnector = null;


                    List<CustomConnector> connectorsWithMaxVolume = new List<CustomConnector>();


                    foreach (CustomConnector customConnector in Connectors)
                    {

                        if (customConnector.Flow > maxVolume && customConnector.Type == ConnectorType.End)
                        {
                            maxVolume = customConnector.Flow;
                            selectedConnector = customConnector;
                        }


                    }
                    if (selectedConnector != null)
                    {
                        selectedConnector.IsSelected = true;
                        NextOwnerId = selectedConnector.NextOwnerId;
                    }

                }



                
            }
            catch(Exception ex)
            {

               
            }




        }


    }

}