using Prism.Navigation.Regions;
using System.Windows;
using System.Windows.Controls;

namespace CarsBill.WPF;

/// <summary>
/// MainWindow 代码后台 - 导航控制
/// </summary>
public partial class MainWindow : Window
{
    private readonly IRegionManager _regionManager;

    // 页面标题映射
    private readonly Dictionary<string, string> _pageTitles = new()
    {
        { "DashboardView", "Dashboard" },
        { "BusinessView", "Business" },
        { "BaseInfoView", "Base Info" },
        { "ReportView", "Reports" },
        { "SettingsView", "Settings" }
    };

    public MainWindow(IRegionManager regionManager)
    {
        InitializeComponent();
        _regionManager = regionManager;

        // 默认导航到仪表盘
        Loaded += (s, e) => Navigate("DashboardView");
    }

    /// <summary>
    /// 菜单选择变更事件
    /// </summary>
    private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MenuList.SelectedItem is ListBoxItem item && item.Tag is string viewName)
        {
            Navigate(viewName);
        }
    }

    /// <summary>
    /// 导航到指定页面
    /// </summary>
    private void Navigate(string viewName)
    {
        _regionManager.RequestNavigate("ContentRegion", viewName);

        // 更新页面标题
        if (_pageTitles.TryGetValue(viewName, out var title))
        {
            PageTitle.Text = title;
        }
    }
}
