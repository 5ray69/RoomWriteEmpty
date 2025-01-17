using Autodesk.Revit.DB;
using RoomWriteEmpty.Models.MyDll;
using RoomWriteEmpty.Models.MyDll.UserWarningStrings;
using RoomWriteEmpty.MyDll.UserWarningStrings;

namespace RoomWriteEmpty.MyDll
{
    public class ParameterValidatorForEmpty(ErrorModel errorModel)
    {
        private readonly ErrorModel _errorModel = errorModel;

        public void ValidateAndSetParameter(Element element, string parameterName, string value)
        {
            Parameter parameter = element.LookupParameter(parameterName);

            if (parameter == null)
            {
                _errorModel.UserWarning(new ParameterIsMissing().MessageForUser(element, parameterName));
                return; // Завершаем выполнение, если параметр не найден
            }

            if (parameter.IsReadOnly)
            {
                _errorModel.UserWarning(new ParameterIsReadOnly().MessageForUser(element, parameterName));
                return; // Завершаем выполнение, если параметр только для чтения
            }

            if (string.IsNullOrEmpty(parameter.AsString())) // if (paramElemFlat.AsString() == null || paramElemFlat.AsString() == "")
            {
                parameter.Set(value); // Устанавливаем значение, если оно ещё не задано
            }
        }
    }
}
