namespace RoomWriteEmpty.Models.MyDll.UserWarningStrings
{
    public class NoLinks
    {
        public string MessageForUser()
        {
            string message = $@"
В проекте нет ни одного экземпляра связи.
Если они выгружены в диспетчере проекта,
то щелкните по связи правой кнопкой мыши
и выберите Обновить.";

            return message;
        }
    }
}
