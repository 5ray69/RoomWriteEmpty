using Autodesk.Revit.DB.Electrical;

namespace RoomWriteEmpty.MyDll.UserWarningStrings
{
    public class NoConnectCircuit
    {
        public string MessageForUser(ElectricalSystem electricalSystem)
        {
            string message = $@"
Электрическая цепь,
Id которой {electricalSystem.Id.IntegerValue}
не подключена.

Удалите все неподключенные цепи или
подключите каждую из них к панели/щиту.
И запустите код заново.

Неподключенные цепи можно найти
в Диспетчере инженерных систем (F9).
У всех неподключенных цепей будет <без имени>.
";

            return message;
        }
    }
}
