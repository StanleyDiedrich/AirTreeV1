using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autodesk.Revit.DB;

namespace AirTreeV1
{
    public class CustomBranch
    {
        public Autodesk.Revit.DB.Document Document { get; set; }
        private static int _counter = 0;
        public int Number { get; set; }
        public double Pressure { get; set; }
        public double PBTot { get; set; }
        public bool IsStart { get; set; }
        public bool IsVisited { get; set; }
        public bool IsMain { get; set; }
        public List<CustomElement> Elements { get; set; } = new List<CustomElement>();
        public CustomBranch (Autodesk.Revit.DB.Document document, ElementId elementId)
        {
            Document = document;
            Number = _counter;
            _counter++;
        }
        public CustomBranch (Autodesk.Revit.DB.Document document)
        {
            Document = document;
            Number = _counter;
            _counter++;
        }
        /*public CustomBranch (Autodesk.Revit.DB.Document document)
        {
            Document = document;
        }*/
        public void Add (CustomElement customElement)
        {
            if (customElement != null)
            {
                // Находим индекс узла с таким же ElementId
                var existingNodeIndex = Elements.FindIndex(n => n.ElementId == customElement.ElementId);

                if (existingNodeIndex >= 0)
                {
                    // Если найден, заменяем существующий узел
                    Elements[existingNodeIndex] = customElement;
                }
                else
                {
                    // Если не найден, добавляем новый узел
                    Elements.Add(customElement);
                }
            }
        }
        public void AddSpecial (CustomElement customElement)
        {
            if (customElement != null)
            {
                Elements.Add(customElement);
            }
        }
        public void Remove(CustomElement customElement)
        {
            // Находим индекс узла с указанным ElementId
            var nodeIndex = Elements.FindIndex(n => n.ElementId == customElement.ElementId);

            if (nodeIndex >= 0)
            {
                // Если найден, удаляем узел
                Elements.RemoveAt(nodeIndex);
            }
        }
        public void AddRange(CustomBranch branch)
        {
            if (branch != null)
            {
                foreach (var node in branch.Elements)
                {
                    Add(node); // Использует метод Add, который уже включает логику уникальности
                }
            }
        }
        public void BranchCalc(int nextelement)
        {
            for (int i = 1; i < nextelement; i++)
            {


                Elements[i].Ptot = Elements[i].PDyn + Elements[i].PStat + Elements[i - 1].Ptot;


            }
        }
        public void BranchCalc()
        {

            for (int i = 1; i < Elements.Count; i++)
            {


                Elements[i].Ptot = Elements[i].PDyn + Elements[i].PStat + Elements[i - 1].Ptot;
               

            }
            PBTot = Elements.Last().Ptot;
        }
        public void CreateNewBranch(Document document, ElementId airterminal)
        {
            //int i = 0;
            ElementId nextElement=null;
            CustomElement customElement = new CustomElement(document, airterminal);
            do
            {
                
                Elements.Add(customElement);
                nextElement = customElement.NextElementId;
                customElement = new CustomElement(document, nextElement);
                //ВОТ ЭТУ ШТУКУ ДОБАВИЛ 04.02.25
                if (customElement.DetailType==CustomElement.Detail.TapAdjustable || customElement.DetailType == CustomElement.Detail.Tee)
                {
                    CustomElement customElement2 = new CustomElement(document, nextElement);
                    Elements.Add(customElement2);
                }

                    //ВОТ ЭТУ ШТУКУ ДОБАВИЛ 04.02.25
                //i++;
            }
            while (nextElement != null /*|| i==10000*/);

        }

        


        
    }
}
