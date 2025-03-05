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
        public CustomElement Element { get; set; }
        public List<CustomElement> Elements { get; set; }
        public List<CustomElement> Children { get; set; } = new List<CustomElement>();
        public int Index { get; set; }
        public int Level { get; set; }
        public double Ptot { get; set; }
        public double PtotLeft { get; set; }
        public double PtotRight { get; set; }
        public double NodePtot { get; set; }
        public bool IsInTree { get; set; }
        public bool IsVisited { get; set; }

        public TreeNode LeftChild { get; set; }
        public TreeNode RightChild { get; set; }

        public TreeNode Parent { get; set; }
        public Direction DirectionType { get; set; }
        public enum Direction
        {
            L,
            R
        }
        public bool IsSelected { get; internal set; }

        public TreeNode(CustomElement element, int index, List<CustomElement> elements)
        {
            Element = element;
            Index = index;
            Elements = elements;
            Children = GetChildren();
            Ptot = GetNodePressure();

        }



        public List<CustomElement> GetChildren()
        {
            List<CustomElement> children = new List<CustomElement>();

            int i = Index - 1;

            while (i >= 0)
            {
                CustomElement previousElement = Elements[i + 1];
                CustomElement currentElement = Elements[i];
                /*if (previousElement.DetailType.Contains("TapAdjustable"))
                {
                    i--;
                }*/
                if (currentElement.DetailType==CustomElement.Detail.Tee || currentElement.DetailType==CustomElement.Detail.TapAdjustable || currentElement.DetailType==CustomElement.Detail.DuctTap)
                {
                    break;
                }

                children.Add(currentElement);
                i--;
            }
            Children = children;
            return Children;
        }

        public double GetNodePressure()
        {
            Ptot = Children.Sum(x => x.PDyn + x.PStat);
            return Ptot;
        }

        public override string ToString()
        {
            return $"{Element.ElementId};{Element.DetailType};{Ptot}";
        }
    }

}