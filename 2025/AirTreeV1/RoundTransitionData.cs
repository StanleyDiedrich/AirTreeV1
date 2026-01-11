using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Mechanical;

namespace AirTreeV1
    {
        public class RoundTransitionData
        {
            public double[,] Values { get; private set; }
            public DuctSystemType SystemType { get; set; }
            public double LocRes { get; set; }


            public RoundTransitionData(DuctSystemType ductSystemType, double relA, double angle)
            {
                SystemType = ductSystemType;
                if (SystemType == DuctSystemType.ExhaustAir && relA > 1)
                {
                    Values = new double[,]
                     {

                                             {0,10,15,20,30,45,60,90,120,180 },
                                             {1.0,0,0,0,0,0,0,0,0,0 },
                                             {2,0.234,0.255,0.274,0.298,0.336,0.329,0.325,0.322,0.317 },
                                             {4,0.252,0.336,0.445,0.661,0.802,0.754,0.63,0.623,0.614 },
                                              {6,0.277,0.408,0.486,0.673,0.897,0.906,0.768,0.756,0.742 },
                                             {10,0.340,0.45,0.564,0.789,1.02,0.96,0.861,0.860,0.856 },



                     };

                }
                else if (SystemType == DuctSystemType.ExhaustAir && relA < 1)
                {
                    Values = new double[,]
                    {
                    {0,10,15,20,30,40,60,90,120,150,180 },
                    {0.10,0.05,0.05,0.05,0.04,0.04,0.08,0.19,0.29,0.37,0.43 },
                     {0.17,0.05,0.04,0.04,0.04,0.04,0.07,0.18,0.28,0.36,0.42 },
                    {0.25,0.05,0.04,0.04,0.04,0.05,0.07,0.17,0.27,0.35,0.41 },
                    {0.5,0.05,0.05,0.05,0.05,0.05,0.06,0.12,0.18,0.24,0.26 },
                    {1.0,0,0,0,0,0,0,0,0,0,0 },


                    };
                }
                else if (SystemType == DuctSystemType.SupplyAir && relA > 1)
                {
                    Values = new double[,]
                    {

                                            {0,10,15,20,30,45,60,90,120,180 },
                                            {1.0,0,0,0,0,0,0,0,0,0 },
                                             {2,0.234,0.255,0.274,0.298,0.336,0.329,0.325,0.322,0.317 },
                                             {4,0.252,0.336,0.445,0.661,0.802,0.754,0.63,0.623,0.614 },
                                              {6,0.277,0.408,0.486,0.673,0.897,0.906,0.768,0.756,0.742 },
                                             {10,0.340,0.45,0.564,0.789,1.02,0.96,0.861,0.860,0.856 },



                    };
                }
                else if (SystemType == DuctSystemType.SupplyAir && relA < 1)
                {
                    Values = new double[,]
                   {
                    {0,10,15,20,30,40,60,90,120,150,180 },
                    {0.10,0.05,0.05,0.05,0.04,0.04,0.08,0.19,0.29,0.37,0.43 },
                     {0.17,0.05,0.04,0.04,0.04,0.04,0.07,0.18,0.28,0.36,0.42 },
                    {0.25,0.05,0.04,0.04,0.04,0.05,0.07,0.17,0.27,0.35,0.41 },
                    {0.5,0.05,0.05,0.05,0.05,0.05,0.06,0.12,0.18,0.24,0.26 },
                    {1.0,0,0,0,0,0,0,0,0,0,0 },
                   };
                }
            }

        public double Interpolation2(double relA, double relQ)
        {
            double result = 0;
            int rows = Values.GetLength(0) - 1;
            int columns = Values.GetLength(1) - 1;
            List<int> possibleA = new List<int>();
            List<int> possibleQ = new List<int>();
            
            for (int k = 1; k <= rows; k++)
            {
                if (Values[k, 0] >= relA)
                {
                    possibleA.Add(k);
                }
            }

            for (int l = 1; l <= columns; l++)
            {
                if (Values[0, l] >= relQ)
                {
                    possibleQ.Add(l);
                }
            }
          


            for (int i = possibleA.Min(); i <= possibleA.Max(); i++)
            {
                if (Values[i, 0] == relA)
                {
                    for (int j = possibleQ.Min(); j <= possibleQ.Max(); j++)
                    {
                        if (Values[0, j] == relQ)
                        {
                            return LocRes = Values[i, j];
                        }
                        else if (Values[0, j - 1] < relQ || Values[0, j] >= relQ)
                        {
                            double x0 = Values[0, j - 1];
                            double x1 = Values[0, j];
                            double y0 = Values[i, j - 1];
                            double y1 = Values[i, j];

                            return LocRes = LinearInterpolation(x0, x1, y0, y1, relQ);
                        }
                    }

                }
                else if (Values[i - 1, 0] < relA || Values[i, 0] >= relA)
                {
                    for (int j = possibleQ.Min(); j <= possibleQ.Max(); j++)
                    {
                        if (Values[0, j] == relQ)
                        {
                            double x0 = Values[i - 1, 0];
                            double x1 = Values[i, 0];
                            double y0 = Values[i - 1, j];
                            double y1 = Values[i, j];

                            return LocRes = LinearInterpolation(x0, x1, y0, y1, relA);
                        }
                        else if (Values[0, j - 1] < relQ || Values[0, j] > relQ)
                        {



                            return LocRes = BiLinearInterPolation(Values, i, j, relA, relQ);

                        }
                    }
                }
            }











            return LocRes;
        }

        private double BiLinearInterPolation(double[,] values, int i, int j, double relA, double relQ)
        {
            double res = 0;

            double A1 = values[i - 1, 0];
            double A2 = values[i, 0];
            double A = relA;
            double B1 = values[0, j - 1];
            double B2 = values[0, j];
            double B = relQ;

            double C11 = values[i - 1, j - 1];
            double C12 = values[i - 1, j];
            double C21 = values[i, j - 1];
            double C22 = values[i, j];

            double res1 = (((B2 - B) / (B2 - B1) * C11) + (B - B1) / (B2 - B1) * C12) * ((A2 - A) / (A2 - A1));
            double res2 = (((B2 - B) / (B2 - B1) * C21) + (B - B1) / (B2 - B1) * C22) * (A - A1) / (A2 - A1);
            res = res1 + res2;



            return res;
        }

        private double LinearInterpolation(double x0, double x1, double y0, double y1, double target)
        {
            double res = y0 + (y1 - y0) * (target - x0) / (x1 - x0);
            return res;
        }

        public double Acot(double d)
        {
            if (d < 0) return Math.PI - Math.Atan(1 / -d);
            return Math.Atan(1.0 / d);
        }
    }
    

    

        }

    
