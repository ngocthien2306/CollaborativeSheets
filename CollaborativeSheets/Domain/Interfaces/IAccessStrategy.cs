namespace CollaborativeSheets.Domain.Interfaces
{
    public interface IAccessStrategy
    {
        bool CanEdit(string user, string sheetName);
    }
}
