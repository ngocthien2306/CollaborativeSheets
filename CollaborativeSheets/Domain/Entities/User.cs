
using CollaborativeSheets.Domain.Interfaces;

namespace CollaborativeSheets.Domain.Entities
{
    public class User : IObserver
    {
        public string Name { get; }

        public User(string name)
        {
            Name = name;
        }

        public void Update(string sheetName, (int, int) position, string value)
        {
            Console.WriteLine($"User {Name} received update for sheet {sheetName}: Position {position} changed to {value}");

        }
    }
}
