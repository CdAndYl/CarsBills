# CarsBill - Fleet Billing Management System

[![Language: English](https://img.shields.io/badge/Language-English-1f6feb)](README.md) [![语言: 简体中文](https://img.shields.io/badge/语言-简体中文-d23f31)](README.zh-CN.md)

A WPF desktop application for fleet transportation billing management.

## 中文说明

一个用于车队运输计费管理的 WPF 桌面应用。完整中文文档请查看 [README.zh-CN.md](README.zh-CN.md)。

### 快速开始（中文）

1. 安装 MySQL 8.0，并创建数据库 `carsbill`
2. 在 `App.xaml.cs` 中更新连接字符串
3. 构建并运行：
   ```bash
   cd src/CarsBill.WPF
   dotnet build
   dotnet run
   ```

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
