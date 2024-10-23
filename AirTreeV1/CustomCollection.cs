using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public List<ElementId> ShowControlElements()
        {
            // Параметр number должен находиться в допустимом диапазоне
           /* if (number < 0 || number >= Collection.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "Значение number должно быть в пределах диапазона коллекции.");
            }*/

            List<ElementId> elements = new List<ElementId>();

            // Перебираем все ветви в указанной коллекции



            // Перебираем все элементы в текущей ветви
            foreach (var branch in Collection)
            {
                foreach (var element in branch.Elements)
                {
                    if (element != null) // проверяем, что элемент не null
                    {
                        elements.Add(element.ElementId);
                    }
                }
                
            }


            return elements;
        }
        public CustomCollection MarkCollection()
        {
            var branchWithMostElements = Collection
                .OrderByDescending(branch => branch.Elements?.Count ?? 0)
                .FirstOrDefault();

            if (branchWithMostElements == null || branchWithMostElements.Elements == null)
            {
                throw new Exception("No Branches!!!");
            }

            var mostElements = new HashSet<ElementId>();

            // Сохраняем ElementId элементов, находящихся в ветви с наибольшими элементами
            foreach (var element in branchWithMostElements.Elements)
            {
                mostElements.Add(element.ElementId);
            }

            // Создаем новую коллекцию для возвращаемых ветвей
            var newCollection =new CustomCollection(Document);

            // Проходим по всем ветвям коллекции
            foreach (var branch in Collection)
            {
                if (branch == branchWithMostElements || branch.Elements == null)
                {
                    newCollection.Add(branch); // Добавляем ветвь с наибольшими элементами в новую коллекцию
                    continue;
                }

                // Создаем новую коллекцию для элементов данной ветви
                var newElements = new CustomBranch(Document,branch.Elements.First().ElementId);

                // Добавляем только те элементы, которые не содержатся в mostElements
                foreach (var element in branch.Elements)
                {
                    if (!mostElements.Contains(element.ElementId))
                    {
                        newElements.Add(element);
                    }
                }

                // Создаем новую ветвь с отфильтрованными элементами и добавляем её в новую коллекцию
                newCollection.Add(newElements);
            }

            // Возвращаем новую коллекцию с изменениями
            return newCollection;
        }
    }
}
