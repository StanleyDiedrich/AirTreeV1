using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Autodesk.Revit.UI;

namespace AirTreeV1
{
    public class ElbowData
    {
        // Свойство для хранения значений
        public double[,] Values { get; private set; }

        // Конструктор класса
        public ElbowData()
        {
            Values = new double[,]
            {
            { 0, 0.25, 0.5, 0.75, 1, 1.15, 2, 3, 4, 6, 8 },
            { 0.5, 1.28, 1.12, 1.04, 0.94, 0.84, 0.8, 0.8, 0.84, 0.85, 0.83 },
            { 0.75, 0.64, 0.56, 0.52, 0.47, 0.42, 0.4, 0.4, 0.42, 0.45, 0.46 },
            { 1.0, 0.35, 0.29, 0.26, 0.24, 0.21, 0.2, 0.2, 0.21, 0.22, 0.23 },
            { 1.5, 0.33, 0.27, 0.24, 0.21, 0.19, 0.18, 0.17, 0.18, 0.19, 0.20 },
            { 2.0, 0.35, 0.26, 0.23, 0.21, 0.18, 0.17, 0.17, 0.17, 0.18, 0.18 },
            { 2.5, 0.35, 0.26, 0.23, 0.20, 0.18, 0.17, 0.16, 0.16, 0.17, 0.17 },
            { 4.0, 0.44, 0.31, 0.26, 0.23, 0.20, 0.18, 0.17, 0.17, 0.18, 0.18 }
            };
        }

        public double Interpolation(double hw, double rw)
        {
            double result = 0;
            double Y1 = 0;
            double Y2 = 0;
            double X1 = 0;
            double X2 = 0;
            for (int h = 1; h < 11; h++)
            {
                if (hw == Values[0, h])
                {
                    for (int w = 1; w < 8; w++)
                    {
                        if (rw == Values[w, 0])
                        {
                            result = Values[w, h];
                            break;
                        }
                        else if (rw > Values[w - 1, 0] && rw < Values[w, 0])
                        {
                            double contr1 = Values[w - 1, 0];
                            double contr2 = Values[w, 0];
                            X1 = Values[w - 1, 0];
                            X2 = Values[w, 0];
                            Y1 = Values[w - 1, h];
                            Y2 = Values[w, h];

                            result = Y2 + (Y1 - Y2) / (X1 - X2) * (rw - X1);
                            break;

                        }
                    }
                }
                else if (hw > Values[0, h - 1] && hw < Values[0, h])
                {
                    for (int w = 1; w < 8; w++)
                    {
                        if (rw == Values[w, 0])
                        {
                            double contr1 = Values[w, h - 1];
                            double contr2 = Values[w, h];
                            X1 = Values[0, h - 1];
                            X2 = Values[0, h];
                            Y1 = Values[w, h - 1];
                            Y2 = Values[w, h];

                            result = Y1 + ((hw - X1) * (Y2 - Y1)) / (X2 - X1);
                            break;

                        }

                        else if (rw > Values[w - 1, 0] && rw < Values[w, 0])
                        {
                            double A1 = Values[w - 1, 0];
                            double A2 = Values[w, 0];
                            double A = rw;
                            double B1 = Values[0, h - 1];
                            double B2 = Values[0, h];
                            double B = hw;
                            double C11 = Values[w - 1, h - 1];
                            double C12 = Values[w - 1, h];
                            double C21 = Values[w, h - 1];
                            double C22 = Values[w, h];

                            double res1 = (((B2 - B) / (B2 - B1) * C11) + (B - B1) / (B2 - B1) * C12) * ((A2 - A) / (A2 - A1));
                            double res2 = (((B2 - B) / (B2 - B1) * C21) + (B - B1) / (B2 - B1) * C22) * (A - A1) / (A2 - A1);

                            result = res1 + res2;
                            break;

                        }
                    }
                }
            }

            return result;
        }
    }

    





}
