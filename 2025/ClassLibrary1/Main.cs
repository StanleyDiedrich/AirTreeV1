using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using Microsoft.Win32;

namespace AirTree_2025
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]




    public class Main : IExternalCommand
    {



        public static Branch GetNewAdditionalBranches(Document doc, ElementId elementId, List<Branch> mainnodes)
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
                    Node newnode = new Node(doc, doc.GetElement(elementId), systemtype, shortsystemname, mode, mainnodes);
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
                string shortsystemname2 = "";
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
                if (airterminal.IntegerValue == 5982031)
                {
                    var airterminal2 = airterminal;
                }
                if (doc.GetElement(airterminal) != null)
                {
                    FamilyInstance fI = doc.GetElement(airterminal) as FamilyInstance;
                    if (fI != null)
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

        static AddInId AddInId = new AddInId(new Guid("05B398F6-85A5-4AAF-8EDC-CD14C2DF8E73"));

       
            public string GetFileName(Document doc)
            {
                string fileName = doc.Title;
            try
            {
                string resultName = "";
                string[] parts = fileName.Split('_');
                int startIndex = -1;
                int endIndex = -1;
                bool isDetached = false;
                // Найти индекс начала и конца
                for (int k = 0; k < parts.Length; k++)
                {
                    if (parts[k].StartsWith("R2"))
                    {
                        startIndex = k;
                    }

                    if (parts[k].Contains("mep"))
                    {
                        isDetached = false;
                        endIndex = k;
                        break;
                    }
                    if (parts[k].Contains("отсоединено"))
                    {
                        isDetached = true;
                        endIndex = k;
                        break;
                    }


                    /*if (parts[k].Contains("mep"))
                    {
                        endIndex = k;
                        break;
                    }
                    if (parts[k].Contains("отсоединено"))
                    {
                        endIndex = k;
                        break;
                    }*/

                }

                // Если не найден индекс конца, добавим "_ОТКРЕП"
                if (endIndex == -1)
                {
                    endIndex = parts.Length;
                    resultName += "_ОТКРЕП";
                }

                // Формируем результат
                resultName = "_";
                for (int g = startIndex; g < endIndex; g++)
                {
                    resultName += parts[g];
                    if (g < endIndex - 1)
                    { // добавляем "_" между частями
                        resultName += "_";
                    }
                }
                if (!fileName.Contains("mep"))
                {
                    resultName += "_ОТКРЕП";
                }



                return resultName;
            }
            catch
            {
                return fileName+"_";
            }
               
            }

        
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
                    if (newduct != null)
                    {
                        if (!systemnumbers.Contains(newduct.LookupParameter("Имя системы").AsString()))
                        {
                            systemnumbers.Add(newduct.LookupParameter("Имя системы").AsString());
                        }
                    }
                }
                catch (Exception ex)
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
            sysNums = sortedSysNums;
            UserControl1 window = new UserControl1();
            MainViewModel mainViewModel = new MainViewModel(doc, window, sysNums);
            window.DataContext = mainViewModel;
            window.ShowDialog();
            List<ElementId> elIds = new List<ElementId>();
            var systemnames = mainViewModel.SystemNumbersList.Select(x => x).Where(x => x.IsSelected == true);


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
                if (selectedterminals.Count != 0)
                {
                    CustomCollection collection = GetCollection(doc, selectedterminals);


                    try
                    {
                        collection.Calcualate(mainViewModel.Density);
                    }
                    catch
                    {
                        CustomElement element = collection.ActiveElement;
                        TaskDialog.Show("Ошибка", $"ошибка в элементе{element.ElementId}");
                    }

                    collection.ResCalculate();
                    collection.MarkBranches();

                    collection.SelectMainBranch();
                    collection.TeeReCalc(collection);

                    collection.ResCalculate();
                    collection.OrderCollection();
                    collection.GetUniqueElements();
                    collection.MarkFirstBranch();






                    //collection.MarkCollection(selectedbranch);
                    
                    string content = collection.GetContent();
                    string filemname = collection.FirstElement;
                    string resName = GetFileName(doc);
                    try
                    {
                        collection.SaveFile(resName,content);

                    }
                    catch
                    {
                        TaskDialog.Show("R", $"Система {filemname} имеет ошибку ");
                    }
                }
                else
                {
                    TaskDialog.Show("AirTree", $"Система {systemName} не имеет воздухораспределителей");
                }

               
            }
            return Result.Succeeded;
        }

       
    }
  
}
