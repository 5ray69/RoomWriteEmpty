using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomWriteEmpty.MyDll
{
    public class BordersRoomProcessor
    {
        private const double MinLineLength = 0.001; // Минимальная длина линии в единицах Revit

        /// <summary>
        /// Получение замкнутых линий из списка линий границ помещения
        /// </summary>
        /// <param name="lines">Список исходных линий</param>
        /// <returns>Список замкнутых линий</returns>
        public List<Line> ClosePolygonWithAdjustedPoints(List<Line> lines)
        {
            var closedLines = new List<Line>(lines);

            for (int i = 0; i < lines.Count; i++)
            {
                Line currentLine = lines[i];
                // Оператор остаток от деления % позволяет не выйти за пределы списка, автоматически "перепрыгивая" на начало, когда индекс достигает конца.
                // lines.Count/lines.Count = 1 и остаток от деления получается 0.
                // Когда i = 0, то делим 1 на 4. Целая часть = 0 (1 меньше 4). Остаток = 1.
                Line nextLine = lines[(i + 1) % lines.Count]; // Циклический доступ для замыкания

                XYZ currentEnd = currentLine.GetEndPoint(1); // Конечная точка текущей линии
                XYZ nextStart = nextLine.GetEndPoint(0);    // Начальная точка следующей линии

                // Проверяем расстояние между текущими точками
                if (currentEnd.DistanceTo(nextStart) > MinLineLength)
                {
                    closedLines.Add(Line.CreateBound(currentEnd, nextStart));
                }
                else
                {
                    // Если точки слишком близко, ищем альтернативные комбинации точек
                    XYZ alternativePoint = FindAlternativePoint(currentLine, nextLine);
                    if (alternativePoint != null)
                    {
                        closedLines.Add(Line.CreateBound(currentEnd, alternativePoint));
                        closedLines.Add(Line.CreateBound(alternativePoint, nextStart));
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
                    if (p1.DistanceTo(p2) > MinLineLength)
                    {
                        return p2;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Проекция точки на бесконечную линию
        /// </summary>
        private XYZ ProjectPointOnLine(XYZ point, Line line)
        {
            // Проекция точки на бесконечную линию
            using (Line unboundLine = Line.CreateUnbound(line.Origin, line.Direction))
            {
                return unboundLine.Project(point).XYZPoint;
            }
        }
    }
}
