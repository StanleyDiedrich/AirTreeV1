using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;

namespace AirTreeV1
{
    public class RectTeeData
    {
        public double[,] Values { get; private set; }
        public DuctSystemType SystemType { get; set; }
        public double LocRes { get; set; }
        public double Coeff { get; set; } = 1;
        public bool IsStraight { get; set; }

        public RectTeeData(DuctSystemType ductSystemType, bool isstraight, double relA, double relQ, double relC, CustomConnector inletconnector)
        {
            SystemType = ductSystemType;
            if (ductSystemType == DuctSystemType.SupplyAir)
            {
                if (isstraight)
                {
                    if (relA < 0.4)
                    {
                        relA = 0.4;
                    }
                    else if (relA > 0.4)
                    {
                        relA = 1;
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
                    relQ = relC;
                    if (relA<0.66)
                    {
                        relA = 0.66;
                    }
                    else if(relA>1)
                    {
                        relA = 1;
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
                    IsStraight = isstraight;
                    

                    Values = new double[,]
                    {
                        { 0, 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 },
                        { 1000000, 0.55, 0.59, 0.6, 0.59, 0.57, 0.53, 0.46, 0.38, 0.27, 0.16, 0 }
                    };
                    
                    return;
                }
                else
                {
                    if (relA <= 0.35)
                    {
                        Coeff = 1;
                    }
                    else if (relA > 0.35 && relQ <= 0.4)
                    {
                        Coeff = 0.9 * (1 - relQ);
                    }
                    else if (relA > 0.35 && relQ > 0.4)
                    {
                        Coeff = 0.55;
                    }
                    double cs = inletconnector.Velocity;
                    if (cs<6)
                    {
                        relA = 0;
                    }
                    else if (cs>6)
                    {
                        relA = 6;
                    }
                    Values = new double[,]
                    {
                        { 0, 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 },
                        { 100000, 0, -0.75, -0.53, -0.03, 0.33, 0.8, 1.4, 2.15, 2.93, 4.18, 4.78 },
                        { 100000, 6, -0.69, -0.21, 0.23, 0.67, 1.17, 1.66, 2.67, 3.36, 3.93, 5.13 }

                    };
                    

                }
            }
        }
        public double Interpolation(double reynolds, double relA, double relQ)
        {
            if (SystemType==DuctSystemType.ExhaustAir && IsStraight==true)
            {
                LocRes = 1.55 * (1 - relQ) - Math.Pow((1 - relQ), 2);
                return LocRes;
            }
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
            for (int j = 1; j <= Values.GetLength(1); j++)
            {
                if (relQ > Values[0, j - 1] && relQ <= Values[0, j])
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

            for (int i = indexA.Min(); i <= indexA.Max(); i++)
            {
                if (relA == Values[i, 1])
                {
                    if (indexB.Count == 0)
                    {
                        if (relQ == Values[i, 2])
                        {
                            return Values[i, 2];
                        }
                    }
                    else
                    {
                        for (int j = indexB.Min(); j <= indexB.Max(); j++)
                        {
                            if (relQ == Values[0, j])
                            {
                                return Values[i, j];
                            }
                            else
                            {

                                for (j = indexB[0]; j <= indexB[indexB.Count - 1]; j++)
                                {
                                    if (relQ > Values[0, j - 1] && relQ < Values[0, j])
                                    {
                                        // Линейная интерполяция
                                        double x0 = Values[0, j - 1]; // предыдущее значение
                                        double x1 = Values[0, j];     // текущее значение
                                        double y0 = Values[i, j - 1];
                                        double y1 = Values[i, j];

                                        // Линейная интерполяция
                                        result = (y0 + (relQ - x0) / (x1 - x0) * (y1 - y0));
                                        break;
                                    }
                                }

                            }
                        }
                    }


                }
                else if (relA >= Values[i - 1, 1] && relA <= Values[i, 1])
                {
                    for (int k = indexC.Min(); k <= indexC.Max(); k++)
                    {

                        for (int j = indexB.Min(); j <= indexB.Max(); j++)
                        {

                            if (relQ == Values[0, j])
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
                                double B = relQ;
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


            return LocRes = result*Coeff;
        }
    }
}
