using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;

namespace AirTreeV1
{
    public  class RectTransitionData
    {
        public double[,] Values { get; private set; }
        public DuctSystemType SystemType { get; set; }
        public double LocRes { get; set; }


        public RectTransitionData (DuctSystemType ductSystemType)
        {
            SystemType = ductSystemType;
            if(SystemType==DuctSystemType.ExhaustAir)
            {
                Values = new double[,]
                {
                    
                        {0,0,10,15,20,30,45,60,90,120,180 },
                        {100000,10,0.455,0.56,0.64,0.73,0.83,0.88,0.94,0.51,0.88 },
                        {100000,6,0.405,0.515,0.580,0.65,0.72,0.775,0.78,0.775,0.76 },
                        {100000,4,0.365,0.443,0.495,0.55,0.6,0.63,0.65,0.65,0.64 },
                        {100000,2,0.235,0.268,0.290,0.31,0.33,0.340,0.340,0.320,0.31 }

                };
            }
        }
        public double Interpolation(double reynolds, double relA, double angle)
        {
            double result=0;
            List<int> indexA = new List<int>();
            List<int> indexB = new List<int>();
            for (int i = 1; i < Values.GetLength(0); i++)
            {
                if (reynolds> Values[i-1, 0] && reynolds < Values[i,0])
                {
                    indexA.Append(i);
                }
            }
            for (int j= 2;j<Values.GetLength(1);j++)
            {
                if (angle > Values[0,j-1] && angle < Values[0,j])
                {
                    indexB.Append(j);
                }
            }
            foreach (int i in indexA)
            {
                foreach (int j in indexB)
                {
                    // Необходимо реализовать логику интерполяции
                    double x0 = Values[i - 1, 0];
                    double x1 = Values[i, 0];
                    double y0 = Values[i - 1, j];
                    double y1 = Values[i, j];

                    // Линейная интерполяция
                    result += (y0 + (reynolds - x0) / (x1 - x0) * (y1 - y0));
                }
            }






            return result;
        }
            
        
        public  double Acot(double d)
        {
            if (d<0) return Math.PI - Math.Atan(1 / -d);
            return Math.Atan(1.0 / d);
        }
    }
}
