using CollaborativeSheets.Domain.Interfaces;

namespace CollaborativeSheets.Application.Strategies
{
    public class DefaultAccessStrategy : IAccessStrategy
    {
        public bool CanEdit(string user, string sheetName) => true;
    }

}
