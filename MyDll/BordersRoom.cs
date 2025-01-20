using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;

namespace RoomWriteEmpty.Models.MyDll
{
    public class BordersRoom(Room room, Transform linkTransform)
    {
        public readonly double _levelZ = room.Level.Elevation;

        /// <summary>
        /// <para>ЛИНИИ ГРАНИЦ ПОМЕЩЕНИЯ
        /// <para>на уровне координаты Z уровня помещения
        /// <para>с учетом трансформации связи"""
        /// </summary>
        /// <returns></returns>
        public List<Line> GetBordersToCenter()
        {
            //назначаем переменную на свойство
            SpatialElementBoundaryOptions roomoptions = new()
            {
                //установили свойство границы помещения по осевой линии / центру
                SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center
            };

            //ГРАНИЦЫ ПОМЕЩЕНИЯ в уровне координаты Z того уровня, к которому привязано помещение
            List<Line> roomLines = [];
            foreach (IList<BoundarySegment> segmentList in room.GetBoundarySegments(roomoptions))
            {
                foreach (BoundarySegment boundarySegment in segmentList)
                {
                    Curve curveSegment = boundarySegment.GetCurve();

                    XYZ startXYZ = curveSegment.GetEndPoint(0);
                    XYZ endXYZ = curveSegment.GetEndPoint(1);
                    roomLines.Add(Line.CreateBound(
                                    linkTransform.OfPoint(new XYZ(startXYZ.X, startXYZ.Y, _levelZ)),
                                    linkTransform.OfPoint(new XYZ(endXYZ.X, endXYZ.Y, _levelZ))
                                    )
                                );
                }
            }
            return roomLines;
        }


        /// <summary>
        /// <para>XYZ ВЕРШИН УГЛОВ ГРАНИЦ ПОМЕЩЕНИЯ
        /// <para>образованы только началами линий
        /// <para>на уровне координаты Z уровня помещения
        /// <para>с учетом трансформации связи
        /// </summary>
        /// <returns></returns>
        public ICollection<XYZ> GetXYZVerticesBorders()
        {
            ICollection<XYZ> verticesRoomLines = [];

            foreach (Line linesBorder in this.GetBordersToCenter())
                verticesRoomLines.Add(linkTransform.OfPoint(new XYZ(
                                linesBorder.GetEndPoint(0).X, linesBorder.GetEndPoint(0).Y, _levelZ
                                )));

            return verticesRoomLines;
        }
    }
}
