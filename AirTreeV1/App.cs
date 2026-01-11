using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AirTreeV1
{
    internal class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            throw new NotImplementedException();
        }

        public Result OnStartup(UIControlledApplication application)
        {

            RibbonPanel ribbonPanel = application.CreateRibbonPanel(Tab.AddIns, "SID");

            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyPath = assembly.Location;

            //Create Button
            PushButton startButton = ribbonPanel.AddItem(new PushButtonData(
                "Start", "Start", assemblyPath, "AirTreeV1.Main")) as PushButton;
            startButton.ToolTip = "ToolTip";
            startButton.LargeImage = new BitmapImage(new Uri($"pack://application:,,,/AirTreeV1;component/Image24/air32.ico"));

           

            

           

            return Result.Succeeded;
        }
    }
}
