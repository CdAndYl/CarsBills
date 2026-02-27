# CarsBill - Fleet Billing Management System

[English](README.md) | [简体中文](README.zh-CN.md)

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
|-- docs/                         # Documentation
|-- src/CarsBill.WPF/            # WPF Application
|   |-- Common/                   # Common utilities/converters
|   |-- Data/                     # EF Core DbContext
|   |-- Extensions/               # DI/extension registration
|   |-- Models/                   # Entity models
|   |-- Resources/                # Themes/resources
|   |-- Services/                 # Business services
|   |-- ViewModels/               # View models (MVVM)
|   |-- Views/                    # Views (XAML)
|   |-- App.xaml
|   |-- App.xaml.cs
|   |-- MainWindow.xaml
|   `-- MainWindow.xaml.cs
|-- .gitignore
|-- CarsBill.sln
|-- README.md
`-- README.zh-CN.md
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
