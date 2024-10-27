using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;

namespace AirTreeV1
{
    public class RectTransitionData
    {
        public double[,] Values { get; private set; }
        public DuctSystemType SystemType { get; set; }
        public double LocRes { get; set; }


        public RectTransitionData(DuctSystemType ductSystemType, double relA, double angle)
        {
            SystemType = ductSystemType;
            if (SystemType == DuctSystemType.ExhaustAir)
            {
                Values = new double[,]
                 {

                                             {0,0,10,15,20,30,45,60,90,120,180 },
                                             {100000,2,0.235,0.268,0.290,0.31,0.33,0.340,0.340,0.320,0.31 },
                                             {100000,4,0.365,0.443,0.495,0.55,0.6,0.63,0.65,0.65,0.64 },
                                              {100000,6,0.405,0.515,0.580,0.65,0.72,0.775,0.78,0.775,0.76 },
                                             {100000,10,0.455,0.56,0.64,0.73,0.83,0.88,0.94,0.51,0.88 },



                 };

            }
        }

            public double Interpolation(double reynolds, double relA, double angle)
            {
                double result = 0;
                List<int> indexA = new List<int>();
                List<int> indexB = new List<int>();
                List<int> indexC = new List<int>();
                // Поиск индексов для reynolds
                for (int i = 1; i < Values.GetLength(0); i++)
                {
                    if (reynolds >= Values[i - 1, 0] && reynolds <= Values[i, 0])
                    {
                        indexA.Add(i);
                    }
                }

                // Поиск индексов для angle
                for (int j = 1; j < Values.GetLength(1); j++)
                {
                    if (angle > Values[0, j - 1] && angle <= Values[0, j])
                    {
                        indexB.Add(j);
                    }
                }
                for (int k = 1; k < Values.GetLength(0); k++)
                {
                    if (relA > Values[k - 1, 1] && relA < Values[k, 1])
                    {
                        indexC.Add(k);
                    }
                    else if (relA < Values[k, 1])
                    {

                        indexC.Add(k);
                        relA = Math.Round(relA, 0);
                    }
                }
                // Интерполяция для найденных индексов

                for (int i = indexA.Min(); i < indexA.Max(); i++)
                {
                    if (relA == Values[i, 1])
                    {
                        if (indexB.Count==0)
                        {
                            if (angle == Values[i,2])
                            {
                                return Values[i, 2];
                            }
                        }
                        else
                        {
                            for (int j = indexB.Min(); j <= indexB.Max(); j++)
                            {
                                if (angle == Values[0, j])
                                {
                                    return Values[i, j];
                                }
                                else
                                {

                                    for (j = indexB[0]; j <= indexB[indexB.Count - 1]; j++)
                                    {
                                        if (angle > Values[0, j - 1] && angle < Values[0, j])
                                        {
                                            // Линейная интерполяция
                                            double x0 = Values[0, j - 1]; // предыдущее значение
                                            double x1 = Values[0, j];     // текущее значение
                                            double y0 = Values[i, j - 1];
                                            double y1 = Values[i, j];

                                            // Линейная интерполяция
                                            result = (y0 + (angle - x0) / (x1 - x0) * (y1 - y0));
                                            break;
                                        }
                                    }

                                }
                            }
                    }
                        

                    }
                    else if (relA > Values[i - 1, 1] && relA < Values[i, 1])
                    {
                        for (int k = indexC.Min(); k <= indexC.Max(); k++)
                        {
                            
                                for (int j = indexB.Min(); j <= indexB.Max(); j++)
                                {

                                    if (angle == Values[0, j])
                                    {
                                        double x0 = Values[k - 1, 1];
                                        double x1 = Values[k, 1];
                                        double y0 = Values[k - 1, j];
                                        double y1 = Values[k, j];

                                        result = (y0 + (relA - x0) / (x1 - x0) * (y1 - y0));
                                    }
                                else
                                {
                                    double A1 = Values[k - 1, 1];
                                    double A2 = Values[k, 1];
                                    double A = relA;
                                    double B1 = Values[0, j - 1];
                                    double B2 = Values[0, j];
                                    double B = angle;
                                    double C11 = Values[k - 1, j - 1];
                                    double C12 = Values[k - 1, j];
                                    double C21 = Values[k, j - 1];
                                    double C22 = Values[k, j];
                                    double res1 = (((B2 - B) / (B2 - B1) * C11) + (B - B1) / (B2 - B1) * C12) * ((A2 - A) / (A2 - A1));
                                    double res2 = (((B2 - B) / (B2 - B1) * C21) + (B - B1) / (B2 - B1) * C22) * (A - A1) / (A2 - A1);
                                    result = res1 + res2;
                                    break;
                                }
                            
                                

                            }
                        }


                    }



                }


                return LocRes = result;
            }

        public double Acot(double d)
        {
            if (d < 0) return Math.PI - Math.Atan(1 / -d);
            return Math.Atan(1.0 / d);
        }

    }
        
    } 
