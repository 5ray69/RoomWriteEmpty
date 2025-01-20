using Autodesk.Revit.DB;

namespace RoomWriteEmpty.MyDll.UserWarningStrings
{
    public class ParameterIsReadOnly
    {
        public string MessageForUser(Element el, string str)
        {
            string message = $@"
Параметр
{str}
только для чтения

у элемента с именем:
{el.Name}

c Id элемента:
{el.Id.IntegerValue}

категория:
{el.Category?.Name ?? "у элемента нет категории"}

Обратитесь к координатору для восстановления параметра.

После исправления параметров можно будет
пользоваться кодом заново.";

            return message;
        }
    }
}
