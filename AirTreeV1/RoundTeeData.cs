using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;

namespace AirTreeV1
{
    public  class RoundTeeData
    {
        public double[,] Values { get; private set; }
        public DuctSystemType SystemType { get; set; }
        public double LocRes { get; set; }
        public RoundTeeData(DuctSystemType ductSystemType,bool isstraight, double relA, double relQ)
        {
            if (ductSystemType == DuctSystemType.ExhaustAir)
            {
                if (isstraight)
                {
                    Values = new double[,]
                    {
                        {0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,1.0 },
                        {0.79,0.71,0.63,0.55,0.47,0.38,0.28,0.16,0 }
                    };
                }
                else
                {
                    Values = new double[,]
                    {
                        { 0,0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,1},
                        {0.1,0.4,3.8,9.2,16,26,37,43,65,82,101 },
                        { 0.2,-0.37,0.72,2.3,4.3,6.8,9.7,13,17,21,26},
                        { 0.3,-0.41,0.17,1,2.1,3.2,4.7,6.3,7.9,9.7,12},
                        {0.4,-0.46,-0.1,0.25,0.66,1.1,1.6,2.1,2.7,3.4,4 },
                        { 0.5,-0.5,-0.2,0.14,0.42,0.8,1.15,1.5,1.9,2.3,2.85},
                        { 0.6,-0.5,-0.25,0,0.26,0.66,0.92,1.2,1.5,1.8,2.1},
                        {0.8,-0.51,-0.25,0,0.2,0.49,0.69,0.88,1.1,1.2,1.4 },
                        {1,-0.52,-0.25,-0.05,0.2,0.42,0.57,0.72,0.86,0.99,1.1 }
                    };
                }
            }
        }
    }
}
