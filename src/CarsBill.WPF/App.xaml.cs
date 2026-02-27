using AutoMapper;
using CarsBill.WPF.Common;
using CarsBill.WPF.Data;
using CarsBill.WPF.Models;
using CarsBill.WPF.Services;
using CarsBill.WPF.ViewModels;
using CarsBill.WPF.Views;
using Microsoft.EntityFrameworkCore;
using Prism.DryIoc;
using Prism.Ioc;
using System.IO;
using System.Windows;
using System.Windows.Threading;

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

    private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");

    public App()
    {
        // 全局异常处理
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogError("DispatcherUnhandledException", e.Exception);
        MessageBox.Show($"发生错误：\n{e.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            LogError("UnhandledException", ex);
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogError("UnobservedTaskException", e.Exception);
        e.SetObserved();
    }

    private static void LogError(string source, Exception ex)
    {
        try
        {
            var msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{source}]\n{ex}\n\n";
            File.AppendAllText(LogFile, msg);
        }
        catch { }
    }

    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        try
        {
            // 注册 EF Core DbContext
            var serverVersion = ServerVersion.AutoDetect(ConnectionString);
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseMySql(ConnectionString, serverVersion);

            containerRegistry.RegisterInstance(optionsBuilder.Options);
            containerRegistry.Register<AppDbContext>();
        }
        catch (Exception ex)
        {
            LogError("RegisterTypes-DbContext", ex);
            MessageBox.Show($"数据库连接失败：\n{ex.Message}\n\n请检查 MySQL 是否正在运行。", "数据库错误", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }

        // 注册 AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AutoMapperProfile>();
        });
        containerRegistry.RegisterInstance(mapperConfig.CreateMapper());

        // 注册通用CRUD服务
        containerRegistry.Register<IBaseService<CarInfo>, BaseService<CarInfo>>();
        containerRegistry.Register<IBaseService<AddressInfo>, BaseService<AddressInfo>>();
        containerRegistry.Register<IBaseService<MaterialInfo>, BaseService<MaterialInfo>>();
        containerRegistry.Register<IBaseService<ProjectInfo>, BaseService<ProjectInfo>>();
        containerRegistry.Register<IBaseService<BusinessRecord>, BaseService<BusinessRecord>>();
        containerRegistry.Register<IBaseService<PriceRule>, BaseService<PriceRule>>();

        // 注册 ViewModels
        containerRegistry.Register<CarInfoViewModel>();
        containerRegistry.Register<AddressInfoViewModel>();
        containerRegistry.Register<MaterialInfoViewModel>();
        containerRegistry.Register<ProjectInfoViewModel>();
        containerRegistry.Register<BusinessViewModel>();
        containerRegistry.Register<ReportViewModel>();
        containerRegistry.Register<PriceRuleViewModel>();
        containerRegistry.Register<BatchPriceViewModel>();

        // 注册导航页面
        containerRegistry.RegisterForNavigation<DashboardView>();
        containerRegistry.RegisterForNavigation<BusinessView>();
        containerRegistry.RegisterForNavigation<BaseInfoView>();
        containerRegistry.RegisterForNavigation<ReportView>();
        containerRegistry.RegisterForNavigation<PriceRuleView>();
        containerRegistry.RegisterForNavigation<BatchPriceView>();
        containerRegistry.RegisterForNavigation<SettingsView>();
    }

    protected override void OnInitialized()
    {
        try
        {
            // 确保数据库已创建
            using var scope = Container.Resolve<AppDbContext>();
            scope.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            LogError("OnInitialized-EnsureCreated", ex);
            MessageBox.Show($"数据库初始化失败：\n{ex.Message}", "数据库错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        base.OnInitialized();
    }
}
