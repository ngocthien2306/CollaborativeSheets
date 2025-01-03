using CollaborativeSheets.Domain.Entities;
using CollaborativeSheets.Domain.Interfaces;

namespace CollaborativeSheets.Application.Strategies
{
    public class RestrictedAccessStrategy : IAccessStrategy
    {
        private readonly Dictionary<(string, string), AccessRight> _rights = new();

        public void SetAccess(string user, string sheetName, bool isReadOnly)
        {
            _rights[(user, sheetName)] = new AccessRight(isReadOnly);
        }

        public bool CanEdit(string user, string sheetName)
        {
            return !_rights.ContainsKey((user, sheetName)) ||
                   !_rights[(user, sheetName)].IsReadOnly;
        }
    }

}
