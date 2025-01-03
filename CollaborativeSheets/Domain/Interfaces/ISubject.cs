namespace CollaborativeSheets.Domain.Interfaces
{
    public interface ISubject
    {
        void Attach(IObserver observer, string sheetName);
        void Detach(IObserver observer, string sheetName);
        void Notify(string sheetName, (int, int) position, string value);
    }
}
