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

        public double Interpolation(double hw,double rw)
        {
            double result = 0;
            double valueA1 = 0;
            double valueA2 = 0;
            double valueB1 = 0;
            double ValueB2 = 0;
            for (int h =1;h<11;h++)
            {
                if (hw == Values[0,h])
                {
                    for (int w =1;w<7;w++)
                    {
                        if (rw == Values[w,0])
                        {
                            result = Values[w, h];
                        }
                    }
                }
            }

            return result;
        }
    }

    





}
