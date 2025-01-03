namespace CollaborativeSheets.Domain.Interfaces
{
    public interface IObserver
    {
        void Update(string sheetName, (int, int) position, string value);
    }
}
