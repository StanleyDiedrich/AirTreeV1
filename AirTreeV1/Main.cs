using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using Microsoft.Win32;

namespace AirTreeV1
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]



    
    public class Main : IExternalCommand
    {



        public static Branch  GetNewAdditionalBranches (Document doc, ElementId elementId, List<Branch> mainnodes)
        {
            int counter = 0;
            bool mode = true;
            //List<Branch> mainnodes = new List<Branch>();
            Branch additionalNodes = new Branch();
            Branch mainnode = new Branch();
            Autodesk.Revit.DB.Mechanical.DuctSystemType systemtype;
            string shortsystemname;
            if (doc.GetElement(elementId) is Duct)
            {
                systemtype = ((((doc.GetElement(elementId) as Duct) as MEPCurve).MEPSystem as MechanicalSystem)).SystemType;
                shortsystemname = (doc.GetElement(elementId) as Duct).LookupParameter("Сокращение для системы").AsString();
                Node newnode = new Node(doc, doc.GetElement(elementId), systemtype, shortsystemname, mode, mainnodes);
                additionalNodes.Add(newnode);
                foreach (var node in additionalNodes.Nodes)
                {
                    if (node.AdditionalNodes.Count > 0)
                    {
                        foreach (var branch in node.AdditionalNodes)
                        {
                            mainnodes.Add(branch);
                        }

                    }
                }
            }
            else if (doc.GetElement(elementId) is FamilyInstance)
            {
                shortsystemname = (doc.GetElement(elementId) as FamilyInstance).LookupParameter("Сокращение для системы").AsString();
                var connectors = ((doc.GetElement(elementId) as FamilyInstance)).MEPModel.ConnectorManager.Connectors;
                foreach (Connector connector in connectors)
                {
                    systemtype = connector.DuctSystemType;
                    Node newnode = new Node(doc, doc.GetElement(elementId), systemtype, shortsystemname, mode,mainnodes);
                    additionalNodes.Add(newnode);
                    foreach (var node in additionalNodes.Nodes)
                    {
                        if (node.AdditionalNodes.Count > 0)
                        {
                            foreach (var branch in node.AdditionalNodes)
                            {
                                mainnodes.Add(branch);
                            }

                        }
                    }
                }
            }

            Node lastnode = null;

            do
            {
                lastnode = additionalNodes.Nodes.Last(); // Get the last added node
                DuctSystemType systemtype2;
                string shortsystemname2 = "";
                if (doc.GetElement(elementId) is Duct)
                {
                    systemtype2 = ((((doc.GetElement(elementId) as Duct) as MEPCurve).MEPSystem as MechanicalSystem)).SystemType;
                    shortsystemname2 = (doc.GetElement(elementId) as Duct).LookupParameter("Сокращение для системы").AsString();
                    Node newnode = new Node(doc, doc.GetElement(elementId), systemtype2, shortsystemname2, mode, mainnodes);
                    additionalNodes.Add(newnode);
                   
                }
                else if (doc.GetElement(elementId) is FamilyInstance)
                {
                    shortsystemname2 = (doc.GetElement(elementId) as FamilyInstance).LookupParameter("Сокращение для системы").AsString();
                    var connectors = ((doc.GetElement(elementId) as FamilyInstance)).MEPModel.ConnectorManager.Connectors;
                    foreach (Connector connector in connectors)
                    {
                        systemtype2 = connector.DuctSystemType;
                        Node newnode = new Node(doc, doc.GetElement(elementId), systemtype2, shortsystemname2, mode, mainnodes);
                        additionalNodes.Add(newnode);
                        foreach (var node in additionalNodes.Nodes)
                        {
                            if (node.AdditionalNodes.Count > 0)
                            {
                                foreach (var branch in node.AdditionalNodes)
                                {
                                    mainnodes.Add(branch);
                                }

                            }
                        }

                    }
                }



                try
                {
                    var nextElement = doc.GetElement(lastnode.NextOwnerId);
                    Node newnode = new Node(doc, nextElement, lastnode.DuctSystemType, shortsystemname2, mode, mainnodes);
                    additionalNodes.Add(newnode);
                    foreach (var node  in additionalNodes.Nodes)
                    {
                        if (node.AdditionalNodes.Count>0)
                        {
                            foreach (var branch in node.AdditionalNodes)
                            {
                                mainnodes.Add(branch);
                            }
                            
                        }
                    }
                    // Add the new node to the nodes list
                    //lastnode =additionalNodes.Nodes.Last();
                }
                catch
                {
                    break;
                }



            }
            while (lastnode.NextOwnerId != null);
            
            return additionalNodes;
            
        }

        public void SelectAllNodes(UIDocument uidoc, List<Branch> mainnodes)
        {
            List<ElementId> totalids = new List<ElementId>();
            foreach (var mainnode in mainnodes)
            {
                foreach (var node in mainnode.Nodes)
                {


                    totalids.Add(node.ElementId);


                }
            }

            uidoc.Selection.SetElementIds(totalids);
        }

        public string GetContent(Autodesk.Revit.DB.Document doc, List<Branch> mainnodes)
        {
            List<ElementId> checkednodes = new List<ElementId>();
            string csvcontent = "";
            int branchcounter = 0;
            foreach (var mainnode in mainnodes)
            {
                int counter = 0;
                foreach (var node in mainnode.Nodes)
                {
                    if (checkednodes.Contains(node.ElementId))
                    {
                        continue;
                    }
                    else
                    {
                        ModelElement modelelement = new ModelElement(doc, node, branchcounter, counter);
                        checkednodes.Add(node.ElementId);
                        string a = $"{modelelement.ModelElementId};{modelelement.ModelTrack};{modelelement.ModelLvl};{modelelement.ModelBranchNumber};{modelelement.ModelTrackNumber};{modelelement.ModelName};{modelelement.ModelDiameter};{modelelement.ModelLength};{modelelement.ModelVolume};{modelelement.Type.ToString()};{modelelement.ModelTrack}-{modelelement.ModelLvl}-{modelelement.ModelBranchNumber}-{modelelement.ModelTrackNumber}\n";
                        csvcontent += a;
                        counter++;
                    }



                }
                branchcounter++;
            }
            return csvcontent;
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

        private ElementId GetStartDuct(Autodesk.Revit.DB.Document document, string selectedSystemNumber)
        {
            ElementId startDuct = null;
            List<Element> ducts = new List<Element>();
            List<Element> sysducts = new List<Element>();
            ducts = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_DuctCurves).WhereElementIsNotElementType().ToElements().ToList();

            foreach (var duct in ducts)
            {
                //var newpipe = duct as Duct;
              // var fI = newpipe as MEPCurve;
              try
                {
                    if (duct.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString().Equals(selectedSystemNumber))
                    {
                        sysducts.Add(duct);
                    }
                }
                catch
                { }
                
            }

            double maxflow = -100000000;
            Element startpipe = null;
            foreach (var pipe in sysducts)
            {
                var flow = pipe.get_Parameter(BuiltInParameter.RBS_DUCT_FLOW_PARAM).AsDouble();
                if (flow > maxflow)
                {
                    startpipe = pipe;
                    maxflow = flow;
                }
            }
            startDuct = startpipe.Id;

            return startDuct;
        }

        private List<Branch> AlgorithmDuctTraverse(Document doc, List<ElementId> startelements)
        {
            List<Branch> mainnodes = new List<Branch>(); // тут стояк 
            List<Branch> secondarynodes = new List<Branch>();
            List<Branch> secondarySupernodes = new List<Branch>();
            List<Branch> branches = new List<Branch>();
            List<Branch> additionalNodes = new List<Branch>();
            Branch addnodes = new Branch();
            List<Branch> secAdditionalNodes = new List<Branch>();
            List<ModelElement> modelElements = new List<ModelElement>();
            PipeSystemType systemtype;
            string shortsystemname;

            foreach (var startelement in startelements)
            {
                (mainnodes, additionalNodes) = GetNewBranches(doc, startelement);


            }

            

            foreach (var branch in mainnodes)
            {
                foreach (var node in branch.Nodes)
                {
                    if (node.Connectors.Count == 2 && node.Connectors.Any(x => x.IsSelected == false) && node.Element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                    {
                        addnodes.Add(node);
                    }
                }
            }
            var totalIds = new HashSet<int>();
            foreach (var el in addnodes.Nodes)
            {
                el.IsOCK = false;
            }
            foreach (var startelement in addnodes.Nodes)
            {
                var nextStartelement = startelement.ElementId;
                (secondarynodes, secAdditionalNodes) = GetNewBranches(doc, nextStartelement);

                foreach (var secondarynode in secondarynodes)
                {
                    Branch branch = new Branch();
                    foreach (var node in secondarynode.Nodes)
                    {
                        node.IsOCK = false;
                        if (totalIds.Add(node.ElementId.IntegerValue))
                        {
                            branch.Add(node);
                        }
                    }
                    // Добавляем только уникальные элементы
                    if (branch.Nodes.Count != 0)
                    {
                        secondarySupernodes.Add(branch);
                    }
                    else
                    { continue; }

                }
            }
            mainnodes.AddRange(secondarySupernodes);

            foreach (var branch in additionalNodes)
            {
               
                    mainnodes.Add(branch);
                
            }
            return mainnodes;
        }

        public (List<Branch> mainnodes, List<Branch> additionalNodes) GetNewBranches(Document doc, ElementId elementId)
        {
            int counter = 0;
            bool mode = false;
            List<Branch> mainnodes = new List<Branch>();
            List<Branch> additionalNodes = new List<Branch>();
            Branch additionalBranch = new Branch();
            Branch mainnode = new Branch();
            Autodesk.Revit.DB.Mechanical.DuctSystemType systemtype;
            string shortsystemname;
            if (doc.GetElement(elementId) is Duct)
            {
                systemtype = ((((doc.GetElement(elementId) as Duct) as MEPCurve).MEPSystem as MechanicalSystem)).SystemType;
                shortsystemname = (doc.GetElement(elementId) as Duct).LookupParameter("Сокращение для системы").AsString();
                Node newnode = new Node(doc, doc.GetElement(elementId), systemtype, shortsystemname, mode, mainnodes);
                mainnode.Add(newnode);
                if (newnode.AdditionalNodes.Count > 0)
                {
                    foreach (var branch in newnode.AdditionalNodes)
                    {
                        additionalNodes.Add(branch);

                    }
                }


            }
            else if (doc.GetElement(elementId) is FamilyInstance)
            {
                shortsystemname = (doc.GetElement(elementId) as FamilyInstance).LookupParameter("Сокращение для системы").AsString();
                var connectors = ((doc.GetElement(elementId) as FamilyInstance)).MEPModel.ConnectorManager.Connectors;
                foreach (Connector connector in connectors)
                {
                    systemtype = connector.DuctSystemType;
                    Node newnode = new Node(doc, doc.GetElement(elementId), systemtype, shortsystemname, mode, mainnodes);
                    mainnode.Add(newnode);
                    if (newnode.AdditionalNodes.Count > 0)
                    {
                        foreach (var branch in newnode.AdditionalNodes)
                        {
                            additionalNodes.Add(branch);

                        }
                    }

                }
            }

            Node lastnode = null;

            do
            {
                lastnode = mainnode.Nodes.Last(); // Get the last added node
                DuctSystemType systemtype2;
                string shortsystemname2="";
                if (doc.GetElement(elementId) is Duct)
                {
                    systemtype2 = ((((doc.GetElement(elementId) as Duct) as MEPCurve).MEPSystem as MechanicalSystem)).SystemType;
                    shortsystemname2 = (doc.GetElement(elementId) as Duct).LookupParameter("Сокращение для системы").AsString();
                    Node newnode = new Node(doc, doc.GetElement(elementId), systemtype2, shortsystemname2, mode, mainnodes);
                    mainnode.Add(newnode);
                    if (newnode.AdditionalNodes.Count > 0)
                    {
                        foreach (var branch in newnode.AdditionalNodes)
                        {
                            additionalNodes.Add(branch);

                        }
                    }


                }
                else if (doc.GetElement(elementId) is FamilyInstance)
                {
                    shortsystemname2 = (doc.GetElement(elementId) as FamilyInstance).LookupParameter("Сокращение для системы").AsString();
                    var connectors = ((doc.GetElement(elementId) as FamilyInstance)).MEPModel.ConnectorManager.Connectors;
                    foreach (Connector connector in connectors)
                    {
                        systemtype2 = connector.DuctSystemType;
                        Node newnode = new Node(doc, doc.GetElement(elementId), systemtype2, shortsystemname2, mode, mainnodes);
                        mainnode.Add(newnode);
                        if (newnode.AdditionalNodes.Count > 0)
                        {
                            foreach (var branch in newnode.AdditionalNodes)
                            {
                                additionalNodes.Add(branch);

                            }
                        }
                    }
                }



                try
                {
                    if(lastnode.ElementId.IntegerValue== 5632232)
                    {
                        lastnode = lastnode;
                    }
                    var nextElement = doc.GetElement(lastnode.NextOwnerId);
                    Node newnode = new Node(doc, nextElement, lastnode.DuctSystemType, shortsystemname2, mode, mainnodes);
                    mainnode.Add(newnode); // Add the new node to the nodes list
                    if (newnode.AdditionalNodes.Count > 0)
                    {
                        foreach (var branch in newnode.AdditionalNodes)
                        {
                            additionalNodes.Add(branch);

                        }
                    }
                    lastnode = mainnode.Nodes.Last();
                }
                catch
                {
                    break;
                }

                

            }
            while (lastnode.NextOwnerId != null);
            mainnodes.Add(mainnode);
           /* string a = "";
            foreach (var branch in additionalNodes)
            {
                foreach (var node in branch.Nodes)
                {
                    a += node.ElementId.ToString() + ";";
                }
            }
            TaskDialog.Show("R", a);
            foreach (var branch in additionalNodes)
            {
                mainnodes.Add(branch);
            }
            string a = "";
            foreach (var branch in mainnodes)
            {
                foreach (var node in branch.Nodes)
                {
                    a += node.ElementId.ToString() + ";";
                }
            }
            TaskDialog.Show("R", a);*/

            var additionalConnectors = mainnodes.SelectMany(branch => branch.Nodes)
            .SelectMany(node => node.Connectors)
            .Where(connector => connector.IsSelected == false)
            .ToList();
            List<ElementId> additionalElements = additionalConnectors.Select(x => x).Where(x => x.IsSelected == false).Select(x => x.NextOwnerId).ToList();

            List<ElementId> startelement = new List<ElementId>();
            foreach (var addel in additionalElements)
            {

                if (doc.GetElement(addel) is Duct)
                {
                    systemtype = ((((doc.GetElement(addel) as Duct) as MEPCurve).MEPSystem as MechanicalSystem)).SystemType;
                    shortsystemname = (doc.GetElement(addel) as Duct).LookupParameter("Сокращение для системы").AsString();
                    Node newnode = new Node(doc, doc.GetElement(addel), systemtype, shortsystemname, mode, mainnodes);
                    if (newnode.AdditionalNodes.Count > 0)
                    {
                        foreach (var branch in newnode.AdditionalNodes)
                        {
                            additionalNodes.Add(branch);

                        }
                    }
                    additionalBranch.Add(newnode);

                }
                else
                {
                    shortsystemname = (doc.GetElement(addel) as FamilyInstance).LookupParameter("Сокращение для системы").AsString();
                    var connectors = ((doc.GetElement(addel) as FamilyInstance)).MEPModel.ConnectorManager.Connectors;
                    foreach (Connector connector in connectors)
                    {
                        systemtype = connector.DuctSystemType;
                        Node newnode = new Node(doc, doc.GetElement(addel), systemtype, shortsystemname, mode, mainnodes);
                        if (newnode.AdditionalNodes.Count > 0)
                        {
                            foreach (var branch in newnode.AdditionalNodes)
                            {
                                additionalNodes.Add(branch);

                            }
                        }

                        additionalBranch.Add(newnode);

                    }
                }

            }

            mainnodes.Add(additionalBranch);





            var secadditionalNodes = additionalNodes.SelectMany(x => x.Nodes).SelectMany(x => x.Connectors).Where(connector => connector.IsSelected == false).Select(x => x.NextOwnerId).ToList();

            foreach (var addel in secadditionalNodes)
            {
                /*if (addel.IntegerValue == 6246778)
                {
                    ElementId addel2 = addel;
                }*/
                if (addel != null)
                {
                    if (doc.GetElement(addel) is Duct)
                    {
                        systemtype = ((((doc.GetElement(addel) as Duct) as MEPCurve).MEPSystem as MechanicalSystem)).SystemType;
                        shortsystemname = (doc.GetElement(addel) as Duct).LookupParameter("Сокращение для системы").AsString();
                        Node newnode = new Node(doc, doc.GetElement(addel), systemtype, shortsystemname, mode, mainnodes);
                        if (newnode.AdditionalNodes.Count > 0)
                        {
                            foreach (var branch in newnode.AdditionalNodes)
                            {
                                additionalNodes.Add(branch);

                            }
                        }
                        additionalBranch.Add(newnode);


                    }
                    else
                    {
                        shortsystemname = (doc.GetElement(addel) as FamilyInstance).LookupParameter("Сокращение для системы").AsString();
                        var connectors = ((doc.GetElement(addel) as FamilyInstance)).MEPModel.ConnectorManager.Connectors;
                        foreach (Connector connector in connectors)
                        {
                            systemtype = connector.DuctSystemType;
                            Node newnode = new Node(doc, doc.GetElement(addel), systemtype, shortsystemname, mode, mainnodes);
                            if (newnode.AdditionalNodes.Count > 0)
                            {
                                foreach (var branch in newnode.AdditionalNodes)
                                {
                                    additionalNodes.Add(branch);

                                }
                            }
                            additionalBranch.Add(newnode);

                        }
                    }
                }
                else
                {
                    continue;
                }
            }
            additionalNodes.Add(additionalBranch);
            // Continue while NextOwnerId is not null
            return (mainnodes, additionalNodes);
        }

        static AddInId AddInId = new AddInId(new Guid("05B398F6-85A5-4AAF-8EDC-CD14C2DF8E73"));
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uIDocument = uiapp.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uIDocument.Document;

            List<string> systemnumbers = new List<string>();

            IList<Element> ducts = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctCurves).WhereElementIsNotElementType().ToElements();

            foreach (Element duct in ducts)
            {
                var newduct = duct as Duct;

                try
                {
                    if (newduct!=null)
                    {
                        if (!systemnumbers.Contains(newduct.LookupParameter("Имя системы").AsString()))
                        {
                            systemnumbers.Add(newduct.LookupParameter("Имя системы").AsString());
                        }
                    }
                }
                catch(Exception ex)
                {
                    TaskDialog.Show("Revit", ex.ToString());
                }
            }

            ObservableCollection<SystemNumber> sysNums = new ObservableCollection<SystemNumber>();
            foreach (var systemnumber in systemnumbers)
            {
                SystemNumber system = new SystemNumber(systemnumber);
                sysNums.Add(system);
            }
            var sortedSysNums = new ObservableCollection<SystemNumber>(sysNums.OrderBy(x => x.SystemName));

            // Присвоение отсортированной коллекции обратно (если необходимо)
            sysNums = sortedSysNums;


            UserControl1 window = new UserControl1();
            MainViewModel mainViewModel = new MainViewModel(doc, window, sysNums);

            window.DataContext = mainViewModel;
            window.ShowDialog();

            //var selected_workset = mainViewModel.SelectedWorkSet.Name;




            List<ElementId> elIds = new List<ElementId>();
            var systemnames = mainViewModel.SystemNumbersList.Select(x => x).Where(x => x.IsSelected == true);
            //var systemelements = mainViewModel.SystemElements;

            List<ElementId> startelements = new List<ElementId>();

            //Ну тут вроде норм
            foreach (var systemname in systemnames)
            {
                string systemName = systemname.SystemName;

                var maxpipe = GetStartDuct(doc, systemName);
                startelements.Add(maxpipe);

            }

            List<Branch> mainnodes = new List<Branch>();


            mainnodes = AlgorithmDuctTraverse(doc, startelements);
            SelectAllNodes(uIDocument, mainnodes);
            /*var selectedMode = mainViewModel.CalculationModes
            .FirstOrDefault(x => x.IsMode == true);

            if (selectedMode != null)
            {
                int mode = selectedMode.CalculationId;  // Получаем Id расчета
                                                        // Инициализируем список для главных узлов

                switch (mode)
                {
                    case 0:
                        mainnodes = AlgorithmDuctTraverse(doc, startelements);
                        SelectAllNodes(uIDocument, mainnodes);
                        //string csvcontent = GetContent(doc, mainnodes);
                        //SaveFile(csvcontent);
                        break;  // Обязательно добавляем break для правильного выполнения

                    

                    default:  // Обработка случая, если mode не совпадает ни с одним из вышеуказанных
                        throw new InvalidOperationException($"Неизвестный режим расчета: {mode}");
                }
            }*/

            return Result.Succeeded;
        }

        
    }
}
