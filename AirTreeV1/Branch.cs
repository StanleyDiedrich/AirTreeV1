﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using System.Xml.Linq;

namespace AirTreeV1
{
   


    
        public class Branch
        {
            private static int _counter = 0;
            public int Number { get; set; }
            public double DPressure { get; set; }
            public List<Node> Nodes { get; set; }
            public bool MainBranch { get; set; }
            public bool IsOCK { get; set; }
            public Branch()
            {
                Number = _counter++;
                Nodes = new List<Node>();

            }
            public void GetBranchNumber()
            {
                foreach (var node in Nodes)
                {
                    node.BranchNumber = Number;
                }

            }

        public void Add(Node node)
        {
            if (node != null)
            {
                // Находим индекс узла с таким же ElementId
                var existingNodeIndex = Nodes.FindIndex(n => n.ElementId == node.ElementId);

                if (existingNodeIndex >= 0)
                {
                    // Если найден, заменяем существующий узел
                    Nodes[existingNodeIndex] = node;
                }
                else
                {
                    // Если не найден, добавляем новый узел
                    Nodes.Add(node);
                }
            }
        }

        public void AddRange(Branch branch)
        {
            if (branch != null)
            {
                foreach (var node in branch.Nodes)
                {
                    Add(node); // Использует метод Add, который уже включает логику уникальности
                }
            }
        }
        /*public void Add(Node node)
        {
            Nodes.Add(node);
        }
        public void AddRange(Branch branch)
        {
            foreach (var node in branch.Nodes)
            {
                Nodes.Add(node);
            }
        }*/
        public List<Node> GetNodes()
            {
                return Nodes;
            }
            public void OCKCheck()
            {
                if (IsOCK == true)
                {
                    foreach (var node in Nodes)
                    {

                        node.IsOCK = true;
                    }
                }
               
                else
                {
                    foreach (var node in Nodes)
                    {
                        node.IsOCK = false;
                    }
                }
            }
            public void RemoveNull()
            {
                // Create a new list to store non-null nodes
                List<Node> nonNullNodes = new List<Node>();

                foreach (var node in Nodes)
                {
                    if (node != null)
                    {
                        nonNullNodes.Add(node);  // Add non-null nodes to the new list
                    }
                }

                Nodes = nonNullNodes;  // Update the Nodes list
            }



            public double GetPressure() // Эту шляпу определили с целью поиска общей потери давления на ответвлении
            {
                double pressure = 0;

                foreach (var node in Nodes)
                {
                    if (node != null)
                    {
                        Element element = node.Element;
                        if (element is Pipe)
                        {
                            try
                            {
                                if ((element as Pipe).LookupParameter("Падение давления").AsDouble() != null)
                                {
                                    double dpressure = (element as Pipe).LookupParameter("Падение давления").AsDouble();
                                    pressure += dpressure;
                                }
                                else if ((element as Pipe).LookupParameter("Рабочее давление").AsDouble() != null)
                                {
                                    double dpressure = (element as Pipe).LookupParameter("Рабочее давление").AsDouble();
                                    pressure += dpressure;
                                }

                            }
                            catch
                            {
                                ConnectorSet connectors = (element as Pipe).ConnectorManager.Connectors;
                                foreach (Connector connector in connectors)
                                {
                                    if (connector.PipeSystemType == PipeSystemType.SupplyHydronic && connector.Direction == FlowDirectionType.In)
                                    {
                                        double dpressure = connector.PressureDrop;
                                        pressure += dpressure;
                                    }
                                    else if (connector.PipeSystemType == PipeSystemType.ReturnHydronic && connector.Direction == FlowDirectionType.Out)
                                    {
                                        double dpressure = connector.PressureDrop;
                                        pressure += dpressure;
                                    }
                                }
                            }

                        }
                    }

                }
                DPressure = pressure;
                return DPressure;
            }
        }
    }


