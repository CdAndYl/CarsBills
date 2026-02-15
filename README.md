# CarsBill - Fleet Billing Management System

A WPF desktop application for fleet transportation billing management.

## Tech Stack

- **Language**: C# / .NET 8
- **UI Framework**: WPF + Material Design
- **Architecture**: Prism + DryIoc (MVVM)
- **Database**: MySQL 8.0 (via Pomelo.EntityFrameworkCore.MySql)
- **ORM**: Entity Framework Core 8
- **Charts**: LiveCharts2
- **Excel Export**: EPPlus
- **Object Mapping**: AutoMapper

## Project Structure

```
NewCarBills/
©À©¤©¤ docs/                        # Documentation
©À©¤©¤ src/CarsBill.WPF/           # WPF Application
©¦   ©À©¤©¤ Models/                  # Entity Models
©¦   ©À©¤©¤ Views/                   # Views (XAML)
©¦   ©À©¤©¤ ViewModels/              # View Models
©¦   ©À©¤©¤ Data/                    # EF Core DbContext
©¦   ©À©¤©¤ Services/                # Business Services
©¦   ©À©¤©¤ Common/                  # Common Utilities
©¦   ©¸©¤©¤ Resources/               # Resources
©À©¤©¤ .gitignore
©À©¤©¤ CarsBill.sln
©¸©¤©¤ README.md
```

## Getting Started

1. Install MySQL 8.0 and create database `carsbill`
2. Update connection string in `App.xaml.cs`
3. Build and run:
   ```bash
   cd src/CarsBill.WPF
   dotnet build
   dotnet run
   ```
