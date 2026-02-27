# CarsBill - 车队运费计费管理系统

[English](README.md) | [简体中文](README.zh-CN.md)

一个用于车队运输计费管理的 WPF 桌面应用。

## 技术栈

- **语言**: C# / .NET 8
- **UI 框架**: WPF + Material Design
- **架构**: Prism + DryIoc (MVVM)
- **数据库**: MySQL 8.0（通过 Pomelo.EntityFrameworkCore.MySql）
- **ORM**: Entity Framework Core 8
- **图表**: LiveCharts2
- **Excel 导出**: EPPlus
- **对象映射**: AutoMapper

## 项目结构

```text
NewCarBills/
|-- docs/                         # 文档目录
|-- src/CarsBill.WPF/            # WPF 应用程序
|   |-- Common/                   # 公共工具/转换器
|   |-- Data/                     # EF Core DbContext
|   |-- Extensions/               # DI/扩展注册
|   |-- Models/                   # 实体模型
|   |-- Resources/                # 主题/资源
|   |-- Services/                 # 业务服务
|   |-- ViewModels/               # 视图模型（MVVM）
|   |-- Views/                    # 视图（XAML）
|   |-- App.xaml
|   |-- App.xaml.cs
|   |-- MainWindow.xaml
|   `-- MainWindow.xaml.cs
|-- .gitignore
|-- CarsBill.sln
|-- README.md
`-- README.zh-CN.md
```

## 快速开始

1. 安装 MySQL 8.0，并创建数据库 `carsbill`
2. 在 `App.xaml.cs` 中更新连接字符串
3. 构建并运行：
   ```bash
   cd src/CarsBill.WPF
   dotnet build
   dotnet run
   ```
