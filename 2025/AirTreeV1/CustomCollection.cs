using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Globalization;
using System.IO;
using System.Text;

namespace AirTreeV1
{


    public class CustomCollection
    {
        List<CustomBranch> Collection { get; set; } = new List<CustomBranch>();
        public CustomBranch SelectedBranch { get; set; }
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
                        if (element.ElementId.Value == 22896055)
                        {
                            var element2 = element;
                        }
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
                                    if (element.ElementId.Value == 603207)
                                    {
                                        var element2 = element;
                                    }

                                    CustomAirTerminal customAirTerminal = new CustomAirTerminal(Document, element);
                                   
                                    element.PDyn = customAirTerminal.PDyn;
                                    element.Ptot = customAirTerminal.PDyn;
                                    branch.Pressure += element.PDyn;
                                   
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
                                    if (element.ElementId.Value == 10626298)
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
                           /* else if (element.DetailType == CustomElement.Detail.Tee)
                            {
                                try
                                {
                                    if (element.ElementId.IntegerValue == 7331419)
                                    {
                                        var element2 = element;
                                    }




                                    CustomTee2 customDuctInsert = new CustomTee2(Document, element, Collection, false);
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


                                    branch.Pressure += element.PDyn;
                                }
                                catch
                                {
                                    ActiveElement = element;
                                    ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                    //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                                }

                            }*/
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
                                    if (element.ElementId.Value == 8968461)
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

                            /*else if (element.DetailType == CustomElement.Detail.TapAdjustable)
                            {


                                {
                                    var element2 = element;
                                }


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
                            }*/

                            else if (element.DetailType == CustomElement.Detail.Transition)
                            {
                                if (element.ElementId.Value == 11359778)
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
                                if (element.ElementId.Value == 6448413)
                                {
                                    var element2 = element;
                                }
                               
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
                                string[] pressureDropString = element.Element.get_Parameter(BuiltInParameter.RBS_PRESSURE_DROP).AsValueString().Split();
                                element.PStat = double.Parse(pressureDropString[0], formatter);
                                branch.Pressure += element.PStat;

                            }
                            else if (element.DetailType == CustomElement.Detail.FireProtectValve)
                            {
                                if (element.ElementId.Value == 20659396)
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
                           
                            ActiveElement = element;
                            ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                           
                        }
                    }

                }

                FirstElement = Collection.First().Elements.First().SystemName;
            }


        }

        public void CalculateBranchById (CustomElement customElement)
        {
            foreach (var branch in Collection)
            {
                int elNumber = -1;
                for (int i=0; i<branch.Elements.Count;i++)
                {
                    CustomElement element = branch.Elements[i];
                    if (element.ElementId.Value == customElement.ElementId.Value)
                    {
                        elNumber = i;
                    }
                }
                for (int j=1; j<elNumber;j++)
                {
                    branch.Elements[j].PtotControl = branch.Elements[j].PDyn + branch.Elements[j].PStat + branch.Elements[j - 1].PtotControl;
                }
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

            if (ErrorString == null)
            {

            }
            else if (ErrorString != null || ErrorString.Length != 0)
            {
                TaskDialog.Show("Ошибка в системе", $"Система {FirstElement}\n {ErrorString}");
            }

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



        public  void OrderCollection()
        {
           
            foreach (var branch in Collection)
            {
                branch.Pressure = branch.Elements.Last().Ptot;
            }
            Collection = Collection.OrderByDescending(x => x.Pressure).ToList();
            

           

        }







        //foreach (var element in branch.Elements)*/



        public CustomBranch SelectMainBranch()
        {
            
           
            double maxPressure = -1000000000;
            foreach (var branch in Collection)
            {
                if (branch.Elements.Last().Ptot>maxPressure)
                {
                    SelectedBranch= branch;
                    maxPressure = branch.Elements.Last().Ptot;
                }
            }

            return SelectedBranch;
        }

        public void MarkCollection(CustomBranch customBranch)
        {
            List<CustomBranch> newCustomCollection = new List<CustomBranch>();
            HashSet<ElementId> checkedElements = new HashSet<ElementId>();

            // Сначала обрабатываем основную ветвь 
            foreach (var branch in Collection)
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
            }

            


            // Обрабатываем остальные ветви 
            foreach (var branch in Collection)
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
                    if(element.ElementId.Value == 10307806)
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
                           
                            trackCounter++; 
                        }
                        else
                        {
                            continue;
                        }
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

            // Обновляем коллекцию 
            Collection = newCustomCollection;
        }



        public string GetContent()
        {

            var csvcontent = new StringBuilder();
            int maxLines = 50;       // Максимальное количество строк
            int linesCount = 0;      // Счётчик добавленных строк
            //csvcontent.AppendLine("ElementId;DetailType;ElementName;SystemName;Level;BranchNumber;SectionNumber;Volume;Length;Width;Height;Diameter;HydraulicDiameter;HydraulicArea;IA;IQ;IC;O1A;O1Q;O1C;O2A;O2Q;O2C;RA;RQ;RC;Velocity;PStat;KMS;PDyn;Ptot;Code;MainTrack");
            csvcontent.AppendLine("ElementId;DetailType;ElementName;SystemName;Level;BranchNumber;SectionNumber;Volume;Length;Width;Height;Diameter;HydraulicDiameter;HydraulicArea;Velocity;PStat;KMS;PDyn;Ptot;Code;MainTrack");
            foreach (var branch in Collection)
            {
                //for (int i=0; i<51;i++)
                foreach (var element in branch.Elements)
                {
                    
                        //var element = branch.Elements[i];
                        if (element.IsNonPrinted == false)
                        {



                            element.NewModelWidth = Convert.ToString(Convert.ToDouble(element.ModelWidth));
                            element.NewModelHeight = Convert.ToString(Convert.ToDouble(element.ModelHeight));
                            element.ModelVelocity = Convert.ToString(Math.Round(Convert.ToDouble(element.ModelVelocity), 2));
                            element.ModelDiameter = Convert.ToString(Math.Round(Convert.ToDouble(element.ModelDiameter), 2));
                            string a = $"{element.ElementId};{element.DetailType};{element.Name};{element.SystemName};{element.Lvl};{element.BranchNumber};{element.TrackNumber};" +
                                $"{element.Volume};{element.ModelLength};{element.NewModelWidth};{element.NewModelHeight};{element.ModelDiameter};{element.ModelHydraulicDiameter};{element.ModelHydraulicArea};{element.ModelVelocity};{element.PStat};{Math.Round(element.LocRes, 2)};{Math.Round(element.PDyn, 2)};{Math.Round(element.Ptot, 2)};" +

                                $"{element.SystemName}-{element.Lvl}-{element.BranchNumber}-{element.TrackNumber};{element.MainTrack}";
                            csvcontent.AppendLine(a);
                        }


                   /* linesCount++;
                    if (linesCount >= maxLines)
                        return csvcontent.ToString();*/
                }
               /* if (linesCount >= maxLines)
                    return csvcontent.ToString();*/
            }

            return csvcontent.ToString();
        }
        public void SaveFile(string resName,string content) // спрятали функцию сохранения 
        {
            var  saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
            saveFileDialog.Title = "Save CSV File";
            saveFileDialog.FileName = Collection.First().Elements.First().SystemName+$"{resName}" + ".csv";
            if (saveFileDialog.ShowDialog() == true)
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


        public void MarkBranches()
        {
            
            int branchNumber = 0;
            foreach (var branch in Collection)
            {
                CustomBranch reverseBranch = new CustomBranch(Document);
                for (int i = branch.Elements.Count - 1; i > -1; i--)
                {
                    branch.Elements[i].BranchNumber = branchNumber;
                    branch.Elements[i].TrackNumber = i;
                    reverseBranch.Elements.Add(branch.Elements[i]);
                }
                branchNumber++;
               
            }
            
        }
        public void ReverseBranches()
        {

            List<CustomBranch> newCustomCollection = new List<CustomBranch>();
            int branchNumber = 0;
            foreach (var branch in Collection)
            {
                CustomBranch reverseBranch = new CustomBranch(Document);
                for (int i= branch.Elements.Count-1; i>-1;i--)
                {
                    branch.Elements[i].BranchNumber = branchNumber;
                    branch.Elements[i].TrackNumber = i;
                    reverseBranch.Elements.Add(branch.Elements[i]);
                }
                branchNumber++;
                newCustomCollection.Add(reverseBranch);
            }
            Collection = newCustomCollection;
        }

        public List<CustomElement> TryGetElements (CustomElement element)
        {
            List<CustomElement> nextelements = new List<CustomElement>();
            foreach (var branch in Collection)
            {
               for (int j=0; j<branch.Elements.Count; j++)
                {
                    if (branch.Elements[j].ElementId.Value == element.ElementId.Value)
                    {
                        nextelements.Add (branch.Elements[j]);
                    }
                }
            }
            return nextelements;
        }
        public  void OrganizeSystem(CustomBranch selectedbranch)
        {
            List<CustomBranch> newCollection = new List<CustomBranch>();
            CustomBranch mainBranch = new CustomBranch(Document);
           
            CustomElement currentElement = selectedbranch.Elements.Last();
            ElementId stopId = null;
            do
            {
                CustomElement lastElement = selectedbranch.Elements.First();
                stopId = lastElement.ElementId;
                if (currentElement.DetailType == CustomElement.Detail.Tee)
                {

                    List<CustomElement> customElements = TryGetElements(currentElement);
                    //для каждого элемента в этом списке можем пересчитать тройники! 

                    try
                    {
                        if (currentElement.ElementId.Value == 7223473)
                        {
                            var element2 = currentElement;
                        }
                        foreach (var element in customElements)
                        {
                            CustomTee2 customDuctInsert = new CustomTee2(Document, element, Collection, false);
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

                        SelectedBranchesCalculate(customElements);
                       
                        selectedbranch = SelectMainBranchReverse(customElements);
                        CustomElement nextElement = TryGetNextElement(selectedbranch, currentElement);
                        currentElement = nextElement;
                        mainBranch.Elements.Add(currentElement);
                    }
                    catch
                    {
                        ActiveElement = currentElement;
                        ErrorString = "Ошибка в элементе" + $"{currentElement.ElementId}" + "\n";
                        //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                    }

                }
                else
                {
                    CustomElement nextElement = TryGetNextElement(selectedbranch, currentElement);
                    currentElement = nextElement;
                    mainBranch.Elements.Add(currentElement);
                    if (currentElement==null)
                    { break; }
                }


            }
            while (currentElement.ElementId.Value != stopId.Value );
            foreach (var els in mainBranch.Elements)
            {
                els.MainTrack = true;
            }

            
        }
        private CustomBranch SelectMainBranchReverse(List<CustomElement> customElements)
        {
            CustomBranch selectedBranch = null;
            double maxPressure = 0;

            foreach (var el in customElements)
            {
                int branchNumber = el.BranchNumber;

                foreach (var branch in Collection)
                {
                    if (branch.Elements.First().BranchNumber == branchNumber)
                    {
                        double press = branch.Elements.First().Ptot;
                        if (press > maxPressure)
                        {
                            maxPressure = press; // Update to max pressure found
                            selectedBranch = branch;
                        }
                    }
                }
            }

            return selectedBranch; // Return after processing all elements
        }

        private void SelectedBranchesCalculate(List<CustomElement> customElements)
        {
           foreach (var element in customElements)
            {
                int branchNumber = element.BranchNumber;
                CustomBranch selectedBranch = Collection.FirstOrDefault(x => x.Elements.First().BranchNumber == branchNumber);

                for (int i = selectedBranch.Elements.Count-2; i >0; i--)
                {

                    selectedBranch.Elements[i-1].Ptot = selectedBranch.Elements[i].PDyn + selectedBranch.Elements[i].PStat + selectedBranch.Elements[i].Ptot;

                }
            }
        }

        private CustomElement TryGetNextElement(CustomBranch selectedbranch, CustomElement currentElement)
        {
            for (int i= selectedbranch.Elements.Count-1;i>0;i--)
            {
                if (selectedbranch.Elements[i].ElementId.Value == currentElement.ElementId.Value)
                {
                    return selectedbranch.Elements[i - 1];
                }
            }
            return null;
        }

        internal void TeeReCalc(CustomCollection collection)
        {
            foreach (var branch in Collection)
            {

                foreach (var element in branch.Elements)
                {
                    if (element.DetailType == CustomElement.Detail.Tee)
                    {
                        List<CustomElement> elements = TryGetAllTeeElements(element);

                        CustomElement mainTee = elements.OrderByDescending(x => x.Ptot).FirstOrDefault();
                        CustomElement previous = GetPrevious(mainTee);
                        if (element.DetailType == CustomElement.Detail.Tee)
                        {
                            try
                            {
                                if (element.ElementId.Value == 5897783)
                                {
                                    var element2 = element;
                                }

                                CustomTee2 customDuctInsert = new CustomTee2(Document, mainTee, Collection, false);
                                mainTee.IA = customDuctInsert.IA;
                                mainTee.IQ = customDuctInsert.IQ;
                                mainTee.IC = customDuctInsert.IC;
                                mainTee.O1A = customDuctInsert.O1A;
                                mainTee.O1Q = customDuctInsert.O1Q;
                                mainTee.O1C = customDuctInsert.O1C;
                                mainTee.O2A = customDuctInsert.O2A;
                                mainTee.O2Q = customDuctInsert.O2Q;
                                mainTee.RA = customDuctInsert.RA;
                                mainTee.RQ = customDuctInsert.RQ;
                                mainTee.RC = customDuctInsert.RC;
                                mainTee.LocRes = customDuctInsert.LocRes;
                                mainTee.PDyn = Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * mainTee.LocRes;
                                collection.CalculateBranchById(element);
                                collection.SelectMainBranchByControlPressure();

                            }
                            catch
                            {
                                ActiveElement = element;
                                ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                            }

                        }
                        foreach (var el in elements)
                        {
                            CustomElement prev = GetPrevious(el);
                            
                            if (el.PluginId ==mainTee.PluginId)
                            {
                                continue;
                            }
                            
                            try
                            {
                                if (el.ElementId.Value == 7331419)
                                {
                                    var element2 = el;
                                }
                                if (prev.ElementId == previous.ElementId)
                                {
                                    CustomTee2 customDuctInsert = new CustomTee2(Document, el, Collection, false);
                                    el.IA = customDuctInsert.IA;
                                    el.IQ = customDuctInsert.IQ;
                                    el.IC = customDuctInsert.IC;
                                    el.O1A = customDuctInsert.O1A;
                                    el.O1Q = customDuctInsert.O1Q;
                                    el.O1C = customDuctInsert.O1C;
                                    el.O2A = customDuctInsert.O2A;
                                    el.O2Q = customDuctInsert.O2Q;
                                    el.RA = customDuctInsert.RA;
                                    el.RQ = customDuctInsert.RQ;
                                    el.RC = customDuctInsert.RC;
                                    el.LocRes = customDuctInsert.LocRes;
                                    el.PDyn = Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * el.LocRes;
                                    collection.CalculateBranchById(element);
                                    collection.SelectMainBranchByControlPressure();
                                }
                                else
                                {
                                    CustomTee2 customDuctInsert = new CustomTee2(Document, el, Collection, true);
                                    el.IA = customDuctInsert.IA;
                                    el.IQ = customDuctInsert.IQ;
                                    el.IC = customDuctInsert.IC;
                                    el.O1A = customDuctInsert.O1A;
                                    el.O1Q = customDuctInsert.O1Q;
                                    el.O1C = customDuctInsert.O1C;
                                    el.O2A = customDuctInsert.O2A;
                                    el.O2Q = customDuctInsert.O2Q;
                                    el.RA = customDuctInsert.RA;
                                    el.RQ = customDuctInsert.RQ;
                                    el.RC = customDuctInsert.RC;
                                    el.LocRes = customDuctInsert.LocRes;
                                    el.PDyn = Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * el.LocRes;
                                    collection.CalculateBranchById(element);
                                    collection.SelectMainBranchByControlPressure();
                                }
                               
                                



                            }
                            catch
                            {
                                ActiveElement = element;
                                ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                                //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                            }
                        }
                    }
                    
                    if (element.DetailType == CustomElement.Detail.DuctTap)
                    {
                        List<CustomElement> elements = TryGetAllTeeElements(element);

                        CustomElement mainTee = elements.OrderByDescending(x => x.Ptot).FirstOrDefault();
                        CustomElement previous = GetPrevious(mainTee);
                        try
                        {
                            if (element.ElementId.Value == 5897783)
                            {
                                var detailType = element.DetailType;
                                var element2 = element;
                            }

                            CustomDuctInsert3 customDuctInsert = new CustomDuctInsert3(Document, mainTee, Collection, false);
                            mainTee.DetailType = customDuctInsert.Detail;
                            mainTee.IA = customDuctInsert.IA;
                            mainTee.IQ = customDuctInsert.IQ;
                            mainTee.IC = customDuctInsert.IC;
                            mainTee.O1A = customDuctInsert.O1A;
                            mainTee.O1Q = customDuctInsert.O1Q;
                            mainTee.O1C = customDuctInsert.O1C;
                            mainTee.O2A = customDuctInsert.O2A;
                            mainTee.O2Q = customDuctInsert.O2Q;
                            mainTee.RA = customDuctInsert.RA;
                            mainTee.RQ = customDuctInsert.RQ;
                            mainTee.RC = customDuctInsert.RC;
                            mainTee.LocRes = customDuctInsert.LocRes;
                            mainTee.PDyn = Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * mainTee.LocRes;
                            collection.CalculateBranchById(element);
                            collection.SelectMainBranchByControlPressure();


                        }
                        catch
                        {
                            ActiveElement = element;
                            ErrorString = "Ошибка в элементе" + $"{element.ElementId}" + "\n";
                            //TaskDialog.Show("Ошибка", $"Ошибка в элементе {element.ElementId}");
                        }

                        foreach (var el in elements)
                        {
                            CustomElement prev = GetPrevious(el);
                            if (element.ElementId.Value == 5897783)
                            {
                                var detailType = element.DetailType;
                                var element2 = element;
                            }
                            if (prev.DetailType ==CustomElement.Detail.TapAdjustable)
                            {
                                if (el.DetailType.ToString().Contains("Insert"))
                                {
                                    continue;
                                }
                                else
                                {
                                    CustomDuctInsert3 customDuctInsert = new CustomDuctInsert3(Document, el, Collection, true);
                                    el.DetailType = customDuctInsert.Detail;
                                    el.IA = customDuctInsert.IA;
                                    el.IQ = customDuctInsert.IQ;
                                    el.IC = customDuctInsert.IC;
                                    el.O1A = customDuctInsert.O1A;
                                    el.O1Q = customDuctInsert.O1Q;
                                    el.O1C = customDuctInsert.O1C;
                                    el.O2A = customDuctInsert.O2A;
                                    el.O2Q = customDuctInsert.O2Q;
                                    el.RA = customDuctInsert.RA;
                                    el.RQ = customDuctInsert.RQ;
                                    el.RC = customDuctInsert.RC;
                                    el.LocRes = customDuctInsert.LocRes;
                                    el.PDyn = Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * el.LocRes;
                                    collection.CalculateBranchById(element);
                                    collection.SelectMainBranchByControlPressure();
                                }
                               
                            }
                            if (el.DetailType.ToString().Contains("Insert"))
                            {
                               /* CustomDuctInsert3 customDuctInsert = new CustomDuctInsert3(Document, el, Collection, false);
                                el.DetailType = customDuctInsert.Detail;
                                el.IA = customDuctInsert.IA;
                                el.IQ = customDuctInsert.IQ;
                                el.IC = customDuctInsert.IC;
                                el.O1A = customDuctInsert.O1A;
                                el.O1Q = customDuctInsert.O1Q;
                                el.O1C = customDuctInsert.O1C;
                                el.O2A = customDuctInsert.O2A;
                                el.O2Q = customDuctInsert.O2Q;
                                el.RA = customDuctInsert.RA;
                                el.RQ = customDuctInsert.RQ;
                                el.RC = customDuctInsert.RC;
                                el.LocRes = customDuctInsert.LocRes;
                                el.PDyn = Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * el.LocRes;
                                collection.CalculateBranchById(element);
                                collection.SelectMainBranchByControlPressure();*/
                                continue;
                            }
                            else 
                            {
                                CustomDuctInsert3 customDuctInsert = new CustomDuctInsert3(Document, el, Collection, false);
                                el.DetailType = customDuctInsert.Detail;
                                el.IA = customDuctInsert.IA;
                                el.IQ = customDuctInsert.IQ;
                                el.IC = customDuctInsert.IC;
                                el.O1A = customDuctInsert.O1A;
                                el.O1Q = customDuctInsert.O1Q;
                                el.O1C = customDuctInsert.O1C;
                                el.O2A = customDuctInsert.O2A;
                                el.O2Q = customDuctInsert.O2Q;
                                el.RA = customDuctInsert.RA;
                                el.RQ = customDuctInsert.RQ;
                                el.RC = customDuctInsert.RC;
                                el.LocRes = customDuctInsert.LocRes;
                                el.PDyn = Density * Math.Pow(customDuctInsert.Velocity, 2) / 2 * el.LocRes;
                                collection.CalculateBranchById(element);
                                collection.SelectMainBranchByControlPressure();
                            }




                        }
                    }

                   
                   
                    
                }
                
            }
        }

        private CustomBranch SelectMainBranchByControlPressure()
        {
           CustomBranch customBranch = null;
           double maxPressure = -1000000;
           foreach (var branch in Collection)
            {
                foreach (var el in branch.Elements)
                {
                    if (el.PtotControl>maxPressure)
                    {
                        maxPressure = el.PtotControl;
                        customBranch = branch;
                    }
                }
            }
            return customBranch;
        }

        private CustomElement GetPrevious(CustomElement mainTee)
        {
            CustomElement selectedElement = null;
            foreach (var branch in Collection)
            {
                int branchnumber = branch.Elements.First().BranchNumber;
                if (mainTee.BranchNumber == branchnumber)
                {
                    for (int i =0; i<branch.Elements.Count-1;i++)
                    {
                        if (branch.Elements[i].ElementId == mainTee.ElementId)
                        {
                            selectedElement = branch.Elements[i - 1];
                            return selectedElement;
                        }
                    }
                }
            }
            return selectedElement;
        }

        private List<CustomElement> TryGetAllTeeElements(CustomElement element)
        {
            List<CustomElement> elements = new List<CustomElement>();

            foreach (var branch in Collection)
            {
                for (int i =0; i<branch.Elements.Count-1;i++)
                {
                    if (branch.Elements[i].ElementId.Value == element.ElementId.Value)
                    {
                        elements.Add(branch.Elements[i]);

                    }
                }
            }
            return elements;
        }

        public  void GetUniqueElements()
        {
            // если айдишник уже есть, то проверяем на то какой элемент. 
            // если это тройник или врезка, то надо получить максимальное давление на проход и на брэнч
            List<ElementId> elementIds = new List<ElementId>();
            List<int> pluginIds = new List<int>();
            List<CustomBranch> customBranches = new List<CustomBranch>();

            foreach (var branch in Collection)
            {
                CustomBranch customBranch = new CustomBranch(Document);
                foreach (var el in branch.Elements)
                {
                   
                        if(!elementIds.Contains(el.ElementId))
                        {
                            elementIds.Add(el.ElementId);
                            pluginIds.Add(el.PluginId);
                            customBranch.Elements.Add(el);
                        }
                        if (elementIds.Contains(el.ElementId) && !pluginIds.Contains(el.PluginId))
                        {
                            
                            if(el.DetailType.ToString().Contains("Tee") || el.DetailType.ToString().Contains("Insert"))
                            {
                                if (!pluginIds.Contains(el.PluginId))
                                {
                                    customBranch.Elements.Add(el);
                                    break;
                                }
                               
                               
                            }
                            
                            
                        }
                        
                       
                    
                   
                   
                   
                }
                customBranches.Add(customBranch);
            }


            
            Collection = customBranches;
        }

        internal void MarkFirstBranch()
        {
            var selectedBranch = Collection.First();
            foreach (var el in selectedBranch.Elements)
            {
                el.MainTrack = true;
            }
        }
    }
}




