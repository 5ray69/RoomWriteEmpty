using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoomWriteEmpty.MyDll
{
    public class ElevationDouble(Document doc, double zElement)
    {
        //ОБЯЗАТЕЛЬНО ПРИВЕДЕНИЕ К ЦЕЛЫМ ЧИСЛАМ В СРАВНЕНИЯХ (усечение вместо округления)
        //иначе полоса привязанная к уровню может не совпасть из-за неточностей в двоичных числах

        //ПРИВЕДЕНИЕ К ЦЕЛЫМ ЧИСЛАМ:
        //(long)(double * 1000) - преобразуем в целое число для сравнения, отбрасывая дробную часть
        //умножаем на 1000, чтоб сравнивать числа до трех знаков после запятаой

        /// <summary>
        /// Над каким уровнем высотная отметка объекта в виде double
        /// </summary>
        /// <returns></returns>
        public Level AboveLevel()
        {
            // Получаем уровни из кэша
            List<Level> levels = LevelCache.GetSortedLevels(doc);

            // Самый нижний уровень
            Level lowLevel = levels.FirstOrDefault();
            if (lowLevel == null)
                return null;  // нет уровней

            //double lowLevelElevation = (long)(lowLevel.Elevation * 1000);
            double lowLevelElevation = Math.Round(lowLevel.Elevation, 3, MidpointRounding.AwayFromZero);

            // Проверка, если элемент ниже самого нижнего уровня
            if (zElement < lowLevelElevation)
            {
                return lowLevel;
            }
            else
            {
                Level closestLevel = null;
                double minDifference = double.MaxValue;  //наибольшее возможное значение типа Double

                // Ищем ближайший уровень
                foreach (Level level in levels)
                {
                    //double roundedElevation = (long)(level.Elevation * 1000);
                    double roundedElevation = Math.Round(level.Elevation, 3, MidpointRounding.AwayFromZero);

                    //если высота отметки уровня <= отметки элемента
                    //уровни, которые ниже элемента
                    if (roundedElevation <= zElement)
                    {
                        //разность высотной отметки элемента и уровня
                        //обязательно округленная, иначе полоса привязанная к уровню может не совпасть из-за неточностей в двоичных числах
                        double difference = zElement - roundedElevation;
                        //если разность меньше минимальной разности,
                        //то минимальной разности присваиваем значение разности/высоты элемента над уровнем, а closestLevel присваиваем значение уровня
                        //так находим минимальную разность
                        if (difference < minDifference)
                        {
                            minDifference = difference;
                            closestLevel = level;
                        }
                    }
                    //else
                    //{
                    //    // Прерываем цикл, так как уровни отсортированы
                    //    break;
                    //}
                }

                return closestLevel;
            }
        }
    }
}
