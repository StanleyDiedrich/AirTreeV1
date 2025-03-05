using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTreeV1
{
    public class NodeBranch
    {
        public int Number { get; set; }
        public double Pressure { get; set; }
        public TreeNode Node { get; set; }
        public List<CustomElement> Elements { get; set; }
        public List<TreeNode> Nodes { get; set; } = new List<TreeNode>();

        public NodeBranch()
        {
            Elements = new List<CustomElement>();
        }
        public NodeBranch (CustomBranch branch)
        {
            Elements = new List<CustomElement>();
            var elements = branch.Elements;
            Elements.AddRange(elements);
        }
        public void GetFirstNode()
        {
            var res = Elements.First();
            int index = Elements.IndexOf(res);
            Node = new TreeNode(res, index, Elements);

        }
        public void GetNodes()
        {
            Nodes.Add(Node);

            var res = Elements.Select(x => x).Where(x => x.DetailType ==CustomElement.Detail.Tee || x.DetailType == CustomElement.Detail.TapAdjustable || x.DetailType==CustomElement.Detail.DuctTap).ToList();
            foreach (var el in res)
            {
                int index = Elements.IndexOf(el);
                TreeNode treeNode = new TreeNode(el, index, Elements);
                treeNode.GetChildren();
                treeNode.GetNodePressure();
                Nodes.Add(treeNode);
            }

        }


    }
}
