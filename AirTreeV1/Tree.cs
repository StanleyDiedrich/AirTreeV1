using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
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
                            checkedElements.Add(element.ElementId);
                            treeNode.FindElements(element, CustomCollection);
                            treeNode.NodeCalcPressure();
                            TreeNodes.Add(treeNode);
                        }
                        else if (element.DetailType == CustomElement.Detail.Tee || element.DetailType == CustomElement.Detail.TapAdjustable)
                        {
                            TreeNode treeNode = new TreeNode(element);
                            checkedElements.Add(element.ElementId);
                            treeNode.FindElements(element, CustomCollection);
                            treeNode.NodeCalcPressure();
                            TreeNodes.Add(treeNode);
                        }
                      
                    }
                }
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
                    var foundNode = TreeNodes.FirstOrDefault(x => x.Id.IntegerValue == nextnode.ElementId.IntegerValue);
                    var k = TreeNodes.IndexOf(foundNode);

                    AdjacencyMatrix[i, k] = TreeNodes[i].Pressure;
                    for (int j = 0; j < size; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }
                        else if (j == k)
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
                        sb.Append(TreeNodes[i].Name.ToString());
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
