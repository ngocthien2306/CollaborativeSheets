using CollaborativeSheets.Application.Common;
using CollaborativeSheets.Application.Strategies;
using CollaborativeSheets.Domain.Entities;
using CollaborativeSheets.Domain.Interfaces;
using CollaborativeSheets.Domain.ValueObjects;

namespace CollaborativeSheets.Application.Services
{
    public class CollaborativeSystem : ISubject
    {
        private readonly Dictionary<string, HashSet<IObserver>> _sheetObservers = new Dictionary<string, HashSet<IObserver>>(); // can be use new() instead of new Dictionary<string, HashSet<IObserver>>()
        private readonly Dictionary<string, User> _users = new Dictionary<string, User>();
        private readonly Dictionary<string, Sheet> _sheets = new Dictionary<string, Sheet>();
        private IAccessStrategy _accessStrategy = new DefaultAccessStrategy();

        // Strategy Pattern usage
        public void EnableAccessControl()
        {
            _accessStrategy = new RestrictedAccessStrategy();
        }

        public void DisableAccessControl()
        {
            _accessStrategy = new DefaultAccessStrategy();
        }

        // Observer Pattern implementation
        public void Attach(IObserver observer, string sheetName)
        {
            if (!_sheetObservers.ContainsKey(sheetName))
            {
                _sheetObservers[sheetName] = new HashSet<IObserver>();
            }
            _sheetObservers[sheetName].Add(observer);
            Logger.Log($"Observer {(observer as User)?.Name ?? "Unknown"} attached to sheet {sheetName}");
        }

        public void Detach(IObserver observer, string sheetName)
        {
            if (_sheetObservers.ContainsKey(sheetName))
            {
                _sheetObservers[sheetName].Remove(observer);
                Logger.Log($"Observer {(observer as User)?.Name ?? "Unknown"} detached from sheet {sheetName}");
            }
        }

        public void Notify(string sheetName, (int, int) position, string value)
        {
            if (_sheetObservers.ContainsKey(sheetName))
            {
                foreach (var observer in _sheetObservers[sheetName])
                {
                    observer.Update(sheetName, position, value);
                }
            }
        }

        // Functional operations using LINQ and immutable data
        public Option<User> CreateUser(string name)
        {
            if (!_users.ContainsKey(name))
            {
                var user = new User(name);
                _users[name] = user;
                Logger.Log($"User {name} created");
                return Option.Some(user);
            }
            Logger.Log($"Failed to create user {name} - user already exists");
            return Option.None<User>();
        }

        public Option<Sheet> CreateSheet(string user, string name)
        {
            if (_users.ContainsKey(user) && !_sheets.ContainsKey(name))
            {
                var sheet = new Sheet(name, user, new Dictionary<(int, int), Cell>());
                _sheets[name] = sheet;
                Logger.Log($"Sheet {name} created by user {user}");
                return Option.Some(sheet);
            }
            Logger.Log($"Failed to create sheet {name} for user {user}");
            return Option.None<Sheet>();
        }

        public Option<Sheet> GetSheet(string name)
        {
            return _sheets.ContainsKey(name)
                ? Option.Some(_sheets[name])
                : Option.None<Sheet>();
        }

        public bool CheckUserAndSheetExist(string user, string sheetName)
        {
            return CheckUserExist(user) && _sheets.ContainsKey(sheetName);
        }

        public Option<User> GetUser(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Logger.Log($"Attempted to get user with empty name");
                return Option.None<User>();
            }

            if (_users.TryGetValue(name, out var user))
            {
                Logger.Log($"Retrieved user: {name}");
                return Option.Some(user);
            }

            Logger.Log($"Failed to get user: {name} - user does not exist");
            return Option.None<User>();
        }

        // Helper method to check if user exists
        public bool CheckUserExist(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && _users.ContainsKey(name);
        }

        // Functional calculation handler
        private static double EvaluateExpression(string expr)
        {
            try
            {
                var expression = expr.Replace(" ", "");
                if (!expression.Contains("+") && !expression.Contains("-") &&
                    !expression.Contains("*") && !expression.Contains("/"))
                {
                    return double.Parse(expression);
                }

                var numbers = new List<double>();
                var operators = new List<char>();
                var currentNumber = "";

                foreach (char c in expression)
                {
                    if (char.IsDigit(c) || c == '.' || (c == '-' && currentNumber == ""))
                    {
                        currentNumber += c;
                    }
                    else if ("+-*/".Contains(c))
                    {
                        if (!string.IsNullOrEmpty(currentNumber))
                        {
                            numbers.Add(double.Parse(currentNumber));
                            currentNumber = "";
                        }
                        operators.Add(c);
                    }
                }
                if (!string.IsNullOrEmpty(currentNumber))
                {
                    numbers.Add(double.Parse(currentNumber));
                }

                var result = numbers[0];
                for (int i = 0; i < operators.Count; i++)
                {
                    switch (operators[i])
                    {
                        case '+': result += numbers[i + 1]; break;
                        case '-': result -= numbers[i + 1]; break;
                        case '*': result *= numbers[i + 1]; break;
                        case '/': result /= numbers[i + 1]; break;
                    }
                }
                return result;
            }
            catch
            {
                return 0;
            }
        }

        public Option<Sheet> UpdateCell(string user, string sheetName, int row, int col, string value)
        {
            // Check if user exists
            if (!_users.ContainsKey(user))
            {
                Logger.Log($"Failed to update cell - user {user} not found");
                return Option.None<Sheet>();
            }

            // Check if sheet exists
            if (!_sheets.ContainsKey(sheetName))
            {
                Logger.Log($"Failed to update cell - sheet {sheetName} not found");
                return Option.None<Sheet>();
            }

            if (!_accessStrategy.CanEdit(user, sheetName))
            {
                Logger.Log($"Access denied - user {user} attempted to edit sheet {sheetName}");
                Console.WriteLine("You have ReadOnly access. Editing is not permitted. Please contact the administrator.");
                return Option.None<Sheet>();
            }

            var sheet = _sheets[sheetName];
            var evaluatedValue = EvaluateExpression(value);
            var newCells = new Dictionary<(int, int), Cell>(sheet.Cells)
            {
                [(row, col)] = new Cell(value, evaluatedValue)
            };

            var updatedSheet = sheet with { Cells = newCells };
            _sheets[sheetName] = updatedSheet;
            Logger.Log($"Cell updated in sheet {sheetName} at position ({row},{col}) with value {value} by user {user}");
            Notify(sheetName, (row, col), evaluatedValue.ToString());
            return Option.Some(updatedSheet);
        }


        public bool RevokeAccess(string sheetName, string userName)
        {
            if (_accessStrategy is RestrictedAccessStrategy strategy)
            {
                if (_users.TryGetValue(userName, out var user))
                {
                    strategy.SetAccess(userName, sheetName, true); // Set to ReadOnly
                    Detach(user, sheetName); // Remove from observers
                    Logger.Log($"Access revoked for user {userName} on sheet {sheetName}");
                    return true;
                }
            }
            return false;
        }

        public bool SetAccess(string sheetName, string user, bool isReadOnly)
        {
            // Validate if sheet exists
            if (!_sheets.ContainsKey(sheetName))
            {
                Logger.Log($"Failed to set access - sheet {sheetName} not found");
                return false;
            }

            // Validate if user exists 
            if (!CheckUserExist(user))
            {
                Logger.Log($"Failed to set access - user {user} not found");
                return false;
            }

            if (_accessStrategy is RestrictedAccessStrategy strategy)
            {
                strategy.SetAccess(user, sheetName, isReadOnly);
                Logger.Log($"Access rights updated for user {user} on sheet {sheetName} - ReadOnly: {isReadOnly}");
                return true;
            }

            Logger.Log($"Failed to set access - access control is not enabled");
            return false;
        }
    }

}
