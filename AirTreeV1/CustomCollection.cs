using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
       

        public void  Calcualate()
        {
            foreach (var branch in Collection)
            {
                foreach (var element in branch.Elements)
                {
                    if (element.DetailType==CustomElement.Detail.AirTerminal)
                    {
                        branch.Pressure += 10;
                    }
                    else if (element.DetailType==CustomElement.Detail.Elbow)
                    {
                        branch.Pressure += 5;
                    }
                    else if (element.DetailType==CustomElement.Detail.Tee)
                    {
                        branch.Pressure += 7;
                    }
                    else if (element.DetailType==CustomElement.Detail.TapAdjustable)
                    {
                        branch.Pressure += 1;
                    }
                    else if (element.DetailType == CustomElement.Detail.Transition)
                    {
                        branch.Pressure += 2;
                    }
                    else if (element.DetailType==CustomElement.Detail.Duct)
                    {
                        branch.Pressure += element.Element.get_Parameter(BuiltInParameter.RBS_PRESSURE_DROP).AsDouble();
                    }
                    else if (element.DetailType==CustomElement.Detail.FireProtectValve)
                    {
                        branch.Pressure += 6;
                    }
                }
            }
        }

        public CustomBranch SelectMainBranch()
        {
            List<CustomBranch> branches = new List<CustomBranch>();
            foreach (var branch in Collection)
            {
                branches.Add(branch);
            }
            var maxbranch = branches.OrderByDescending(x => x.Pressure).FirstOrDefault();
            return maxbranch;
        }

        public void MarkCollection(CustomBranch customBranch)
        {
            List<CustomBranch> newCustomCollection = new List<CustomBranch>();
            foreach (var branch in Collection)
            {

                if (branch.Number == customBranch.Number)
                {
                    int trackcounter = 0;
                    foreach (var element in branch.Elements)
                    {
                        element.TrackNumber = trackcounter;
                        element.BranchNumber = branch.Number;
                        element.MainTrack = true;
                        trackcounter++;

                    }
                    newCustomCollection.Add(branch);
                }
            }
            foreach (var branch in Collection)
            {
                 
                    CustomBranch newcustomBranch = new CustomBranch(Document);
                    foreach (var element in branch.Elements)
                    {
                        if (customBranch.Elements.Select(x => x.ElementId).ToList().Contains(element.ElementId))
                        {
                            continue;
                        }
                    }
                    int trackcounter = 0;
                    foreach (var element in branch.Elements)
                    {
                        element.TrackNumber = trackcounter;
                        element.BranchNumber = branch.Number;
                        trackcounter++;
                        newcustomBranch.Add(element);
                    }
                    
                    newCustomCollection.Add(newcustomBranch);
                 
            }
            Collection = newCustomCollection;  
        }

         public    
        

        public List<ElementId> ShowElements()
        {
            List<ElementId> selectedelements = new List<ElementId>();
            foreach (var branch in Collection)
            {
                foreach (var element in branch.Elements)
                {
                    if (!selectedelements.Contains(element.ElementId))
                    {
                        selectedelements.Add(element.ElementId);
                    }
                }
            }
            return selectedelements;
        }
    }
}
