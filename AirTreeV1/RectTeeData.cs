using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
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
                       /* { 0, 0, 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 },
                        { 100000, 0.4, 0.4, 0.324, 0.256, 0.196, 0.144, 0.1, 0.064, 0.036, 0.016, 0.004, 0 },
                        { 100000, 1, 0.3, 0.194, 0.115, 0.059, 0.021, 0, -0.064, -0.072, -0.048, -0.016, 0 }*/

                       
                       
                        { 0.4,0,0.4 },
                        { 0.4,0,0.324 },
                        { 0.4,0.1,0.256 },
                        { 0.4,0.2,0.196 },
                        { 0.4,0.3,0.144 },
                        { 0.4,0.4,0.1 },
                        { 0.4,0.5,0.064 },
                        { 0.4,0.6,0.036 },
                        { 0.4,0.7,0.016 },
                        { 0.4,0.8,0.004 },
                        { 0.4,0.9,0 },
                        { 1,0,0.3 },
                        { 1,0,0.194 },
                        { 1,0.1,0.115 },
                        { 1,0.2,0.059 },
                        { 1,0.3,0.021 },
                        { 1,0.4,0 },
                        { 1,0.5,-0.064 },
                        { 1,0.6,-0.072 },
                        { 1,0.7,-0.048 },
                        { 1,0.8,-0.016 },
                        { 1,0.9,0 }


                    };
                }
                else
                {
                    relQ = relC;
                    if (relA<0.66)
                    {
                        relA = 0.65;
                    }
                    else if(relA>1)
                    {
                        relA = 1;
                    }
                    Values = new double[,]
                    {
                       /* { 0, 0, 0, 0.1, 0.2, 0.4, 0.6, 0.8, 1, 1.2, 1.4, 1.6, 2 },
                        { 100000, 0.01, 1, 1.01, 1.04, 1.16, 1.35, 1.64, 2, 2.44, 2.96, 3.54, 4.6 },
                        { 100000,   1, 1, 1, 1.01, 1.05, 1.11, 1.19, 1.3, 1.43, 1.59, 1.77, 2.2 }*/

                        { 0.66,0,1 },
                        { 0.66,0,1.01 },
                        { 0.66,0.1,1.04 },
                        { 0.66,0.2,1.16 },
                        { 0.66,0.4,1.35 },
                        { 0.66,0.6,1.64 },
                        { 0.66,0.8,2 },
                        { 0.66,1,2.44 },
                        { 0.66,1.2,2.96 },
                        { 0.66,1.4,3.54 },
                        { 0.66,1.6,4.6 },
                        {0.66, 2,4.6 },
                        { 1,0,1 },
                        { 1,0.1,1 },
                        { 1,0.2,1.01 },
                        { 1,0.4,1.05 },
                        { 1,0.6,1.11 },
                        { 1,0.8,1.19 },
                        { 1,1,1.3 },
                        { 1,1.2,1.43 },
                        { 1,1.4,1.59 },
                        { 1,1.6,1.77 },
                        { 1,2,2.2 },
                      


                    };
                }
            }
            else
            {
                if (isstraight)
                {
                    IsStraight = isstraight;
                    

                   /* Values = new double[,]
                    {
                        { 0, 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 },
                        { 1000000, 0.55, 0.59, 0.6, 0.59, 0.57, 0.53, 0.46, 0.38, 0.27, 0.16, 0 }
                    };*/
                    
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
                   /* if (cs<6)
                    {
                        relA = 0;
                    }
                    else if (cs>6)
                    {
                        relA = 6;
                    }*/
                    Values = new double[,]
                    {
                        /*{ 0, 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 },
                        { 100000, 0, -0.75, -0.53, -0.03, 0.33, 0.8, 1.4, 2.15, 2.93, 4.18, 4.78 },
                        { 100000, 6, -0.69, -0.21, 0.23, 0.67, 1.17, 1.66, 2.67, 3.36, 3.93, 5.13 }*/

                        { 6,0.1,-0.75 },
                        { 6,0.2,-0.53 },
                        { 6,0.3,-0.03 },
                        { 6,0.4,0.33 },
                        { 6,0.5,0.8 },
                        { 6,0.6,1.4 },
                        { 6,0.7,2.15 },
                        { 6,0.8,2.93 },
                        { 6,0.9,4.18 },
                        { 6,1,4.78 },
                        { 25,0.1,-0.69 },
                        { 25,0.2,-0.21 },
                        { 25,0.3,0.23 },
                        { 25,0.4,0.67 },
                        { 25,0.5,1.17 },
                        { 25,0.6,1.66 },
                        { 25,0.7,2.67 },
                        { 25,0.8,3.36 },
                        { 25,0.9,3.93 },
                        { 25,1,5.13 },




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
                    indexC.Add(k-1);
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
                else if (relA >= Values[i , 1] && relA <= Values[i, 1])
                {
                    for (int k = indexC.Min(); k <= indexC.Max(); k++)
                    {

                        for (int j = indexB.Min(); j < indexB.Max(); j++)
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


        public double Interpolation2 (double A, double Q)
        {
            LocRes = 0;
           


            double minA = double.MinValue;
            List<double> indexA = new List<double>();
            if (SystemType == DuctSystemType.ExhaustAir && IsStraight == true)
            {
                LocRes = 1.55 * (1 - Q) - Math.Pow((1 - Q), 2);
                return LocRes;
            }


            else
            {
                List<double> uniqueRelAValues = new List<double>();
                List<double> uniqueRelQValues = new List<double>();
                // Собираем уникальные значения relA
                for (int i = 0; i < Values.GetLength(0); i++)
                {
                    double relA = Values[i, 0];
                    if (!uniqueRelAValues.Contains(relA))
                    {
                        uniqueRelAValues.Add(relA);
                    }
                }

                for (int i = 0; i < Values.GetLength(0); i++)
                {
                    double relQ = Values[i, 1];
                    if (!uniqueRelQValues.Contains(relQ))
                    {
                        uniqueRelQValues.Add(relQ);
                    }
                }

                // Находим значение relA , ближе всего к A
                double closestRelA = uniqueRelAValues.OrderBy(x => Math.Abs(x - A)).First();
                double closestRelQ = uniqueRelQValues.OrderBy(x => Math.Abs(x - Q)).First();
                // Выбираем все relA из массива, которые близки к найденному


                // Теперь перебираем все значения Q для выбранных relA
                double bestValue = double.NegativeInfinity;
                double bestRelA = double.NaN;
                double bestRelQ = double.NaN;

               
                for (int i=0; i<Values.GetLength(0);i++)
                {
                    if (Values[i,0]==closestRelA)
                    {
                       if (Values[i,1]==closestRelQ)
                        {
                            LocRes = Values[i, 2];
                        }
                    }
                }



               

                return LocRes;
            }
            
            
            
        }
    }
}
