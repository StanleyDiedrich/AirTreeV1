using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;

namespace AirTreeV1
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class Main : IExternalCommand
    {
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

            var selected_workset = mainViewModel.SelectedWorkSet.Name;




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
                        break;  // Обязательно добавляем break для правильного выполнения

                    

                    default:  // Обработка случая, если mode не совпадает ни с одним из вышеуказанных
                        throw new InvalidOperationException($"Неизвестный режим расчета: {mode}");
                }
            }

            return Result.Succeeded;
        }

        private ElementId GetStartDuct(Document doc, string systemName)
        {
            throw new NotImplementedException();
        }

        private List<Branch> AlgorithmDuctTraverse(Document doc, List<ElementId> startelements)
        {
            throw new NotImplementedException();
        }
    }
}
