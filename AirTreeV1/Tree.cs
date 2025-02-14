using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace AirTreeV1
{
    public  class Tree
    {
        List<TreeNode> TreeNodes { get; set; } = new List<TreeNode>();
        HashSet<ElementId> checkedElements { get; set; } = new HashSet<ElementId>();
        CustomCollection CustomCollection { get; set; }

        public double[,] AdjacencyMatrix;
        public Tree (CustomCollection customCollection)
        {
            CustomCollection = customCollection;
            //HashSet<ElementId> checkedElements = new HashSet<ElementId>();
            
        }

        public void MatrixCalc()
        {
            double maxpressure = double.MinValue;
            TreeNode selectedNode = null;
            int size = TreeNodes.Count;
            for (int i=0; i<size;i++)
            {
                if (TreeNodes[i].IsStart==true && TreeNodes[i].IsVisited==false)
                {
                    var nextnode = TreeNodes[i].NextNode;
                    int index = 0;
                    for (int k =0; k<size;k++)
                    {
                        if (TreeNodes[k].Id.IntegerValue == nextnode.ElementId.IntegerValue)
                        {
                            index = k;
                        }
                    }
                    CustomElement element = TreeNodes[i].NextNode;

                    if (element.DetailType == CustomElement.Detail.Tee)
                    {
                        CustomTee2 customDuctInsert = new CustomTee2(CustomCollection.Document, element, CustomCollection.Collection, false);
                        element.LocRes = customDuctInsert.LocRes;
                        element.PDyn = CustomCollection.Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * element.LocRes;

                        TreeNodes[i].Pressure += element.PDyn;
                        TreeNodes[index].Pressure += TreeNodes[i].Pressure;
                    }
                    if (element.DetailType == CustomElement.Detail.TapAdjustable)
                    {
                        //bool isReversed = FindPrevious(element, branch);

                        CustomDuctInsert2 customDuctInsert = new CustomDuctInsert2(CustomCollection.Document, element, CustomCollection.Collection, false);
                        element.LocRes = customDuctInsert.LocRes;
                        element.PDyn = CustomCollection.Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * element.LocRes;
                        TreeNodes[i].Pressure += element.PDyn;
                        TreeNodes[index].Pressure += TreeNodes[i].Pressure;
                    }
                    TreeNodes[i].IsVisited = true;
                }
                UpdateAdjacencyMatrix();
            }
            int left_nodes = TreeNodes.Select(x => x).Where(x => x.IsVisited == false).Count();
            int res_counter = 0;

            
                //res_counter++;
                left_nodes = TreeNodes.Select(x => x).Where(x => x.IsVisited == false).Count();
                for (int k = 0; k < size; k++)
                {
                    int counter = 0;
                    if (TreeNodes[k].IsStart == false && TreeNodes[k].IsVisited == false)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            if (TreeNodes[i].NextNode != null && TreeNodes[k].CElement != null)
                            {
                                if (TreeNodes[i].NextNode.ElementId.IntegerValue == TreeNodes[k].CElement.ElementId.IntegerValue)
                                {
                                    if (counter == 1)
                                    {
                                        int check_index = 0;
                                        CustomElement element = TreeNodes[k].CElement;

                                        if (element.DetailType == CustomElement.Detail.Tee)
                                        {
                                            check_index = k;
                                            CustomTee2 customDuctInsert = new CustomTee2(CustomCollection.Document, element, CustomCollection.Collection, false);
                                            element.LocRes = customDuctInsert.LocRes;
                                            element.PDyn = CustomCollection.Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * element.LocRes;

                                            TreeNodes[i].Pressure += element.PDyn;
                                            TreeNodes[k].Pressure += TreeNodes[i].Pressure;
                                        }
                                    if (element.DetailType == CustomElement.Detail.TapAdjustable)
                                    {
                                        //bool isReversed = FindPrevious(element, branch);
                                        check_index = k;
                                        CustomDuctInsert2 customDuctInsert = new CustomDuctInsert2(CustomCollection.Document, element, CustomCollection.Collection, false);
                                        element.LocRes = customDuctInsert.LocRes;
                                        element.PDyn = CustomCollection.Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * element.LocRes;
                                        TreeNodes[i].Pressure += element.PDyn;
                                        TreeNodes[k].Pressure += TreeNodes[i].Pressure;
                                    }
                                    TreeNodes[check_index].IsVisited = true;
                                        CustomCollection.ResCalculate();
                                        UpdateAdjacencyMatrix();
                                    }
                                    else
                                    {
                                        counter++;
                                    }

                                }
                            }
                            else
                            { continue; }

                        }
                    }

                }

                
            

            

        }
        public void AddNodes(CustomCollection customCollection)
        {
            foreach (var branch in customCollection.Collection)
            {
                foreach(var element in branch.Elements)
                {
                    if (!checkedElements.Contains(element.ElementId))
                    {
                        if (element.DetailType == CustomElement.Detail.AirTerminal)
                        {
                            TreeNode treeNode = new TreeNode(element);
                            if (treeNode.Id.IntegerValue == 643925)
                            {
                                var treenode2 = treeNode;
                            }
                                if (!checkedElements.Contains(element.ElementId))
                            {
                                treeNode.FindElements(treeNode.CElement, branch);
                                treeNode.NodeCalcPressure();
                                TreeNodes.Add(treeNode);
                                checkedElements.Add(element.ElementId);
                            }
                        }
                        if (element.DetailType==CustomElement.Detail.Tee || element.DetailType == CustomElement.Detail.TapAdjustable)
                        {
                            TreeNode treeNode = new TreeNode(element);

                            if (!checkedElements.Contains(element.ElementId))
                            {
                                treeNode.FindElements(treeNode.CElement, branch);
                                treeNode.NodeCalcPressure();
                                TreeNodes.Add(treeNode);
                                checkedElements.Add(element.ElementId);
                            }
                        }
                        

                    }
                }
                /*foreach (var node in TreeNodes)
                {
                    if (node.Id.IntegerValue == 643847)
                    {
                        var treenode2 = node;
                    }
                    node.FindElements(node.CElement, branch);
                    node.NodeCalcPressure();
                }*/
            }

            
           
            UpdateAdjacencyMatrix();
        }

        private void UpdateAdjacencyMatrix()
        {
            int size = TreeNodes.Count;
            AdjacencyMatrix = new double[size, size];
            for (int i=0; i<size;i++)
            {

                var nextnode = TreeNodes[i].NextNode;
                if (TreeNodes[i].NextNode != null)
                {

                    int foundedelem = 0;
                    for (int k = 0; k < size;k++)
                    {
                        if (TreeNodes[k].Id.IntegerValue == nextnode.ElementId.IntegerValue)
                        {
                            foundedelem = k;
                            break;
                        }
                    }
                   /* var foundNode = TreeNodes.FirstOrDefault(x => x.Id.IntegerValue == nextnode.ElementId.IntegerValue);
                    var k = TreeNodes.IndexOf(foundNode);*/

                    AdjacencyMatrix[i, foundedelem] = TreeNodes[i].Pressure;
                    if (size==0)
                    {
                        continue;
                    }
                    else
                    {
                        for (int j = 0; j < size; j++)
                        {
                            if (i == j)
                            {
                                continue;
                            }
                            else if (j == foundedelem)
                            {
                                continue;
                            }
                            else
                            {
                                /*if (TreeNodes[i].Edge.Contains(TreeNodes[j].NextNode))
                                {
                                    AdjacencyMatrix[i, j] = TreeNodes[i].Pressure;
                                }*/


                                AdjacencyMatrix[i, j] = 0;

                            }

                        }
                    }
                    
                }
                


               

               
            }
        }

        public string PrintMatrix()
        {
            int rows = AdjacencyMatrix.GetLength(0);
            int cols = AdjacencyMatrix.GetLength(1);
            StringBuilder sb = new StringBuilder();
            sb.Append("");
            sb.Append(";");
            sb.Append(";");
            for(int k=0;k<TreeNodes.Count;k++)
            {
                sb.Append(TreeNodes[k].StrId.ToString());
                sb.Append(";");
               
            }
            sb.AppendLine();
            for (int i=0; i<rows;i++)
            {
               
                for (int j=0; j<cols;j++)
                {
                    if(j==0)
                    {
                        sb.Append(TreeNodes[i].StrId.ToString());
                        sb.Append(";");
                        sb.Append(TreeNodes[i].CElement.DetailType.ToString());
                        sb.Append(";");
                    }
                    sb.Append(AdjacencyMatrix[i, j]);
                    
                    if (j < cols - 1)
                    {
                        sb.Append(";"); // Разделитель между элементами
                    }
                }
                if (i < rows - 1)
                {
                    sb.AppendLine(); // Перенос строки между строками матрицы
                }
            }
            return sb.ToString();
        }


        public void SaveFile(string content) // спрятали функцию сохранения 
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
            saveFileDialog.Title = "Save CSV File";
            //saveFileDialog.FileName = Collection.First().Elements.First().SystemName + ".csv";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {

                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        writer.Write(content);
                    }

                    Console.WriteLine("CSV file saved successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error saving CSV file: " + ex.Message);
                }
            }
        }
    }
}
