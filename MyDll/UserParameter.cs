using Autodesk.Revit.DB;
using RoomWriteEmpty.Models.MyDll.UserWarningStrings;

namespace RoomWriteEmpty.Models.MyDll
{
    public class UserParameter
    {
        public void ValueToNull(Element el, string str)
        {
            Parameter parameter = el.LookupParameter(str);

            if (parameter != null)
            {
                StorageType storageType = parameter.StorageType;

                if (storageType == StorageType.Integer)
                    parameter.Set(0);
                if (storageType == StorageType.Double)
                    parameter.Set(0.0);
                if (storageType == StorageType.String)
                    parameter.Set("");
                if (storageType == StorageType.ElementId)
                    parameter.Set(new ElementId(-1));
            }

            else
            {
                ErrorModel errorModel = new();
                errorModel.UserWarning(new ParameterIsMissing().MessageForUser(el, str));
            }
        }
    }
}
