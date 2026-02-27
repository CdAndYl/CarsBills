using MaterialDesignThemes.Wpf;
using Prism.Navigation.Regions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CarsBill.WPF;

/// <summary>
/// MainWindow 代码后台 - 导航控制
/// </summary>
public partial class MainWindow : Window
{
    private readonly IRegionManager _regionManager;
    private readonly PaletteHelper _paletteHelper = new();
    private bool _isInitialized = false;
    private bool _isDarkTheme = false;

    // 页面标题映射
    private readonly Dictionary<string, string> _pageTitles = new()
    {
        { "DashboardView", "仪表盘" },
        { "BusinessView", "业务录入" },
        { "BaseInfoView", "基础信息" },
        { "ReportView", "报表统计" },
        { "PriceRuleView", "单价规则" },
        { "BatchPriceView", "批量调价" },
        { "SettingsView", "系统设置" }
    };

    public MainWindow(IRegionManager regionManager)
    {
        InitializeComponent();
        _regionManager = regionManager;
        ApplyTheme(_isDarkTheme);

        StateChanged += (_, _) => UpdateMaxButtonGlyph();

        // 默认导航到仪表盘（等待窗口完全加载后再执行）
        Loaded += (s, e) =>
        {
            _isInitialized = true;
            Navigate("DashboardView");
            UpdateMaxButtonGlyph();
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

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (IsClickOnButton(e.OriginalSource as DependencyObject))
            return;

        if (e.ClickCount == 2)
        {
            ToggleWindowState();
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void MinButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaxButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleWindowState();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _isDarkTheme = !_isDarkTheme;
        ApplyTheme(_isDarkTheme);
    }

    private void ToggleWindowState()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void UpdateMaxButtonGlyph()
    {
        MaxButton.Content = WindowState == WindowState.Maximized ? "❐" : "□";
    }

    private void ApplyTheme(bool dark)
    {
        var theme = _paletteHelper.GetTheme();
        theme.SetBaseTheme(dark ? BaseTheme.Dark : BaseTheme.Light);
        _paletteHelper.SetTheme(theme);

        if (dark)
        {
            SetBrushColor("AppBackgroundBrush", "#0B1220");
            SetBrushColor("CardSurfaceBrush", "#111A2B");
            SetBrushColor("CardBorderBrush", "#243248");
            SetBrushColor("SubtleBorderBrush", "#2A3A53");
            SetBrushColor("SubtleTextBrush", "#94A3B8");
            SetBrushColor("TitleTextBrush", "#E2E8F0");
            SetBrushColor("WindowTopBarBrush", "#0A1324");
            SetBrushColor("WindowTopBarBorderBrush", "#1F2B40");
            SetBrushColor("WindowBrandTextBrush", "#E2E8F0");
            SetBrushColor("WindowTitlePrimaryBrush", "#E2E8F0");
            SetBrushColor("WindowTitleSecondaryBrush", "#94A3B8");
            SetBrushColor("WindowLeftPanelBrush", "#0E172A");
            SetBrushColor("WindowLeftPanelBorderBrush", "#1F2B40");
            SetBrushColor("WindowMenuHeaderBrush", "#7EA0C4");
            SetBrushColor("WindowMenuTextBrush", "#AFC2D8");
            SetBrushColor("WindowMenuHoverBrush", "#1D2B40");
            SetBrushColor("WindowMenuSelectedBrush", "#24364F");
            SetBrushColor("WindowAccentBrush", "#38BDF8");
            SetBrushColor("WindowVersionTextBrush", "#8FABCA");
            SetBrushColor("WindowVersionSubTextBrush", "#6886A7");
            SetBrushColor("WindowSeparatorBrush", "#20324A");
            SetBrushColor("TitleButtonForegroundBrush", "#CBD5E1");
            SetBrushColor("TitleButtonHoverBrush", "#1E2A3D");
            SetBrushColor("TitleButtonPressedBrush", "#293A55");
            ThemeToggleButton.Content = "浅色";
        }
        else
        {
            SetBrushColor("AppBackgroundBrush", "#EEF3FA");
            SetBrushColor("CardSurfaceBrush", "#FFFFFF");
            SetBrushColor("CardBorderBrush", "#DCE6F2");
            SetBrushColor("SubtleBorderBrush", "#E8EEF7");
            SetBrushColor("SubtleTextBrush", "#5B6B82");
            SetBrushColor("TitleTextBrush", "#1B2A40");
            SetBrushColor("WindowTopBarBrush", "#1E3A8A");
            SetBrushColor("WindowTopBarBorderBrush", "#27438F");
            SetBrushColor("WindowBrandTextBrush", "#EFF6FF");
            SetBrushColor("WindowTitlePrimaryBrush", "#FFFFFF");
            SetBrushColor("WindowTitleSecondaryBrush", "#DBEAFE");
            SetBrushColor("WindowLeftPanelBrush", "#F4F7FD");
            SetBrushColor("WindowLeftPanelBorderBrush", "#D7E1EE");
            SetBrushColor("WindowMenuHeaderBrush", "#6B7D96");
            SetBrushColor("WindowMenuTextBrush", "#31445F");
            SetBrushColor("WindowMenuHoverBrush", "#E5EEFA");
            SetBrushColor("WindowMenuSelectedBrush", "#D6E6FF");
            SetBrushColor("WindowAccentBrush", "#2563EB");
            SetBrushColor("WindowVersionTextBrush", "#55708F");
            SetBrushColor("WindowVersionSubTextBrush", "#7390B0");
            SetBrushColor("WindowSeparatorBrush", "#D6E2F1");
            SetBrushColor("TitleButtonForegroundBrush", "#E2E8F0");
            SetBrushColor("TitleButtonHoverBrush", "#3859A8");
            SetBrushColor("TitleButtonPressedBrush", "#2C4B95");
            ThemeToggleButton.Content = "深色";
        }
    }

    private static void SetBrushColor(string key, string hexColor)
    {
        if (Application.Current is null)
            return;

        if (ColorConverter.ConvertFromString(hexColor) is not Color color)
            return;

        if (Application.Current.Resources[key] is SolidColorBrush brush && !brush.IsFrozen)
        {
            brush.Color = color;
            return;
        }

        Application.Current.Resources[key] = new SolidColorBrush(color);
    }

    private static bool IsClickOnButton(DependencyObject? source)
    {
        while (source != null)
        {
            if (source is ButtonBase)
                return true;

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }
}
