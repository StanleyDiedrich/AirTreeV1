﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AirTreeV1
{
    public class CustomConnector
    {
        public ElementId OwnerId { get; set; }
        public Connector Connector { get; set; }
        public Domain Domain { get; set; }
        public FlowDirectionType DirectionType { get; set; }
        public ConnectorType ConnectorType { get; set; }
        public ElementId NextOwnerId { get; set; }
        public ElementId Neighbourg { get; set; }
        public double Flow { get; set; }
        public double Coefficient { get; set; }
        public bool IsSelected { get; set; }

        public double Diameter { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public ConnectorProfileType Shape { get; set; }
        public ConnectorType Type { get; set; }

        public double EquiDiameter { get; set; }
        

        public double PressureDrop { get; set; }
        List<CustomConnector> Connectors { get; set; } = new List<CustomConnector>();

        public CustomConnector(Autodesk.Revit.DB.Document document, ElementId elementId, Autodesk.Revit.DB.Mechanical.DuctSystemType ductSystemType)
        {
            OwnerId = elementId;

           

        }

    }
}