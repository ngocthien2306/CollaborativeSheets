# Collaborative Spreadsheet System

A .NET-based collaborative spreadsheet system that allows multiple users to work on shared spreadsheets with real-time updates and access control.

## Features

- User Management
  - Create and manage users
  - Track user activities and permissions

- Spreadsheet Operations
  - Create new spreadsheets
  - View spreadsheet contents
  - Update cell values with support for basic calculations
  - Real-time updates for collaborators

- Access Control
  - Set read/write permissions for users
  - Toggle between default and restricted access modes
  - Support for read-only and editable access rights

- Collaboration
  - Share spreadsheets between users
  - Real-time notifications of changes
  - Observer pattern implementation for updates

## Architecture

The project follows clean architecture principles and implements several design patterns:

- **Observer Pattern**: For real-time updates and notifications
- **Strategy Pattern**: For access control implementation
- **Functional Programming**: Using Option type for better error handling
- **SOLID Principles**: Clean separation of concerns and interfaces

### Project Structure

```
CollaborativeSheets/
├── Application/
│   ├── Common/         # Common utilities like Logger
│   ├── Services/       # Main application services
│   └── Strategies/     # Access control strategies
├── Domain/
│   ├── Entities/       # Core business entities
│   ├── Interfaces/     # Core interfaces
│   └── ValueObjects/   # Value objects like Option
└── Infrastructure/
    └── Persistence/    # Data storage implementation
```

## Requirements

- .NET 8.0
- Visual Studio 2022

## Getting Started

### Prerequisites
- Visual Studio 2022 (Community, Professional, or Enterprise)
- .NET 8.0 SDK
- Git (optional, for cloning)

### Installation Methods

#### Method 1: Using Visual Studio 2022
1. Clone or download the repository
2. Open Visual Studio 2022
3. Click on "Open a project or solution"
4. Navigate to the project folder and select `CollaborativeSheets.sln`
5. Once opened, ensure the solution builds without errors
   - Right-click on the solution in Solution Explorer
   - Select "Build Solution" (or press Ctrl + Shift + B)
6. Run the program:
   - Press F5 to run with debugging
   - Or press Ctrl + F5 to run without debugging

#### Method 2: Using Command Line
1. Open Command Prompt or PowerShell
2. Navigate to the project directory
```bash
cd path/to/CollaborativeSheets
```
3. Build the project
```bash
dotnet build
```
4. Run the application
```bash
dotnet run
```

### Troubleshooting
- If you encounter build errors:
  - Right-click on the solution and select "Restore NuGet Packages"
  - Clean the solution (Build > Clean Solution)
  - Rebuild the solution
- If the program won't start:
  - Check if .NET 8.0 SDK is properly installed
  - Verify the startup project is set to CollaborativeSheets
  - Check the Output window for error messages

## Usage

The application provides a console-based interface with the following options:

1. Create a user
2. Create a sheet
3. Check a sheet
4. Change a value in a sheet
5. Change a sheet's access right
6. Collaborate with another user
7. Exit

### Example Usage:

```
1. Create users:
   > Enter user name: Alice

2. Create a sheet:
   > Enter username and sheet name: Alice Sheet1

3. Update values:
   > Enter position and value: 0 0 42

4. Share with others:
   > Enter sheet owner: Alice
   > Enter sheet name: Sheet1
   > Enter user to share with: Bob
```

## Features in Detail

### Spreadsheet Operations
- 3x3 grid system
- Support for basic mathematical expressions
- Real-time calculation of cell values

### Access Control
- Two modes: Default (open) and Restricted
- ReadOnly and Editable permission levels
- Owner-based access management

### Collaboration
- Multiple users can work on the same sheet
- Real-time updates for all collaborators
- Change notifications

## Error Handling

The system uses the Option pattern for error handling, providing:
- Type-safe error handling
- No null reference exceptions
- Clear success/failure paths

## Logging

All important operations are logged to `collaborative_system.log`, including:
- User creation
- Sheet modifications
- Access control changes
- Error events

## Design Patterns

1. **Observer Pattern**
   - Used for real-time updates
   - Implemented through IObserver and ISubject interfaces

2. **Strategy Pattern**
   - Used for access control
   - Easily switchable between different access strategies

3. **Immutable Records**
   - Used for core entities
   - Ensures thread safety and data consistency

## Contributing

Feel free to submit issues and enhancement requests.

## License

This project is licensed under the MIT License.