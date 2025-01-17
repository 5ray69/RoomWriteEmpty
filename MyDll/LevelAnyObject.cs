using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using RoomWriteEmpty.Models.MyDll;
using RoomWriteEmpty.MyDll.UserWarningStrings;
using System;

namespace RoomWriteEmpty.MyDll
{
    public class LevelAnyObject(Document doc)
    {
        private readonly Document _doc = doc;


        /// <summary>
        /// <para>извлекает уровень, на котором находится любой объект электрика,
        /// <para>включая семейства на грани (не привязанные к уровню) и аннотации
        /// </summary>
        /// <returns></returns>
        public Level GetLevel(Element element)
        {
            //НЕЛЬЗЯ МЕНЯТЬ ПОРЯДОК СЛЕДОВАНИЯ УСЛОВИЙ if
            //некоторые if подпадают под условия друг друга (семейства имеющие Host и HostFace)


            // Проверка на наличие свойства LevelId и извлечение его значения
            //familyinstance, соед.детали кабельных лотков, соед.детали коробов
            // LevelId у тех, у кого семейство НЕ на основе рабочей плоскости/НЕ на грани
            if (element.LevelId != null && element.LevelId.IntegerValue != -1)
            {
                return _doc.GetElement(element.LevelId) as Level;
            }


            // HostFace is null у тех семейств, у которых рабочая плоскость это уровень, а не на грани геометрии
            // Светильники, выключатели, розетки... и здесь полоса заземления, которая размещена на уровне
            if (element is FamilyInstance familyInstance && familyInstance.HostFace is null)
            {
                return familyInstance.Host as Level;
            }


            // короба свойство ReferenceLevel
            if (element is Conduit conduit && conduit.ReferenceLevel is Level refLevel)
            {
                return refLevel;
            }


            // кабельные лотки свойство ReferenceLevel
            if (element is CableTray cableTray && cableTray.ReferenceLevel is Level referenceLevel)
            {
                return referenceLevel;
            }


            if (element is ElectricalSystem electricalSystem)
            {
                var baseEquipment = electricalSystem.BaseEquipment;
                if ( baseEquipment == null ) // цепь не подключена "код завершил работу" с предупреждением пользователю
                {
                    ErrorModel errorModel = new();
                    errorModel.UserWarning(new NoConnectCircuit().MessageForUser(electricalSystem));
                }

                if (baseEquipment?.LevelId != null && baseEquipment.LevelId.IntegerValue != -1)
                {
                    return _doc.GetElement(baseEquipment.LevelId) as Level;
                }

                if (baseEquipment?.Host is Level baseHostLevel)
                {
                    return baseHostLevel;
                }
            }


            // полоса заземления и другие семейства на основе рабочей плоскости/на грани
            // если element это FamilyInstance и его свойство HostFace возвращает тип Reference (не null)
            // синтаксис if использует шаблоны с условиями C# (8.0 и выше)
            // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns#property-patterns
            if (element is FamilyInstance { HostFace: Reference })
            {
                double zElement = 0;

                if (element.Location is LocationCurve locationCurve)
                {
                    //нижняя точка линии, та у которой значение высотной отметки меньше
                    double minZ = Math.Round(
                        Math.Min(locationCurve.Curve.GetEndPoint(0).Z, locationCurve.Curve.GetEndPoint(1).Z),
                        3, MidpointRounding.AwayFromZero);
                    zElement = minZ;
                }
                else if (element.Location is LocationPoint locationPoint)
                {
                    double zPoint = Math.Round(locationPoint.Point.Z, 3, MidpointRounding.AwayFromZero);
                    zElement = zPoint;
                }

                return new ElevationDouble(_doc, zElement).AboveLevel();
            }


            // Проверка на наличие OwnerViewId, принадлежности виду и тем, что OwnerView это ViewPlan исключаем возможность,
            // что аннотация не находится на ViewDrafting, ViewSection или ViewSheet

            //# текстовые примечания, линии детализации, марки электрооборудования, марки осветительных приборов,
            //# марки помещений, марки нескольких категорий, марки коробов, типовые аннотации (стрелка выносок), размеры
            if (element.OwnerViewId != null && _doc.GetElement(element.OwnerViewId) is ViewPlan ownerView)
            {
                return ownerView.GenLevel as Level;
            }


            return null;
        }
    }
}

        ///// <summary>
        ///// Id уровня, на котором находится любой объект электрика
        ///// </summary>
        ///// <returns></returns>
        //public ElementId GetElementId(Element element)
        //{
        //    var level = GetLevel(element);
        //    return level?.Id;
        //}

        ///// <summary>
        ///// Имя уровня, на котором находится любой объект электрика
        ///// </summary>
        ///// <returns></returns>
        //public string GetName(Element element)
        //{
        //    var level = GetLevel(element);
        //    return level?.Name;
        //}

        ///// <summary>
        ///// Высотная отметка уровня, на котором находится любой объект электрика
        ///// </summary>
        ///// <returns></returns>
        //public double? GetElevation(Element element)
        //{
        //    var level = GetLevel(element);
        //    return level?.Elevation;
        //}
