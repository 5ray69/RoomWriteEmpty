using Autodesk.Revit.DB;
using RoomWriteEmpty.MyDll;
using System.Collections.Generic;
using System.Linq;

namespace RoomWriteEmpty.Infrastructure.MyDll
{
    public class AllObjectsElectric(Document doc, List<string> userSelect)
    {
        private readonly Document _doc = doc;
        private readonly List<string> _levelNames = userSelect;
        private readonly LevelAnyObject _levelAnyObject = new(doc);

        public ICollection<ElementId> GetElementId()
        {
            ICollection<ElementId> elementIds = [];


            ICollection<BuiltInCategory> cats =
                    [
                        BuiltInCategory.OST_ElectricalFixtures,
                        BuiltInCategory.OST_ElectricalEquipment,
                        BuiltInCategory.OST_LightingDevices,
                        BuiltInCategory.OST_LightingFixtures,
                        BuiltInCategory.OST_ConduitFitting,
                        BuiltInCategory.OST_CableTrayFitting,
                        BuiltInCategory.OST_FireAlarmDevices,
                        BuiltInCategory.OST_MechanicalEquipment,
                        BuiltInCategory.OST_Conduit,
                        BuiltInCategory.OST_CableTray,
                        BuiltInCategory.OST_TextNotes,  // текстовые примечания
                        BuiltInCategory.OST_Lines,  // линии детализации
                        BuiltInCategory.OST_ElectricalEquipmentTags,  // марки электрооборудования
                        BuiltInCategory.OST_LightingFixtureTags,  // марки осветительных приборов
                        BuiltInCategory.OST_RoomTags,  // марки помещений
                        BuiltInCategory.OST_MultiCategoryTags,  // марки нескольких категорий
                        BuiltInCategory.OST_ConduitTags,  // марки коробов
                        BuiltInCategory.OST_GenericAnnotation,  // типовые аннотации (стрелка выносок)
                        BuiltInCategory.OST_Dimensions,  // размеры
                        BuiltInCategory.OST_IOSDetailGroups,  // группы элементов узлов
                        BuiltInCategory.OST_ElectricalCircuit
                    ];

            ElementMulticategoryFilter filter = new(cats);
            FilteredElementCollector collector = new(_doc);
            IEnumerable<Element> elements = collector.WherePasses(filter)
                                                    .WhereElementIsNotElementType()
                                                    .Where(element =>
                                                                    {
                                                                        // является ли элемент типом FamilyInstance
                                                                        // и имеет ли свойство SuperComponent,
                                                                        // если SuperComponent == null, то элемент в выборку не попадает
                                                                        if (element is FamilyInstance familyInstance)
                                                                            return familyInstance.SuperComponent == null;
                                                                        // для остальных элементов просто включаем их в коллекцию
                                                                        return true;
                                                                    }
                                                            );

            foreach (Element element in elements)
            {
                if (_levelNames.Contains(_levelAnyObject?.GetLevel(element).Name))
                    elementIds.Add(element.Id);
            }

            return elementIds;
        }
    }
}
