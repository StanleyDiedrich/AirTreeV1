using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

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
                        

                    }
                }



                try
                {
                    var nextElement = doc.GetElement(lastnode.NextOwnerId);
                    Node newnode = new Node(doc, nextElement, lastnode.DuctSystemType, shortsystemname2, mode, mainnodes);
                    additionalNodes.Add(newnode);
                    /*foreach (var node in additionalNodes.Nodes)
                    {
                        if (node.AdditionalNodes.Count > 0)
                        {
                            foreach (var branch in node.AdditionalNodes)
                            {
                                mainnodes.Add(branch);
                            }

                        }
                    }*/
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
                    foreach (var node in newnode.AdditionalNodes)
                    {
                        additionalNodes.Add(node);
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
                        foreach (var node in newnode.AdditionalNodes)
                        {
                            additionalNodes.Add(node);
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
                        foreach (var node in newnode.AdditionalNodes)
                        {
                            additionalNodes.Add(node);
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
                            foreach (var node in newnode.AdditionalNodes)
                            {
                                additionalNodes.Add(node);
                            }
                        }

                    }
                }



                try
                {
                    var nextElement = doc.GetElement(lastnode.NextOwnerId);
                    Node newnode = new Node(doc, nextElement, lastnode.DuctSystemType, shortsystemname2, mode, mainnodes);
                    mainnode.Add(newnode); // Add the new node to the nodes list
                    if (newnode.AdditionalNodes.Count > 0)
                    {
                        foreach (var node in newnode.AdditionalNodes)
                        {
                            additionalNodes.Add(node);
                        }
                    }
                }
                catch
                {
                    break;
                }



            }
            while (lastnode.NextOwnerId != null);
            mainnodes.Add(mainnode);
            foreach (var node in additionalNodes)
            {
                mainnodes.Add(node);
            }

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
            List<ElementId> selectedterminals = new List<ElementId>();
            List<ElementId> selectedelements = new List<ElementId>();
            //Ну тут вроде норм
            string systemName = null;
            foreach (var systemname in systemnames)
            {
                 systemName = systemname.SystemName;

                //var maxpipe = GetStartDuct(doc, systemName);

                selectedterminals = GetAirTerminals(doc, systemName);
                if (selectedterminals.Count!=0)
                {
                    CustomCollection collection = GetCollection(doc, selectedterminals);

                    //selectedelements = collection.ShowElements(0);
                    //collection.MarkCollection();
                    try
                    {
                        
                        collection.Calcualate(mainViewModel.Density);
                    }
                    catch
                    {
                        CustomElement element = collection.ActiveElement;
                        TaskDialog.Show("Ошибка", $"ошибка в элементе{element.ElementId}");
                    }
                    //List<Tuple<CustomBranch, CustomBranch>> pair = SortPairs(sortedLists);
                    List<CustomBranch> secondaryBranches = new List<CustomBranch>();
                    List<CustomBranch> newCollection = new List<CustomBranch>();
                    List<CustomBranch> returnedBranches = collection.Collection;
                    List<CustomBranch> notSelectedBranch = new List<CustomBranch>();
                    List<CustomBranch> sortedLists = sortLists(returnedBranches);
                    collection.ReorderBranches(sortedLists);
                    collection.CalculateReorderedBranches();
                    collection.ReOrderCollection();

                    






                   

                    //collection.Collection = newCollection;
                    /*collection.MarkCollection();
                    collection.ReMarkCollection(collection.Collection[0]);*/



                    // ЭТО ВАЖНО!!!!


                    //string content = collection.GetContent(selectedBranch);


                    string content = collection.GetContent();
                    string filemname = collection.FirstElement;
                    try
                    {
                        collection.SaveFile(content);

                    }
                    catch
                    {
                        TaskDialog.Show("R", $"Система {filemname} имеет ошибку ");
                    }


                    //ВЕРНИ КАК БЫЛО!
                }
                else
                {
                    TaskDialog.Show("AirTree", $"Система {systemName} не имеет воздухораспределителей"); 
                }
                
                //selectedelements=  collection.ShowElements();
                
                //selectedelements = collection.ShowControlElements();
                //selectedelements = collection;
            }


            /*uIDocument.Selection.SetElementIds(selectedelements);

            List<Branch> mainnodes = new List<Branch>();


             mainnodes = AlgorithmDuctTraverse(doc, startelements);


            //SelectAllNodes(uIDocument, mainnodes);
            var selectedMode = mainViewModel.CalculationModes
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
        private static Tuple<int, int> GetFirstSplitter(Autodesk.Revit.DB.Document doc, CustomBranch branch1, CustomBranch branch2, List<CustomBranch> collection, MainViewModel mainViewModel)
        {
            CustomElement element1 = null;
            CustomElement element2 = null;

            for (int i = 0; i < branch1.Elements.Count; i++)
            {
                for (int j = 0; j < branch2.Elements.Count; j++)
                {
                    if (branch1.Elements[i].ElementId == (branch2.Elements[j].ElementId))
                    {
                        if (branch1.Elements[i].DetailType == CustomElement.Detail.Tee)

                        {
                            if (branch1.Elements[i].IsVisited == false && branch2.Elements[j].IsVisited == false)
                            {

                                CustomElement customElement1 = branch1.Elements[i];
                                CustomElement customElement2 = branch2.Elements[j];

                                CalculateSplitter(doc, customElement1, collection, mainViewModel, customElement1.IsReversed);
                                CalculateSplitter(doc, customElement2, collection, mainViewModel, customElement1.IsReversed);
                                branch1.Elements[i].IsVisited = true;
                                branch2.Elements[j].IsVisited = true;
                                return Tuple.Create(i + 1, j + 1);
                            }
                        }
                    }
                    else
                    {
                        if (branch1.Elements[i].DetailType == CustomElement.Detail.TapAdjustable)
                        {
                            if (branch1.Elements[i].IsVisited == false)
                            {
                                element1 = branch1.Elements[i];
                                foreach (var branch in collection)
                                {
                                    foreach (var el2 in branch.Elements)
                                    {
                                        if (el2.ElementId == element1.NextElementId)
                                        {
                                            element2 = el2;
                                            CalculateSplitter(doc, element1, collection, mainViewModel, true);
                                            CalculateSplitter(doc, element2, collection, mainViewModel, false);
                                            branch1.Elements[i].IsVisited = true;
                                            branch2.Elements[j].IsVisited = true;
                                            return Tuple.Create(i + 1, j + 1);
                                        }
                                    }
                                }
                            }
                        }
                        if (branch1.Elements[i].DetailType == CustomElement.Detail.DuctTap)
                        {
                            if (branch1.Elements[i].ElementId.IntegerValue == 10562834)
                            {
                                var el = branch1.Elements[i];
                            }

                                if (branch1.Elements[i].IsVisited == false)
                                {
                                
                                    CustomElement customElement1 = branch1.Elements[i];
                                    CustomElement customElement2 = null;

                                    foreach (var br2 in collection)
                                    {
                                        foreach (var el2 in br2.Elements)
                                        {
                                            if (el2.ElementId == customElement1.TapId) // Тут ищем TapAdjustable 
                                            {
                                                if (el2.DetailType == CustomElement.Detail.TapAdjustable)
                                                {
                                                    customElement2 = el2;
                                                    CalculateSplitter(doc, customElement1, collection, mainViewModel, true);
                                                    CalculateSplitter(doc, customElement2, collection, mainViewModel, false);
                                                    branch1.Elements[i].IsVisited = true;
                                                    branch2.Elements[j].IsVisited = true;
                                                    return Tuple.Create(i + 1, j + 1);
                                                
                                                }

                                            }
                                        }
                                    }








                               
                                
                            }
                        }
                    }
                       
                        
                 }
                        

                    
                    

                
            }
            return null; // Если нет общих элементов
        }

        private static void CalculateSplitter(Autodesk.Revit.DB.Document doc, CustomElement customElement,List<CustomBranch> collection, MainViewModel mainViewModel, bool isReversed)
        {
            if (customElement.DetailType == CustomElement.Detail.Tee)
            {
                CustomTee2 customTee2 = new CustomTee2(doc, customElement, collection, isReversed);
                UpdateElementProperties(customElement, customTee2, mainViewModel);
            }
            if (customElement.DetailType == CustomElement.Detail.TapAdjustable)
            {
                CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(doc, customElement, collection, isReversed);
                UpdateInsertElementProperties(customElement, customDuctInsert2, collection, mainViewModel);
            }
            if (customElement.DetailType == CustomElement.Detail.DuctTap)
            {
                CustomElement element = null;
                foreach (var branch in collection)
                {
                    foreach (var el in branch.Elements )
                    {
                        if (customElement.TapId == el.ElementId)
                        {
                            element = el;
                        }
                    }
                }

                CustomDuctInsert2 customDuctInsert2 = new CustomDuctInsert2(doc, element, collection, isReversed);
                UpdateInsertElementProperties(customElement, customDuctInsert2, collection, mainViewModel);
            }
        }
        private static void UpdateElementProperties(CustomElement element, CustomTee2 customTee, MainViewModel mainViewModel)
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


            element.PDyn = mainViewModel.Density * Math.Pow(customTee.Velocity, 2) / 2 * element.LocRes;
        }

        private static void UpdateInsertElementProperties(CustomElement element, CustomDuctInsert2 customTee, List<CustomBranch> collection, MainViewModel mainViewModel)
        {
            if (element.DetailType == CustomElement.Detail.DuctTap)
            {
                if (element.ElementId.IntegerValue == 10562871)
                {
                    var el = element;
                }
                foreach (var branch in collection)
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
                foreach (var branch in collection)
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


            element.PDyn = mainViewModel.Density * Math.Pow(customTee.Velocity, 2) / 2 * element.LocRes;
        }
        private static double GetPressure(CustomBranch branch1, int index)
        {
            double pressure = 0;
            for (int i = 0; i < index; i++)
            {
                
                CustomElement element = branch1.Elements[i];
                pressure +=  element.PStat + element.PDyn;
            }
            return pressure;

        }

        private int  GetSplitter(CustomBranch branch)
        {
            for (int i = 0; i < branch.Elements.Count - 1; i++)
            {
                CustomElement observableELement = branch.Elements[i];
                CustomElement selectedElement = null;
                if (observableELement.IsVisited == false)
                {
                    if (observableELement.DetailType == CustomElement.Detail.Tee)  
                    {
                        selectedElement = observableELement;
                        observableELement.IsVisited = true;
                        return i;
                    }
                    else if (observableELement.DetailType == CustomElement.Detail.TapAdjustable)
                    {
                        selectedElement = observableELement;
                        observableELement.IsVisited = true;
                        return i;
                    }
                    else if (observableELement.DetailType ==CustomElement.Detail.DuctTap)
                    {
                        selectedElement = observableELement;
                        observableELement.IsVisited = true;
                        return i;
                    }
                    else
                    {
                        observableELement.IsVisited = true;
                    }
                }
                else
                {
                    continue;
                }

            }
            return 0;
        }

        private (CustomBranch, int) GetMinimalIndex(ElementId elementId, List<CustomBranch> customBranches, CustomBranch selectedBranch)
        {
            
            List<CustomBranch> resultBranches = new List<CustomBranch>();

            for (int i = 0; i < customBranches.Count; i++)
            {
                if ((customBranches[i].Elements.First().BranchNumber != selectedBranch.Elements.First().BranchNumber) && customBranches[i].IsVisited==false)
                {
                    if (customBranches[i].Elements.Select(x=>x).Where(x=>!x.IsVisited).Any(el => el.ElementId == elementId))
                    {
                        resultBranches.Add(customBranches[i]);
                    }
                }
                
            }

            // Если есть найденные ветки, выбираем ту, где минимальный индекс ElementId
            if (resultBranches.Count > 0)
            {
                var minIndexBranch = resultBranches
                    .Select(branch => new
                    {
                        Branch = branch,
                        MinIndex = branch.Elements.FindIndex(el => el.ElementId == elementId)
                    })
                    .OrderBy(x => x.MinIndex)
                    .FirstOrDefault();

                return (minIndexBranch?.Branch, minIndexBranch.MinIndex);
            }

            // Если не найдено ни одной ветки
            return (null,0);
        }
        
        private List<Tuple<CustomBranch, CustomBranch>> SortPairs(List<CustomBranch> sortedLists)
        {
            List<Tuple<CustomBranch, CustomBranch>> pairs = new List<Tuple<CustomBranch, CustomBranch>>();

            

            return pairs;
        }

        private List<CustomBranch> sortLists(List<CustomBranch> lists) // Это оставляем
        {
            return lists.OrderByDescending(x => x.PBTot).ToList();
           
        }

        private CustomCollection GetCollection(Document doc, List<ElementId> selectedterminals)
        {
            CustomCollection collection = new CustomCollection(doc);
            foreach (var terminal in selectedterminals)
            {
                collection.CreateBranch(doc, terminal);
            }
            
            return collection;
        }

        private List<ElementId> GetAirTerminals(Document doc, string systemName)
        {
            List<ElementId> resultterminals = new List<ElementId>();
            var airterminals = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctTerminal).WhereElementIsNotElementType().ToElementIds().ToList();
            foreach (var airterminal in airterminals)
            {
                if (airterminal.IntegerValue== 5982031)
                {
                   var airterminal2 = airterminal;
                }
                if (doc.GetElement(airterminal)!=null)
                {
                    FamilyInstance fI = doc.GetElement(airterminal) as FamilyInstance;
                    if (fI!=null)
                    {
                        var checksystem = fI.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();
                        if (checksystem == null)
                        {
                            continue;
                        }
                        else if (checksystem.Equals(systemName))
                        {
                            resultterminals.Add(airterminal);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return resultterminals;
        }
    }
}
