using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AirTreeV1
{
    public class CustomBranch
    {
        private static int _counter = 0;
        public int Number { get; set; }
        public double Pressure { get; set; }
        public List<CustomElement> Elements { get; set; }

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

    }
}
