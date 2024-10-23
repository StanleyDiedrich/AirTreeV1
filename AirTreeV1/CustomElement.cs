using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;
using System.Runtime.Remoting.Contexts;

namespace AirTreeV1
{
   public  class CustomElement
    {
        public Element Element { get; set; }
        public ElementId ElementId { get; set; }
        public MEPSystem System { get; set; }
        public string SystemName { get; set; }
        public string ShortSystemName { get; set; }
        public DuctSystemType SystemType { get; set; }

        public CustomConnector SelectedConnector { get; set; }
        public List<CustomConnector> SecondaryConnectors { get; set; }

        public ConnectorSet OwnConnectors { get; set; }

        


       

        public CustomElement (Autodesk.Revit.DB.Document doc, Element element)
        {
            Element = element;
            ElementId = Element.Id;
            if (Element is Duct)
            {
                System = (Element as MEPCurve).MEPSystem;
                SystemType = (System as MechanicalSystem).SystemType;
                ShortSystemName = Element.LookupParameter("Сокращение для системы").AsString();
                
            }
            if (Element is FamilyInstance)
            {
                OwnConnectors = (Element as FamilyInstance).MEPModel.ConnectorManager.Connectors;
                foreach (Connector connector in OwnConnectors)
                { ЛЛЛЛЛЛ}

            }
           
        }
    }
}
