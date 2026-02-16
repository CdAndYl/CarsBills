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
    private bool _isInitialized = false;

    // 页面标题映射
    private readonly Dictionary<string, string> _pageTitles = new()
    {
        { "DashboardView", "仪表盘" },
        { "BusinessView", "业务录入" },
        { "BaseInfoView", "基础信息" },
        { "ReportView", "报表统计" },
        { "SettingsView", "系统设置" }
    };

    public MainWindow(IRegionManager regionManager)
    {
        InitializeComponent();
        _regionManager = regionManager;

        // 默认导航到仪表盘（等待窗口完全加载后再执行）
        Loaded += (s, e) =>
        {
            _isInitialized = true;
            Navigate("DashboardView");
        };
    }

    /// <summary>
    /// 菜单选择变更事件
    /// </summary>
    private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 忽略初始化阶段的选择事件（Region尚未就绪）
        if (!_isInitialized) return;

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
