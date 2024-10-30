using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
                    if (element.ElementId.IntegerValue == 6250255)
                    {
                        var element2 = element;
                    }
                    if (element.DetailType==CustomElement.Detail.AirTerminal)
                    {
                        branch.Pressure += 10;
                    }
                    else if (element.DetailType==CustomElement.Detail.Elbow)
                    {
                        CustomElbow customElbow = new CustomElbow(Document, element);
                        element.LocRes = customElbow.LocRes;
                        //branch.Pressure += 5;
                    }
                    else if (element.DetailType==CustomElement.Detail.Tee)
                    {
                        if (element.ElementId.IntegerValue== 6253444)
                        {
                            var element2 = element;
                        }
                            CustomTee customTee = new CustomTee(Document, element);
                        element.LocRes = customTee.LocRes;
                        branch.Pressure += 7;
                    }
                    else if (element.DetailType==CustomElement.Detail.TapAdjustable)
                    {
                        branch.Pressure += 1;
                    }
                    else if (element.DetailType == CustomElement.Detail.Transition)
                    {
                        try
                        {
                            CustomTransition customTransition = new CustomTransition(Document, element);
                        
                            element.LocRes = customTransition.LocRes;
                        }
                        catch
                        {
                            element.LocRes = 0.5;
                        }
                       
                    }
                    else if (element.DetailType==CustomElement.Detail.RectangularDuct || element.DetailType == CustomElement.Detail.RoundDuct)
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
            }

            // Обновляем коллекцию 
            Collection = newCustomCollection;
        }



        public string GetContent()
        {
            var csvcontent = new StringBuilder();
            csvcontent.AppendLine("ElementId;DetailType;SystemName;Level;BranchNumber;SectionNumber;Volume;Length;Width;Height;Diameter;HydraulicDiameter;HydraulicArea;Velocity;KMS;Code;MainTrack");

            foreach (var branch in Collection)
            {
                foreach (var element in branch.Elements)
                {
                    string a = $"{element.ElementId};{element.DetailType};{element.SystemName};{element.Lvl};{element.BranchNumber};{element.TrackNumber};" +
                         $"{element.Volume};{element.ModelLength};{element.ModelWidth};{element.ModelHeight};{element.ModelDiameter};{element.ModelHydraulicDiameter};{element.ModelHydraulicArea};{element.ModelVelocity};{element.LocRes};" +
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
    }
}
