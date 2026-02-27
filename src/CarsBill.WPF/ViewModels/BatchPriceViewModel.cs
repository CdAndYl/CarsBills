using CarsBill.WPF.Models;
using CarsBill.WPF.Services;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;

namespace CarsBill.WPF.ViewModels;

/// <summary>
/// 批量调价
/// </summary>
public class BatchPriceViewModel : BindableBase
{
    private readonly IBaseService<BusinessRecord> _businessService;
    private readonly IBaseService<PriceRule> _ruleService;
    private readonly IBaseService<AddressInfo> _addressService;
    private readonly IBaseService<MaterialInfo> _materialService;
    private readonly IBaseService<ProjectInfo> _projectService;

    public BatchPriceViewModel(
        IBaseService<BusinessRecord> businessService,
        IBaseService<PriceRule> ruleService,
        IBaseService<AddressInfo> addressService,
        IBaseService<MaterialInfo> materialService,
        IBaseService<ProjectInfo> projectService)
    {
        _businessService = businessService;
        _ruleService = ruleService;
        _addressService = addressService;
        _materialService = materialService;
        _projectService = projectService;

        QueryCommand = new DelegateCommand(async () => await QueryAsync());
        RefreshRulesCommand = new DelegateCommand(async () => await LoadRulesAsync());
        ApplyCustomPriceCommand = new DelegateCommand(async () => await ApplyCustomPriceAsync());
        ApplySelectedRuleCommand = new DelegateCommand(async () => await ApplySelectedRuleAsync());

        FilterStartDate = DateTime.Now.Date.AddDays(-30);
        FilterEndDate = DateTime.Now.Date;
        FilterOnlyEmptyPrice = true;

        _ = InitAsync();
    }

    #region Filters

    private DateTime _filterStartDate;
    public DateTime FilterStartDate
    {
        get => _filterStartDate;
        set => SetProperty(ref _filterStartDate, value);
    }

    private DateTime _filterEndDate;
    public DateTime FilterEndDate
    {
        get => _filterEndDate;
        set => SetProperty(ref _filterEndDate, value);
    }

    private LookupOption? _filterStartAddress;
    public LookupOption? FilterStartAddress
    {
        get => _filterStartAddress;
        set => SetProperty(ref _filterStartAddress, value);
    }

    private LookupOption? _filterEndAddress;
    public LookupOption? FilterEndAddress
    {
        get => _filterEndAddress;
        set => SetProperty(ref _filterEndAddress, value);
    }

    private LookupOption? _filterMaterial;
    public LookupOption? FilterMaterial
    {
        get => _filterMaterial;
        set => SetProperty(ref _filterMaterial, value);
    }

    private LookupOption? _filterProject;
    public LookupOption? FilterProject
    {
        get => _filterProject;
        set => SetProperty(ref _filterProject, value);
    }

    private string _filterPlate = string.Empty;
    public string FilterPlate
    {
        get => _filterPlate;
        set => SetProperty(ref _filterPlate, value);
    }

    private string _filterMemoKeyword = string.Empty;
    public string FilterMemoKeyword
    {
        get => _filterMemoKeyword;
        set => SetProperty(ref _filterMemoKeyword, value);
    }

    private bool _filterOnlyEmptyPrice;
    public bool FilterOnlyEmptyPrice
    {
        get => _filterOnlyEmptyPrice;
        set => SetProperty(ref _filterOnlyEmptyPrice, value);
    }

    #endregion

    #region Lookup Data

    private ObservableCollection<LookupOption> _startAddressOptions = new();
    public ObservableCollection<LookupOption> StartAddressOptions
    {
        get => _startAddressOptions;
        set => SetProperty(ref _startAddressOptions, value);
    }

    private ObservableCollection<LookupOption> _endAddressOptions = new();
    public ObservableCollection<LookupOption> EndAddressOptions
    {
        get => _endAddressOptions;
        set => SetProperty(ref _endAddressOptions, value);
    }

    private ObservableCollection<LookupOption> _materialOptions = new();
    public ObservableCollection<LookupOption> MaterialOptions
    {
        get => _materialOptions;
        set => SetProperty(ref _materialOptions, value);
    }

    private ObservableCollection<LookupOption> _projectOptions = new();
    public ObservableCollection<LookupOption> ProjectOptions
    {
        get => _projectOptions;
        set => SetProperty(ref _projectOptions, value);
    }

    #endregion

    #region Preview & Rules

    private ObservableCollection<BusinessRecord> _previewItems = new();
    public ObservableCollection<BusinessRecord> PreviewItems
    {
        get => _previewItems;
        set => SetProperty(ref _previewItems, value);
    }

    private ObservableCollection<PriceRule> _activeRules = new();
    public ObservableCollection<PriceRule> ActiveRules
    {
        get => _activeRules;
        set => SetProperty(ref _activeRules, value);
    }

    private PriceRule? _selectedRule;
    public PriceRule? SelectedRule
    {
        get => _selectedRule;
        set => SetProperty(ref _selectedRule, value);
    }

    private string _customPrice = string.Empty;
    public string CustomPrice
    {
        get => _customPrice;
        set => SetProperty(ref _customPrice, value);
    }

    private string _summaryText = "请设置筛选条件后点击查询";
    public string SummaryText
    {
        get => _summaryText;
        set => SetProperty(ref _summaryText, value);
    }

    #endregion

    #region Commands

    public DelegateCommand QueryCommand { get; }
    public DelegateCommand RefreshRulesCommand { get; }
    public DelegateCommand ApplyCustomPriceCommand { get; }
    public DelegateCommand ApplySelectedRuleCommand { get; }

    #endregion

    private async Task InitAsync()
    {
        await LoadLookupOptionsAsync();
        await LoadRulesAsync();
        await QueryAsync();
    }

    private async Task LoadLookupOptionsAsync()
    {
        var startOptions = new List<LookupOption> { new() { Id = null, Name = "全部" } };
        var endOptions = new List<LookupOption> { new() { Id = null, Name = "全部" } };
        var materialOptions = new List<LookupOption> { new() { Id = null, Name = "全部" } };
        var projectOptions = new List<LookupOption> { new() { Id = null, Name = "全部" } };

        startOptions.AddRange((await _addressService.GetAllAsync())
            .OrderBy(x => x.SpaceName)
            .Select(x => new LookupOption { Id = x.SpaceId, Name = x.SpaceName }));
        endOptions.AddRange((await _addressService.GetAllAsync())
            .OrderBy(x => x.SpaceName)
            .Select(x => new LookupOption { Id = x.SpaceId, Name = x.SpaceName }));
        materialOptions.AddRange((await _materialService.GetAllAsync())
            .OrderBy(x => x.MaterialName)
            .Select(x => new LookupOption { Id = x.MaterialId, Name = x.MaterialName }));
        projectOptions.AddRange((await _projectService.GetAllAsync())
            .OrderBy(x => x.ProjectName)
            .Select(x => new LookupOption { Id = x.ProjectId, Name = x.ProjectName }));

        StartAddressOptions = new ObservableCollection<LookupOption>(startOptions);
        EndAddressOptions = new ObservableCollection<LookupOption>(endOptions);
        MaterialOptions = new ObservableCollection<LookupOption>(materialOptions);
        ProjectOptions = new ObservableCollection<LookupOption>(projectOptions);

        FilterStartAddress = StartAddressOptions.FirstOrDefault();
        FilterEndAddress = EndAddressOptions.FirstOrDefault();
        FilterMaterial = MaterialOptions.FirstOrDefault();
        FilterProject = ProjectOptions.FirstOrDefault();
    }

    private async Task LoadRulesAsync()
    {
        var rules = await _ruleService.QueryAsync(x => x.IsActive == 1);
        ActiveRules = new ObservableCollection<PriceRule>(
            rules.OrderBy(x => x.Priority).ThenBy(x => x.RuleId));
    }

    private async Task QueryAsync()
    {
        var startDate = FilterStartDate.Date;
        var endDate = FilterEndDate.Date.AddDays(1);

        var list = await _businessService.QueryAsync(x =>
            x.IsCancelled == 0 &&
            x.BusinessDate >= startDate &&
            x.BusinessDate < endDate);

        if (FilterStartAddress?.Id != null)
        {
            list = list.Where(x => x.StartAddrId == FilterStartAddress.Id).ToList();
        }

        if (FilterEndAddress?.Id != null)
        {
            list = list.Where(x => x.EndAddrId == FilterEndAddress.Id).ToList();
        }

        if (FilterMaterial?.Id != null)
        {
            list = list.Where(x => x.MaterialId == FilterMaterial.Id).ToList();
        }

        if (FilterProject?.Id != null)
        {
            list = list.Where(x => x.ProjectId == FilterProject.Id).ToList();
        }

        if (!string.IsNullOrWhiteSpace(FilterPlate))
        {
            var plates = ParsePlateInput(FilterPlate);
            if (plates.Count > 1)
            {
                var set = new HashSet<string>(plates, StringComparer.OrdinalIgnoreCase);
                list = list.Where(x => set.Contains(x.LicensePlate)).ToList();
            }
            else
            {
                var keyword = FilterPlate.Trim();
                list = list.Where(x => x.LicensePlate.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        if (!string.IsNullOrWhiteSpace(FilterMemoKeyword))
        {
            var keyword = FilterMemoKeyword.Trim();
            list = list.Where(x => x.Memo?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true).ToList();
        }

        if (FilterOnlyEmptyPrice)
        {
            list = list.Where(x => x.Price <= 0).ToList();
        }

        list = list
            .OrderByDescending(x => x.BusinessDate)
            .ThenByDescending(x => x.BusinessId)
            .ToList();

        PreviewItems = new ObservableCollection<BusinessRecord>(list);
        UpdateSummary();
    }

    private async Task ApplyCustomPriceAsync()
    {
        if (!decimal.TryParse(CustomPrice, out var price) || price <= 0)
        {
            MessageBox.Show("请输入大于 0 的有效单价。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await ApplyPriceToPreviewAsync(price);
    }

    private async Task ApplySelectedRuleAsync()
    {
        if (SelectedRule == null)
        {
            MessageBox.Show("请先选择一条规则。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await ApplyPriceToPreviewAsync(SelectedRule.Price);
    }

    private async Task ApplyPriceToPreviewAsync(decimal price)
    {
        if (PreviewItems.Count == 0)
        {
            MessageBox.Show("当前没有可更新的数据，请先查询。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MessageBox.Show($"确定将当前 {PreviewItems.Count} 条记录的单价更新为 {price:F2} 吗？",
                "确认更新",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return;
        }

        foreach (var item in PreviewItems)
        {
            item.Price = price;
            item.TotalAmount = item.CarCount * price;
            item.UpdatedAt = DateTime.Now;
            await _businessService.UpdateAsync(item);
        }

        await QueryAsync();
    }

    private void UpdateSummary()
    {
        var count = PreviewItems.Count;
        var totalCars = PreviewItems.Sum(x => x.CarCount);
        var totalAmount = PreviewItems.Sum(x => x.TotalAmount);
        var emptyPriceCount = PreviewItems.Count(x => x.Price <= 0);

        SummaryText = $"共 {count} 条记录，车辆总数 {totalCars}，总金额 {totalAmount:N2}，其中空单价 {emptyPriceCount} 条";
    }

    private static List<string> ParsePlateInput(string input)
    {
        return input
            .Split([',', '，', ';', '；', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
