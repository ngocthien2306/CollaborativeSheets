using System;
using System.Collections.Generic;
using System.Linq;

// Immutable data structures for functional approach
public record Cell(string Value, Func<string, double> Evaluate);
public record Sheet(string Name, string Owner, Dictionary<(int, int), Cell> Cells);
public record User(string Name);
public record AccessRight(bool IsReadOnly);

// Observer Pattern for collaboration
public interface IObserver
{
    void Update(string sheetName, (int, int) position, string value);
}

public interface ISubject
{
    void Attach(IObserver observer);
    void Detach(IObserver observer);
    void Notify(string sheetName, (int, int) position, string value);
}

// Strategy Pattern for access control
public interface IAccessStrategy
{
    bool CanEdit(string user, string sheetName);
}

public class DefaultAccessStrategy : IAccessStrategy
{
    public bool CanEdit(string user, string sheetName) => true;
}

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

// Main collaborative system using functional approach
public class CollaborativeSystem : ISubject
{
    private readonly List<IObserver> _observers = new();
    private readonly Dictionary<string, User> _users = new();
    private readonly Dictionary<string, Sheet> _sheets = new();
    private IAccessStrategy _accessStrategy = new DefaultAccessStrategy();

    // Observer Pattern implementation
    public void Attach(IObserver observer) => _observers.Add(observer);
    public void Detach(IObserver observer) => _observers.Remove(observer);
    public void Notify(string sheetName, (int, int) position, string value)
    {
        foreach (var observer in _observers)
        {
            observer.Update(sheetName, position, value);
        }
    }

    // Functional operations using LINQ and immutable data
    public Option<User> CreateUser(string name)
    {
        if (!_users.ContainsKey(name))
        {
            var user = new User(name);
            _users[name] = user;
            return Option.Some(user);
        }
        return Option.None<User>();
    }

    public Option<Sheet> CreateSheet(string user, string name)
    {
        if (_users.ContainsKey(user) && !_sheets.ContainsKey(name))
        {
            var sheet = new Sheet(name, user, new Dictionary<(int, int), Cell>());
            _sheets[name] = sheet;
            return Option.Some(sheet);
        }
        return Option.None<Sheet>();
    }

    public Option<Sheet> GetSheet(string name)
    {
        return _sheets.ContainsKey(name)
            ? Option.Some(_sheets[name])
            : Option.None<Sheet>();
    }

    // Functional calculation handler
    private static double EvaluateExpression(string expr)
    {
        var tokens = expr.Split(new[] { '+', '-', '*', '/' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 1) return double.Parse(expr);

        var numbers = tokens.Select(double.Parse).ToList();
        var operators = expr.Where(c => !char.IsDigit(c) && c != '.' && c != ' ').ToList();

        return operators.Zip(numbers.Skip(1), (op, num) => (op, num))
            .Aggregate(numbers[0], (acc, tuple) => tuple.op switch
            {
                '+' => acc + tuple.num,
                '-' => acc - tuple.num,
                '*' => acc * tuple.num,
                '/' => acc / tuple.num,
                _ => throw new ArgumentException("Invalid operator")
            });
    }

    public Option<Sheet> UpdateCell(string user, string sheetName, int row, int col, string value)
    {
        if (!_sheets.ContainsKey(sheetName) || !_accessStrategy.CanEdit(user, sheetName))
            return Option.None<Sheet>();

        var sheet = _sheets[sheetName];
        var newCells = new Dictionary<(int, int), Cell>(sheet.Cells)
        {
            [(row, col)] = new Cell(value, _ => EvaluateExpression(value))
        };

        var updatedSheet = sheet with { Cells = newCells };
        _sheets[sheetName] = updatedSheet;
        Notify(sheetName, (row, col), value);
        return Option.Some(updatedSheet);
    }

    // Strategy Pattern usage
    public void EnableAccessControl()
    {
        _accessStrategy = new RestrictedAccessStrategy();
    }

    public void DisableAccessControl()
    {
        _accessStrategy = new DefaultAccessStrategy();
    }

    public void SetAccess(string sheetName, string user, bool isReadOnly)
    {
        if (_accessStrategy is RestrictedAccessStrategy strategy)
        {
            strategy.SetAccess(user, sheetName, isReadOnly);
        }
    }
}

// Option type for functional error handling
public class Option<T>
{
    private readonly T _value;
    private readonly bool _hasValue;

    private Option(T value, bool hasValue)
    {
        _value = value;
        _hasValue = hasValue;
    }

    public static Option<T> Some(T value) => new(value, true);
    public static Option<T> None() => new(default, false);

    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none) =>
        _hasValue ? some(_value) : none();
}

public static class Option
{
    public static Option<T> Some<T>(T value) => Option<T>.Some(value);
    public static Option<T> None<T>() => Option<T>.None();
}

// Main program
class Program
{
    private static void PrintSheet(CollaborativeSystem system, string sheetName)
    {
        system.GetSheet(sheetName).Match<bool>(
            sheet =>
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var cell = sheet.Cells.GetValueOrDefault((i, j), new Cell("0", _ => 0));
                        Console.Write($"{cell.Value}, ");
                    }
                    Console.WriteLine();
                }
                return true;
            },
            () =>
            {
                Console.WriteLine("Sheet not found");
                return false;
            });
    }

    static void Main()
    {
        var system = new CollaborativeSystem();
        var running = true;

        while (running)
        {
            Console.WriteLine("---------------Menu---------------");
            Console.WriteLine("1. Create a user");
            Console.WriteLine("2. Create a sheet");
            Console.WriteLine("3. Check a sheet");
            Console.WriteLine("4. Change a value in a sheet");
            Console.WriteLine("5. Change a sheet's access right");
            Console.WriteLine("6. Collaborate with another user");
            Console.WriteLine("7. Exit");
            Console.WriteLine("----------------------------------");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Enter user name: ");
                    var userName = Console.ReadLine();
                    Console.Write("> ");
                    system.CreateUser(userName);
                    Console.WriteLine($"Create a user named \"{userName}\".");
                    break;

                case "2":
                    Console.Write("> ");
                    var input = Console.ReadLine().Split(' ');
                    var owner = input[0];
                    var sheetName = input[1];
                    system.CreateSheet(owner, sheetName);
                    Console.WriteLine($"Create a sheet named \"{sheetName}\" for \"{owner}\".");
                    break;

                case "3":
                    Console.Write("> ");
                    var checkInput = Console.ReadLine().Split(' ');
                    var checkOwner = checkInput[0];
                    var checkSheetName = checkInput[1];
                    system.GetSheet(checkSheetName).Match<bool>(
                        sheet =>
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                for (int j = 0; j < 3; j++)
                                {
                                    var cell = sheet.Cells.GetValueOrDefault((i, j), new Cell("0", _ => 0));
                                    Console.Write($"{cell.Value}, ");
                                }
                                Console.WriteLine();
                            }
                            return true;
                        },
                        () =>
                        {
                            Console.WriteLine("Sheet not found");
                            return false;
                        });
                    break;

                case "4":
                    Console.Write("> ");
                    var updateInput = Console.ReadLine().Split(' ');
                    var updateUser = updateInput[0];
                    var updateSheet = updateInput[1];
                    PrintSheet(system, updateSheet);
                    Console.Write("> ");
                    var valueInput = Console.ReadLine().Split(' ');
                    if (valueInput.Length == 3)
                    {
                        string updateResult = system.UpdateCell(updateUser, updateSheet,
                            int.Parse(valueInput[0]), int.Parse(valueInput[1]), valueInput[2])
                            .Match<string>(
                                sheet => "Updated successfully",
                                () => "Update failed");
                        Console.WriteLine(updateResult);
                    }
                    break;

                case "5":
                    Console.Write("Enter sheet name: ");
                    var accessSheet = Console.ReadLine();
                    Console.Write("Enter user name: ");
                    var accessUser = Console.ReadLine();
                    Console.Write("Enter access type (ReadOnly/Editable): ");
                    var accessType = Console.ReadLine();
                    system.EnableAccessControl();
                    system.SetAccess(accessSheet, accessUser, accessType == "ReadOnly");
                    Console.WriteLine("Access rights updated");
                    break;

                case "6":
                    Console.Write("Enter sheet owner: ");
                    var shareOwner = Console.ReadLine();
                    Console.Write("Enter sheet name: ");
                    var shareSheet = Console.ReadLine();
                    Console.Write("Enter user to share with: ");
                    var shareWith = Console.ReadLine();
                    system.CreateUser(shareWith);
                    system.SetAccess(shareSheet, shareWith, false);
                    Console.WriteLine($"Shared sheet {shareSheet} with {shareWith}");
                    break;

                case "7":
                    running = false;
                    break;
            }
        }
    }
}