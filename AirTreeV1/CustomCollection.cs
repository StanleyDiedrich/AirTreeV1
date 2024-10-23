using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace AirTreeV1
{
    public  class CustomCollection
    {
        List<CustomBranch> Collection { get; set; } = new List<CustomBranch>();
        Autodesk.Revit.DB.Document Document { get; set; }

        public void Add (CustomBranch branch)
        {
            Collection.Add(branch);
        }

        public void CreateBranch(Document document,ElementId airterminal)
        {
            CustomBranch customBranch = new CustomBranch(Document, airterminal);
            customBranch.CreateNewBranch(Document, airterminal);
            //CustomElement customElement = new CustomElement(Document,  airterminal);
            Collection.Add(customBranch);

            
        }
        public CustomCollection (Autodesk.Revit.DB.Document doc)
        {
            Document = doc;
        }

        public List<ElementId> ShowElements(int number)
        {
            // Параметр number должен находиться в допустимом диапазоне
            if (number < 0 || number >= Collection.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "Значение number должно быть в пределах диапазона коллекции.");
            }

            List<ElementId> elements = new List<ElementId>();

            // Перебираем все ветви в указанной коллекции
            
                

                // Перебираем все элементы в текущей ветви
                foreach (var element in Collection[number].Elements)
                {
                    if (element != null) // проверяем, что элемент не null
                    {
                        elements.Add(element.ElementId);
                    }
                }
            

            return elements;
        }
    }
}
