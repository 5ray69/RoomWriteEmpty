using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using Line = Autodesk.Revit.DB.Line;

namespace RoomWriteEmpty.Models.MyDll
{
    public class ObjectToLine(Object lineOrXYZ, Room room, double lengthOutLine = 100000)
    {
        public readonly double _levelZ = room.Level.Elevation;
        //lengthOutLine = 100000 длина здания около 60000мм (длинный коридор)
        public readonly double _lengthOutLine = UnitUtils.ConvertToInternalUnits(lengthOutLine, UnitTypeId.Millimeters);


        //        /// <summary>
        //        /// <para>создает линию из Line, или из XYZ,
        //        /// <para>длиной 100 метров,
        //        /// <para>на уровне координаты Z уровня помещения
        //        /// </summary>
        //        /// <returns></returns>
        public Line GetCreatedLine()
        {
            Transform rotationDefault = Transform.CreateRotation(XYZ.BasisZ, 3 * (Math.PI / 180));// поворот на 3 градуса в радианах
            Transform translationDefault = Transform.CreateTranslation(new XYZ(0, _lengthOutLine, 0));
            Transform rotationTranslationDefault = rotationDefault.Multiply(translationDefault);

            if (lineOrXYZ is Line line)
            {
                XYZ startPoint = line.GetEndPoint(0);
                XYZ endPoint = line.GetEndPoint(1);

                XYZ projectedStart = new(startPoint.X, startPoint.Y, _levelZ);
                XYZ projectedEnd = new(endPoint.X, endPoint.Y, _levelZ);

                XYZ direction = projectedEnd - projectedStart;
                double distance = projectedStart.DistanceTo(projectedEnd);


                //если линия расположена вертикально, параллельно оси Z, то её проекция будет точкой
                //if (line.Direction.Normalize().Z == 1)
                if (Math.Abs(line.Direction.Z) > 0.999)
                {
                    XYZ rotatedPoint = rotationDefault.OfPoint(
                        projectedStart + translationDefault.OfVector(XYZ.BasisY)
                    );
                    return Line.CreateBound(projectedStart, rotatedPoint);
                }
                else
                {
                    // Направление линии
                    XYZ vectorOnPlaneProect = (projectedEnd - projectedStart).Normalize();

                    // Создаем вращение относительно направления линии
                    Transform rotationRelativeToLine = Transform.CreateRotation(XYZ.BasisZ, 3 * (Math.PI / 180));

                    // Применяем вращение к вектору направления линии
                    XYZ rotatedVector = rotationRelativeToLine.OfVector(vectorOnPlaneProect) * _lengthOutLine;

                    // Конечная точка после применения трансформации
                    XYZ twoPoint = projectedStart + rotatedVector;

                    return Line.CreateBound(projectedStart, twoPoint);

                }
            }


            if (lineOrXYZ is XYZ xyz)
            {

                XYZ miniStartPoint = new(xyz.X, xyz.Y, _levelZ);
                XYZ miniEndPoint = miniStartPoint + rotationTranslationDefault.OfVector(
                    new XYZ(0, _lengthOutLine, 0)
                    );
                return Line.CreateBound(miniStartPoint, miniEndPoint);

            }

            return null;
        }


        public double? GetIntersectionPointSum(Line projLine, Line lineRoom)
        {
            // Используем метод Intersect для определения пересечения
            IntersectionResultArray intersectionResult = null;
            projLine.Intersect(lineRoom, out intersectionResult);

            // Проверяем наличие пересечения
            if (intersectionResult != null && intersectionResult.Size > 0)
            {
                XYZ point = intersectionResult.get_Item(0).XYZPoint;

                // Возвращаем сумму координат точки пересечения
                //return point.X + point.Y + point.Z;
                return point.X + point.Y;
            }

            return null; // Пересечения нет
        }
    }
}
