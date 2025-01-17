using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using RoomWriteEmpty.Models;
using RoomWriteEmpty.Models.MyDll;
using RoomWriteEmpty.MyDll;
using RoomWriteEmpty.MyDll.UserWarningStrings;
using RoomWriteEmpty.View.Windows;
using RoomWriteEmpty.ViewModels;
using System.Collections.Generic;

namespace RoomWriteEmpty
{
    [TransactionAttribute(TransactionMode.Manual)]
    //[RegenerationAttribute(RegenerationOption.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Document doc = uidoc.Document;


                ErrorModel errorModel = new();


                #region ОКНО ПОЛЬЗОВАТЕЛЯ
                var linksService = new LinksNames(doc);
                var viewModel = new MainWindowViewModel(linksService);
                var mainWindow = new MainWindow
                {
                    DataContext = viewModel  // Устанавливаем DataContext для View
                    //ViewModel(MainWindowViewModel) требует параметр IProjectLinksService в конструкторе:
                    //public MainWindowViewModel(IProjectLinksService projectLinksService)
                    //Поэтому нельзя создать экземпляр ViewModel в XAML напрямую, так как XAML не умеет передавать параметры в конструктор
                };

                RevitLinkInstance userSelectedLinkInstance = null;

                if (mainWindow.ShowDialog() == true)
                {
                    var selectedLink = viewModel.SelectedLink;
                    if (selectedLink == null)
                        errorModel.UserWarning(new ButtonApplyNoSelectUser().MessageForUser());
                    else
                        userSelectedLinkInstance = linksService.GetLinkByName(selectedLink);


                    //TaskDialog.Show("Выбор пользователя",
                    //    $"Выбранная связь: {selectedLink}");
                }

                //TaskDialog.Show("Экземпляр связи по имени",
                //        $"Связь по имени: {userSelectedLinkInstance.Name}");
                #endregion

                if (userSelectedLinkInstance != null)  // если пользователь не нажал Отмена
                {
                    Document linkDoc = userSelectedLinkInstance.GetLinkDocument();
                    RevitLinkType linkType = doc.GetElement(userSelectedLinkInstance.GetTypeId()) as RevitLinkType;
                    Transform linkTransform = userSelectedLinkInstance.GetTotalTransform();


                    using Transaction t = new(doc, "RoomWriteEmpty");
                    t.Start();

                    #region КОЛЛЕКТОР ЭЛЕМЕНТОВ
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
                                                        BuiltInCategory.OST_ElectricalCircuit
                                                        ];

                    ElementMulticategoryFilter categoryFilter = new(cats);
                    FilteredElementCollector collector = new(doc);
                    IEnumerable<Element> collectorElements = collector.WherePasses(categoryFilter)
                                                                    .WhereElementIsNotElementType();

                    ICollection<Element> finalElements = [];
                    foreach (Element element in collectorElements)
                    {
                        if (element is FamilyInstance familyInstance)
                        {
                            if (familyInstance.SuperComponent == null)
                            {
                                finalElements.Add(element);
                            }
                        }
                        else
                        {
                            finalElements.Add(element);
                        }
                    }
                    #endregion


                    #region КОЛЛЕКТОР ПОМЕЩЕНИЙ
                    // Создаем фильтр для категории помещений
                    ElementCategoryFilter roomsFilter = new(BuiltInCategory.OST_Rooms);

                    // Фильтр для исключения типов элементов
                    ElementClassFilter elementTypeFilter = new(typeof(ElementType), inverted: true);

                    // Параметр "Area" имеет значение больше 0. Так исключаем неразмещенные, неокруженные и избыточные помещения.
                    BuiltInParameter areaParameter = BuiltInParameter.ROOM_AREA;
                    FilterRule areaRule = ParameterFilterRuleFactory.CreateGreaterRule(new ElementId(areaParameter), 0.0, 1e-6); // Допуск для сравнения double
                    ElementParameterFilter areaFilter = new(areaRule);

                    // Комбинируем фильтры
                    LogicalAndFilter combinedFilter = new([
                                                            roomsFilter,
                                                            elementTypeFilter,
                                                            areaFilter
                                                            ]);

                    IEnumerable<Element> collectRooms = new FilteredElementCollector(linkDoc).WherePasses(combinedFilter);
                    #endregion


                    LocationAnyObject locationAnyObject = new();
                    ParameterValidatorForEmpty validator = new(errorModel);
                    var levelDict = LevelCache.GetLevelDictionary(doc);  // словарь уровней из основного файла


                    #region ПРОВЕРКА ЕСТЬ ЛИ ИМЕНА УРОВНЕЙ СВЯЗИ В УРОВНЯХ ОСНОВНОГО ФАЙЛА
                    ICollection<string> noLevelNames = LevelCache.GetMissingLevels(doc, linkDoc);

                    if (noLevelNames.Count > 0)
                    {
                        errorModel.UserWarning(new LevelsNotInDocument().MessageForUser(noLevelNames));
                    }
                    #endregion


                    LevelAnyObject levelAnyObject = new(doc);
                    foreach (Element roomEl in collectRooms)
                    {
                        Room room = roomEl as Room;
                        // Извлекаем ElementId из словаря по имени уровня
                        ElementId roomLevelId = levelDict[room.Level.Name];

                        string valueParamRoomName = room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                        string valueParamRoomNumber = room.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString();
                        string valueParamRoomFlat = room.LookupParameter("БУДОВА_Номер квартиры").AsString();

                        //if (valueParamRoomNumber == "461")  // тест, попадает ли элемент в это помещение

                        foreach (Element elem in finalElements)
                        {
                            //сравнение уровня комнаты и элемента можно делать только по именам, потому что сравниваемые уровни в разных документах,
                            //но чтоб не сравнивать строками сделали кэш Id в словаре
                            if (levelAnyObject.GetLevel(elem).Id == roomLevelId)
                            {
                                var xyzOrCurve = locationAnyObject.GetSpatialElementOrLocationXyzOrLine(elem);
                                ObjectToLine objectToLine = new(xyzOrCurve, room);
                                IsLocated isLocated = new(room, objectToLine.GetCreatedLine(), linkTransform);
                                if (isLocated.InsideTheBorders())
                                {
                                    validator.ValidateAndSetParameter(elem, "БУДОВА_Номер квартиры", valueParamRoomFlat);
                                    validator.ValidateAndSetParameter(elem, "БУДОВА_Номер помещения", valueParamRoomNumber);
                                    validator.ValidateAndSetParameter(elem, "БУДОВА_Имя помещения", valueParamRoomName);
                                }
                            }
                        }
                    }
                    t.Commit();
                }


                TaskDialog.Show("Уведомление", "Код завершил работу");
                return Result.Succeeded;
            }
            catch (UserNotificationException)
            {
                TaskDialog.Show("Уведомление", "Код завершил работу");
                return Result.Cancelled;
            }
        }
    }
}




////запуск таймера
//System.Diagnostics.Stopwatch watch = new();
//watch.Restart();

////останов таймера
//watch.Stop();
//MessageBox.Show(string.Format("отработало за {0} секунд.", watch.Elapsed.Seconds));


//using (Transaction createDirectShape = new(doc, "Create DirectShape"))
//{
//    createDirectShape.Start();

//    //в этот список добавляем геометрию для директшейпа - точку, линию, множество точек или линий
//    List<GeometryObject> geometryList = new List<GeometryObject>();
//    ICollection<Line> bordersRoom = new BordersRoom(room, linkTransform).GetBordersToCenter();
//    foreach (Line roomLine in bordersRoom)
//    {
//        geometryList.Add(roomLine);
//    }

//    DirectShape shape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
//    shape.SetShape(geometryList);

//    createDirectShape.Commit();
//}









//ICollection<int> resultRoom = [];

//foreach (Room room in collector.Cast<Room>())
//{
//    RoomPlaced roomPlaced = new(room);
//    resultRoom.Add(roomPlaced.CheckPlaced());
//}
//MessageBox.Show($"список результатов помещений: {string.Join(",", resultRoom)}");


//Reference myRef = uidoc.Selection.PickObject(ObjectType.Element, "Выберите элемент для вывода его Id");
//Element element = doc.GetElement(myRef);
//FamilyInstance familyInstance = element as FamilyInstance;
//ElementId id = element.Id;
//TaskDialog.Show("Id элемента Id: ", id.ToString());



////запуск таймера
//System.Diagnostics.Stopwatch watch = new();
//watch.Restart();

////останов таймера
//watch.Stop();
//MessageBox.Show(string.Format("отработало за {0} секунд.", watch.Elapsed.Seconds));




//// Флаг для отмены выполнения
//bool cancelExecution = false;

//// Подписка на события
//var linksService = new LinksNames(doc);
//linksService.NoLinksFound += (s, e) => cancelExecution = true;
//linksService.GetLinksNames();
//if (cancelExecution)
//{
//    return Result.Cancelled;
//}


//public event EventHandler<EventArgs> NoLinksFound;

//InvokeNoLinks();

//protected virtual void InvokeNoLinks()
//{
//    TaskDialog.Show("Внимание!",
//                    "В проекте нет ни одного экземпляра связи." +
//                    "\nЕсли они выгружены в диспетчере проекта," +
//                    "\nто щелкните по связи правой кнопкой мыши" +
//                    "\nи выберите Обновить."
//                    );
//    NoLinksFound?.Invoke(this, EventArgs.Empty);
//}


//FilteredElementCollector collectorLink = new(doc);
//RevitLinkInstance linkInstance = collectorLink.OfClass(typeof(RevitLinkInstance)).FirstOrDefault() as RevitLinkInstance;
//Document linkDoc = linkInstance.GetLinkDocument();
//RevitLinkType linkType = doc.GetElement(linkInstance.GetTypeId()) as RevitLinkType;
//Transform linkTransform = linkInstance.GetTotalTransform();