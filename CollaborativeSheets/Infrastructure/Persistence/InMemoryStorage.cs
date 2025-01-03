using CollaborativeSheets.Domain.Entities;
using CollaborativeSheets.Domain.ValueObjects;

namespace CollaborativeSheets.Infrastructure.Persistence
{
    public class InMemoryStorage
    {
        private readonly Dictionary<string, User> _users = new();
        private readonly Dictionary<string, Sheet> _sheets = new();

        public bool TryAddUser(string name, User user)
        {
            if (_users.ContainsKey(name)) return false;
            _users[name] = user;
            return true;
        }

        public bool TryAddSheet(string name, Sheet sheet)
        {
            if (_sheets.ContainsKey(name)) return false;
            _sheets[name] = sheet;
            return true;
        }

        public Option<User> GetUser(string name) =>
            _users.ContainsKey(name) ? Option.Some(_users[name]) : Option.None<User>();

        public Option<Sheet> GetSheet(string name) =>
            _sheets.ContainsKey(name) ? Option.Some(_sheets[name]) : Option.None<Sheet>();
    }
}
