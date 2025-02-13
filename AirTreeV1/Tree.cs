using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTreeV1
{
    public  class Tree
    {
        List<TreeNode> TreeNodes { get; set; } = new List<TreeNode>();


        public Tree (CustomCollection customCollection)
        {
            HashSet<ElementId> checkedElements = new HashSet<ElementId>();
            foreach (var branch in customCollection.Collection)
            {
                foreach (var element in branch.Elements)
                {
                    if (!checkedElements.Contains(element.ElementId))
                    {
                        if (element.DetailType == CustomElement.Detail.AirTerminal)
                        {
                            TreeNode treeNode = new TreeNode(element);
                            checkedElements.Add(element.ElementId);
                            treeNode.FindElements(element, branch);
                            treeNode.NodeCalcPressure();
                            TreeNodes.Add(treeNode);
                        }
                        else if (element.DetailType == CustomElement.Detail.Tee || element.DetailType == CustomElement.Detail.TapAdjustable)
                        {
                            TreeNode treeNode = new TreeNode(element);
                            checkedElements.Add(element.ElementId);
                            treeNode.FindElements(element,branch);
                            treeNode.NodeCalcPressure();
                            TreeNodes.Add(treeNode);
                        }

                    }
                    
                }
            }
        }
        
    }
}
