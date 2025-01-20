using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;
using Line = Autodesk.Revit.DB.Line;

namespace RoomWriteEmpty.Models.MyDll
{
    public class IsLocated
    {
        /// <summary>
        /// <para>ВОЗВРАЩАЕТ TRUE ЕСЛИ ОБЪЕКТ В ПОМЕЩЕНИИ.
        /// <para>Принимает преобразованную Line из ObjectToLine.
        /// <para>Даже если одна точка/край линии лежит на границе помещения, а вся остальная часть линии вне.
        /// </summary>
        /// <returns></returns>
        public bool InsideTheBorders(List<Line> linesRoomBordersToZ, Line projectLine)
        {
            CountIntersectionsWithPolygon countIntersectionsWithPolygon = new(linesRoomBordersToZ, projectLine);
            HashSet<double> projectIntersect = countIntersectionsWithPolygon.GetHashSet();

            //Провепка очень длинной линией. Начало линии всегда внутри помещения из которого линия исходит

            //ЕСЛИ КОЛИЧЕСТВО ПЕРЕСЕЧЕНИЙ В HashSet РАВНО НУЛЮ = ВНЕ ПОМЕЩЕНИЯ
            //остаток от деления нуля на 2 тоже ноль = четное
            if (projectIntersect.Count == 0)
                return false;

            //ЕСЛИ КОЛИЧЕСТВО ПЕРЕСЕЧЕНИЙ В HashSet ЧЕТНОЕ = ВНЕ ПОМЕЩЕНИЯ
            if (projectIntersect.Count % 2 == 0)
                return false;


            //ЕСЛИ КОЛИЧЕСТВО ПЕРЕСЕЧЕНИЙ В HashSet НЕЧЕТНОЕ = ВНУТРИ ПОМЕЩЕНИЯ
            if (projectIntersect.Count % 2 != 0)
                return true;

            // Возвращение значения по умолчанию
            return false;
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
