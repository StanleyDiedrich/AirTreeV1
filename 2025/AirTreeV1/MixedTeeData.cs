using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;

namespace AirTreeV1
{
    public class MixedTeeData
    {
        public double[,] Values { get; private set; }
        public DuctSystemType SystemType { get; set; }
        public double LocRes { get; set; }
        public double Coeff { get; set; } = 1;
        public bool IsStraight { get; set; }
        public double RelA { get; set; }
        public double RelQ { get; set; }

        public MixedTeeData(DuctSystemType ductSystemType, bool isstraight, double relA, double relQ, double relC)
        {
            RelQ = relQ;
            SystemType = ductSystemType;
            if (ductSystemType == DuctSystemType.SupplyAir)
            {
                if (isstraight)
                {
                    if (relA < 0.4)
                    {
                        RelA = 0.4;
                    }
                    else if (relA > 0.4)
                    {
                        RelA = 1;
                    }
                    Values = new double[,]
                    {
                        { 0, 0, 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 },
                        { 100000, 0.4, 0.4, 0.324, 0.256, 0.196, 0.144, 0.1, 0.064, 0.036, 0.016, 0.004, 0 },
                        { 100000, 1, 0.3, 0.194, 0.115, 0.059, 0.021, 0, -0.064, -0.072, -0.048, -0.016, 0 }
                    };
                }
                else
                {
                    RelQ = relC;
                    if (relA < 0.66)
                    {
                        RelA = 0.66;
                    }
                    else if (relA > 0.4)
                    {
                        RelA = 1;
                    }
                    Values = new double[,]
                    {
                        { 0, 0, 0, 0.1, 0.2, 0.4, 0.6, 0.8, 1, 1.2, 1.4, 1.6, 2 },
                        { 100000, 0.66, 1, 1.01, 1.04, 1.16, 1.35, 1.64, 2, 2.44, 2.96, 3.54, 4.6 },
                        { 100000, 1, 1, 1, 1.01, 1.05, 1.11, 1.19, 1.3, 1.43, 1.59, 1.77, 2.2 }
                    };
                }
            }
            else
            {
                if (isstraight)
                {
                    if (relA>0.79)
                    {
                        RelA = 0.79;
                    }
                    else if (relA<0.35)
                    {
                        RelA = 0.35;
                    }


                    Values = new double[,] {
                        { 0, 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9,1 },
                        { 100000, 0.35, 0.85, 0.78, 0.71, 0.64, 0.57, 0.5, 0.43, 0.35, 0.25,0.15 },
                        { 100000, 0.79, 0.7, 0.64, 0.61, 0.58, 0.54, 0.49, 0.43, 0.33, 0.23,0.13 }
                    };

                    
                }
                else
                {
                    if (relA > 0.79)
                    {
                        RelA = 0.79;
                    }
                    else if (relA < 0.35)
                    {
                        RelA = 0.35;
                    }
                    Values = new double[,]
                    {
                        {0,0,0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,1},
                        { 100000,0.35,-0.2,0.08,0.5,0.95,1.48,2.17,3.5, 4.5, 5.5,6.5},
                        {100000,0.79,-0.2,-0.14,0.15,0.39,0.58,0.78,0.96,1.13,1.3,1.5 }

                    };


                }
            }
        }
        public double Interpolation(double reynolds)
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
            /*for (int j = 1; j < Values.GetLength(1); j++)
            {
                if (RelQ >= Values[0, j - 1] && RelQ <= Values[0, j])
                {
                    indexB.Add(j);
                }
            }*/

            for (int j = 1; j < Values.GetLength(1); j++)
            {
                if (RelQ >= Values[0, j - 1] && RelQ <= Values[0, j])
                {
                    indexB.Add(j);
                }
            }
            for (int k = 1; k < Values.GetLength(0); k++)
            {
                if (RelA > Values[k - 1, 1] && RelA <= Values[k, 1])
                {
                    indexC.Add(k);
                }
                else if (RelA <= Values[k, 1])
                {

                    indexC.Add(k);
                    
                }
            }
            // Интерполяция для найденных индексов

            for (int i = indexA.Min(); i <= indexA.Max(); i++)
            {
                if (RelA == Values[i, 1])
                {
                    if (indexB.Count == 0)
                    {
                        if (RelQ == Values[i, 2])
                        {
                            return Values[i, 2];
                        }
                    }
                    else
                    {
                        for (int j = indexB.Min(); j <= indexB.Max(); j++)
                        {
                            if (RelQ == Values[0, j])
                            {
                                return Values[i, j];
                            }
                            else
                            {

                                for (j = indexB[0]; j <= indexB[indexB.Count - 1]; j++)
                                {
                                    if (RelQ > Values[0, j - 1] && RelQ < Values[0, j])
                                    {
                                        // Линейная интерполяция
                                        double x0 = Values[0, j - 1]; // предыдущее значение
                                        double x1 = Values[0, j];     // текущее значение
                                        double y0 = Values[i, j - 1];
                                        double y1 = Values[i, j];

                                        // Линейная интерполяция
                                        result = (y0 + (RelQ - x0) / (x1 - x0) * (y1 - y0));
                                        break;
                                    }
                                }

                            }
                        }
                    }


                }
                else if (RelA >= Values[i - 1, 1] && RelA <= Values[i, 1])
                {
                    for (int k = indexC.Min(); k <= indexC.Max(); k++)
                    {

                        for (int j = indexB.Min(); j <= indexB.Max(); j++)
                        {

                            if (RelQ == Values[0, j])
                            {
                                double x0 = Values[k - 1, 1];
                                double x1 = Values[k, 1];
                                double y0 = Values[k - 1, j];
                                double y1 = Values[k, j];

                                result = (y0 + (RelA - x0) / (x1 - x0) * (y1 - y0));
                            }
                            else
                            {
                                double A1 = Values[k - 1, 1];
                                double A2 = Values[k, 1];
                                double A = RelA;
                                double B1 = Values[0, j - 1];
                                double B2 = Values[0, j];
                                double B = RelQ;
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
    }
}

