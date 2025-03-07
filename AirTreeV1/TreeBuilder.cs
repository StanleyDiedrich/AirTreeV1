using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace AirTreeV1
{
    public class TreeBuilder
    {
        public Autodesk.Revit.DB.Document Document { get; set; }
        public List<CustomBranch>  Collection { get; set; }
        public double Density { get; set; }
        public List<List<TreeNode>> TreeNodesCollection { get; set; } = new List<List<TreeNode>>();
        public TreeNode Root { get; set; }
        public List<TreeNode> Leafs { get; set; } = new List<TreeNode>();
        public List<List<TreeNode>> AllNodes { get; set; } = new List<List<TreeNode>>();
        public List<List<TreeNode>> TreeResult { get; set; } = new List<List<TreeNode>>();

        public TreeBuilder(Autodesk.Revit.DB.Document doc, List<List<TreeNode>> treeNodesCollection, List<CustomBranch> collection, double density)
        {
            Document = doc;
            Collection = collection;
            TreeNodesCollection = treeNodesCollection;

            //Root = SetRoot();
            //Leafs = SetLeafs();
            //AllNodes = BFSGetOtherNodes();
            AllNodes = ProcessNodes();
            Density = density;
            TreeResult = TreeCalc();
        }

        private TreeNode SetRoot()
        {
            throw new NotImplementedException();
        }


        public List<List<TreeNode>> ProcessNodes()
        {
            List<List<TreeNode>> allNodes = new List<List<TreeNode>>();
            List<ElementId> visitedNodes = new List<ElementId>();
            foreach (var treenodeBranch in TreeNodesCollection)
            {
                List<TreeNode> nodes = new List<TreeNode>();

                for (int i = 0; i < treenodeBranch.Count; i++)
                {
                    TreeNode element = treenodeBranch[i];
                    element.Level = i;
                    if (!visitedNodes.Contains(element.Element.ElementId))
                    {
                        if (element.Element.DetailType==CustomElement.Detail.AirTerminal)
                        {
                            element.LeftChild = null;
                            element.PtotLeft = element.Ptot;
                            element.Parent = treenodeBranch[i + 1];
                            visitedNodes.Add(element.Element.ElementId);

                        }
                        else if (i == treenodeBranch.Count - 1)
                        {
                            element.Parent = null;
                            Root = element;
                            element.LeftChild = treenodeBranch[i - 1];
                            element.PtotLeft = element.Ptot;
                            element.RightChild = GetRightChild(element, TreeNodesCollection);
                            visitedNodes.Add(element.Element.ElementId);
                            visitedNodes.Add(element.Element.ElementId);

                        }
                        
                        else
                        {
                            element.LeftChild = treenodeBranch[i - 1];
                            element.PtotLeft = element.Ptot;
                            
                            element.RightChild = GetRightChild(element, TreeNodesCollection);
                            visitedNodes.Add(element.Element.ElementId);
                        }
                        nodes.Add(element);
                    }



                }
                allNodes.Add(nodes);
            }
            return allNodes;
        }

        private TreeNode GetRightChild(TreeNode element, List<List<TreeNode>> treeNodesCollection)
        {
            foreach (var treeNodeBranch in treeNodesCollection)
            {
                for (int j = 0; j < treeNodeBranch.Count; j++)
                {
                    
                    if (treeNodeBranch[j].Element.BranchNumber == element.Element.BranchNumber)
                    {
                        break;
                    }
                    else
                    {
                        if (element.Element.ElementId == treeNodeBranch[j].Element.ElementId)
                        {
                            if (/*element.IsVisited == false &&*/ treeNodeBranch[j - 1].IsVisited == false)
                            {

                                element.RightChild = treeNodeBranch[j - 1];
                                if (element.RightChild.Element.ElementId != element.LeftChild.Element.ElementId)
                                {
                                    element.IsVisited = true;
                                    treeNodeBranch[j - 1].IsVisited = true;
                                    element.PtotRight = treeNodeBranch[j].Ptot;
                                    return treeNodeBranch[j - 1];
                                }

                            }

                        }

                    }
                }
            }

            return null;
        }

        private void UpdateElementProperties(CustomElement element, CustomTee2 customTee)
        {
            element.IA = customTee.IA;
            element.IQ = customTee.IQ;
            element.IC = customTee.IC;
            element.O1A = customTee.O1A;
            element.O1Q = customTee.O1Q;
            element.O1C = customTee.O1C;
            element.O2A = customTee.O2A;
            element.O2Q = customTee.O2Q;
            element.RA = customTee.RA;
            element.RQ = customTee.RQ;
            element.RC = customTee.RC;
            element.LocRes = customTee.LocRes;


            element.PDyn = Density * Math.Pow(customTee.Velocity, 2) / 2 * element.LocRes;
        }

        private void UpdateElementProperties(CustomElement element, CustomDuctInsert2 customTee)
        {
            element.IA = customTee.IA;
            element.IQ = customTee.IQ;
            element.IC = customTee.IC;
            element.O1A = customTee.O1A;
            element.O1Q = customTee.O1Q;
            element.O1C = customTee.O1C;
            element.O2A = customTee.O2A;
            element.O2Q = customTee.O2Q;
            element.RA = customTee.RA;
            element.RQ = customTee.RQ;
            element.RC = customTee.RC;
            element.LocRes = customTee.LocRes;


            element.PDyn = Density * Math.Pow(customTee.Velocity, 2) / 2 * element.LocRes;
        }



        public List<List<TreeNode>> TreeCalc()
        {
            List<List<TreeNode>> result = new List<List<TreeNode>>();
            List<TreeNode> nodes = new List<TreeNode>();
            List<ElementId> visitedNodes = new List<ElementId>();
            foreach (var branch in AllNodes)
            {
                nodes.AddRange(branch);
            }

            var groupedNodes = nodes.GroupBy(x => x.Level);

            foreach (var group in groupedNodes)
            {
                foreach (var node in group)
                {
                    node.IsVisited = false;
                }
            }



            foreach (var group in groupedNodes)
            {
                List<TreeNode> resBranch = new List<TreeNode>();
                foreach (var node in group)
                {
                    if (node.IsVisited == false)
                    {
                        if (node.LeftChild == null && node.RightChild == null)
                        {
                            if (!visitedNodes.Contains(node.Element.ElementId))
                            {
                                visitedNodes.Add(node.Element.ElementId);
                                resBranch.Add(node);
                                continue;
                            }
                            
                        }
                        else
                        {
                            if (!visitedNodes.Contains(node.Element.ElementId))
                            {
                                visitedNodes.Add(node.Element.ElementId);
                                TreeNode leftChild = node.LeftChild;
                                TreeNode rightChild = node.RightChild;
                                double leftPressure = node.PtotLeft;
                                double rightPressure = node.PtotRight;

                                //Обработка тройника 
                                // TeeDefinition()

                                if (node.Element.DetailType == CustomElement.Detail.Tee)
                                {
                                    CustomElement customElement = node.Element;
                                    CustomTee2 customTee2 = new CustomTee2(Document, customElement, Collection, false);
                                    UpdateElementProperties(customElement, customTee2);
                                    resBranch.Add(node);
                                }
                                else if (node.Element.DetailType==CustomElement.Detail.TapAdjustable)
                                {
                                    CustomElement customElement = node.Element;
                                    CustomDuctInsert2 customTee2 = new CustomDuctInsert2(Document, customElement, Collection, false);
                                    UpdateElementProperties(customElement, customTee2);
                                    resBranch.Add(node);
                                }
                                else if (node.Element.DetailType == CustomElement.Detail.DuctTap)
                                {
                                    CustomElement customElement = null;
                                   foreach (var branch in Collection)
                                    {
                                        foreach (var el in branch.Elements)
                                        {
                                            if (el.TapId!=null)
                                            {
                                                if (el.ElementId == node.Element.TapId)
                                                {
                                                    customElement = el;
                                                    CustomDuctInsert2 customTee2 = new CustomDuctInsert2(Document, customElement, Collection, false);
                                                    UpdateElementProperties(customElement, customTee2);
                                                    resBranch.Add(node);
                                                }
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    resBranch.Add(node);
                                }
                                



                            }

                        }
                    }
                    else
                    {
                        continue;
                    }


                   
                }
                result.Add(resBranch);
            }



            return result;
        }



        public string GetContent()
        {

            var csvcontent = new StringBuilder();
            //csvcontent.AppendLine("ElementId;DetailType;ElementName;SystemName;Level;BranchNumber;SectionNumber;Volume;Length;Width;Height;Diameter;HydraulicDiameter;HydraulicArea;IA;IQ;IC;O1A;O1Q;O1C;O2A;O2Q;O2C;RA;RQ;RC;Velocity;PStat;KMS;PDyn;Ptot;Code;MainTrack");
            csvcontent.AppendLine("ElementId;DetailType;ElementName;SystemName;Level;BranchNumber;SectionNumber;Volume;Length;Width;Height;Diameter;HydraulicDiameter;HydraulicArea;Velocity;PStat;KMS;PDyn;Ptot;Code;MainTrack");
            foreach (var branch in TreeResult)
            {

                foreach (var el in branch)
                {
                    CustomElement element = el.Element;
                    if (element.IsNonPrinted)
                    {
                        continue;
                    }
                    /*string a = $"{element.ElementId};{element.DetailType};{element.Name};{element.SystemName};{element.Lvl};{element.BranchNumber};{element.TrackNumber};" +
                         $"{element.Volume};{element.ModelLength};{element.ModelWidth};{element.ModelHeight};{element.ModelDiameter};{element.ModelHydraulicDiameter};{element.ModelHydraulicArea};{element.IA};{element.IQ};{element.IC};{element.O1A};{element.O1Q};{element.O1C};{element.O2A};{element.O2Q};{element.O2C};{element.RA};{element.RQ};{element.RC};{element.ModelVelocity};{element.PStat};{Math.Round(element.LocRes, 2)};{Math.Round(element.PDyn, 2)};{Math.Round(element.Ptot, 2)};" +

                         $"{element.SystemName}-{element.Lvl}-{element.BranchNumber}-{element.TrackNumber};{element.MainTrack}";*/

                    element.NewModelWidth = Convert.ToString(Convert.ToDouble(element.ModelWidth));
                    element.NewModelHeight = Convert.ToString(Convert.ToDouble(element.ModelHeight));
                    element.ModelVelocity = Convert.ToString(Math.Round(Convert.ToDouble(element.ModelVelocity), 2));
                    element.ModelDiameter = Convert.ToString(Math.Round(Convert.ToDouble(element.ModelDiameter), 2));
                    string a = $"{element.ElementId};{element.DetailType};{element.Name};{element.SystemName};{element.Lvl};{element.BranchNumber};{element.TrackNumber};" +
                        $"{element.Volume};{element.ModelLength};{element.NewModelWidth};{element.NewModelHeight};{element.ModelDiameter};{element.ModelHydraulicDiameter};{element.ModelHydraulicArea};{element.ModelVelocity};{element.PStat};{Math.Round(element.LocRes, 2)};{Math.Round(element.PDyn, 2)};{Math.Round(element.Ptot, 2)};" +

                        $"{element.SystemName}-{element.Lvl}-{element.BranchNumber}-{element.TrackNumber};{element.MainTrack}";
                    csvcontent.AppendLine(a);


                }
            }

            return csvcontent.ToString();
        }
        public string PrintTree(bool printType)
        {
            StringBuilder result = new StringBuilder(); // Используем StringBuilder для повышения эффективности
            List<TreeNode> nodes = new List<TreeNode>();

            // Собираем все узлы в один список
            foreach (var branch in AllNodes)
            {
                nodes.AddRange(branch); // Оптимизированный способ добавления элементов
            }

            // Группируем узлы по уровню
            var groupedNodes = nodes.GroupBy(x => x.Level);
            foreach (var group in groupedNodes)
            {
                string separator = printType ? "\n" : "\t"; // Определяем разделитель в зависимости от printType
                foreach (var node in group)
                {
                    string leftChildId = node.LeftChild?.Element.ElementId.ToString() ?? "null";
                    string rightChildId = node.RightChild?.Element.ElementId.ToString() ?? "null";
                    result.Append($"{node.Level};{node.Element.ElementId};{node.Element.DetailType};{leftChildId};{rightChildId};{node.DirectionType};{separator}");

                    // Проверка на null перед обращением к элементам правого и левого детей



                }
                result.Append("\n");
            }

            return result.ToString(); // Возвращаем итоговую строку
        }

    }
}