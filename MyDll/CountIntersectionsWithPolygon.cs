using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using Line = Autodesk.Revit.DB.Line;

namespace RoomWriteEmpty.Models.MyDll
{
    public class CountIntersectionsWithPolygon(ICollection<Line> roomBorders, Line projectLine)
    {
        private readonly ICollection<Line> _roomBorders = roomBorders;
        private readonly Line _projectLine = projectLine;


        /// <summary>
        /// <para>КОЛИЧЕСТВО ПЕРЕСЕЧЕНИЙ С МНОГОУГОЛЬНИКОМ ГРАНИЦ ПОМЕЩЕНИЯ
        /// <para>Возвращает HashSet, в котором количество значений -
        /// <para>это количество пересечений, тестовой линией
        /// </summary>
        /// <returns></returns>
        public HashSet<double> GetHashSet()
        {
            HashSet<double> projectIntersect = [];

            XYZ projectStart = _projectLine.GetEndPoint(0);
            XYZ projectEnd = _projectLine.GetEndPoint(1);

            foreach (Line roomLine in _roomBorders)
            {


                // Проходя через вершину, линия пересекает две линии границы помещения сразу и дает 2 пересечения.
                // Если такая вершина уже есть в GetHashSet, то второе пересечение ее же, но в другой линии,
                // не будет добавлено в GetHashSet так как такая сумма координат точки пересечения
                // уже есть и будет зачтено как одно пересечение

                // если проекция линии пересекает границу помещения
                double? intersectionSum = GetIntersectionPointSum(_projectLine, roomLine);
                // Проверяем, что intersectionSum не null
                if (intersectionSum.HasValue)
                {
                    projectIntersect.Add(intersectionSum.Value);
                }


                XYZ roomStart = roomLine.GetEndPoint(0);
                XYZ roomEnd = roomLine.GetEndPoint(1);

                // Если начальная точка _projectLine находится на линии границы помещения
                // Измеряем расстояние от точки до начала и конца отрезка. Точка лежит на отрезке, если сумма этих двух расстояний равна длине отрезка
                if (IsPointOnLine(projectStart, roomStart, roomEnd) || IsPointOnLine(projectEnd, roomStart, roomEnd))
                {
                    //перед выходом из метода очистили HashSet
                    projectIntersect.Clear();

                    //перед выходом из метода и после очистки HashSet добавили любое число
                    projectIntersect.Add(3);
                    return projectIntersect;
                }


                // Если начальная точка _projectLine совпадает с начальной точкой roomLine
                // если линия начинается в вершине многоугольника, то может быть два пересечения
                if (projectStart.IsAlmostEqualTo(roomStart) ||
                    projectEnd.IsAlmostEqualTo(roomStart) ||
                    projectStart.IsAlmostEqualTo(roomEnd) ||
                    projectEnd.IsAlmostEqualTo(roomEnd))
                {
                    //перед выходом из метода очистили HashSet
                    projectIntersect.Clear();

                    //перед выходом из метода и после очистки HashSet добавили любое число
                    projectIntersect.Add(1);
                    return projectIntersect;
                }
            }

            return projectIntersect;
        }


        public double? GetIntersectionPointSum(Line line1, Line line2)
        {
            // Прямая проверка пересечения
            IntersectionResultArray intersectionResult = null;
            line1.Intersect(line2, out intersectionResult);

            // Если есть пересечение, возвращаем сумму координат точки
            if (intersectionResult != null && intersectionResult.Size > 0)
            {
                XYZ point = intersectionResult.get_Item(0).XYZPoint;

                //округлили до 3 знаков после запятой и отбросили дробную часть
                long endX = (long)(point.X * 1000);
                long endY = (long)(point.Y * 1000);

                //Побитовое соединение чисел endX и endY
                //long combinedEnd = (endX << 32) | endY;
                return (endX << 32) | endY;
            }

            // Пересечений нет
            return null;
        }


        /// <summary>
        /// Проверяет, лежит ли точка на отрезке (линии границы помещения).
        /// </summary>
        /// <param name="projectPoint">Проверяемая точка</param>
        /// <param name="roomStartPoint">Начальная точка отрезка</param>
        /// <param name="roomEndPoint">Конечная точка отрезка</param>
        /// <param name="tolerance">Допустимая погрешность</param>
        /// <returns>True, если точка лежит на отрезке</returns>

        public bool IsPointOnLine(XYZ projectPoint, XYZ roomStartPoint, XYZ roomEndPoint, double tolerance = 0.001)
        {

            // длина линии границы помещения
            double lengthLineRoom = roomStartPoint.DistanceTo(roomEndPoint);

            //длина от точки до начала линии границы помещения
            double distanceToStartLineRoom = projectPoint.DistanceTo(roomStartPoint);

            //длина от точки до конца линии границы помещения
            double distanceToEndLineRoom = projectPoint.DistanceTo(roomEndPoint);

            // Измеряем расстояние от точки до начала и конца отрезка. Точка лежит на отрезке, если сумма этих двух расстояний равна длине отрезка
            // Проверяем, равна ли сумма расстояний длине отрезка (с учетом допуска)
            return Math.Abs((distanceToStartLineRoom + distanceToEndLineRoom) - lengthLineRoom) < tolerance;
        }
    }
}



//projectLine.Intersect(roomLine, out IntersectionResultArray resultArray) == SetComparisonResult.Equal
//Метод Intersect возвращает тип SetComparisonResult, который может быть:
//Disjoint: Линии не пересекаются.
//Disjoint - непересекающиеся/несвязаны (Оба набора/линии не пусты и не перекрываются)
//Overlap: Линии пересекаются(полностью или частично).
//Overlap - пересечение (Перекрытие двух наборов/линий не является пустым и строгим подмножеством обоих наборов)
//Subset: Одна линия является частью другой.
//Superset: Одна линия содержит другую.
//Equal: Линии совпадают полностью.
//Equal - совпадение двух линий (частичное совпадение неодинаковых линий, но лежащих одна в другой или совпадение абсолютных копий двух линий)

