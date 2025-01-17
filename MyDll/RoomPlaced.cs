using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;

namespace RoomWriteEmpty.Models.MyDll
{
    public class RoomPlaced(Room room)
    {
        private readonly Room _room = room;

        /// <summary>
        ///возвращает 1 если помещение размещено
        ///возвращает 2 если помещение не размещено
        ///возвращает 3 если помещение не окружено
        ///возвращает 4 если помещение избыточное
        /// </summary>
        /// <returns></returns>
        public int CheckPlaced()
        {
            //если помещение размещено
            if (_room.Area > 0)
                return 1;

            //если помещение не размещено
            else if (null == _room.Location)
                return 2;

            else
            {
                // если помещение не окружено 3 или избыточное 4

                SpatialElementBoundaryOptions opt = new();

                IList<IList<BoundarySegment>> segs = _room.GetBoundarySegments(opt);

                return (null == segs || segs.Count == 0) ? 3 : 4;

            }
        }
    }
}

//возвращает 1 если помещение размещено Placed
//возвращает 2 если помещение не размещено NotPlaced - если удалить помещение на плане и не удалить через спецификацию, его можно разместить
//возвращает 3 если помещение не окружено NotEnclosed - не полностью замкруто стенами, дырка между стен, или размещено вообще вне стен
//возвращает 4 если помещение избыточное Redundant - два помещения в одном пространстве стен, появляется, когда удалили стену между двумя помещениями

//public enum RoomState
//{
//    Unknown,
//    Placed,
//    NotPlaced,
//    NotEnclosed,
//    Redundant
//}

///// <summary>
///// Distinguish 'Not Placed',  'Redundant' 
///// and 'Not Enclosed' rooms.
///// </summary>
//RoomState DistinguishRoom(Room room)
//{
//    RoomState res = RoomState.Unknown;

//    if (room.Area > 0)
//    {
//        // Placed if having Area

//        res = RoomState.Placed;
//    }
//    else if (null == room.Location)
//    {
//        // No Area and No Location => Unplaced

//        res = RoomState.NotPlaced;
//    }
//    else
//    {
//        // must be Redundant or NotEnclosed

//        SpatialElementBoundaryOptions opt
//          = new SpatialElementBoundaryOptions();

//        IList<IList<BoundarySegment>> segs
//          = room.GetBoundarySegments(opt);

//        res = (null == segs || segs.Count == 0)
//          ? RoomState.NotEnclosed
//          : RoomState.Redundant;
//    }
//    return res;
//}
