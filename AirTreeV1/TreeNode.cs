using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTreeV1
{
    public class TreeNode
    {
        public string Name { get; set; }
        public ElementId Id { get; set; }
         public CustomElement CElement { get; set; }
         public CustomElement NextNode { get; set; }
        public bool IsStart { get; set;}
        public bool IsVisited { get; set; }
         public double Pressure { get; set; }
        public List<CustomElement> Edge { get; set; } = new List<CustomElement>();
        public TreeNode (CustomElement customElement)
        {
            CElement = customElement;
            Name = CElement.Name;
            Id = CElement.ElementId;

            if (CElement.DetailType == CustomElement.Detail.AirTerminal)
            {
                IsStart = true;
            }
            
        }

        public void NodeCalcPressure()
        {
            foreach(var element in Edge)
            {
                Pressure += element.PDyn + element.PStat;
            }
            
        }

        internal void FindElements(CustomElement element, CustomBranch branch)
        {
            int index = 0;

            // Assuming CustomBranch contains a collection of CustomElements
            for (int i =0; i<branch.Elements.Count;i++)
            {
                // Perform your logic to find the element
                if (branch.Elements[i].ElementId.IntegerValue == element.ElementId.IntegerValue)
                {
                    index = i;
                }
               
            }
            index++;
            for (int j = index;j<branch.Elements.Count;j++)
            {
                if (branch.Elements[j].DetailType == CustomElement.Detail.Tee || branch.Elements[j].DetailType == CustomElement.Detail.TapAdjustable)
                {
                    NextNode = branch.Elements[j];
                    break;
                }
                else
                {
                    Edge.Add(branch.Elements[j]);

                }
            }

        }
    }
}
