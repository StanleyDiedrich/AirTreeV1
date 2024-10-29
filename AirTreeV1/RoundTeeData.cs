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
                    { { },{ } };
                }
                else
                {
                    Values = new double[,]
                    { { },{ } };
                }
            }
        }
    }
}
