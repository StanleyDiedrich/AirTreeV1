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

namespace AirTreeV1
{


    public class CustomCollection
    {
        public List<CustomBranch> Collection { get; set; } = new List<CustomBranch>();
        Autodesk.Revit.DB.Document Document { get; set; }
        public double Density { get; set; }
        public CustomElement ActiveElement { get; set; }
        public string FirstElement { get; set; }

        public string ErrorString { get; set; }
        public void Add(CustomBranch branch)
        {
            Collection.Add(branch);
        }

        public void CreateBranch(Document document, ElementId airterminal)
        {
            CustomBranch customBranch = new CustomBranch(Document, airterminal);
            customBranch.CreateNewBranch(Document, airterminal);
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
                                    branch.Pressure += element.PDyn;
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
                                    branch.Pressure += element.PDyn;
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
                                    if (element.ElementId.IntegerValue == 6856285)
                                    {
                                        var element2 = element;
                                    }
                                    /* CustomTee customTee = new CustomTee(Document, element);
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
                                    branch.Pressure += 0;
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
                                    branch.Pressure += element.PDyn;


                                    //branch.Pressure += 1;
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                    //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                                }
                            }

                           /* else if (element.DetailType == CustomElement.Detail.TapAdjustable)
                            {



                                {
                                    var element2 = element;
                                }

                                CustomDuctInsert customDuctInsert = new CustomDuctInsert(Document, element);
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
                                branch.Pressure += 1;
                            }*/
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
                                    branch.Pressure += element.PDyn;
                                }
                                catch
                                {
                                    CustomTransition customTransition = new CustomTransition(Document, element);
                                    ActiveElement = element;
                                    element.LocRes = 0.11;
                                    element.PDyn = Density * Math.Pow(customTransition.Velocity, 2) / 2 * element.LocRes;
                                    branch.Pressure += element.PDyn;
                                }

                            }
                            else if (element.DetailType == CustomElement.Detail.RectangularDuct || element.DetailType == CustomElement.Detail.RoundDuct)
                            {
                                if (element.ElementId.IntegerValue == 6448413)
                                {
                                    var element2 = element;
                                }
                                //branch.Pressure += element.Element.get_Parameter(BuiltInParameter.RBS_PRESSURE_DROP).AsDouble();
                                string[] pressureDropString = element.Element.get_Parameter(BuiltInParameter.RBS_PRESSURE_DROP).AsValueString().Split();
                                try
                                {

                                    element.PStat = double.Parse(pressureDropString[0], formatter);
                                    branch.Pressure += element.PStat;
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    element.PStat = double.Parse(pressureDropString[0], formatter2);
                                    branch.Pressure += element.PStat;
                                }
                                // Проверяем, что строка не пустая или null

                            }
                            else if (element.DetailType == CustomElement.Detail.RectFlexDuct || element.DetailType == CustomElement.Detail.RoundFlexDuct)
                            {
                                //branch.Pressure += element.Element.get_Parameter(BuiltInParameter.RBS_PRESSURE_DROP).AsDouble();
                                string[] pressureDropString = element.Element.get_Parameter(BuiltInParameter.RBS_PRESSURE_DROP).AsValueString().Split();
                                element.PStat = double.Parse(pressureDropString[0], formatter);
                                branch.Pressure += element.PStat;

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

                }

                FirstElement = Collection.First().Elements.First().SystemName;
            }


        }
        public void ResCalculate()
        {
            foreach (var branch in Collection)
            {
                branch.PBTot = 0;

                for (int i = 1; i < branch.Elements.Count; i++)
                {

                    branch.Elements[i].Ptot = branch.Elements[i].PDyn + branch.Elements[i].PStat + branch.Elements[i - 1].Ptot;

                }

            }

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


            foreach (var branch in Collection)
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


                           /* CustomElement element2 = new CustomElement(Document, branch.Elements[i + 1].ElementId);
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
                            i++;*/

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
                        
                    }
                    
                    catch
                    {
                        //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                        /*ActiveElement = element;
                        ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";*/
                        //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                    }
                   
                }
                
               
            }
            







































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


            foreach (var branch in Collection)
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











        //foreach (var element in branch.Elements)*/



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
                    if(element.ElementId.IntegerValue == 10307806)
                    {
                        var element2 = element;
                    }
                    if (checkedElements.Contains(element.ElementId))
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
                            newCustomBranch.Add(element);
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
                        newCustomBranch.Add(element);
                        //checkedElements.Add(element.ElementId);
                        trackCounter++;  // Увеличиваем trackCounter только после успешного добавления элемента
                    }

                    
                }

                newCustomCollection.Add(newCustomBranch);
            }

            // Обновляем коллекцию 
            Collection = newCustomCollection;
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

                    /*string a = $"{element.ElementId};{element.DetailType};{element.Name};{element.SystemName};{element.Lvl};{element.BranchNumber};{element.TrackNumber};" +
                         $"{element.Volume};{element.ModelLength};{element.ModelWidth};{element.ModelHeight};{element.ModelDiameter};{element.ModelHydraulicDiameter};{element.ModelHydraulicArea};{element.IA};{element.IQ};{element.IC};{element.O1A};{element.O1Q};{element.O1C};{element.O2A};{element.O2Q};{element.O2C};{element.RA};{element.RQ};{element.RC};{element.ModelVelocity};{element.PStat};{Math.Round(element.LocRes, 2)};{Math.Round(element.PDyn, 2)};{Math.Round(element.Ptot, 2)};" +

                         $"{element.SystemName}-{element.Lvl}-{element.BranchNumber}-{element.TrackNumber};{element.MainTrack}";*/
                    element.NewModelWidth = Convert.ToString(Convert.ToDouble(element.ModelWidth) );
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
    }
}