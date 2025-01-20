using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Line = Autodesk.Revit.DB.Line;

namespace RoomWriteEmpty.MyDll
{
    public class PolygonBordersRoom(UIApplication uiapp)
    {
        private readonly double MinLineLength = uiapp.Application.ShortCurveTolerance; // Минимальная длина линии в единицах Revit

        /// <summary>
        /// Создаем новый многоугольник, приведенный к координате Z уровня помещения,
        /// замкнув линии, если были не замкнуты
        /// </summary>
        public List<Line> CreateCopy(List<Line> linesRoomBorders, Room room)
        {
            double _levelZ = room.Level.Elevation;

            // Приводим линии к одной плоскости
            List<Line> linesToZ = LinesToPlane(linesRoomBorders, _levelZ);

            // Проверяем, замкнут ли многоугольник
            return IsClosed(linesRoomBorders) ? linesToZ : CloseLines(linesToZ);
        }



        /// <summary>
        /// Приводим все линии границ помещения к координате Z уровня
        /// </summary>
        private List<Line> LinesToPlane(List<Line> linesRoomBorders, double newZ)
        {
            var result = new List<Line>(linesRoomBorders.Count);
            double roundedNewZ = Math.Round(newZ, 3);

            for (int i = 0; i < linesRoomBorders.Count; i++)
            {
                var line = linesRoomBorders[i];
                var start = line.GetEndPoint(0);
                var end = line.GetEndPoint(1);

                // Округление координат Z до 3 знаков
                double startZ = Math.Round(start.Z, 3);
                double endZ = Math.Round(end.Z, 3);

                // Проверка координат с округлением
                if (startZ == roundedNewZ && endZ == roundedNewZ)
                {
                    result.Add(line); // Линия остается без изменений
                }
                else
                {
                    // Создаем новую линию на заданной высоте
                    var newStart = new XYZ(start.X, start.Y, newZ);
                    var newEnd = new XYZ(end.X, end.Y, newZ);
                    result.Add(Line.CreateBound(newStart, newEnd));
                }
            }

            return result;
        }


        /// <summary>
        /// Проверяет, является ли многоугольник замкнутым.
        /// </summary>
        /// <param name="lines">Список линий, образующих многоугольник.</param>
        /// <returns>True, если многоугольник замкнут, иначе False.</returns>
        public bool IsClosed(List<Line> lines)
        {
            // Словарь для подсчета начальных и конечных точек
            Dictionary<XYZ, int> pointCounts = new Dictionary<XYZ, int>(new XyzEqualityComparer());

            foreach (var line in lines)
            {
                XYZ start = line.GetEndPoint(0);
                XYZ end = line.GetEndPoint(1);

                // Увеличиваем счётчики для начальной и конечной точек
                if (pointCounts.ContainsKey(start))
                    pointCounts[start]++;
                else
                    pointCounts[start] = 1;

                if (pointCounts.ContainsKey(end))
                    pointCounts[end]++;
                else
                    pointCounts[end] = 1;
            }

            // Проверяем, что каждая точка имеет чётное количество вхождений
            return pointCounts.Values.All(count => count % 2 == 0);
        }


        /// <summary>
        /// Класс для сравнения объектов XYZ.
        /// </summary>
        private class XyzEqualityComparer : IEqualityComparer<XYZ>
        {
            public bool Equals(XYZ x, XYZ y)
            {
                return x.IsAlmostEqualTo(y);
            }

            public int GetHashCode(XYZ obj)
            {
                return obj.GetHashCode();
            }
        }


        /// <summary>
        /// Получение замкнутых линий из списка линий границ помещения
        /// </summary>
        /// <param name="lines">Список исходных линий</param>
        /// <returns>Список замкнутых линий</returns>
        public List<Line> CloseLines(List<Line> lines)
        {
            var closedLines = new List<Line>(lines);

            for (int i = 0; i < lines.Count; i++)
            {
                Line currentLine = lines[i];
                Line nextLine = lines[(i + 1) % lines.Count]; // Циклический доступ для замыкания

                XYZ currentEnd = currentLine.GetEndPoint(1); // Конечная точка текущей линии
                XYZ nextStart = nextLine.GetEndPoint(0);    // Начальная точка следующей линии

                // Проверяем расстояние между текущими точками
                double distance = currentEnd.DistanceTo(nextStart);
                if (distance > MinLineLength)
                {
                    closedLines.Add(Line.CreateBound(currentEnd, nextStart));
                }
                else
                {
                    // Если точки слишком близко, ищем альтернативные комбинации точек
                    XYZ alternativePoint = FindAlternativePoint(currentLine, nextLine);
                    if (alternativePoint != null)
                    {
                        double distanceToAlt1 = currentEnd.DistanceTo(alternativePoint);
                        double distanceToAlt2 = alternativePoint.DistanceTo(nextStart);

                        if (distanceToAlt1 > MinLineLength)
                        {
                            closedLines.Add(Line.CreateBound(currentEnd, alternativePoint));
                        }

                        if (distanceToAlt2 > MinLineLength)
                        {
                            closedLines.Add(Line.CreateBound(alternativePoint, nextStart));
                        }
                    }
                }
            }

            return closedLines;
        }

        /// <summary>
        /// Поиск альтернативной точки соединения
        /// </summary>
        private XYZ FindAlternativePoint(Line line1, Line line2)
        {
            var points1 = new[] { line1.GetEndPoint(0), line1.GetEndPoint(1) }; // Точки линии 1
            var points2 = new[] { line2.GetEndPoint(0), line2.GetEndPoint(1) }; // Точки линии 2

            foreach (var p1 in points1)
            {
                foreach (var p2 in points2)
                {
                    if (p1.DistanceTo(p2) > 0.001) // погрешность для сравнения двоичных чисел
                    {
                        return p2;
                    }
                }
            }
            return null;
        }
    }
}
