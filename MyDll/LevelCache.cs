using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace RoomWriteEmpty.MyDll
{
    public class LevelCache
    {
        private static List<Level> _cachedLevels;
        private static Dictionary<string, ElementId> _levelDictionary;
        private static readonly object _lock = new();

        /// <summary>
        /// Получает кэшированную коллекцию уровней или создаёт её при первом запросе.
        /// </summary>
        public static List<Level> GetSortedLevels(Document doc)
        {
            if (_cachedLevels == null)
            {
                lock (_lock) //lock (замок) гарантирует, что только один поток может выполнить критический участок кода, заключённый в блок lock
                {
                    if (_cachedLevels == null)
                    {
                        FilteredElementCollector levelCollector = new(doc);
                        _cachedLevels = levelCollector.OfClass(typeof(Level))
                                                       .OfType<Level>()
                                                       .OrderBy(level => level.Elevation)
                                                       .ToList();
                    }
                }
            }
            return _cachedLevels;
        }

        /// <summary>
        /// Получает словарь, где ключ — имя уровня, а значение — его ElementId.
        /// </summary>
        public static Dictionary<string, ElementId> GetLevelDictionary(Document doc)
        {
            if (_levelDictionary == null)
            {
                lock (_lock)
                {
                    //  _levelDictionary ?? это все равно что if (_levelDictionary == null)
                    _levelDictionary ??= GetSortedLevels(doc)
                            .ToDictionary(level => level.Name, level => level.Id);
                }
            }
            return _levelDictionary;
        }


        /// <summary>
        /// Проверяет, все ли имена уровней из связанного файла есть в уровнях основного файла.
        /// </summary>
        public static ICollection<string> GetMissingLevels(Document doc, Document linkDoc)
        {
            var levelDict = GetLevelDictionary(doc); // словарь уровней из основного файла

            FilteredElementCollector levelCollector = new(linkDoc);
            IEnumerable<Level> linkedLevels = levelCollector.OfClass(typeof(Level)).OfType<Level>(); // уровни из связанного файла

            List<string> noLevelNames = new();
            foreach (Level linkedLevel in linkedLevels)
            {
                string levelName = linkedLevel.Name;
                if (!levelDict.ContainsKey(levelName)) // проверяем, есть ли имя уровня
                {
                    noLevelNames.Add(levelName);
                }
            }
            return noLevelNames;
        }

    }
}

