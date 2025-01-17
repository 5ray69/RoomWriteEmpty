using Autodesk.Revit.UI;

namespace RoomWriteEmpty.Models.MyDll
{
    public class ErrorModel
    {
        public void UserWarning(string message)
        {
            TaskDialog.Show("Внимание!", message);
            throw new UserNotificationException();  // Останавливает выполнение с исключением
        }
    }
}
