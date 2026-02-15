using AutoMapper;
using CarsBill.WPF.Common;
using CarsBill.WPF.Data;
using CarsBill.WPF.Views;
using Microsoft.EntityFrameworkCore;
using Prism.DryIoc;
using Prism.Ioc;
using System.Windows;

namespace CarsBill.WPF;

/// <summary>
/// Prism 应用程序入口
/// </summary>
public partial class App : PrismApplication
{
    /// <summary>
    /// MySQL 连接字符串
    /// </summary>
    private const string ConnectionString = "Server=localhost;Port=3306;Database=carsbill;User=root;Password=root;charset=utf8mb4;";

    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 注册 EF Core DbContext
        var serverVersion = ServerVersion.AutoDetect(ConnectionString);
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySql(ConnectionString, serverVersion);

        containerRegistry.RegisterInstance(optionsBuilder.Options);
        containerRegistry.Register<AppDbContext>();

        // 注册 AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AutoMapperProfile>();
        });
        containerRegistry.RegisterInstance(mapperConfig.CreateMapper());

        // 注册导航页面
        containerRegistry.RegisterForNavigation<DashboardView>();
        containerRegistry.RegisterForNavigation<BusinessView>();
        containerRegistry.RegisterForNavigation<BaseInfoView>();
        containerRegistry.RegisterForNavigation<ReportView>();
        containerRegistry.RegisterForNavigation<SettingsView>();
    }

    protected override void OnInitialized()
    {
        // 确保数据库已创建
        using var scope = Container.Resolve<AppDbContext>();
        scope.Database.EnsureCreated();

        base.OnInitialized();
    }
}
