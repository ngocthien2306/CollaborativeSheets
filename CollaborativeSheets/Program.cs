using CollaborativeSheets.Application.Common;
using CollaborativeSheets.Application.Services;
using CollaborativeSheets.Domain.Entities;

namespace CollaborativeSpreadsheet
{
    public class Program
    {
        private static void DisplayMenu()
        {
            Console.WriteLine("\n---------------Menu---------------");
            Console.WriteLine("1. Create a user");
            Console.WriteLine("2. Create a sheet");
            Console.WriteLine("3. Check a sheet");
            Console.WriteLine("4. Change a value in a sheet");
            Console.WriteLine("5. Change a sheet's access right");
            Console.WriteLine("6. Collaborate with another user");
            Console.WriteLine("7. Exit");
            Console.WriteLine("----------------------------------");
            Console.Write("Please enter your choice (1-7): ");
        }

        private static void PrintSheet(CollaborativeSystem system, string sheetName)
        {
            system.GetSheet(sheetName).Match<bool>(
                sheet =>
                {
                    Console.WriteLine($"\nSheet content for \"{sheetName}\":");
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            var cell = sheet.Cells.GetValueOrDefault((i, j), new Cell("0", 0));
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

        private static void HandleCreateUser(CollaborativeSystem system)
        {
            Console.Write("Enter user name: ");
            var userName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userName))
            {
                Console.WriteLine("User name cannot be empty.");
                return;
            }

            var newUser = system.CreateUser(userName);
            newUser.Match<bool>(
                user =>
                {
                    // system.Attach(user);
                    Console.WriteLine($"Created and attached user {userName}");
                    return true;
                },
                () =>
                {
                    Console.WriteLine("Failed to create user - User may already exist");
                    return false;
                });
        }

        private static void HandleCreateSheet(CollaborativeSystem system)
        {
            Console.Write("Enter username and sheet name (e.g., Kevin SheetA) > ");
            var input = Console.ReadLine()?.Split(' ');

            if (input?.Length != 2)
            {
                Console.WriteLine("Invalid input format. Please enter both username and sheet name.");
                return;
            }

            var owner = input[0];
            var sheetName = input[1];

            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(sheetName))
            {
                Console.WriteLine("Username and sheet name cannot be empty.");
                return;
            }

            system.CreateSheet(owner, sheetName).Match<bool>(
                sheet =>
                {
                    Console.WriteLine($"Sheet named \"{sheetName}\" created successfully for user \"{owner}\".");
                    return true;
                },
                () =>
                {
                    Console.WriteLine($"Failed to create sheet. Please check if user \"{owner}\" exists.");
                    return false;
                });
        }

        private static void HandleCheckSheet(CollaborativeSystem system)
        {
            Console.Write("Enter username and sheet name (e.g., Kevin SheetA) > ");
            var checkInput = Console.ReadLine()?.Split(' ');

            if (checkInput?.Length != 2)
            {
                Console.WriteLine("Invalid input format. Please enter both username and sheet name.");
                return;
            }

            var checkOwner = checkInput[0];
            var checkSheetName = checkInput[1];

            if (string.IsNullOrWhiteSpace(checkOwner) || string.IsNullOrWhiteSpace(checkSheetName))
            {
                Console.WriteLine("Username and sheet name cannot be empty.");
                return;
            }

            if (!system.CheckUserAndSheetExist(checkOwner, checkSheetName))
            {
                Console.WriteLine($"User \"{checkOwner}\" does not exist.");
                return;
            }

            system.GetSheet(checkSheetName).Match<bool>(
                sheet =>
                {
                    if (sheet.Owner != checkOwner)
                    {
                        Console.WriteLine($"Sheet \"{checkSheetName}\" does not belong to user \"{checkOwner}\".");
                        return false;
                    }

                    PrintSheet(system, checkSheetName);
                    return true;
                },
                () =>
                {
                    Console.WriteLine($"Sheet \"{checkSheetName}\" not found.");
                    return false;
                });
        }

        private static void HandleUpdateCell(CollaborativeSystem system)
        {
            Console.Write("Enter username and sheet name (e.g., Kevin SheetA) > ");
            var updateInput = Console.ReadLine()?.Split(' ');

            if (updateInput?.Length != 2)
            {
                Console.WriteLine("Invalid input. Please enter user and sheet name.");
                return;
            }

            var updateUser = updateInput[0];
            var updateSheet = updateInput[1];

            if (!system.CheckUserAndSheetExist(updateUser, updateSheet))
            {
                Console.WriteLine($"User {updateUser} or Sheet {updateSheet} does not exist. Please create the user and sheet first.");
                return;
            }


            PrintSheet(system, updateSheet);

            Console.Write("Enter position and value to update (row column value, e.g., 1 2 3) > ");
            var valueInput = Console.ReadLine()?.Split(' ');

            if (valueInput?.Length != 3)
            {
                Console.WriteLine("Invalid input. Please enter row column value.");
                return;
            }

            if (!int.TryParse(valueInput[0], out int row) ||
                !int.TryParse(valueInput[1], out int col))
            {
                Console.WriteLine("Invalid row or column. Please enter numeric values.");
                return;
            }

            if (row < 0 || row > 2 || col < 0 || col > 2)
            {
                Console.WriteLine("Row and column must be between 0 and 2.");
                return;
            }

            string updateResult = system.UpdateCell(updateUser, updateSheet, row, col, valueInput[2])
                .Match<string>(
                    sheet => "Updated successfully",
                    () => "Update failed - Check if user exists and has permission to edit this sheet"
                );

            Console.WriteLine(updateResult);
            PrintSheet(system, updateSheet);
        }

        private static void HandleAccessRights(CollaborativeSystem system)
        {
            Console.Write("Enter sheet name: ");
            var accessSheet = Console.ReadLine();
            Console.Write("Enter user name: ");
            var accessUser = Console.ReadLine();
            Console.Write("Enter access type (ReadOnly/Editable): ");
            var accessType = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(accessSheet) ||
                string.IsNullOrWhiteSpace(accessUser))
            {
                Console.WriteLine("Sheet name and user name cannot be empty");
                return;
            }

            if (accessType != "ReadOnly" && accessType != "Editable")
            {
                Console.WriteLine("Invalid access type. Please enter either 'ReadOnly' or 'Editable'");
                return;
            }

            system.EnableAccessControl();
            if (accessType == "ReadOnly")
            {
                // Execute RevokeAccess when right is ReadOnly
                bool success = system.RevokeAccess(accessSheet, accessUser);
                if (success)
                {
                    Console.WriteLine($"Successfully revoked edit access for user {accessUser} on sheet {accessSheet}. Sheet is now ReadOnly.");
                }
                else
                {
                    Console.WriteLine("Failed to revoke access. Please check if the sheet exists and user has access.");
                }
            }
            else // accessType == "Editable"
            {
                bool success = system.SetAccess(accessSheet, accessUser, false);
                if (success)
                {
                    Console.WriteLine($"Edit access granted for user {accessUser} on sheet {accessSheet}");
                }
                else
                {
                    Console.WriteLine("Failed to grant edit access. Please check if the sheet and user exist.");
                }
            }
        }

        private static void HandleCollaboration(CollaborativeSystem system)
        {
            Console.Write("Enter sheet owner: ");
            var shareOwner = Console.ReadLine();
            Console.Write("Enter sheet name: ");
            var shareSheet = Console.ReadLine();
            Console.Write("Enter user to share with: ");
            var shareWith = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(shareOwner) ||
                string.IsNullOrWhiteSpace(shareSheet) ||
                string.IsNullOrWhiteSpace(shareWith))
            {
                Console.WriteLine("All fields must be filled.");
                return;
            }

            // Check if owner exists
            var ownerResult = system.GetUser(shareOwner);
            if (ownerResult.Match(
                owner => false, 
                () =>
                {
                    Console.WriteLine($"Sheet owner {shareOwner} does not exist.");
                    return true; 
                }))
            {
                return;
            }

            // Check if sheet exists
            var sheetResult = system.GetSheet(shareSheet);
            if (sheetResult.Match(
                sheet =>
                {
                    // Verify ownership
                    if (sheet.Owner.ToLower() != shareOwner.ToLower())
                    {
                        Console.WriteLine($"Sheet {shareSheet} does not belong to {shareOwner}");
                        return true; 
                    }
                    return false;
                },
                () =>
                {
                    Console.WriteLine($"Sheet {shareSheet} does not exist.");
                    return true; 
                }))
            {
                return;
            }

            system.EnableAccessControl();

            // Get or create collaborator user
            system.CreateUser(shareWith);

            // First make sure owner is attached as observer if not already
            system.GetUser(shareOwner).Match(
                owner => {
                    system.Attach(owner, shareSheet);
                    return true;
                },
                () => false
            );

            // Add attach the collaborator
            system.GetUser(shareWith).Match(
                user => {
                    system.SetAccess(shareSheet, shareWith, false);
                    system.Attach(user, shareSheet);
                    Console.WriteLine($"{shareOwner} shared sheet {shareSheet} with {shareWith}");
                    return true;
                },
                () => {
                    Console.WriteLine($"Failed to share sheet with {shareWith}");
                    return false;
                });
        }

        static void Main()
        {
            var system = new CollaborativeSystem();
            // system.EnableAccessControl();
            var running = true;

            Console.WriteLine("Welcome to Collaborative Spreadsheet System!");

            while (running)
            {
                try
                {
                    DisplayMenu();
                    var choice = Console.ReadLine();

                    Console.WriteLine(); // Add a blank line for better readability

                    switch (choice)
                    {
                        case "1":
                            HandleCreateUser(system);
                            break;

                        case "2":
                            HandleCreateSheet(system);
                            break;

                        case "3":
                            HandleCheckSheet(system);
                            break;

                        case "4":
                            HandleUpdateCell(system);
                            break;

                        case "5":
                            HandleAccessRights(system);
                            break;

                        case "6":
                            HandleCollaboration(system);
                            break;

                        case "7":
                            Logger.Log("System shutdown");
                            running = false;
                            Console.WriteLine("Thank you for using Collaborative Spreadsheet System!");
                            break;

                        default:
                            Console.WriteLine("Invalid choice. Please enter a number between 1 and 7.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Logger.Log($"Error: {ex.Message}");
                }
            }
        }
    }
}