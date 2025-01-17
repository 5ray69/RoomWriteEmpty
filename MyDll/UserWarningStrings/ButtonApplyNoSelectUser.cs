namespace RoomWriteEmpty.MyDll.UserWarningStrings
{
    public class ButtonApplyNoSelectUser
    {
        public string MessageForUser()
        {
            string message = $@"
Пользователь нажал кнопку Применить, не сделав выбор.

Закройте это окно и запустите код заново,
но уже сделав выбор в окне,
перед тем как нажимать кнопку Применить.";

            return message;
        }
    }
}
