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

namespace AirTreeV1
{


    public class CustomCollection
    {
        public List<CustomBranch> Collection { get; set; } = new List<CustomBranch>();
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
                                    if (element.ElementId.IntegerValue == 643925)
                                    {
                                        var element2 = element;
                                    }

                                    CustomAirTerminal customAirTerminal = new CustomAirTerminal(Document, element);
                                    //element.Volume = customAirTerminal.Volume.ToString();
                                    /*element.ModelWidth = customAirTerminal.Width.ToString();
                                    element.ModelHeight = customAirTerminal.Height.ToString();*/
                                    /*element.ModelDiameter = customAirTerminal.Diameter.ToString();
                                    element.ModelHydraulicArea = customAirTerminal.HArea.ToString();*/
                                    element.PDyn = customAirTerminal.PDyn;
                                    element.Ptot = customAirTerminal.PDyn;
                                    //branch.Pressure += element.PDyn;
                                   
                                   // branch.Pressure += element.Ptot;
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                    //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                                }
                                //Сюда допишем простую логику на воздухораспределитель по magicad
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
                                    
                                    //branch.Pressure += element.PDyn;
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                    //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                                }
                            }
                            else if (element.DetailType == CustomElement.Detail.Tee)
                            {
                                try
                                {
                                    if (element.ElementId.IntegerValue == 644205)
                                    {
                                        var element2 = element;
                                    }
                                   
                                    Tees.Add(element);
                                   
                                    

                                    //newbranch.Add(element);

                                    /*CustomTee2 customTee = new CustomTee2(Document, element, Collection, false);
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
                                     element.PDyn = Density * Math.Pow(customTee.Velocity, 2) / 2 * element.LocRes;*/


                                    //branch.Pressure += 7;
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                    //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                                }

                            }
                            else if (element.DetailType == CustomElement.Detail.Equipment)
                            {
                                try
                                {
                                    element.LocRes = 0;
                                    element.PDyn = 0;
                                   
                                    //branch.Pressure += 0;
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                    //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
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
                                    
                                    //branch.Pressure += element.PDyn;


                                    //branch.Pressure += 1;
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



                                {
                                    var element2 = element;
                                }
                               
                                Tees.Add(element);
                                
                                //newbranch = new CustomBranch(Document);
                                

                                /* CustomDuctInsert2 customDuctInsert = new CustomDuctInsert2(Document, element, Collection, false);
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
                                 element.PDyn = Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * element.LocRes; */
                                //branch.Pressure += 1;
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
                                    
                                    //branch.Pressure += element.PDyn;
                                }
                                catch
                                {
                                    CustomTransition customTransition = new CustomTransition(Document, element);
                                    ActiveElement = element;
                                    element.LocRes = 0.11;
                                    element.PDyn = Density * Math.Pow(customTransition.Velocity, 2) / 2 * element.LocRes;
                                   
                                    //branch.Pressure += element.PDyn;
                                }

                            }
                            else if (element.DetailType == CustomElement.Detail.RectangularDuct || element.DetailType == CustomElement.Detail.RoundDuct)
                            {
                                if (element.ElementId.IntegerValue == 644200)
                                {
                                    var element2 = element;
                                }
                                //branch.Pressure += element.Element.get_Parameter(BuiltInParameter.RBS_PRESSURE_DROP).AsDouble();
                                string[] pressureDropString = element.Element.get_Parameter(BuiltInParameter.RBS_PRESSURE_DROP).AsValueString().Split();
                                try
                                {

                                    element.PStat = double.Parse(pressureDropString[0], formatter);
                                    
                                    //branch.Pressure += element.PStat;
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    element.PStat = double.Parse(pressureDropString[0], formatter2);
                                   
                                    //branch.Pressure += element.PStat;
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
                                
                                //element.PDyn = Density * Math.Pow(customValve.Velocity, 2) / 2 * customValve.LocRes;
                                //element.LocRes = customValve.LocRes;
                                /* if (element.Element.LookupParameter("AirTree_dP").AsDouble() != 0)
                                 { element.PDyn = element.Element.LookupParameter("AirTree_dP").AsDouble(); }
                                 element.ModelHydraulicArea = Convert.ToString(customValve.AirTree_Area);*/
                                //branch.Pressure += 6;
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





        /*public void ResCalculate()
        {
            foreach (var branch in Collection)
            {
                

                for (int i = 1; i < branch.Elements.Count; i++)
                {
                   
                    
                    branch.Elements[i].Ptot = branch.Elements[i].PDyn + branch.Elements[i].PStat + branch.Elements[i - 1].Ptot;

                }
                branch.PBTot = branch.Elements.Last().Ptot;
            }
        }*/
           /* List<CustomBranch> newCollection = new List<CustomBranch>();

            foreach (var branch in Collection)
            {
                CustomBranch customBranch = new CustomBranch(Document);
                foreach (var element in branch.Elements)
                {
                    if (element.DetailType == CustomElement.Detail.TapAdjustable)
                    {
                        customBranch.AddSpecial(element);
                        customBranch.AddSpecial(element);
                    }
                    else if (element.DetailType == CustomElement.Detail.Tee)
                    {
                        customBranch.AddSpecial(element);
                        customBranch.AddSpecial(element);
                    }
                    else
                    {
                        customBranch.Add(element);
                    }
                   
                }
                newCollection.Add(customBranch);
            }
            Collection = newCollection;*/


            /*foreach (var branch in Collection)
            {
                for (int i =0; i<branch.Elements.Count;i++)
                //foreach (var element in branch.Elements)
                {
                    try
                    {


                        var element = branch.Elements[i];
                        if (element.DetailType == CustomElement.Detail.TapAdjustable)
                        {
                            if (element.ElementId.IntegerValue == 10307806)

                            {
                                var element3 = element;
                            }

                            bool isReversed = FindPrevious(element,branch);

                            CustomDuctInsert2 customDuctInsert = new CustomDuctInsert2(Document, element, Collection, isReversed);
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


                           *//* CustomElement element2 = new CustomElement(Document, branch.Elements[i + 1].ElementId);
                            CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(Document, element2, Collection, true);
                            element2.IA = customDuctInsert2.IA;
                            element2.IQ = customDuctInsert2.IQ;
                            element2.IC = customDuctInsert2.IC;
                            element2.O1A = customDuctInsert2.O1A;
                            element2.O1Q = customDuctInsert2.O1Q;
                            element2.O1C = customDuctInsert2.O1C;
                            element2.O2A = customDuctInsert2.O2A;
                            element2.O2Q = customDuctInsert2.O2Q;
                            element2.RA = customDuctInsert2.RA;
                            element2.RQ = customDuctInsert2.RQ;
                            element2.RC = customDuctInsert2.RC;
                            element2.LocRes = customDuctInsert2.LocRes;
                            element2.PDyn = Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * element2.LocRes;
                            i++;*//*

                        }

                        
                        else if (element.DetailType == CustomElement.Detail.Tee)
                        {
                            if (element.ElementId.IntegerValue == 5956301)

                            {
                                var element3 = element;
                            }
                            bool isReversed = FindPrevious(element, branch);
                            CustomTee2 customDuctInsert = new CustomTee2(Document, element, Collection, isReversed);
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

                            

                        }
                        
                    }*/
                    
                    /*catch
                    {
                        //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                        *//*ActiveElement = element;
                        ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";*//*
                        //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                    }*/
                   
            /*    }
                
               
            }*/
            







































            /*List<CustomBranch> newCollection = new List<CustomBranch>();
            CustomBranch newBranch = new CustomBranch(Document);
            foreach (var branch in Collection)
            {
                

                foreach (var element in branch.Elements)
                {
                    newBranch.Add(element);

                    try
                    {

                        if (element.DetailType == CustomElement.Detail.TapAdjustable)
                        {
                            if (element.ElementId.IntegerValue == 10307806)

                            {
                                var element3 = element;
                            }

                            CustomDuctInsert2 customDuctInsert = new CustomDuctInsert2(Document, element, Collection, element.IsReversed);
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
                            newBranch.AddSpecial (element);

                            var element2 = new CustomElement(Document, element.ElementId);
                            CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(Document, element2, Collection, element.IsReversed);
                            element2.IA = customDuctInsert2.IA;
                            element2.IQ = customDuctInsert2.IQ;
                            element2.IC = customDuctInsert2.IC;
                            element2.O1A = customDuctInsert2.O1A;
                            element2.O1Q = customDuctInsert2.O1Q;
                            element2.O1C = customDuctInsert2.O1C;
                            element2.O2A = customDuctInsert2.O2A;
                            element2.O2Q = customDuctInsert2.O2Q;
                            element2.RA = customDuctInsert2.RA;
                            element2.RQ = customDuctInsert2.RQ;
                            element2.RC = customDuctInsert2.RC;
                            element2.LocRes = customDuctInsert2.LocRes;
                            element2.PDyn = Density * Math.Pow(customDuctInsert2.Velocity, 2) / 2 * element2.LocRes;
                            newBranch.AddSpecial(element2);


                        }


                        else if (element.DetailType == CustomElement.Detail.Tee)
                        {
                            if (element.ElementId.IntegerValue == 5956301)

                            {
                                var element3 = element;
                            }
                            CustomTee2 customDuctInsert = new CustomTee2(Document, element, Collection, element.IsReversed);
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
                            newBranch.AddSpecial(element);
                            var element2 = new CustomElement(Document, element.ElementId);
                            CustomTee2 customDuctInsert2 = new CustomTee2(Document, element2, Collection, element.IsReversed);
                            element2.IA = customDuctInsert2.IA;
                            element2.IQ = customDuctInsert2.IQ;
                            element2.IC = customDuctInsert2.IC;
                            element2.O1A = customDuctInsert2.O1A;
                            element2.O1Q = customDuctInsert2.O1Q;
                            element2.O1C = customDuctInsert2.O1C;
                            element2.O2A = customDuctInsert2.O2A;
                            element2.O2Q = customDuctInsert2.O2Q;
                            element2.RA = customDuctInsert2.RA;
                            element2.RQ = customDuctInsert2.RQ;
                            element2.RC = customDuctInsert2.RC;
                            element2.LocRes = customDuctInsert2.LocRes;
                            element2.PDyn = Density * Math.Pow(customDuctInsert2.Velocity, 2) / 2 * element2.LocRes;
                            newBranch.AddSpecial(element2);

                        }
                        else
                        {
                            newBranch.Add(element);
                        }
                        newCollection.Add(newBranch);
                    }
                    catch
                    {
                        ActiveElement = element;
                        ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                    }
                }
               
            }

            Collection = newCollection;*/









            /* List<CustomElement> newElements = new List<CustomElement>();
             foreach (var branch in Collection)
             {

                 foreach (var element in branch.Elements)
                 {

                     try
                     {

                         if (element.DetailType == CustomElement.Detail.TapAdjustable)
                         {
                             if (element.ElementId.IntegerValue == 10307806)

                             {
                                 var element3 = element;
                             }

                             CustomDuctInsert2 customDuctInsert = new CustomDuctInsert2(Document, element, Collection, element.IsReversed);
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
                             newElements.Add(element);

                             var element2 = new CustomElement(Document, element.ElementId);
                             CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(Document, element2, Collection, element.IsReversed);
                             element2.IA = customDuctInsert2.IA;
                             element2.IQ = customDuctInsert2.IQ;
                             element2.IC = customDuctInsert2.IC;
                             element2.O1A = customDuctInsert2.O1A;
                             element2.O1Q = customDuctInsert2.O1Q;
                             element2.O1C = customDuctInsert2.O1C;
                             element2.O2A = customDuctInsert2.O2A;
                             element2.O2Q = customDuctInsert2.O2Q;
                             element2.RA = customDuctInsert2.RA;
                             element2.RQ = customDuctInsert2.RQ;
                             element2.RC = customDuctInsert2.RC;
                             element2.LocRes = customDuctInsert2.LocRes;
                             element2.PDyn = Density * Math.Pow(customDuctInsert2.Velocity, 2) / 2 * element2.LocRes;
                             newElements.Add(element2);


                         }


                         else if (element.DetailType == CustomElement.Detail.Tee)
                         {
                             if (element.ElementId.IntegerValue == 5956301)

                             {
                                 var element3 = element;
                             }
                             CustomTee2 customDuctInsert = new CustomTee2(Document, element, Collection, element.IsReversed);
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
                             newElements.Add(element);
                             var element2 = new CustomElement(Document, element.ElementId);
                             CustomTee2 customDuctInsert2 = new CustomTee2(Document, element2, Collection, element.IsReversed);
                             element2.IA = customDuctInsert2.IA;
                             element2.IQ = customDuctInsert2.IQ;
                             element2.IC = customDuctInsert2.IC;
                             element2.O1A = customDuctInsert2.O1A;
                             element2.O1Q = customDuctInsert2.O1Q;
                             element2.O1C = customDuctInsert2.O1C;
                             element2.O2A = customDuctInsert2.O2A;
                             element2.O2Q = customDuctInsert2.O2Q;
                             element2.RA = customDuctInsert2.RA;
                             element2.RQ = customDuctInsert2.RQ;
                             element2.RC = customDuctInsert2.RC;
                             element2.LocRes = customDuctInsert2.LocRes;
                             element2.PDyn = Density * Math.Pow(customDuctInsert2.Velocity, 2) / 2 * element2.LocRes;
                             newElements.Add(element2);

                         }
                         else
                         {
                             newElements.Add(element);
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
                 //branch.Elements = newElements;

                 //List<CustomElement> newElements2 = new List<CustomElement>();
                 foreach (var element in branch.Elements)
                 {

                     try
                     {

                     }
                     catch
                     {
                         //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                         *//* ActiveElement = element;
                          ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";*//*
                     }

                 }
                 branch.Elements = newElements;
             }*/




            /*
                        List<CustomBranch> newCollection = new List<CustomBranch>();

                        //foreach (var branch in Collection)
                        foreach (var branch in Collection)
                        {

                            CustomBranch newbranch = new CustomBranch(Document);

                            foreach (var element in branch.Elements)
                            {

                                try
                                {
                                    if (element.DetailType == CustomElement.Detail.TapAdjustable)
                                    {
                                        if (element.ElementId.IntegerValue == 6044542)

                                        {
                                            var element3 = element;
                                        }


                                        //element2 = element;
                                        //element2 = element.Clone(Document, element.ElementId, element);

                                        //ПЕРВЫЙ ПРОХОД
                                        CustomDuctInsert2 customDuctInsert = new CustomDuctInsert2(Document, element, Collection, element.IsReversed);
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
                                        newbranch.AddSpecial(element);
                                        // ВТОРОЙ ПРОХОД
                                        var element2 = new CustomElement(Document, element.ElementId);
                                        CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(Document, element2, Collection, element.IsReversed);
                                        element2.IA = customDuctInsert2.IA;
                                        element2.IQ = customDuctInsert2.IQ;
                                        element2.IC = customDuctInsert2.IC;
                                        element2.O1A = customDuctInsert2.O1A;
                                        element2.O1Q = customDuctInsert2.O1Q;
                                        element2.O1C = customDuctInsert2.O1C;
                                        element2.O2A = customDuctInsert2.O2A;
                                        element2.O2Q = customDuctInsert2.O2Q;
                                        element2.RA = customDuctInsert2.RA;
                                        element2.RQ = customDuctInsert2.RQ;
                                        element2.RC = customDuctInsert2.RC;
                                        element2.LocRes = customDuctInsert2.LocRes;
                                        element2.PDyn = Density * Math.Pow(customDuctInsert2.Velocity, 2) / 2 * element2.LocRes;
                                        newbranch.AddSpecial(element2);
                                    }
                                    else
                                    {
                                        newbranch.Add(element);
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
                            newbranch.Number++;

                            newCollection.Add(newbranch);
                        }

                        Collection = newCollection;*/


            /*List<CustomBranch> newCollection2 = new List<CustomBranch>();

            foreach (var branch in Collection)
            {
                CustomBranch newbranch = new CustomBranch(Document);

                foreach (var element in branch.Elements)
                {
                    try
                    {
                        if (element.DetailType == CustomElement.Detail.Tee)
                        {
                            if (element.ElementId.IntegerValue == 5956301)

                            {
                                var element3 = element;
                            }

                            //CustomElement element2 = new CustomElement(Document, element.ElementId);
                            //newbranch.Add(element2);
                            //element2 = element.Clone(Document, element.ElementId);

                            CustomTee2 customDuctInsert = new CustomTee2(Document, element, Collection, element.IsReversed);
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
                            newbranch.AddSpecial(element);

                            var element2 = new CustomElement(Document, element.ElementId);
                            CustomTee2 customDuctInsert2 = new CustomTee2(Document, element2, Collection, element.IsReversed);
                            element2.IA = customDuctInsert2.IA;
                            element2.IQ = customDuctInsert2.IQ;
                            element2.IC = customDuctInsert2.IC;
                            element2.O1A = customDuctInsert2.O1A;
                            element2.O1Q = customDuctInsert2.O1Q;
                            element2.O1C = customDuctInsert2.O1C;
                            element2.O2A = customDuctInsert2.O2A;
                            element2.O2Q = customDuctInsert2.O2Q;
                            element2.RA = customDuctInsert2.RA;
                            element2.RQ = customDuctInsert2.RQ;
                            element2.RC = customDuctInsert2.RC;
                            element2.LocRes = customDuctInsert2.LocRes;
                            element2.PDyn = Density * Math.Pow(customDuctInsert2.Velocity, 2) / 2 * element2.LocRes;
                            newbranch.AddSpecial(element2);

                        }
                        else
                        {
                            newbranch.Add(element);
                        }
                    }
                    catch
                    {
                        //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                        ActiveElement = element;
                        ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                    }

                }
                newCollection2.Add(newbranch);

            }
            Collection = newCollection2;*/


          /*  foreach (var branch in Collection)
            {
                branch.PBTot = 0;

                for (int i = 1; i < branch.Elements.Count; i++)
                {

                    branch.Elements[i].Ptot = branch.Elements[i].PDyn + branch.Elements[i].PStat + branch.Elements[i - 1].Ptot;

                }

            }
            if (ErrorString == null)
            {

            }
            else if (ErrorString != null || ErrorString.Length != 0)
            {
                TaskDialog.Show("Ошибка в системе", $"Система {FirstElement}\n {ErrorString}");
            }







            //Финальный пересчет


        }*/

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











        //foreach (var element in branch.Elements)*/
        /*public void TeeFinder()
        {
            List<ElementId> checkedIds = new List<ElementId>();
            Collection = Collection.OrderByDescending(x => x.PBTot).ToList();

            List<CustomBranch> newCollection = new List<CustomBranch>();
            //CustomBranch selectedBranch = Collection.OrderByDescending(x => x.PBTot).FirstOrDefault();
            foreach (var selectedBranch in Collection)
            {
                if (selectedBranch.IsVisited==true)
                {
                    continue;
                }

                if (selectedBranch.Elements.First().DetailType == CustomElement.Detail.AirTerminal)
                {
                    CustomElement lastElement = null;
                   
                    //selectedBranch.IsVisited = true;
                    CustomBranch newBranch = new CustomBranch(Document);
                    newBranch = selectedBranch;
                    //CustomBranch selectedBranch2 = selectedBranch;
                    lastElement=  selectedBranch.Elements.Last();
                        *//*do
                        {*//*
                        ElementId selectedElement = lastElement.NextElementId;
                    CustomElement customElement = Tees.Select(x => x).Where(x => x.ElementId == selectedElement;
                            if (Tees.Select(x=>x).Where(x=>x.ElementId==selectedElement) )
                            {
                                if (element.ElementId.IntegerValue == 644205)
                                {
                                    var element2 = element;
                                }
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
                                    checkedIds.Add(element.ElementId);
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
                                    checkedIds.Add(element.ElementId);
                                }
                                selectedBranch.Add(element);
                                selectedBranch.BranchCalc();
                            }
                        

                    CustomElement nextElement = null;
                    do
                    {
                        nextElement = FindNext(selectedBranch, Collection);
                    }
                    while (nextElement != null);


                   
                    
                   
                    newCollection.Add(selectedBranch);
                }

             }

            Collection = newCollection;
        }*/

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
        private int GetNeighbour(ElementId element, List<CustomBranch> collection)
        {


            /*foreach (var branch in collection)
            {
                foreach (var element in branch.Elements)
                {
                    if (element.ElementId == neighbourg)
                    {
                       
                        return branch.Number; // возвращаем элемент сразу после нахождения
                    }
                }
            }
            // если не найден, просто возвращаем null*/
            return -1;
        }




        /*public void TeeSolver(CustomBranch selectedBranch)
        {
            List<CustomBranch> newCollection = new List<CustomBranch>();
            CustomBranch researchedBranch = selectedBranch;

            double pressure1 = 0;
            int selectedEnd = 0;
            ElementId elementId = null;

            ProcessSelectedBranch(selectedBranch, ref pressure1, ref selectedEnd, ref elementId);

            int minimalIndex = int.MaxValue;
            int correctBranch = -1;
            FindCorrectBranch(ref minimalIndex, ref correctBranch, elementId, researchedBranch);

            if (correctBranch != -1)
            {
                UpdateCollectionElements(correctBranch, minimalIndex);
            }
        }*/
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
                    selectedend = i; // Так как i увеличится в следующей итерации
                    

                    return selectedend;

                }
            }
            return -1;
        }
        public (List<CustomBranch>, CustomBranch) TeeSolver()
        {
            CustomBranch selectedBranch = null;
            List<CustomElement> tees = new List<CustomElement>();
            CustomElement selectedTee = null;
            int selectedBranchNumber;
            //Тут обработали ветку, на которой расположен тройник, который имеет только присоединение одной решетки.
            foreach (var branch in Collection)
            {
                for (int ind =0;ind<branch.Elements.Count; ind++)
                {
                    CustomElement el = branch.Elements[ind];
                    if (el.ElementId.IntegerValue== 644211)
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
            int nextelement = -1;

            for (int i = 0; i < selectedBranch.Elements.Count; i++)
            {
                if (selectedBranch.Elements[i].ElementId.IntegerValue == 644211)
                {
                    var el2 = selectedBranch.Elements[i];
                }
                if (selectedBranch.Elements[i].ElementId.IntegerValue == selectedTee.ElementId.IntegerValue)
                {
                    CustomElement element2 = selectedBranch.Elements[i];
                    CustomTee2 customTee2 = new CustomTee2(Document, element2, Collection, false);
                    UpdateElementProperties(element2, customTee2);
                    nextelement = i;
                    selectedBranch.BranchCalc(nextelement);
                    break; // Завершить цикл после обработки первого найденного элемента
                }
            }

            /*for (int i =0; i<selectedBranch.Elements.Count;i++)
            {
                if (selectedBranch.Elements[i].ElementId.IntegerValue == selectedTee.ElementId.IntegerValue)
                {
                    CustomElement element2 = selectedBranch.Elements[i];
                    CustomTee2 customTee2 = new CustomTee2(Document, element2, Collection, false);
                    UpdateElementProperties(element2, customTee2);
                    nextelement = i + 2;
                    selectedBranch.BranchCalc(nextelement);
                    break;
                }
            }*/
            


            // Тут надо обработвть оставшиеся ветки, кроме выбранной ветки selectedBranch

          
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
                            int index = GetElementIndex(branch, 0);
                           
                            branch.BranchCalc(index-1);
                            break;
                                
                            
                        }
                    }
                }
            }
            Collection = Collection.OrderByDescending(x => x.PBTot).ToList();
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
                if (selectedend==-1)
                {
                    break;
                }
                elementId = researchedBranch.Elements[selectedend].ElementId;
                if (researchedBranch.Elements[selectedend].PluginId==44)
                {
                    var el3 = researchedBranch.Elements[selectedend];
                }
                if (elementId == researchedBranch.Elements[selectedend-1].ElementId)
                {
                    continue;
                }
               /* if (elementId.IntegerValue==644208)
                {
                    var el3 = researchedBranch.Elements[selectedend];
                }*/
                //pressure1 = researchedBranch.Elements[selectedend - 2].Ptot;
                pressure1 = researchedBranch.Elements[selectedend-1].Ptot;


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
                        if (Collection[k].Elements.Any(x=>x.ElementId==elementId))
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
                                    pressure2 = Collection[correctBranch].Elements[minimalIndex-1].Ptot;
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
                                    for (int j = minimalIndex+1; j < Collection[correctBranch].Elements.Count; j++)
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
                                    if (selectedend ==-1)
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
            while (researchedBranch.Elements.Last().NextElementId==null);
            researchedBranch.BranchCalc(researchedBranch.Elements.Count-1);
            List<ElementId> checkedIds = new List<ElementId>();
            foreach (var el in researchedBranch.Elements)
            {
                if(!checkedIds.Contains(el.ElementId))
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

        public  void MarkCollection2 (CustomBranch selectedBranch)
        {
            List<CustomBranch> newCustomCollection = new List<CustomBranch>();
            HashSet<ElementId> checkedElements = new HashSet<ElementId>();




        }





            













        /*public void TeeSolver(CustomBranch selectedBranch)
        {
            List<CustomBranch> newCollection = new List<CustomBranch>();
            CustomBranch resultBranch = new CustomBranch(Document);
            CustomBranch researchedBranch = selectedBranch;

            double pressure1 = 0;
            double pressure2 = 0;
            int start = 0;
            int selectedend = 0;
            int branchend = 0;
            ElementId elementId = null;



            for (int i=0; i< researchedBranch.Elements.Count;i++)
            {
                CustomElement element = selectedBranch.Elements[i];
                if (element.DetailType==CustomElement.Detail.Tee)
                {
                    pressure1 = selectedBranch.Elements[i - 1].Ptot;
                    elementId = selectedBranch.Elements[i - 1].ElementId;

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
                    selectedBranch.BranchCalc();
                    pressure1 = selectedBranch.Elements[i].Ptot;
                    i=+2;
                    selectedend = i;

                    break;

                }

            }
            int brNum = 0;
            for (int k=0; k<Collection.Count;k++)
            {
                if (Collection[k] == researchedBranch)
                {
                    continue;
                }
                else
                {
                    for (int j = 0; j < Collection[k].Elements.Count; j++)
                    {
                        if (Collection[k].Elements[j].ElementId == elementId)
                        {
                            Collection[k].Elements[j].IsNonPrinted = true;
                            j++;
                            CustomElement element2 = Collection[k].Elements[j];
                            CustomTee2 customTee2 = new CustomTee2(Document, element2, Collection, false);
                            element2.IA = customTee2.IA;
                            element2.IQ = customTee2.IQ;
                            element2.IC = customTee2.IC;
                            element2.O1A = customTee2.O1A;
                            element2.O1Q = customTee2.O1Q;
                            element2.O1C = customTee2.O1C;
                            element2.O2A = customTee2.O2A;
                            element2.O2Q = customTee2.O2Q;
                            element2.RA = customTee2.RA;
                            element2.RQ = customTee2.RQ;
                            element2.RC = customTee2.RC;

                            element2.LocRes = customTee2.LocRes;
                            element2.PDyn = Density * Math.Pow(customTee2.Velocity, 2) / 2 * element2.LocRes;
                            Collection[k].BranchCalc();
                            pressure2 = Collection[k].Elements[j].Ptot;
                            brNum = k;
                            branchend = j;
                            break;
                        }

                    }

                }
            }
            if (pressure1 > pressure2)
            {
                for (int i = 0; i < selectedend; i++)
                {
                    researchedBranch.Elements[i].MainTrack = true;
                    resultBranch.Add(selectedBranch.Elements[i]);
                }
            }
            else
            {
                for (int j = 0; j < branchend; j++)
                {
                    Collection[brNum].Elements[j].MainTrack = true;
                    resultBranch.Add(Collection[brNum].Elements[j]);

                }
                researchedBranch = Collection[brNum];
            }


            Collection.Add(selectedBranch);


        }*/
        public void ReMarkCollection(CustomBranch selectedBranch)
        {
            List<CustomBranch> newCustomCollection = new List<CustomBranch>();
            HashSet<ElementId> checkedElements = new HashSet<ElementId>();
           
            Dictionary< CustomElement,int> duplicatedElements = new Dictionary<CustomElement , int>();
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
                        if (checkedElements.Contains(element.ElementId))
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
                            element.DetailType == CustomElement.Detail.RectInRoundDuctInsertBranch)
                            {
                                //element.IsNonPrinted = true;
                                
                                
                                element.IsNonPrinted = true;
                                trackCounter--;
                            }
                        }
                        else
                        {
                            element.TrackNumber = trackCounter;
                            element.BranchNumber = branch.Number;
                            element.MainTrack = true;
                            
                            checkedElements.Add(element.ElementId);
                            trackCounter++;
                        }

                    }
                    newCustomCollection.Add(branch);
                    break; // Прекращаем дальнейший обход после нахождения основной ветви 
                }
            }

            // Обрабатываем остальные ветви 
            foreach (var branch in Collection)
            {
                HashSet<ElementId> checkedBranchElements = new HashSet<ElementId>();
                if (branch.Number == selectedBranch.Number)
                {
                    continue;
                }

                CustomBranch newCustomBranch = new CustomBranch(Document);
                int trackCounter = 0;

                foreach (var element in branch.Elements)
                {
                    if (element.ElementId.IntegerValue == 644208)
                    {
                        var element2 = element;
                    }
                    if (checkedElements.Contains(element.ElementId))
                    {
                        if (element.DetailType == CustomElement.Detail.RoundTeeBranch ||
                               element.DetailType == CustomElement.Detail.RoundTeeStraight ||
                               element.DetailType == CustomElement.Detail.RectTeeBranch ||
                               element.DetailType == CustomElement.Detail.RectTeeStraight ||
                               element.DetailType == CustomElement.Detail.RectRoundTeeBranch ||
                               element.DetailType == CustomElement.Detail.RectRoundTeeStraight ||
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
                            if (checkedElements.Contains(element.ElementId))
                            {
                                if (!checkedBranchElements.Contains(element.ElementId))
                                {
                                   
                                    checkedBranchElements.Add(element.ElementId);
                                    element.IsNonPrinted = true;
                                    
                                }
                                else
                                {
                                    element.TrackNumber = trackCounter;
                                    element.BranchNumber = branch.Number;
                                    newCustomBranch.Add(element);
                                    checkedBranchElements.Add(element.ElementId);
                                    trackCounter++;
                                    break;
                                }
                            }
                        }
                       

                               





                        /*if (element.IsReversed==true)
                        {
                            CustomElement newelement = new CustomElement(Document, element.ElementId);
                            if (
                               element.DetailType == CustomElement.Detail.RoundTeeBranch ||
                               element.DetailType == CustomElement.Detail.RoundTeeStraight ||
                               element.DetailType == CustomElement.Detail.RectTeeBranch ||
                               element.DetailType == CustomElement.Detail.RectTeeStraight ||
                               element.DetailType == CustomElement.Detail.RectRoundTeeBranch ||
                               element.DetailType == CustomElement.Detail.RectRoundTeeStraight )
                          

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
                        }
                        // break;
                        var checkedelement = element.NextElementId;
                        //int number = GetNeighbour(checkedelement, Collection);
                        //duplicatedElements.Add(element, number);*/
                    }
                    else
                    {
                        element.TrackNumber = trackCounter;
                        element.BranchNumber = branch.Number;
                        newCustomBranch.Add(element);
                        checkedElements.Add(element.ElementId);
                        trackCounter++;
                    }
                    


                }

                newCustomCollection.Add(newCustomBranch);
               
               
            }

           
                foreach (var kvp in duplicatedElements)
                {
                    var number = kvp.Value;
                    var customElement = kvp.Key;
                 

                    
                        if (number == selectedBranch.Number)
                        {
                            continue;
                        }
                        else
                        {
                            
                        }
                    
                }

            

            // Обновляем коллекцию 
            Collection = newCustomCollection;
            //ResCalculate();
        }



        public string GetContent()
        {

            var csvcontent = new StringBuilder();
            //csvcontent.AppendLine("ElementId;DetailType;ElementName;SystemName;Level;BranchNumber;SectionNumber;Volume;Length;Width;Height;Diameter;HydraulicDiameter;HydraulicArea;IA;IQ;IC;O1A;O1Q;O1C;O2A;O2Q;O2C;RA;RQ;RC;Velocity;PStat;KMS;PDyn;Ptot;Code;MainTrack");
            csvcontent.AppendLine("ElementId;DetailType;ElementName;SystemName;Level;BranchNumber;SectionNumber;Volume;Length;Width;Height;Diameter;HydraulicDiameter;HydraulicArea;Velocity;PStat;KMS;PDyn;Ptot;Code;MainTrack");
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
                        string a = $"{element.ElementId};{element.DetailType};{element.Name};{element.SystemName};{element.Lvl};{element.BranchNumber};{element.TrackNumber};" +
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

        public  void SecondaryBranchSolver(CustomBranch selectedbranch)
        {
            List<ElementId> checkedElements = new List<ElementId>();
            List<CustomBranch> newCollection = new List<CustomBranch>();

            foreach (var el in selectedbranch.Elements)
            {
                checkedElements.Add(el.ElementId);
            }
            newCollection.Add(selectedbranch);
            foreach(CustomBranch customBranch in Collection)
            {
                if (customBranch.Number!= selectedbranch.Number)
                {
                    CustomBranch branch1 = new CustomBranch(Document);
                    foreach (var el in customBranch.Elements)
                    {
                        if (!checkedElements.Contains(el.ElementId))
                        {
                            checkedElements.Add(el.ElementId);
                            branch1.Add(el);
                        }
                        else
                        {
                            if (el.DetailType.ToString().Contains("Tee"))  
                            {
                                CustomTee2 customTee = new CustomTee2(Document, el, Collection, true);
                                UpdateElementProperties(el, customTee);
                                branch1.Add(el);
                            }
                            if (el.DetailType.ToString().Contains("Insert"))
                            {

                            }
                            break;
                        }
                       
                    }
                    branch1.BranchCalc(branch1.Elements.Count - 1);
                    newCollection.Add(branch1);
                }
               
            }
            Collection = newCollection;
        }
    }
}