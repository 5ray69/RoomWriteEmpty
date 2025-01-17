using Autodesk.Revit.DB;

namespace RoomWriteEmpty.Models
{
    public class ProjectDataServiceFactory
    {
        private readonly Document _doc;

        public ProjectDataServiceFactory(Document doc)
        {
            _doc = doc;
        }

        public IProjectLinksService CreateLinksService()
        {
            return new LinksNames(_doc);
        }
    }
}
