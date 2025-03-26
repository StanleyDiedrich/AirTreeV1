using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Globalization;
using Autodesk.Revit.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
using Autodesk.Revit.DB.Structure;
using System.Xml.Linq;
using System.Windows;
using Autodesk.Revit.DB.Visual;
using System.Windows.Documents;
using System.Runtime.CompilerServices;

namespace AirTreeV1
{


    public class CustomCollection
    {
        public List<CustomBranch> Collection { get; set; } = new List<CustomBranch>();
        public List<CustomBranch> CollectionFootPrint { get; set; } = new List<CustomBranch>();
        public Autodesk.Revit.DB.Document Document { get; set; }
        public double Density { get; set; }
        public CustomElement ActiveElement { get; set; }
        public string FirstElement { get; set; }

        public string ErrorString { get; set; }

        public List<CustomElement> Tees { get; set; } = new List<CustomElement>();

        List<ElementId> CreatedElements { get; set; } = new List<ElementId>();
        public void Add(CustomBranch branch)
        {
            Collection.Add(branch);
        }

        public void CreateBranch(Document document, ElementId airterminal)
        {

            CustomBranch customBranch = new CustomBranch(Document, airterminal);
            customBranch.CreateNewBranch(Document, airterminal);
            customBranch.Number++;
            //CustomElement customElement = new CustomElement(Document,  airterminal);
            Collection.Add(customBranch);


        }
        public CustomCollection(Autodesk.Revit.DB.Document doc)
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


        public void Calcualate(double density)
        {
            HashSet<ElementId> checkedTees = new HashSet<ElementId>();
            HashSet<ElementId> checkedTaps = new HashSet<ElementId>();
            HashSet<ElementId> checkedDuctTaps = new HashSet<ElementId>();
            List<ElementId> checkedElements = new List<ElementId>();
            List<CustomBranch> newCollection = new List<CustomBranch>();
            IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "," };
            IFormatProvider formatter2 = new NumberFormatInfo { NumberDecimalSeparator = "." };
            Density = density;
            if (Collection.Count == 0)
            {
                TaskDialog.Show("AirTree", $"Система {Collection.First().Elements.First().SystemName} не имеет ни одного элемента");
                return;
            }
            else
            {
                foreach (var branch in Collection)
                {
                    int branchnnumber = branch.Number;
                   
                    
                    foreach (var element in branch.Elements)
                    {
                        try
                        {
                            if (element.Element == null)
                            {
                                continue;
                            }
                            
                            if (element.DetailType == CustomElement.Detail.AirTerminal)
                            {
                                try
                                {
                                    if (element.ElementId.IntegerValue == 659023)
                                    {
                                        var element2 = element;
                                    }

                                    CustomAirTerminal customAirTerminal = new CustomAirTerminal(Document, element);
                                   
                                    element.PDyn = customAirTerminal.PDyn;
                                    element.Ptot = customAirTerminal.PDyn;
                                  
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                   
                                }
                              
                            }
                            else if (element.DetailType == CustomElement.Detail.Elbow)
                            {
                                try
                                {
                                    if (element.ElementId.IntegerValue == 22017547)
                                    {
                                        var element2 = element;
                                    }
                                    CustomElbow customElbow = new CustomElbow(Document, element);
                                    element.LocRes = customElbow.LocRes;
                                    element.PDyn = Density * Math.Pow(customElbow.Velocity, 2) / 2 * element.LocRes;
                                    
                                    
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                    
                                }
                            }
                            else if (element.DetailType==CustomElement.Detail.DuctTap)
                            {
                               /* if (element.ElementId.IntegerValue == 10562850)
                                {
                                    var element2 = element;
                                }
                                try
                                {
                                    
                                    foreach (var branch2 in Collection)
                                    {
                                        foreach (var el in branch2.Elements)
                                        {
                                            if (element.TapId == el.ElementId)
                                            {
                                                CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(Document, el, Collection, false);
                                                UpdateInsertElementProperties(element, customDuctInsert2);
                                                checkedDuctTaps.Add(el.ElementId);
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                   
                                }*/

                            }
                            else if (element.DetailType == CustomElement.Detail.Tee)
                            {
                               /* try
                                {
                                    if (element.ElementId.IntegerValue == 644205)
                                    {
                                        var element2 = element;
                                    }
                                    CustomTee2 customTee2 = new CustomTee2(Document, element, Collection, false);
                                    UpdateElementProperties(element, customTee2);
                                    checkedTees.Add(element.ElementId);
                                    Tees.Add(element);
                                   
                                    

                                    
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                    
                                }*/

                            }
                            else if (element.DetailType == CustomElement.Detail.Equipment)
                            {
                                try
                                {
                                    element.LocRes = 0;
                                    element.PDyn = 0;
                                   
                                  
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                  
                                }
                            }
                            else if (element.DetailType == CustomElement.Detail.Multiport)
                            {
                                try
                                {
                                    if (element.ElementId.IntegerValue == 8968461)
                                    {
                                        var element2 = element;
                                    }
                                    CustomMultiport customElbow = new CustomMultiport(Document, element);
                                    element.LocRes = customElbow.LocRes;
                                    element.PDyn = Density * Math.Pow(customElbow.Velocity, 2) / 2 * element.LocRes;
                                    
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                    //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                                }
                            }

                           else if (element.DetailType == CustomElement.Detail.TapAdjustable)
                            {
                                /*if (element.ElementId.IntegerValue == 10562871)
                                {
                                    var el = element;
                                }
                                CustomDuctInsert2 customDuctInsert = new CustomDuctInsert2(Document, element, Collection, false);
                                UpdateInsertElementProperties(element, customDuctInsert);
                                checkedTaps.Add(element.ElementId);*/
                                
                            }
                            else if (element.DetailType == CustomElement.Detail.Transition)
                            {
                                if (element.ElementId.IntegerValue == 10562995)
                                {
                                    var element2 = element;
                                }
                                try
                                {
                                    CustomTransition customTransition = new CustomTransition(Document, element);

                                    element.LocRes = customTransition.LocRes;
                                    element.PDyn = Density * Math.Pow(customTransition.Velocity, 2) / 2 * element.LocRes;
                                    
                                   
                                }
                                catch
                                {
                                    CustomTransition customTransition = new CustomTransition(Document, element);
                                    ActiveElement = element;
                                    element.LocRes = 0.11;
                                    element.PDyn = Density * Math.Pow(customTransition.Velocity, 2) / 2 * element.LocRes;
                                   
                                    
                                }

                            }
                            else if (element.DetailType == CustomElement.Detail.RectangularDuct || element.DetailType == CustomElement.Detail.RoundDuct)
                            {
                                if (element.ElementId.IntegerValue == 644200)
                                {
                                    var element2 = element;
                                }
                               
                                string[] pressureDropString = element.Element.get_Parameter(BuiltInParameter.RBS_PRESSURE_DROP).AsValueString().Split();
                                try
                                {

                                    element.PStat = double.Parse(pressureDropString[0], formatter);
                                    
                                   
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    element.PStat = double.Parse(pressureDropString[0], formatter2);
                                   
                                   
                                }
                                // Проверяем, что строка не пустая или null

                            }
                            else if (element.DetailType == CustomElement.Detail.RectFlexDuct || element.DetailType == CustomElement.Detail.RoundFlexDuct)
                            {
                                //branch.Pressure += element.Element.get_Parameter(BuiltInParameter.RBS_PRESSURE_DROP).AsDouble();
                                string[] pressureDropString = element.Element.get_Parameter(BuiltInParameter.RBS_PRESSURE_DROP).AsValueString().Split();
                                element.PStat = double.Parse(pressureDropString[0], formatter);
                                
                                //branch.Pressure += element.PStat;

                            }
                            else if (element.DetailType == CustomElement.Detail.FireProtectValve)
                            {
                                if (element.ElementId.IntegerValue == 20659396)
                                {
                                    var element2 = element;
                                }
                                CustomValve customValve = new CustomValve(Document, element);
                                
                              
                                branch.Pressure += element.PDyn;
                            }
                            else if (element.DetailType == CustomElement.Detail.Union)
                            {
                                
                                branch.Pressure += 0;
                            }
                        }
                        catch
                        {
                            //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                            ActiveElement = element;
                            ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                            //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                        }
                        
                    }
                  
                    branch.Number++;
                }

                FirstElement = Collection.First().Elements.First().SystemName;
            }
            foreach (var branch in Collection)
            {
                foreach (var el in branch.Elements)
                {
                    el.BranchNumber = branch.Number;
                }
                branch.BranchCalc();
            }
            
           
        }

        public void BranchCalculate(CustomBranch branch)
        {
            
            for (int i = 1; i < branch.Elements.Count; i++)
            {


                branch.Elements[i].Ptot = branch.Elements[i].PDyn + branch.Elements[i].PStat + branch.Elements[i - 1].Ptot;

            }
            branch.PBTot = branch.Elements.Last().Ptot;
        }


        private bool FindPrevious(CustomElement element, CustomBranch branch)
        {
            int index = branch.Elements.IndexOf(element);

            // Проверяем, найден ли элемент и есть ли предыдущий элемент
            if (index > 0)
            {
                var previousElement = branch.Elements[index - 1];

                // Сравниваем ID и проверяем поле IsReversed
                if (previousElement.ElementId == element.ElementId)
                {
                    return true;
                }
            }

            return false; // Если элемент не найден или предыдущего элемента нет
        }










        public CustomElement FindNext(CustomBranch selectedBranch, List<CustomBranch> collection )
        {
            if(selectedBranch.Elements.Last().ElementId==null)
            { return null; }
            CustomElement lastElement = null;
            foreach (var branch in collection)
            
                if (branch.IsVisited==false)
                {
                    if (selectedBranch.Elements.Last().NextElementId == branch.Elements.First().ElementId)
                    {
                        branch.IsVisited = true;
                        selectedBranch.AddRange(branch);
                        selectedBranch.BranchCalc();
                        lastElement = selectedBranch.Elements.Last();

                        foreach (var nbranch in Collection)
                        {
                            if (nbranch.IsVisited==false)
                            {
                                if (lastElement.NextElementId == nbranch.Elements.First().ElementId)
                                {
                                    selectedBranch.AddRange(nbranch);
                                    selectedBranch.BranchCalc();
                                    lastElement = nbranch.Elements.Last();


                                    foreach (var element in Tees)
                                    {
                                        if (element.ElementId.IntegerValue== 644206)
                                        {
                                            var element2 = element;
                                        }
                                        if (lastElement.NextElementId==element.ElementId)
                                        {

                                            if (element.DetailType == CustomElement.Detail.Tee)
                                            {
                                                CustomTee2 customTee = new CustomTee2(Document, element, Collection, false);
                                                element.IA = customTee.IA;
                                                element.IQ = customTee.IQ;
                                                element.IC = customTee.IC;
                                                element.O1A = customTee.O1A;
                                                element.O1Q = customTee.O1Q;
                                                element.O1C = customTee.O1C;
                                                element.O2A = customTee.O2A;
                                                element.O2Q = customTee.O2Q;
                                                element.RA = customTee.RA;
                                                element.RQ = customTee.RQ;
                                                element.RC = customTee.RC;

                                                element.LocRes = customTee.LocRes;
                                                element.PDyn = Density * Math.Pow(customTee.Velocity, 2) / 2 * element.LocRes;
                                                //checkedIds.Add(element.ElementId);
                                            }
                                            if (element.DetailType == CustomElement.Detail.TapAdjustable)
                                            {
                                                CustomDuctInsert2 customDuctInsert = new CustomDuctInsert2(Document, element, Collection, false);
                                                element.IA = customDuctInsert.IA;
                                                element.IQ = customDuctInsert.IQ;
                                                element.IC = customDuctInsert.IC;
                                                element.O1A = customDuctInsert.O1A;
                                                element.O1Q = customDuctInsert.O1Q;
                                                element.O1C = customDuctInsert.O1C;
                                                element.O2A = customDuctInsert.O2A;
                                                element.O2Q = customDuctInsert.O2Q;
                                                element.RA = customDuctInsert.RA;
                                                element.RQ = customDuctInsert.RQ;
                                                element.RC = customDuctInsert.RC;
                                                element.LocRes = customDuctInsert.LocRes;
                                                element.PDyn = Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * element.LocRes;
                                                //checkedIds.Add(element.ElementId);
                                            }
                                            selectedBranch.Add(element);
                                            selectedBranch.BranchCalc();
                                            lastElement = element;
                                            return lastElement;
                                        }
                                    }
                                    //return lastElement;
                                }
                            }
                        }
                    }
                }

            return null;
           
        }

        /*public CustomBranch FindLoadedBranch ()*/
        public CustomBranch SelectMainBranch()
        {
            List<CustomBranch> branches = new List<CustomBranch>();
            foreach (var branch in Collection)
            {
                branches.Add(branch);
            }
            var maxbranch = branches.OrderByDescending(x => x.PBTot).FirstOrDefault();
            return maxbranch;
        }

        public void MarkCollection()
        {
            List<CustomBranch> newCustomCollection = new List<CustomBranch>();
            HashSet<ElementId> checkedElements = new HashSet<ElementId>();
            
            // Сначала обрабатываем основную ветвь 
            /*foreach (var branch in Collection)
            {
                if (branch.Number == customBranch.Number)
                {
                    int trackCounter = 0;
                    foreach (var element in branch.Elements)
                    {
                        element.TrackNumber = trackCounter;
                        element.BranchNumber = branch.Number;
                        element.MainTrack = true;
                        checkedElements.Add(element.ElementId);
                        trackCounter++;
                    }
                    newCustomCollection.Add(branch);
                    break; // Прекращаем дальнейший обход после нахождения основной ветви 
                }
            }*/

            //Вписываем новую фичу
            /* List<CustomBranch> newCollection = new List<CustomBranch>();

             newCollection = Collection.OrderByDescending(x => x.Pressure).ToList();


             foreach (var branch in newCollection)
             {
                 if (branch.Number == customBranch.Number)
                 {
                     continue;
                 }

                 CustomBranch newCustomBranch = new CustomBranch(Document);
                 int trackCounter = 0;

                 foreach (var element in branch.Elements)
                 {
                     // Если элемент уже есть в основной ветви, пропускаем его 
                     if (checkedElements.Contains(element.ElementId))
                     {
                         continue;
                     }

                     // Устанавливаем номера и добавляем элемент в новую ветвь 
                     element.TrackNumber = trackCounter;
                     element.BranchNumber = branch.Number;
                     newCustomBranch.Add(element);
                     checkedElements.Add(element.ElementId);
                     trackCounter++;  // Увеличиваем trackCounter только после успешного добавления элемента
                 }

                 newCustomCollection.Add(newCustomBranch);
             }*/



            // Обрабатываем остальные ветви 
            foreach (var branch in Collection)
            {
                /*if (branch.Number == customBranch.Number)
                {
                    continue;
                }*/

                CustomBranch newCustomBranch = new CustomBranch(Document);
                int trackCounter = 0;

                foreach (var element in branch.Elements)
                {
                    // Если элемент уже есть в основной ветви, пропускаем его 
                    if (element.ElementId.IntegerValue == 644209)
                    {
                        var element2 = element;
                    }
                    /*if (checkedElements.Contains(element.ElementId))
                    {*/


                            CustomElement newelement = new CustomElement(Document, element.ElementId);
                            if (
                           element.DetailType == CustomElement.Detail.RoundTeeBranch ||
                           element.DetailType == CustomElement.Detail.RoundTeeStraight ||
                           element.DetailType == CustomElement.Detail.RectTeeBranch ||
                           element.DetailType == CustomElement.Detail.RectTeeStraight ||
                           element.DetailType == CustomElement.Detail.RectRoundTeeBranch ||
                           element.DetailType == CustomElement.Detail.RectRoundTeeStraight)


                            {
                                CustomTee2 customDuctInsert = new CustomTee2(Document, newelement, Collection, true);
                                newelement.LocRes = customDuctInsert.LocRes;
                                newelement.PDyn = Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * newelement.LocRes;
                                newelement.TrackNumber = trackCounter;
                                newelement.BranchNumber = branch.Number;
                                newCustomBranch.AddSpecial(newelement);
                                //checkedElements.Add(newelement.ElementId);
                                trackCounter++;


                            }
                            if (

                           element.DetailType == CustomElement.Detail.RectInRectDuctInsertBranch ||
                           element.DetailType == CustomElement.Detail.RectInRectDuctInsertStraight ||
                           element.DetailType == CustomElement.Detail.RoundInRoundDuctInsertStraight ||
                           element.DetailType == CustomElement.Detail.RoundInRoundDuctInsertBranch ||
                           element.DetailType == CustomElement.Detail.RoundInRectDuctInsertStraight ||
                           element.DetailType == CustomElement.Detail.RoundInRectDuctInsertBranch ||
                           element.DetailType == CustomElement.Detail.RectInRectDuctInsertStraight ||
                           element.DetailType == CustomElement.Detail.RectInRoundDuctInsertStraight ||
                           element.DetailType == CustomElement.Detail.RectInRoundDuctInsertBranch)
                            {
                                CustomDuctInsert2 customDuctInsert = new CustomDuctInsert2(Document, newelement, Collection, true);
                                newelement.LocRes = customDuctInsert.LocRes;
                                newelement.PDyn = Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * newelement.LocRes;
                                newelement.TrackNumber = trackCounter;
                                newelement.BranchNumber = branch.Number;
                                newCustomBranch.AddSpecial(newelement);
                                //checkedElements.Add(newelement.ElementId);
                                trackCounter++;

                            }
                    



                        //}
                        //break;

                        else
                    {
                        element.TrackNumber = trackCounter;
                        element.BranchNumber = branch.Number;
                        newCustomBranch.Add(element);
                        //checkedElements.Add(element.ElementId);
                        trackCounter++;
                    }




                   /* if (checkedElements.Contains(element.ElementId))
                    {
                        // Проверяем DetailType на соответствие списку значений
                        if (element.DetailType == CustomElement.Detail.RectInRectDuctInsertBranch ||
                            element.DetailType == CustomElement.Detail.RectInRectDuctInsertStraight ||
                            element.DetailType == CustomElement.Detail.RoundTeeBranch ||
                            element.DetailType == CustomElement.Detail.RoundTeeStraight ||
                            element.DetailType == CustomElement.Detail.RectTeeBranch ||
                            element.DetailType == CustomElement.Detail.RectTeeStraight ||
                            element.DetailType == CustomElement.Detail.RectRoundTeeBranch ||
                            element.DetailType == CustomElement.Detail.RectRoundTeeStraight ||
                            element.DetailType == CustomElement.Detail.RoundInRoundDuctInsertStraight ||
                            element.DetailType == CustomElement.Detail.RoundInRoundDuctInsertBranch ||
                            element.DetailType == CustomElement.Detail.RoundInRectDuctInsertStraight ||
                            element.DetailType == CustomElement.Detail.RoundInRectDuctInsertBranch ||
                            element.DetailType == CustomElement.Detail.RectInRectDuctInsertStraight ||
                            element.DetailType == CustomElement.Detail.RectInRoundDuctInsertStraight ||
                            element.DetailType == CustomElement.Detail.RectInRoundDuctInsertBranch)
                        {
                            element.TrackNumber = trackCounter;
                            element.BranchNumber = branch.Number;
                            newCustomBranch.AddSpecial(element);
                            //checkedElements.Add(element.ElementId);
                            trackCounter++;  // Увеличиваем trackCounter только после успешного добавления элемента
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // Устанавливаем номера и добавляем элемент в новую ветвь 
                        element.TrackNumber = trackCounter;
                        element.BranchNumber = branch.Number;
                        newCustomBranch.AddSpecial(element);
                        //checkedElements.Add(element.ElementId);
                        trackCounter++;  // Увеличиваем trackCounter только после успешного добавления элемента
                    }*/

                    
                }

                branch.Number++;
                newCustomBranch.Number = branch.Number;
                newCustomCollection.Add(newCustomBranch);
                
            }

            // Обновляем коллекцию 
            Collection = newCustomCollection;
        }
       
        private void ProcessSelectedBranch(CustomBranch selectedBranch, ref double pressure1, ref int selectedEnd, ref ElementId elementId)
        {
            for (int i = 0; i < selectedBranch.Elements.Count; i++)
            {
                var element = selectedBranch.Elements[i];

                // Проверка элемента по элементу ID
                if (element.ElementId.IntegerValue == 644211)
                {
                    var element2 = element; // Возможно, вы хотите это сохранить для дальнейшего использования
                }

                // Проверяем наличие элемента Tee
                if (element.DetailType == CustomElement.Detail.Tee)
                {
                    pressure1 = selectedBranch.Elements[i - 1].Ptot;
                    elementId = element.ElementId;

                    CustomTee2 customTee = new CustomTee2(Document, element, Collection, false);
                    UpdateElementProperties(element, customTee);

                    selectedBranch.BranchCalc();
                    pressure1 = selectedBranch.Elements[i - 1].Ptot;
                    selectedEnd = i + 1;
                    break;
                }
            }
        }

        private void FindCorrectBranch(ref int minimalIndex, ref int correctBranch, ElementId elementId, CustomBranch researchedBranch)
        {
            for (int k = 0; k < Collection.Count; k++)
            {
                if (Collection[k] == researchedBranch || Collection[k].IsVisited)
                {
                    continue;
                }

                // Находим элемент с заданным ElementId
                if (Collection[k].Elements.Any(x => x.ElementId == elementId))
                {
                    int foundedIndex = Collection[k].Elements.FindIndex(x => x.ElementId == elementId);

                    if (foundedIndex < minimalIndex)
                    {
                        minimalIndex = foundedIndex;
                        correctBranch = k;
                    }
                }
            }
        }

        private void UpdateCollectionElements(int correctBranch, int minimalIndex)
        {
            CustomElement element2 = Collection[correctBranch].Elements[minimalIndex];
            CustomTee2 customTee2 = new CustomTee2(Document, element2, Collection, true);

            UpdateElementProperties(element2, customTee2);
            Collection[correctBranch].BranchCalc();
        }

        public void ReorderBranches (List<CustomBranch> sortedBranches)
        {
            List<ElementId> visitedElements = new List<ElementId>();
            List<CustomBranch> newCollection = new List<CustomBranch>();

            foreach (var branch in sortedBranches)
            {
                int tracknumber = 0;
                CustomBranch customBranch = new CustomBranch(Document);
                foreach (var el in branch.Elements)
                {
                    if (!visitedElements.Contains(el.ElementId))
                    {
                        visitedElements.Add(el.ElementId);
                        customBranch.Add(el);
                        el.TrackNumber=tracknumber;
                    }
                    else
                    {
                        if(el.DetailType == CustomElement.Detail.Tee ||
                            el.DetailType==CustomElement.Detail.TapAdjustable ||
                            el.DetailType== CustomElement.Detail.DuctTap)
                        {
                            visitedElements.Add(el.ElementId);
                            customBranch.Add(el);
                            el.TrackNumber=tracknumber;
                        }
                        else
                        {
                            break;
                        }
                        
                    }

                    tracknumber++;
                }
                newCollection.Add(customBranch);
            }
            CollectionFootPrint = newCollection;
        }

        public void CalculateReorderedBranches ()
        {

            CustomBranch selectedBranch = null;
            ElementId lastelementId = null;
            ElementId currentelementId = null;
            ElementId nextelementId = null;
            ElementId prevelementId = null;
            for (int i = CollectionFootPrint.Count - 1; i > -1; i--)
            {
                selectedBranch = CollectionFootPrint[i];
                lastelementId = selectedBranch.Elements.Last().ElementId;

                foreach (var el in selectedBranch.Elements)
                {
                    if (el.ElementId.IntegerValue == 10520945)
                    {
                        var elem = el;
                    }
                }
               
                ProcessBranch(selectedBranch);

                
            }
            //MarkElements();
            
        }

        private void MarkElements()
        {
            ElementId nextelement= null;

            CustomBranch selectedBranch = Collection.OrderBy(x=>x.Elements.Last().Ptot).First(x=>x.IsMain);
            do
            {
                int branchnumber = selectedBranch.Elements.First().BranchNumber;
                nextelement = selectedBranch.Elements.Last().NextElementId;
                CustomElement elem = selectedBranch.Elements.Last();


                CustomBranch foundedBranch = SelectTeeBranch(elem);
                if (foundedBranch!=null)
                {
                    int index = GetTee(foundedBranch, elem);


                    for (int i = 0; i < index; i++)
                    {
                        foundedBranch.Elements[i].MainTrack = false;
                    }
                    for (int i = index; i < foundedBranch.Elements.Count - 1; i++)
                    {
                        foundedBranch.Elements[i].MainTrack = true;
                    }

                    if (nextelement != null)
                    {
                        foreach (var branch in Collection)
                        {
                            int branchnumber2 = branch.Elements.First().BranchNumber;
                            if (branchnumber != branchnumber2)
                            {
                                selectedBranch = branch;
                                break;
                                //test
                            }
                        }
                    }
                }
                else
                {
                    break;
                }
               
            }
            while (nextelement != null);
            


        }

        private void ProcessBranch(CustomBranch selectedBranch)
        {
            int counter = 0;
            int pindex1 = 0;
            int pindex2 = 0;
            do
            {
                int index1 = GetSplitter(selectedBranch);
                if (index1 != -1)
                {
                    CustomElement element1 = selectedBranch.Elements[index1];
                    /*if (selectedBranch.Number == 25)
                    {
                        var br = selectedBranch;
                    }*/
                    if (element1.ElementId.IntegerValue == 10520898)
                    {
                        var el = element1;
                    }
                    if (element1.BranchNumber == 25)
                    {
                        var el = element1;
                    }
                    CustomBranch foundedBranch = SelectBranch(element1);
                    if (foundedBranch != null)
                    {
                        int index2 = GetSplitterId(foundedBranch, element1);
                        if (index2 != -1)
                        {
                            CustomElement element2 = foundedBranch.Elements[index2];
                            if (selectedBranch.Elements[index1].DetailType.ToString().Contains("TapAdjustable")&& selectedBranch.Elements[index1-1].DetailType.ToString().Contains("Insert") )
                            {
                                if (selectedBranch.Elements[index1 - 1].TapId!=null)
                                {
                                    selectedBranch.Elements[index1 - 1].DetailType = CustomElement.Detail.DuctTap;
                                    selectedBranch.Elements[index1 - 1].IsReversed = true;
                                    RecalculateElement(selectedBranch.Elements[index1 - 1], element1);
                                    double pressure1 = ElementsPressure(selectedBranch, index1-1);
                                    double pressure2 = ElementsPressure(selectedBranch, index1);
                                }
                            }
                            else
                            {
                                RecalculateElement(element1, element2);
                                double pressure1 = ElementsPressure(selectedBranch, index1);
                                double pressure2 = ElementsPressure(foundedBranch, index2);
                            }
                           
                           
                            
                        }
                        else
                        {
                            break;
                        }

                    }
                }
                counter++;
            }
            while (counter<selectedBranch.Elements.Count-1);
        }

        private void IsMarkedAsMain(CustomBranch selectedBranch, int index)
        {
            CustomElement lastElement = selectedBranch.Elements.Last();
            for (int i = 0; i < index; i++)
            {

                CustomElement element = selectedBranch.Elements[i];
                if (element.IsVisited == true)
                {
                    element.MainTrack = true;
                }
                else
                {
                    continue;
                }


            }
            
        }

        private int GetTee(CustomBranch foundedBranch, CustomElement element1)
        {
            for (int i = 0; i < foundedBranch.Elements.Count; i++)
            {
                CustomElement element = foundedBranch.Elements[i];

                // Проверка на тип Tee
                if (element1.DetailType.ToString().Contains("Tee"))
                {
                    if (element.ElementId.IntegerValue == element1.ElementId.IntegerValue)
                    {
                        return i; // Возвращаем индекс, если нашли соответствующий элемент
                    }
                }

                if (element1.DetailType.ToString().Contains("Insert"))
                {
                    if (element.ElementId.IntegerValue == element1.NextElementId.IntegerValue || element.ElementId.IntegerValue == element1.TapId.IntegerValue)
                    {
                        return i; // Возвращаем индекс, если нашли соответствующий элемент
                    }
                }
            }
            return -1;
        }
        private int GetSplitterId(CustomBranch foundedBranch, CustomElement element1)
        {
            for (int i = 0; i < foundedBranch.Elements.Count; i++)
            {
                CustomElement element = foundedBranch.Elements[i];

                // Проверка на тип Tee
                if (element1.DetailType == CustomElement.Detail.Tee)
                {
                    if (element.ElementId.IntegerValue == element1.ElementId.IntegerValue)
                    {
                        return i; // Возвращаем индекс, если нашли соответствующий элемент
                    }
                }

                // Проверка на TapAdjustable
                if (element1.DetailType == CustomElement.Detail.TapAdjustable)
                {
                    if (element.ElementId.IntegerValue == element1.NextElementId.IntegerValue)
                    {
                        return i; // Возвращаем индекс, если нашли соответствующий элемент
                    }
                }

                // Проверка на DuctTap
                if (element1.DetailType == CustomElement.Detail.DuctTap)
                {
                    if (element.ElementId.IntegerValue == element1.TapId.IntegerValue)
                    {
                        return i; // Возвращаем индекс, если нашли соответствующий элемент
                    }
                }

                
            }

            return -1; // Рекомендуется возвращать -1, если ничего не найдено
        }


        private void RecalculateElement(CustomElement element1, CustomElement element2)
        {
            if (element1.DetailType == CustomElement.Detail.Tee && element2.DetailType== CustomElement.Detail.Tee)
            {
                CustomTee2 customTee1 = new CustomTee2(Document, element1, Collection, false);
                CustomTee2 customTee2 = new CustomTee2(Document, element2, Collection, element1.IsReversed);
                UpdateElementProperties(element1, customTee1);
                UpdateElementProperties(element2, customTee2);
            }
            if ((element1.DetailType ==CustomElement.Detail.DuctTap) && (element2.DetailType == CustomElement.Detail.TapAdjustable))
            {
                try
                {
                    bool IsSecondPart = GetSecondPart(element1,element2);
                    /*if (IsSecondPart == true)
                    {
                        CustomDuctInsert2 customDuctInsert1 = new CustomDuctInsert2(Document, element2, Collection, true);
                        CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(Document, element2, Collection, element2.IsReversed);
                        UpdateInsertElementProperties(element1, customDuctInsert1);
                        UpdateInsertElementProperties(element2, customDuctInsert2);
                    }
                    else
                    {*/
                        CustomDuctInsert2 customDuctInsert1 = new CustomDuctInsert2(Document, element2, Collection, true);
                        CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(Document, element2, Collection, element2.IsReversed);
                        UpdateInsertElementProperties(element1, customDuctInsert1);
                        UpdateInsertElementProperties(element2, customDuctInsert2);
                    var filteredElements =
                Collection.Select(x => x)
               .Where(x => x.Elements.First().BranchNumber == element2.BranchNumber)
               .SelectMany(x => x.Elements)
               .Where(x => x.ElementId.IntegerValue == element1.ElementId.IntegerValue);
                }
                catch
                {

                }
               
                /*int element1BranchNumber = element1.BranchNumber;
                int res = 0;
                foreach (var branch in Collection)
                {
                    foreach (var el in branch.Elements)
                    {
                        if (el.ElementId.IntegerValue == element2.ElementId.IntegerValue)
                        {
                            if (el.DetailType.ToString().Contains("Insert"))
                            {
                                res = -1;
                                break;
                            }
                            else
                            {
                                res = 0;
                                
                            }
                        }
                       
                    }
                    if (res==1)
                    {
                        break;
                    }
                }*/



                /*  if (res == 0)
                  {
                      CustomDuctInsert2 customDuctInsert1 = new CustomDuctInsert2(Document, element2, Collection, false);
                      CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(Document, element2, Collection, element2.IsReversed);
                      UpdateInsertElementProperties(element1, customDuctInsert1);
                      UpdateInsertElementProperties(element2, customDuctInsert2);
                  }
                  else if (res==-1)
                  {
                      CustomDuctInsert2 customDuctInsert1 = new CustomDuctInsert2(Document, element2, Collection, true);
                      CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(Document, element2, Collection, element2.IsReversed);
                      UpdateInsertElementProperties(element1, customDuctInsert1);
                      UpdateInsertElementProperties(element2, customDuctInsert2);
                  }
                  else
                  {
                      CustomDuctInsert2 customDuctInsert1 = new CustomDuctInsert2(Document, element2, Collection, true);
                      CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(Document, element2, Collection, element2.IsReversed);
                      UpdateInsertElementProperties(element1, customDuctInsert1);
                      UpdateInsertElementProperties(element2, customDuctInsert2);
                  }*/






            }
            if (element1.DetailType == CustomElement.Detail.TapAdjustable && element2.DetailType == CustomElement.Detail.DuctTap)
            {
                CustomDuctInsert2 customDuctInsert1 = new CustomDuctInsert2(Document, element1, Collection, false);
                CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(Document, element1, Collection, element2.IsReversed);
                UpdateInsertElementProperties(element1, customDuctInsert1);
                UpdateInsertElementProperties(element2, customDuctInsert2);
            }
        }

        private bool GetSecondPart(CustomElement element1, CustomElement element2)
        {
            bool secondPart = false;
            int branchnumber = element1.BranchNumber;
            int sectionNumber = 0;
            CustomBranch selectedBranch = null;
            foreach (var branch in Collection)
            {
                if (element1.BranchNumber == branch.Elements.First().BranchNumber)
                {
                    for (int i =0; i<branch.Elements.Count;i++)
                    {
                        sectionNumber = i;
                        selectedBranch = branch;
                    }
                }
                
            }
            

            if (selectedBranch.Elements[sectionNumber-1].DetailType.ToString().Contains("Insert"))
            {
                secondPart = true;
                return secondPart;
            }
            return secondPart;


        }

        private double ElementsPressure(CustomBranch selectedBranch, int index)
        {
           int cindex = index + 1;
            double pressure = 0;
            if (selectedBranch.Elements[index].ElementId.IntegerValue == 10556356)
            {
                var el = selectedBranch.Elements[index];
            }
           for (int i=0; i<cindex;i++)
           {
                pressure += selectedBranch.Elements[i].PDyn + selectedBranch.Elements[i].PStat;
                if (i!=0)
                {
                    
                    if (selectedBranch.Elements[i].DetailType.ToString().Contains("Insert"))
                    {
                        if (selectedBranch.Elements[i-1].DetailType.ToString().Contains("Insert"))
                        {
                            selectedBranch.Elements[i-1].Ptot = selectedBranch.Elements[i - 2].Ptot;
                        }
                        
                    }
                    else
                    {
                        selectedBranch.Elements[i].Ptot = selectedBranch.Elements[i - 1].Ptot + selectedBranch.Elements[i].PStat + selectedBranch.Elements[i].PDyn;
                    }


                }
                
           }
            return pressure;
        }

        private CustomBranch SelectBranch(CustomElement element)
        {
            if (element.ElementId.IntegerValue == 10520886)
            {
                var el = element;
            }
            if (element.DetailType == CustomElement.Detail.Tee)
            {
                for (int i =0; i<Collection.Count; i++)
                {
                    if (Collection[i].Elements.First().BranchNumber!= element.BranchNumber)
                    {
                        CustomBranch customBranch = Collection[i];
                        foreach (var el in customBranch.Elements)
                        {
                            if (el.ElementId.IntegerValue == element.ElementId.IntegerValue)
                            {
                                return customBranch;
                            }
                        }

                    }
                    if (Collection[i].Elements.First().BranchNumber == element.BranchNumber)
                    {
                        CustomBranch customBranch = Collection[i];
                        foreach (var el in customBranch.Elements)
                        {
                            if (el.ElementId.IntegerValue == element.ElementId.IntegerValue)
                            {
                                return customBranch;
                            }
                        }

                    }
                }


               
            }

            if (element.DetailType== CustomElement.Detail.TapAdjustable)
            {
                int elementBranchNumber = GetElementBranchNumber(element);
                int selectedBranchNumber = element.BranchNumber;








                for (int i = 0; i < Collection.Count; i++)
                {

                    if (Collection[i].Elements.First().BranchNumber == elementBranchNumber)
                    {
                        CustomBranch customBranch = Collection[i];
                        foreach (var el in customBranch.Elements)
                        {
                            if (el.ElementId.IntegerValue == element.NextElementId.IntegerValue)
                            {
                                return customBranch;
                            }
                        }

                    }
                    else if (Collection[i].Elements.First().BranchNumber != elementBranchNumber)
                    {
                        CustomBranch customBranch = Collection[i];
                        foreach (var el in customBranch.Elements)
                        {
                            if (el.ElementId.IntegerValue == element.NextElementId.IntegerValue)
                            {
                                return customBranch;
                            }
                        }

                    }

                }




            }
            if (element.DetailType == CustomElement.Detail.DuctTap)
            {

                int elementBranchNumber = GetElementBranchNumber(element) ;
                int selectedBranchNumber = element.BranchNumber;

                

                

                


                for (int i = 0; i < Collection.Count; i++)
                {
                    
                    if (Collection[i].Elements.First().BranchNumber == elementBranchNumber)
                    {
                        CustomBranch customBranch = Collection[i];
                        foreach (var el in customBranch.Elements)
                        {
                            if (el.ElementId.IntegerValue == element.TapId.IntegerValue)
                            {
                                return customBranch;
                            }
                        }

                    }
                    else if (Collection[i].Elements.First().BranchNumber != elementBranchNumber)
                    {
                        CustomBranch customBranch = Collection[i];
                        foreach (var el in customBranch.Elements)
                        {
                            if (el.ElementId.IntegerValue == element.TapId.IntegerValue)
                            {
                                return customBranch;
                            }
                        }

                    }
                    
                }
               
                   
                
               
                
                
            }
            return null;
        }

        private int GetElementBranchNumber(CustomElement element)
        {
            int selectedBranchNumber = -1;
            foreach (var branch in Collection)
            {
                if (branch.Elements.First().BranchNumber == selectedBranchNumber)
                {
                    foreach (var el in branch.Elements)
                    {
                        if (el.ElementId.IntegerValue == element.NextElementId.IntegerValue)
                        {
                            selectedBranchNumber = el.BranchNumber;
                            return selectedBranchNumber;
                        }
                    }
                }
                else
                {
                    foreach (var el in branch.Elements)
                    {
                        if (el.ElementId.IntegerValue == element.NextElementId.IntegerValue)
                        {
                            selectedBranchNumber = el.BranchNumber;
                            return selectedBranchNumber;
                        }
                    }
                }

            }
            return selectedBranchNumber;
        }

        private CustomBranch SelectTeeBranch(CustomElement element)
        {
            if (element.ElementId.IntegerValue == 10562646)
            {
                var el = element;
            }
            if (element.DetailType.ToString().Contains("Tee"))
            {
                for (int i = 0; i < Collection.Count; i++)
                {
                    if (Collection[i].Elements.First().BranchNumber != element.BranchNumber)
                    {
                        CustomBranch customBranch = Collection[i];
                        foreach (var el in customBranch.Elements)
                        {
                            if (el.ElementId.IntegerValue == element.ElementId.IntegerValue)
                            {
                                return customBranch;
                            }
                        }

                    }
                }



            }

            if (element.DetailType.ToString().Contains("Insert"))
            {
                for (int i = 0; i < Collection.Count; i++)
                {
                    if (Collection[i].Elements.First().BranchNumber != element.BranchNumber)
                    {
                        CustomBranch customBranch = Collection[i];
                        foreach (var el in customBranch.Elements)
                        {
                            if (el.ElementId.IntegerValue == element.NextElementId.IntegerValue|| el.ElementId.IntegerValue == element.TapId.IntegerValue)
                            {
                                return customBranch;
                            }
                        }

                    }
                }




            }
           
            return null;
        }

        private int GetSplitter(CustomBranch selectedBranch)
        {
            CustomElement lastElement = selectedBranch.Elements.Last();
           for (int i =0; i<selectedBranch.Elements.Count; i++)
           {
                if (selectedBranch.Elements[i].ElementId.IntegerValue == 10524493)
                {
                    var el = selectedBranch.Elements[i];
                }

                CustomElement element = selectedBranch.Elements[i];
                if (element.IsVisited==false)
                {
                    if (element.ElementId.IntegerValue == lastElement.ElementId.IntegerValue)
                    {
                        if (lastElement.DetailType==CustomElement.Detail.DuctTap)
                        {
                            lastElement.IsNonPrinted = true;
                        }
                        return -1;
                    }
                    else if (element.DetailType == CustomElement.Detail.Tee ||
                    element.DetailType == CustomElement.Detail.TapAdjustable ||
                    element.DetailType == CustomElement.Detail.DuctTap)
                    {
                        return i;
                    }
                   
                    else
                    {
                        element.IsVisited = true;
                    }
                }
                
                  
           }
            return -1;
        }

        private void UpdateElementProperties(CustomElement element, CustomTee2 customTee)
        {
            element.IA = customTee.IA;
            element.IQ = customTee.IQ;
            element.IC = customTee.IC;
            element.O1A = customTee.O1A;
            element.O1Q = customTee.O1Q;
            element.O1C = customTee.O1C;
            element.O2A = customTee.O2A;
            element.O2Q = customTee.O2Q;
            element.RA = customTee.RA;
            element.RQ = customTee.RQ;
            element.RC = customTee.RC;
            element.LocRes = customTee.LocRes;
          

            element.PDyn = Density * Math.Pow(customTee.Velocity, 2) / 2 * element.LocRes;
        }

        private void UpdateInsertElementProperties(CustomElement element, CustomDuctInsert2 customTee)
        {
            if (element.DetailType == CustomElement.Detail.DuctTap)
            {
                if (element.ElementId.IntegerValue == 10562871)
                {
                    var el = element;
                }
                foreach (var branch in Collection)
                {
                    for (int i = 0; i < branch.Elements.Count; i++)
                    {
                        if (branch.Elements[i].ElementId == element.TapId)
                        {
                            

                            element.DetailType = customTee.Detail;

                        }
                    }
                }
               
            }
            else if (element.DetailType == CustomElement.Detail.TapAdjustable)
            {
                foreach (var branch in Collection)
                {
                    for (int i = 0; i < branch.Elements.Count; i++)
                    {
                        if (branch.Elements[i].ElementId == element.ElementId)
                        {

                            element.DetailType = customTee.Detail;
                            //branch.Elements[i-1].DetailType = customTee.Detail;

                        }
                    }
                }
            }



            element.IA = customTee.IA;
            element.IQ = customTee.IQ;
            element.IC = customTee.IC;
            element.O1A = customTee.O1A;
            element.O1Q = customTee.O1Q;
            element.O1C = customTee.O1C;
            element.O2A = customTee.O2A;
            element.O2Q = customTee.O2Q;
            element.RA = customTee.RA;
            element.RQ = customTee.RQ;
            element.RC = customTee.RC;
            element.LocRes = customTee.LocRes;


            element.PDyn = Density * Math.Pow(customTee.Velocity, 2) / 2 * element.LocRes;
        }


        public int GetIndex(CustomBranch researchedBranch, int selectedend)
        {
            if (selectedend == -1)
            {
                return -1;
            }
            for (int i = selectedend; i < researchedBranch.Elements.Count; i++)
            {

                CustomElement element = researchedBranch.Elements[i];



                if (element.ElementId.IntegerValue == 644211)
                {
                    var element2 = element;
                }
                // Проверяем наличие элемента Tee
                if (element.DetailType == CustomElement.Detail.Tee)
                {
                    
                    //ВОТ ЭТО СПОРНЫЙ МОМЕНТ




                    researchedBranch.BranchCalc(i);
                    
                    selectedend = i; 


                    return selectedend;

                }
                if (element.DetailType == CustomElement.Detail.TapAdjustable)
                {
                   
                    
                    

                    researchedBranch.BranchCalc(i);
                    
                    selectedend = i; 


                    return selectedend;

                }
                if (element.DetailType.ToString().Contains("Insert"))
                {
                    researchedBranch.BranchCalc(i);

                    selectedend = i;


                    return selectedend;
                }
            }

            return -1;
        }

        




        public int GetElementIndex(CustomBranch researchedBranch, int selectedend)
        {
            if (selectedend ==-1)
            {
                return -1;
            }
            for (int i = selectedend; i < researchedBranch.Elements.Count; i++)
            {
                CustomElement element = researchedBranch.Elements[i];
                if (element.ElementId.IntegerValue == 644211)
                {
                    var element2 = element;
                }
                // Проверяем наличие элемента Tee
                if (element.DetailType == CustomElement.Detail.Tee)
                {
                    //pressure1 = researchedBranch.Elements[i - 1].Ptot;
                    //elementId = researchedBranch.Elements[i].ElementId;

                    //ВОТ ЭТО СПОРНЫЙ МОМЕНТ
                    CustomTee2 customTee = new CustomTee2(Document, element, Collection, false);
                    UpdateElementProperties(element, customTee);
                    //ВОТ ЭТО СПОРНЫЙ МОМЕНТ
                    researchedBranch.BranchCalc(i);
                    //pressure1 = researchedBranch.Elements[i - 1].Ptot;
                    i++;
                    selectedend = i; // Так как i увеличится в следующей итерации
                    return selectedend;

                }
                if (element.DetailType == CustomElement.Detail.TapAdjustable)
                { 
                    //pressure1 = researchedBranch.Elements[i - 1].Ptot;
                    //elementId = researchedBranch.Elements[i].ElementId;

                    //ВОТ ЭТО СПОРНЫЙ МОМЕНТ
                    CustomDuctInsert2 customTee = new CustomDuctInsert2(Document, element, Collection, false);
                    UpdateInsertElementProperties(element, customTee);
                    
                    
                    //ВОТ ЭТО СПОРНЫЙ МОМЕНТ

                    researchedBranch.BranchCalc(i);
                    //pressure1 = researchedBranch.Elements[i - 1].Ptot;
                    i++;
                    selectedend = i; // Так как i увеличится в следующей итерации


                    return selectedend;

                }
                if (element.DetailType == CustomElement.Detail.DuctTap)
                {
                    foreach (var branch in Collection)
                    {
                        foreach (var el in branch.Elements)
                        {
                            if (element.ElementId == el.TapId)
                            {
                                CustomDuctInsert2 customTee = new CustomDuctInsert2(Document, el, Collection, false);
                                UpdateInsertElementProperties(el, customTee);
                            }
                        }
                    }
                    researchedBranch.BranchCalc(i);
                    
                    i++;
                    selectedend = i;
                }
            }
            return -1;
        }

        public (List<CustomBranch>, CustomBranch) TeeTapSolver(CustomBranch researchedBranch,int nextelement )
        {
            int i = 0;
            CustomBranch resultBranch = new CustomBranch(Document);
            double pressure1 = 0;
            double pressure2 = 0;
            int selectedend = 0;
            int prevBranchNumber = 0;
            CustomElement element = null;
            ElementId elementId = null;
            if (nextelement != -1)
            {
                selectedend = nextelement;
            }
            do
            {

                if (researchedBranch.Elements[i].ElementId.IntegerValue==10562627)
                {
                    var element5 = element;
                }
                selectedend = GetElementIndex(researchedBranch, selectedend);
                if (selectedend == -1)
                {
                    break;
                }
                elementId = researchedBranch.Elements[selectedend].ElementId;
                if (researchedBranch.Elements[selectedend].PluginId == 5236)
                {
                    var el3 = researchedBranch.Elements[selectedend];
                }
                if (elementId == researchedBranch.Elements[selectedend - 1].ElementId)
                {
                    continue;
                }
                pressure1 = researchedBranch.Elements[selectedend - 1].Ptot;
                int brNum = 0;
                int minimalIndex = 1000000;

                int correctBranch = 0;
                prevBranchNumber = researchedBranch.Number;
                CustomElement previousElement = researchedBranch.Elements[selectedend - 1];
                for (int k = 0; k < Collection.Count; k++)
                {
                    // Убедитесь, что мы игнорируем уже посещенные ветви
                    if (Collection[k] == researchedBranch || Collection[k].IsVisited)
                    {
                        continue;
                    }
                    else
                    {
                        if (Collection[k].Elements.Any(x => x.ElementId == elementId))
                        {
                            try
                            {
                                CustomElement foundedElement = Collection[k].Elements.First(x => x.ElementId == elementId);
                                if (foundedElement == null)
                                {
                                    break;
                                }
                                if (foundedElement.ElementId.IntegerValue == 10522159)
                                {
                                    var element5 = element;
                                }
                                // Проверяем, нашли ли мы элемент
                                if (foundedElement != null)
                                {
                                    int foundedIndex = Collection[k].Elements.FindIndex(x => x.ElementId == elementId);
                                    if (foundedIndex < minimalIndex)
                                    {
                                        minimalIndex = foundedIndex;
                                        correctBranch = k;
                                    }


                                    Collection[correctBranch].BranchCalc(minimalIndex);

                                    // Тут находится логика для обработки тройника
                                    CustomElement element2 = Collection[correctBranch].Elements[minimalIndex];

                                    if (element2.DetailType==CustomElement.Detail.Tee)
                                    {
                                        if (element2.ElementId.IntegerValue == 10522159)
                                        {
                                            var el3 = element2;
                                        }
                                            CustomTee2 customTee2 = new CustomTee2(Document, element2, Collection, true);
                                        UpdateElementProperties(element2, customTee2);
                                        Collection[correctBranch].BranchCalc(minimalIndex);
                                        pressure2 = Collection[correctBranch].Elements[minimalIndex - 1].Ptot;
                                        //pressure2 = Collection[correctBranch].Elements[minimalIndex].Ptot;
                                        Collection[correctBranch].IsVisited = true;
                                    }
                                    if (element2.DetailType== CustomElement.Detail.TapAdjustable)
                                    {
                                        //CustomElement element3 = previousElement;
                                        CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(Document, element2, Collection, true);
                                        //element2.DetailType = element2.DetailType;
                                        element2.PStat = 0;
                                        UpdateInsertElementProperties(element2, customDuctInsert2);
                                        
                                        Collection[correctBranch].BranchCalc(minimalIndex);
                                        //pressure2 = Collection[correctBranch].Elements[minimalIndex - 1].Ptot;
                                        pressure2 = Collection[correctBranch].Elements[minimalIndex].Ptot;
                                        Collection[correctBranch].IsVisited = true;
                                        
                                    }
                                    if (element2.DetailType==CustomElement.Detail.DuctTap)
                                    {
                                        foreach (var branch in Collection)
                                        {
                                            foreach (var el in branch.Elements)
                                            {
                                                if (element.ElementId == el.TapId)
                                                {
                                                    CustomDuctInsert2 customTee = new CustomDuctInsert2(Document, el, Collection, true);
                                                    UpdateInsertElementProperties(el, customTee);

                                                    Collection[correctBranch].IsVisited = true;
                                                }
                                            }
                                        }
                                    }
                                    
                                    // Тут находится логика для обработки тройника
                                }

                                if (pressure1 > pressure2)
                                {
                                    for (int l = minimalIndex + 1; l < Collection[correctBranch].Elements.Count; l++)
                                    {
                                        researchedBranch.Elements[l].MainTrack = true;
                                        resultBranch.Add(researchedBranch.Elements[l]);
                                        if (researchedBranch.Elements[l].DetailType == CustomElement.Detail.Tee)
                                        {
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    researchedBranch = Collection[correctBranch];
                                    ElementId elementId2 = Collection[correctBranch].Elements.Last().ElementId;
                                    for (int j = minimalIndex + 1; j < Collection[correctBranch].Elements.Count; j++)
                                    {
                                        Collection[correctBranch].Elements[j].MainTrack = true;
                                        resultBranch.Add(Collection[correctBranch].Elements[j]);
                                      
                                        if (Collection[correctBranch].Elements[j].DetailType == CustomElement.Detail.Tee)
                                        {
                                            break;
                                        }
                                    }
                                    selectedend = GetElementIndex(researchedBranch, selectedend);
                                    if (selectedend == -1)
                                    {
                                        break;
                                    }
                                }
                            }
                            catch
                            {
                                selectedend = GetElementIndex(researchedBranch, selectedend);
                                if (selectedend == -1)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                selectedend = GetElementIndex(researchedBranch, selectedend);
                if (selectedend == -1)
                {
                    break;
                }
                selectedend += 1;
                i++;
            }
            while (researchedBranch.Elements.Last().NextElementId == null);
            researchedBranch.BranchCalc(researchedBranch.Elements.Count - 1);
            List<ElementId> checkedIds = new List<ElementId>();
            foreach (var el in researchedBranch.Elements)
            {
                if (!checkedIds.Contains(el.ElementId))
                {
                    checkedIds.Add(el.ElementId);
                    el.MainTrack = true;
                }
                else
                { continue; }
            }
            return (Collection, researchedBranch);
        }
        public (CustomBranch researchedBranch, int nextelement)  BranchSelector()
        {
            CustomBranch selectedBranch = null;
            CustomElement selectedTee = null;
            int selectedBranchNumber;

            int tapcounter = 0;
            foreach (var branch in Collection)
            {
                foreach (var el in branch.Elements)
                {
                    if (el.DetailType == CustomElement.Detail.TapAdjustable)
                    {
                        tapcounter++;
                    }
                }
            }
            int nextelement = -1;

            // Выбор ветви, если счетчик врезок равен нулю, то выберем вариант алгоритма с тройниками 
            if (tapcounter != 0)
            {
                int index1 = 0;
                selectedBranch = Collection
               .Select(branch => new
               {
                   Branch = branch,
                   index = GetIndex(branch, 0),
                   TapCount = branch.Elements.Count(el => el.DetailType == CustomElement.Detail.TapAdjustable),
                   PBtot = branch.BranchCalc_Pressure(index1)
               })
                .OrderByDescending(x => x.TapCount)
                .ThenByDescending(x => x.PBtot)
                .FirstOrDefault()?.Branch;

                //selectedTee = selectedBranch.Elements.Select(x => x).Where(x => x.DetailType == CustomElement.Detail.TapAdjustable).First();
                selectedTee = selectedBranch.Elements.Select(x => x).Where(x => x.DetailType == CustomElement.Detail.TapAdjustable || x.DetailType==CustomElement.Detail.Tee).First();
                nextelement = selectedBranch.Elements.IndexOf(selectedTee);

                return (selectedBranch, nextelement);
            }
            else
            {
                // В этом случае мы находим тройник с нуль-расход-коннектором.
                // В этом случае он самый плохой и мы обходим по этому пути.
                // И обходим сразу до воздуховода, который сразу после этого тройника
                foreach (var branch in Collection)
                {
                    for (int ind = 0; ind < branch.Elements.Count; ind++)
                    {
                        CustomElement el = branch.Elements[ind];
                        if (el.ElementId.IntegerValue == 644211)
                        {
                            var el2 = el;
                        }
                        if (el.DetailType == CustomElement.Detail.Tee)
                        {
                            foreach (Connector connector in el.OwnConnectors)
                            {
                                if (connector.Flow == 0)
                                {
                                    selectedTee = el;
                                    selectedBranch = branch;
                                    selectedBranchNumber = el.BranchNumber;
                                    ind++;
                                    break;
                                }

                            }
                        }

                    }
                }
                if(selectedBranch!=null)
                {
                    for (int i = 0; i < selectedBranch.Elements.Count; i++)
                    {
                        if (selectedBranch.Elements[i].ElementId.IntegerValue == 658817)
                        {
                            var el2 = selectedBranch.Elements[i];
                        }
                        if (selectedBranch.Elements[i].ElementId.IntegerValue == selectedTee.ElementId.IntegerValue)
                        {
                            if (selectedTee.DetailType == CustomElement.Detail.Tee)
                            {
                                CustomElement element2 = selectedBranch.Elements[i];
                                CustomTee2 customTee2 = new CustomTee2(Document, element2, Collection, false);
                                UpdateElementProperties(element2, customTee2);
                                nextelement = i;
                                selectedBranch.BranchCalc(nextelement);
                                break; // Завершить цикл после обработки первого найденного элемента
                            }


                        }
                    }
                }
                else
                {
                    int index1 = 0;
                    selectedBranch = Collection.OrderByDescending(x => x.PBTot).First();
                    /*selectedBranch = Collection
                   .Select(branch => new
                   {
                       Branch = branch,
                       index = GetIndex(branch, 0),
                      
                       PBtot = branch.BranchCalc_Pressure(index1)
                   })
                    .OrderByDescending(x => x.PBtot)
                    .FirstOrDefault()?.Branch;*/

                   
                    nextelement = 0;
                }
                
            }
           
             
            return (selectedBranch, nextelement);
        }


        public (List<CustomBranch>, CustomBranch) TeeSolver()
        {
            CustomBranch selectedBranch = null;
            List<CustomElement> tees = new List<CustomElement>();
            CustomElement selectedTee = null;
            int selectedBranchNumber;

            int tapcounter = 0;
            foreach (var branch in Collection)
            {
                foreach (var el in branch.Elements)
                {
                    if (el.DetailType == CustomElement.Detail.TapAdjustable)
                    {
                        tapcounter++;
                    }
                }
            }
            int nextelement = -1;
            if (tapcounter != 0)
            {
                int index1 = 0;
                selectedBranch = Collection
               .Select(branch => new
               {
                   Branch = branch,
                   index = GetIndex(branch, 0),
                   TapCount = branch.Elements.Count(el => el.DetailType == CustomElement.Detail.TapAdjustable),
                   PBtot = branch.BranchCalc_Pressure(index1)
               })
                .OrderByDescending(x => x.TapCount)
                .ThenByDescending(x => x.PBtot)
                .FirstOrDefault()?.Branch;

                selectedTee = selectedBranch.Elements.Select(x => x).Where(x => x.DetailType == CustomElement.Detail.TapAdjustable).First();
            }
            else
            {
                foreach (var branch in Collection)
                {
                    for (int ind = 0; ind < branch.Elements.Count; ind++)
                    {
                        CustomElement el = branch.Elements[ind];
                        if (el.ElementId.IntegerValue == 644211)
                        {
                            var el2 = el;
                        }
                        if (el.DetailType == CustomElement.Detail.Tee)
                        {
                            foreach (Connector connector in el.OwnConnectors)
                            {
                                if (connector.Flow == 0)
                                {
                                    selectedTee = el;
                                    selectedBranch = branch;
                                    selectedBranchNumber = el.BranchNumber;
                                    ind++;
                                    break;
                                }

                            }
                        }


                    }
                }

               

                for (int i = 0; i < selectedBranch.Elements.Count; i++)
                {
                    if (selectedBranch.Elements[i].ElementId.IntegerValue == 658817)
                    {
                        var el2 = selectedBranch.Elements[i];
                    }
                    if (selectedBranch.Elements[i].ElementId.IntegerValue == selectedTee.ElementId.IntegerValue)
                    {
                        if (selectedTee.DetailType == CustomElement.Detail.Tee)
                        {
                            CustomElement element2 = selectedBranch.Elements[i];
                            CustomTee2 customTee2 = new CustomTee2(Document, element2, Collection, false);
                            UpdateElementProperties(element2, customTee2);
                            nextelement = i;
                            selectedBranch.BranchCalc(nextelement);
                            break; // Завершить цикл после обработки первого найденного элемента
                        }


                    }
                }
            }
            //Тут обработали ветку, на которой расположен тройник, который имеет только присоединение одной решетки.
           
           


           /* if (selectedTee.DetailType == CustomElement.Detail.TapAdjustable)
            {
                CustomElement element2 = selectedBranch.Elements[i];
                CustomDuctInsert2 customTee2 = new CustomDuctInsert2(Document, element2, Collection, false);
                CustomElement element2next = selectedBranch.Elements[i + 1];
                CustomDuctInsert2 customTee3 = new CustomDuctInsert2(Document, element2, Collection, true);
                UpdateElementProperties(element2, customTee2);
                UpdateElementProperties(element2next, customTee3);
                nextelement = i + 1;
                selectedBranch.BranchCalc(nextelement);
                break; // Завершить цикл после обработки первого найденного элемента
            }*/

            // Тут надо обработвть оставшиеся ветки, кроме выбранной ветки selectedBranch

            int index = 0;
            foreach (var branch in Collection)
            {
                if (branch.Number == selectedBranch.Number)
                {
                    continue;
                }
                else
                {
                    foreach (var el in branch.Elements)
                    {
                        if (el.DetailType == CustomElement.Detail.Tee)
                        {
                             index = GetElementIndex(branch, 0);

                            branch.BranchCalc(index - 1);
                            break;


                        }
                        if (el.DetailType == CustomElement.Detail.TapAdjustable)
                        {
                            try
                            {
                                index = GetElementIndex(branch, 0);
                                branch.BranchCalc(index - 1);
                                break;
                            }
                            catch
                            { }
                            
                        }
                    }
                }
            }
            
            //Collection = Collection.OrderByDescending(x => x.PBTot).ToList();
            selectedBranch = Collection
              .Select(branch => new
              {
                  Branch = branch,
                  index = GetIndex(branch, 0),
                  TapCount = branch.Elements.Count(el => el.DetailType.ToString().Contains("Insert") || el.DetailType.ToString().Contains("TapAdjustable")),
                  PBtot = branch.BranchCalc_Pressure(index)
              })
               .OrderByDescending(x => x.TapCount)
               .ThenByDescending(x => x.PBtot)
               .FirstOrDefault()?.Branch;
            selectedBranch = Collection.First();

            List<CustomBranch> newCollection = new List<CustomBranch>();
            CustomBranch resultBranch = new CustomBranch(Document);
            CustomBranch researchedBranch = selectedBranch;

            /* for (int l = 0; l < researchedBranch.Elements.Count; l++)
             {
                 researchedBranch.Elements[l].MainTrack = true;
                 resultBranch.Add(researchedBranch.Elements[l]);
                 if (researchedBranch.Elements[l].DetailType == CustomElement.Detail.Tee)
                 {
                     break;
                 }
             }*/


            double pressure1 = 0;
            double pressure2 = 0;
            int selectedend = 0;
            CustomElement element = null;
            ElementId elementId = null;
            if (nextelement != -1)
            {
                selectedend = nextelement;
            }
            do
            {

                selectedend = GetElementIndex(researchedBranch, selectedend);
                if (selectedend == -1)
                {
                    break;
                }
                elementId = researchedBranch.Elements[selectedend].ElementId;
                if (researchedBranch.Elements[selectedend].PluginId == 44)
                {
                    var el3 = researchedBranch.Elements[selectedend];
                }
                if (elementId == researchedBranch.Elements[selectedend - 1].ElementId)
                {
                    continue;
                }
                /* if (elementId.IntegerValue==644208)
                 {
                     var el3 = researchedBranch.Elements[selectedend];
                 }*/
                //pressure1 = researchedBranch.Elements[selectedend - 2].Ptot;
                pressure1 = researchedBranch.Elements[selectedend - 1].Ptot;


                int brNum = 0;
                int minimalIndex = 1000000;

                int correctBranch = 0;
                for (int k = 0; k < Collection.Count; k++)
                {

                    // Убедитесь, что мы игнорируем уже посещенные ветви
                    if (Collection[k] == researchedBranch || Collection[k].IsVisited)
                    {
                        continue;
                    }
                    else
                    {
                        if (Collection[k].Elements.Any(x => x.ElementId == elementId))
                        {
                            try
                            {

                                CustomElement foundedElement = Collection[k].Elements.First(x => x.ElementId == elementId);
                                if (foundedElement == null)
                                {
                                    break;
                                }
                                if (foundedElement.ElementId.IntegerValue == 644205)
                                {
                                    var element5 = element;
                                }
                                // Проверяем, нашли ли мы элемент
                                if (foundedElement != null)
                                {
                                    int foundedIndex = Collection[k].Elements.FindIndex(x => x.ElementId == elementId);

                                    // Обновляем minimalIndex, если найденный индекс меньше текущего минимального
                                    if (foundedIndex < minimalIndex)
                                    {
                                        minimalIndex = foundedIndex;
                                        correctBranch = k;
                                        // Вы можете добавить дополнительную логику здесь, если это необходимо
                                    }
                                    Collection[correctBranch].BranchCalc(minimalIndex);
                                    CustomElement element2 = Collection[k].Elements[minimalIndex];
                                    //CustomElement element3 = new CustomElement(Document, element2.ElementId);
                                    CustomTee2 customTee2 = new CustomTee2(Document, element2, Collection, true);
                                    UpdateElementProperties(element2, customTee2);

                                    Collection[correctBranch].BranchCalc(minimalIndex);
                                    pressure2 = Collection[correctBranch].Elements[minimalIndex - 1].Ptot;
                                    Collection[correctBranch].IsVisited = true;
                                }



                                if (pressure1 > pressure2)
                                {
                                    for (int l = minimalIndex + 1; l < Collection[correctBranch].Elements.Count; l++)
                                    {
                                        researchedBranch.Elements[l].MainTrack = true;
                                        resultBranch.Add(researchedBranch.Elements[l]);
                                        if (researchedBranch.Elements[l].DetailType == CustomElement.Detail.Tee)
                                        {
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    researchedBranch = Collection[correctBranch];
                                    ElementId elementId2 = Collection[correctBranch].Elements.Last().ElementId;
                                    for (int j = minimalIndex + 1; j < Collection[correctBranch].Elements.Count; j++)
                                    {
                                        /*do
                                        {*/

                                        Collection[correctBranch].Elements[j].MainTrack = true;
                                        resultBranch.Add(Collection[correctBranch].Elements[j]);
                                        /*if (Collection[correctBranch].Elements[j].ElementId==elementId2)
                                        {
                                            break;
                                        }
                                        if (Collection[correctBranch].Elements[j].NextElementId==null)
                                        {
                                            break;
                                        }*/
                                        if (Collection[correctBranch].Elements[j].DetailType == CustomElement.Detail.Tee)
                                        {
                                            break;
                                        }

                                        /*while (true);*/


                                    }
                                    selectedend = GetElementIndex(researchedBranch, selectedend);
                                    if (selectedend == -1)
                                    {
                                        break;
                                    }
                                }
                            }
                            catch
                            {
                                selectedend = GetElementIndex(researchedBranch, selectedend);
                                if (selectedend == -1)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            /* for (int l = 0; l < Collection[correctBranch].Elements.Count; l++)
                             {
                                 researchedBranch.Elements[l].MainTrack = true;
                                 resultBranch.Add(researchedBranch.Elements[l]);
                                 if (researchedBranch.Elements[l].DetailType == CustomElement.Detail.Tee)
                                 {
                                     break;
                                 }
                             }*/
                            continue;
                        }
                        // Находим элемент с заданным ElementId

                    }


                }
                selectedend = GetElementIndex(researchedBranch, selectedend);
                if (selectedend == -1)
                {
                    break;
                }
                selectedend += 1;
            }
            while (researchedBranch.Elements.Last().NextElementId == null);
            researchedBranch.BranchCalc(researchedBranch.Elements.Count - 1);
            List<ElementId> checkedIds = new List<ElementId>();
            foreach (var el in researchedBranch.Elements)
            {
                if (!checkedIds.Contains(el.ElementId))
                {
                    checkedIds.Add(el.ElementId);
                    el.MainTrack = true;
                }
                else
                { continue; }
            }


            return (Collection, researchedBranch);
            //Collection.Add(researchedBranch);

        }

        public void ReMarkCollection(CustomBranch selectedBranch)
        {
            List<CustomBranch> newCustomCollection = new List<CustomBranch>();
            HashSet<ElementId> checkedElements = new HashSet<ElementId>();
            HashSet<int> pluginIds = new HashSet<int>();
            Dictionary<CustomElement, int> duplicatedElements = new Dictionary<CustomElement, int>();
            // Сначала обрабатываем основную ветвь 
            foreach (var branch in Collection)
            {
                if (branch.Number == selectedBranch.Number)
                {
                    int trackCounter = 0;
                    foreach (var element in branch.Elements)
                    {
                        if (element.ElementId.IntegerValue == 644208)
                        {
                            var element2 = element;
                        }
                        if (checkedElements.Contains(element.ElementId) || pluginIds.Contains(element.PluginId))
                        {
                            if (element.DetailType == CustomElement.Detail.RectInRectDuctInsertBranch ||
                            element.DetailType == CustomElement.Detail.RectInRectDuctInsertStraight ||
                            element.DetailType == CustomElement.Detail.RoundTeeBranch ||
                            element.DetailType == CustomElement.Detail.RoundTeeStraight ||
                            element.DetailType == CustomElement.Detail.RectTeeBranch ||
                            element.DetailType == CustomElement.Detail.RectTeeStraight ||
                            element.DetailType == CustomElement.Detail.RectRoundTeeBranch ||
                            element.DetailType == CustomElement.Detail.RectRoundTeeStraight ||
                            element.DetailType == CustomElement.Detail.RoundInRoundDuctInsertStraight ||
                            element.DetailType == CustomElement.Detail.RoundInRoundDuctInsertBranch ||
                            element.DetailType == CustomElement.Detail.RoundInRectDuctInsertStraight ||
                            element.DetailType == CustomElement.Detail.RoundInRectDuctInsertBranch ||
                            element.DetailType == CustomElement.Detail.RectInRectDuctInsertStraight ||
                            element.DetailType == CustomElement.Detail.RectInRoundDuctInsertStraight ||
                            element.DetailType == CustomElement.Detail.RectInRoundDuctInsertBranch||
                            element.DetailType == CustomElement.Detail.DuctTap||
                            element.DetailType == CustomElement.Detail.TapAdjustable
                            )

                            {
                                //element.IsNonPrinted = true;


                                element.IsNonPrinted = true;
                                trackCounter--;
                                pluginIds.Add(element.PluginId);
                            }
                        }
                        
                        else
                        {
                            element.TrackNumber = trackCounter;
                            element.BranchNumber = branch.Number;
                            element.MainTrack = true;
                            pluginIds.Add(element.PluginId);
                            checkedElements.Add(element.ElementId);
                            trackCounter++;
                        }

                    }
                    newCustomCollection.Add(branch);
                    break; // Прекращаем дальнейший обход после нахождения основной ветви 
                }
            }
            CustomBranch newCustomBranch = new CustomBranch(Document);
          
            int trackCounter2 = 0;
            // Обрабатываем остальные ветви 
            foreach (var branch in Collection)
            {
               
                if (branch.Number != selectedBranch.Number)
                {
                    foreach(var element in branch.Elements)
                    {
                        if (checkedElements.Contains(element.ElementId))
                        {
                            if (!pluginIds.Contains(element.PluginId))
                            {
                                if (element.DetailType.ToString().Contains("Tee") || element.DetailType.ToString().Contains("Insert"))
                                {

                                    if (element.DetailType.ToString().Contains("Tee"))
                                    {
                                        CustomTee2 customTee2 = new CustomTee2(Document, element, Collection, true);
                                        UpdateElementProperties(element, customTee2);
                                        element.TrackNumber = trackCounter2;
                                        element.BranchNumber = branch.Number;
                                        newCustomBranch.Add(element);
                                        checkedElements.Add(element.ElementId);
                                        pluginIds.Add(element.PluginId);
                                        trackCounter2++;
                                        break;

                                    }
                                    if (element.DetailType.ToString().Contains("Insert"))
                                    {
                                        CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(Document, element, Collection, true);
                                        UpdateInsertElementProperties(element, customDuctInsert2);
                                        element.TrackNumber = trackCounter2;
                                        element.BranchNumber = branch.Number;
                                        newCustomBranch.Add(element);
                                        checkedElements.Add(element.ElementId);
                                        pluginIds.Add(element.PluginId);
                                        trackCounter2++;
                                        break;
                                    }

                                }
                            }
                            
                            else
                            {
                                break;
                            }
                           
                        }
                        if (!checkedElements.Contains(element.ElementId))
                        {
                            if (pluginIds.Contains(element.PluginId))
                            {
                                break;
                            }
                            
                                element.TrackNumber = trackCounter2;
                                element.BranchNumber = branch.Number;
                                newCustomBranch.Add(element);
                                checkedElements.Add(element.ElementId);
                                pluginIds.Add(element.PluginId);
                                trackCounter2++;
                                

                            
                        }
                    }
                }
                


                newCustomCollection.Add(newCustomBranch);
                

            }


            



            // Обновляем коллекцию 
            Collection = newCustomCollection;
            //ResCalculate();
        }

        

        public string GetContent()
        {

            var csvcontent = new StringBuilder();
            //csvcontent.AppendLine("ElementId;DetailType;ElementName;SystemName;Level;BranchNumber;SectionNumber;Volume;Length;Width;Height;Diameter;HydraulicDiameter;HydraulicArea;IA;IQ;IC;O1A;O1Q;O1C;O2A;O2Q;O2C;RA;RQ;RC;Velocity;PStat;KMS;PDyn;Ptot;Code;MainTrack");
            csvcontent.AppendLine("PluginId;BranchNum;ElementId;DetailType;ElementName;SystemName;Level;BranchNumber;SectionNumber;Volume;Length;Width;Height;Diameter;HydraulicDiameter;HydraulicArea;Velocity;PStat;KMS;PDyn;Ptot;Code;MainTrack");
            foreach (var branch in Collection)
            {

                foreach (var element in branch.Elements)
                {
                    if (element.IsNonPrinted)
                    {
                        continue;
                    }
                    /*string a = $"{element.ElementId};{element.DetailType};{element.Name};{element.SystemName};{element.Lvl};{element.BranchNumber};{element.TrackNumber};" +
                         $"{element.Volume};{element.ModelLength};{element.ModelWidth};{element.ModelHeight};{element.ModelDiameter};{element.ModelHydraulicDiameter};{element.ModelHydraulicArea};{element.IA};{element.IQ};{element.IC};{element.O1A};{element.O1Q};{element.O1C};{element.O2A};{element.O2Q};{element.O2C};{element.RA};{element.RQ};{element.RC};{element.ModelVelocity};{element.PStat};{Math.Round(element.LocRes, 2)};{Math.Round(element.PDyn, 2)};{Math.Round(element.Ptot, 2)};" +

                         $"{element.SystemName}-{element.Lvl}-{element.BranchNumber}-{element.TrackNumber};{element.MainTrack}";*/

                    element.NewModelWidth = Convert.ToString(Convert.ToDouble(element.ModelWidth));
                        element.NewModelHeight = Convert.ToString(Convert.ToDouble(element.ModelHeight));
                        element.ModelVelocity = Convert.ToString(Math.Round(Convert.ToDouble(element.ModelVelocity), 2));
                        element.ModelDiameter = Convert.ToString(Math.Round(Convert.ToDouble(element.ModelDiameter), 2));
                        string a = $"{element.PluginId};{branch.Number};{element.ElementId};{element.DetailType};{element.Name};{element.SystemName};{element.Lvl};{element.BranchNumber};{element.TrackNumber};" +
                            $"{element.Volume};{element.ModelLength};{element.NewModelWidth};{element.NewModelHeight};{element.ModelDiameter};{element.ModelHydraulicDiameter};{element.ModelHydraulicArea};{element.ModelVelocity};{element.PStat};{Math.Round(element.LocRes, 2)};{Math.Round(element.PDyn, 2)};{Math.Round(element.Ptot, 2)};" +

                            $"{element.SystemName}-{element.Lvl}-{element.BranchNumber}-{element.TrackNumber};{element.MainTrack}";
                        csvcontent.AppendLine(a);
                    
                   
                }
            }

            return csvcontent.ToString();
        }

        public string GetContent(CustomBranch selectedBranch)
        {
            Collection.Add(selectedBranch);
            var csvcontent = new StringBuilder();
            //csvcontent.AppendLine("ElementId;DetailType;ElementName;SystemName;Level;BranchNumber;SectionNumber;Volume;Length;Width;Height;Diameter;HydraulicDiameter;HydraulicArea;IA;IQ;IC;O1A;O1Q;O1C;O2A;O2Q;O2C;RA;RQ;RC;Velocity;PStat;KMS;PDyn;Ptot;Code;MainTrack");
            csvcontent.AppendLine("PluginId;ElementId;DetailType;ElementName;SystemName;Level;BranchNumber;SectionNumber;Volume;Length;Width;Height;Diameter;HydraulicDiameter;HydraulicArea;Velocity;PStat;KMS;PDyn;Ptot;Code;MainTrack");
            foreach (var branch in Collection)
            {

                foreach (var element in branch.Elements)
                {
                    if (element.IsNonPrinted)
                    {
                        continue;
                    }
                    /*string a = $"{element.ElementId};{element.DetailType};{element.Name};{element.SystemName};{element.Lvl};{element.BranchNumber};{element.TrackNumber};" +
                         $"{element.Volume};{element.ModelLength};{element.ModelWidth};{element.ModelHeight};{element.ModelDiameter};{element.ModelHydraulicDiameter};{element.ModelHydraulicArea};{element.IA};{element.IQ};{element.IC};{element.O1A};{element.O1Q};{element.O1C};{element.O2A};{element.O2Q};{element.O2C};{element.RA};{element.RQ};{element.RC};{element.ModelVelocity};{element.PStat};{Math.Round(element.LocRes, 2)};{Math.Round(element.PDyn, 2)};{Math.Round(element.Ptot, 2)};" +

                         $"{element.SystemName}-{element.Lvl}-{element.BranchNumber}-{element.TrackNumber};{element.MainTrack}";*/

                    element.NewModelWidth = Convert.ToString(Convert.ToDouble(element.ModelWidth));
                    element.NewModelHeight = Convert.ToString(Convert.ToDouble(element.ModelHeight));
                    element.ModelVelocity = Convert.ToString(Math.Round(Convert.ToDouble(element.ModelVelocity), 2));
                    element.ModelDiameter = Convert.ToString(Math.Round(Convert.ToDouble(element.ModelDiameter), 2));
                    string a = $"{element.PluginId};{element.ElementId};{element.DetailType};{element.Name};{element.SystemName};{element.Lvl};{element.BranchNumber};{element.TrackNumber};" +
                        $"{element.Volume};{element.ModelLength};{element.NewModelWidth};{element.NewModelHeight};{element.ModelDiameter};{element.ModelHydraulicDiameter};{element.ModelHydraulicArea};{element.ModelVelocity};{element.PStat};{Math.Round(element.LocRes, 2)};{Math.Round(element.PDyn, 2)};{Math.Round(element.Ptot, 2)};" +

                        $"{element.SystemName}-{element.Lvl}-{element.BranchNumber}-{element.TrackNumber};{element.MainTrack}";
                    csvcontent.AppendLine(a);


                }
            }

            return csvcontent.ToString();
        }

        public void SaveFile(string content) // спрятали функцию сохранения 
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
            saveFileDialog.Title = "Save CSV File";
            saveFileDialog.FileName = Collection.First().Elements.First().SystemName + ".csv";
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
        private string GetValue(string primaryvolume)
        {
            // Используем регулярное выражение, чтобы найти и вернуть только числовую часть
            var match = System.Text.RegularExpressions.Regex.Match(primaryvolume, @"\d+(\.\d+)?");
            return match.Success ? match.Value : string.Empty; // Вернуть число или пустую строку, если числ
        }

        

        

        // Метод для проверки типа элемента
       

        // Метод для расчета расхода
        
       

        public CustomBranch Calcualate2(double density , CustomBranch customBranch, int index)
        {
            


            HashSet<ElementId> checkedTees = new HashSet<ElementId>();
            HashSet<ElementId> checkedTaps = new HashSet<ElementId>();
            HashSet<ElementId> checkedDuctTaps = new HashSet<ElementId>();
            List<ElementId> checkedElements = new List<ElementId>();
            List<CustomBranch> newCollection = new List<CustomBranch>();
            IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "," };
            IFormatProvider formatter2 = new NumberFormatInfo { NumberDecimalSeparator = "." };
            Density = density;
            if (Collection.Count == 0)
            {
               
            }
            else
            {
                   for (int i = 0; i<index;i++)
                    {
                    CustomElement element = customBranch.Elements[i];
                        try
                        {
                            if (element.Element == null)
                            {
                                continue;
                            }

                            if (element.DetailType == CustomElement.Detail.AirTerminal)
                            {
                                try
                                {
                                    if (element.ElementId.IntegerValue == 659023)
                                    {
                                        var element2 = element;
                                    }

                                    CustomAirTerminal customAirTerminal = new CustomAirTerminal(Document, element);

                                    element.PDyn = customAirTerminal.PDyn;
                                    element.Ptot = customAirTerminal.PDyn;

                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";

                                }

                            }
                            else if (element.DetailType == CustomElement.Detail.Elbow)
                            {
                                try
                                {
                                    if (element.ElementId.IntegerValue == 22017547)
                                    {
                                        var element2 = element;
                                    }
                                    CustomElbow customElbow = new CustomElbow(Document, element);
                                    element.LocRes = customElbow.LocRes;
                                    element.PDyn = Density * Math.Pow(customElbow.Velocity, 2) / 2 * element.LocRes;


                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";

                                }
                            }
                            else if (element.DetailType == CustomElement.Detail.DuctTap)
                            {
                               

                            }
                            else if (element.DetailType == CustomElement.Detail.Tee)
                            {
                               

                            }
                            else if (element.DetailType == CustomElement.Detail.Equipment)
                            {
                                try
                                {
                                    element.LocRes = 0;
                                    element.PDyn = 0;


                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";

                                }
                            }
                            else if (element.DetailType == CustomElement.Detail.Multiport)
                            {
                                try
                                {
                                    if (element.ElementId.IntegerValue == 8968461)
                                    {
                                        var element2 = element;
                                    }
                                    CustomMultiport customElbow = new CustomMultiport(Document, element);
                                    element.LocRes = customElbow.LocRes;
                                    element.PDyn = Density * Math.Pow(customElbow.Velocity, 2) / 2 * element.LocRes;

                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                    //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                                }
                            }

                            else if (element.DetailType == CustomElement.Detail.TapAdjustable)
                            {
                              

                            }
                            else if (element.DetailType == CustomElement.Detail.Transition)
                            {
                                if (element.ElementId.IntegerValue == 8976273)
                                {
                                    var element2 = element;
                                }
                                try
                                {
                                    CustomTransition customTransition = new CustomTransition(Document, element);

                                    element.LocRes = customTransition.LocRes;
                                    element.PDyn = Density * Math.Pow(customTransition.Velocity, 2) / 2 * element.LocRes;


                                }
                                catch
                                {
                                    CustomTransition customTransition = new CustomTransition(Document, element);
                                    ActiveElement = element;
                                    element.LocRes = 0.11;
                                    element.PDyn = Density * Math.Pow(customTransition.Velocity, 2) / 2 * element.LocRes;


                                }

                            }
                            else if (element.DetailType == CustomElement.Detail.RectangularDuct || element.DetailType == CustomElement.Detail.RoundDuct)
                            {
                                if (element.ElementId.IntegerValue == 644200)
                                {
                                    var element2 = element;
                                }

                                string[] pressureDropString = element.Element.get_Parameter(BuiltInParameter.RBS_PRESSURE_DROP).AsValueString().Split();
                                try
                                {

                                    element.PStat = double.Parse(pressureDropString[0], formatter);


                                }
                                catch
                                {
                                    ActiveElement = element;
                                    element.PStat = double.Parse(pressureDropString[0], formatter2);


                                }
                                // Проверяем, что строка не пустая или null

                            }
                            else if (element.DetailType == CustomElement.Detail.RectFlexDuct || element.DetailType == CustomElement.Detail.RoundFlexDuct)
                            {
                                //branch.Pressure += element.Element.get_Parameter(BuiltInParameter.RBS_PRESSURE_DROP).AsDouble();
                                string[] pressureDropString = element.Element.get_Parameter(BuiltInParameter.RBS_PRESSURE_DROP).AsValueString().Split();
                                element.PStat = double.Parse(pressureDropString[0], formatter);

                                //branch.Pressure += element.PStat;

                            }
                            else if (element.DetailType == CustomElement.Detail.FireProtectValve)
                            {
                                if (element.ElementId.IntegerValue == 20659396)
                                {
                                    var element2 = element;
                                }
                                CustomValve customValve = new CustomValve(Document, element);


                                
                            }
                            else if (element.DetailType == CustomElement.Detail.Union)
                            {

                                
                            }
                        }
                        catch
                        {
                            //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                            ActiveElement = element;
                            ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                            //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                        }

                    }

                    
                }

                customBranch.BranchCalc();
                return customBranch;



            }

        internal void ReOrderCollection()
        {
            HashSet<int> pluginIds = new HashSet<int>();
            HashSet<ElementId> elementIds = new HashSet<ElementId>();
            List<CustomBranch> newCollection = new List<CustomBranch>();
            foreach (var branch in Collection)
            {
                branch.BranchCalc();
            }
            Collection = Collection.OrderByDescending(x => x.PBTot).ToList();
            var selbranch = Collection.First();
            var selbranchLast = selbranch.Elements.Last();
            if (selbranchLast.NextElementId!=null)
            {
                CustomElement nextelement = selbranch.Elements.First();
                do
                {
                    if (nextelement!=null)
                    {
                        nextelement.MainTrack = true;
                        nextelement = GetNextElement(nextelement);
                    }
                }
                while (nextelement != null);
            }
            else
            {
                foreach (var el in selbranch.Elements)
                {
                    el.MainTrack = true;

                }
            }
            

            foreach (var branch  in Collection)
            {
                int tracknumber = 0;
                foreach (var el in branch.Elements)
                {
                    el.TrackNumber = tracknumber;
                    tracknumber++;
                }
            }
            foreach (var branch in Collection)
            {
                CustomBranch newBranch = new CustomBranch(Document);
                foreach (var el in branch.Elements)
                {
                    if (!pluginIds.Contains(el.PluginId))
                    {
                        if(!elementIds.Contains(el.ElementId))
                        {
                            pluginIds.Add(el.PluginId);
                            elementIds.Add(el.ElementId);
                            newBranch.Add(el);
                        }
                        
                    }
                    else
                    {
                        continue;
                    }
                }
                newCollection.Add(newBranch);
            }
            Collection = newCollection;
        }

        private CustomElement GetNextElement(CustomElement nextelement)
        {
            foreach (var branch in Collection)
            {
                foreach (var el in branch.Elements)
                {
                    if (el.ElementId.IntegerValue == nextelement.ElementId.IntegerValue)
                    {
                        return el;
                    }
                }
            }
            return null;
        }
    }



    }
