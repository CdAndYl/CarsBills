using CarsBill.WPF.Models;
using CarsBill.WPF.Services;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;

namespace CarsBill.WPF.ViewModels;

/// <summary>
/// 单价规则管理
/// </summary>
public class PriceRuleViewModel : BindableBase
{
    private readonly IBaseService<PriceRule> _ruleService;
    private readonly IBaseService<AddressInfo> _addressService;
    private readonly IBaseService<MaterialInfo> _materialService;
    private readonly IBaseService<ProjectInfo> _projectService;

    public PriceRuleViewModel(
        IBaseService<PriceRule> ruleService,
        IBaseService<AddressInfo> addressService,
        IBaseService<MaterialInfo> materialService,
        IBaseService<ProjectInfo> projectService)
    {
        _ruleService = ruleService;
        _addressService = addressService;
        _materialService = materialService;
        _projectService = projectService;

        LoadCommand = new DelegateCommand(async () => await LoadDataAsync());
        AddCommand = new DelegateCommand(AddNewRule);
        SaveCommand = new DelegateCommand(async () => await SaveAsync());
        DeleteCommand = new DelegateCommand(async () => await DeleteAsync(), () => SelectedItem != null)
            .ObservesProperty(() => SelectedItem);
        CancelCommand = new DelegateCommand(CancelEdit);

        _ = InitAsync();
    }

    #region List

    private ObservableCollection<PriceRule> _items = new();
    public ObservableCollection<PriceRule> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    private PriceRule? _selectedItem;
    public PriceRule? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value) && value != null)
            {
                LoadSelectedRule(value);
            }
        }
    }

    #endregion

    #region Filters

    private string _filterKeyword = string.Empty;
    public string FilterKeyword
    {
        get => _filterKeyword;
        set
        {
            if (SetProperty(ref _filterKeyword, value))
            {
                _ = LoadDataAsync();
            }
        }
    }

    private bool _showActiveOnly = true;
    public bool ShowActiveOnly
    {
        get => _showActiveOnly;
        set
        {
            if (SetProperty(ref _showActiveOnly, value))
            {
                _ = LoadDataAsync();
            }
        }
    }

    #endregion

    #region Edit Options

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

    #region Edit Fields

    private LookupOption? _editStartAddress;
    public LookupOption? EditStartAddress
    {
        get => _editStartAddress;
        set => SetProperty(ref _editStartAddress, value);
    }

    private LookupOption? _editEndAddress;
    public LookupOption? EditEndAddress
    {
        get => _editEndAddress;
        set => SetProperty(ref _editEndAddress, value);
    }

    private LookupOption? _editMaterial;
    public LookupOption? EditMaterial
    {
        get => _editMaterial;
        set => SetProperty(ref _editMaterial, value);
    }

    private LookupOption? _editProject;
    public LookupOption? EditProject
    {
        get => _editProject;
        set => SetProperty(ref _editProject, value);
    }

    private string _editMemoKeyword = string.Empty;
    public string EditMemoKeyword
    {
        get => _editMemoKeyword;
        set => SetProperty(ref _editMemoKeyword, value);
    }

    private decimal _editPrice;
    public decimal EditPrice
    {
        get => _editPrice;
        set => SetProperty(ref _editPrice, value);
    }

    private int _editPriority = 100;
    public int EditPriority
    {
        get => _editPriority;
        set => SetProperty(ref _editPriority, value);
    }

    private bool _editIsActive = true;
    public bool EditIsActive
    {
        get => _editIsActive;
        set => SetProperty(ref _editIsActive, value);
    }

    private string _editRemark = string.Empty;
    public string EditRemark
    {
        get => _editRemark;
        set => SetProperty(ref _editRemark, value);
    }

    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    private bool _isNew;
    public bool IsNew
    {
        get => _isNew;
        set => SetProperty(ref _isNew, value);
    }

    #endregion

    #region Commands

    public DelegateCommand LoadCommand { get; }
    public DelegateCommand AddCommand { get; }
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand DeleteCommand { get; }
    public DelegateCommand CancelCommand { get; }

    #endregion

    private async Task InitAsync()
    {
        await LoadLookupOptionsAsync();
        await LoadDataAsync();
    }

    private async Task LoadLookupOptionsAsync()
    {
        var startOptions = new List<LookupOption> { new() { Id = null, Name = "不限" } };
        var endOptions = new List<LookupOption> { new() { Id = null, Name = "不限" } };
        var materialOptions = new List<LookupOption> { new() { Id = null, Name = "不限" } };
        var projectOptions = new List<LookupOption> { new() { Id = null, Name = "不限" } };

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

        EditStartAddress = StartAddressOptions.FirstOrDefault();
        EditEndAddress = EndAddressOptions.FirstOrDefault();
        EditMaterial = MaterialOptions.FirstOrDefault();
        EditProject = ProjectOptions.FirstOrDefault();
    }

    private async Task LoadDataAsync()
    {
        var keyword = FilterKeyword.Trim();
        var list = await _ruleService.GetAllAsync();

        if (ShowActiveOnly)
        {
            list = list.Where(x => x.IsActive == 1).ToList();
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            list = list.Where(x =>
                    (x.Remark?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.StartAddr?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.EndAddr?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.MaterialName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.ProjectName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.MemoKeyword?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        Items = new ObservableCollection<PriceRule>(
            list.OrderBy(x => x.Priority).ThenBy(x => x.RuleId));
    }

    private void AddNewRule()
    {
        SelectedItem = null;
        EditStartAddress = StartAddressOptions.FirstOrDefault();
        EditEndAddress = EndAddressOptions.FirstOrDefault();
        EditMaterial = MaterialOptions.FirstOrDefault();
        EditProject = ProjectOptions.FirstOrDefault();
        EditMemoKeyword = string.Empty;
        EditPrice = 0;
        EditPriority = 100;
        EditIsActive = true;
        EditRemark = string.Empty;
        IsEditing = true;
        IsNew = true;
    }

    private void LoadSelectedRule(PriceRule rule)
    {
        EditStartAddress = FindOption(StartAddressOptions, rule.StartAddrId);
        EditEndAddress = FindOption(EndAddressOptions, rule.EndAddrId);
        EditMaterial = FindOption(MaterialOptions, rule.MaterialId);
        EditProject = FindOption(ProjectOptions, rule.ProjectId);
        EditMemoKeyword = rule.MemoKeyword ?? string.Empty;
        EditPrice = rule.Price;
        EditPriority = rule.Priority;
        EditIsActive = rule.IsActive == 1;
        EditRemark = rule.Remark ?? string.Empty;
        IsEditing = true;
        IsNew = false;
    }

    private static LookupOption? FindOption(IEnumerable<LookupOption> source, int? id)
    {
        return source.FirstOrDefault(x => x.Id == id) ?? source.FirstOrDefault();
    }

    private async Task SaveAsync()
    {
        if (EditPrice < 0)
        {
            MessageBox.Show("单价不能小于 0", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (EditPriority < 0)
        {
            MessageBox.Show("优先级不能小于 0", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (IsNew)
        {
            var entity = new PriceRule
            {
                RuleId = $"RULE{DateTime.Now:yyyyMMddHHmmssfff}",
                StartAddrId = EditStartAddress?.Id,
                StartAddr = EditStartAddress?.Id == null ? null : EditStartAddress.Name,
                EndAddrId = EditEndAddress?.Id,
                EndAddr = EditEndAddress?.Id == null ? null : EditEndAddress.Name,
                MaterialId = EditMaterial?.Id,
                MaterialName = EditMaterial?.Id == null ? null : EditMaterial.Name,
                ProjectId = EditProject?.Id,
                ProjectName = EditProject?.Id == null ? null : EditProject.Name,
                MemoKeyword = string.IsNullOrWhiteSpace(EditMemoKeyword) ? null : EditMemoKeyword.Trim(),
                Price = EditPrice,
                Priority = EditPriority,
                IsActive = EditIsActive ? 1 : 0,
                Remark = string.IsNullOrWhiteSpace(EditRemark) ? null : EditRemark.Trim()
            };

            await _ruleService.AddAsync(entity);
        }
        else if (SelectedItem != null)
        {
            SelectedItem.StartAddrId = EditStartAddress?.Id;
            SelectedItem.StartAddr = EditStartAddress?.Id == null ? null : EditStartAddress?.Name;
            SelectedItem.EndAddrId = EditEndAddress?.Id;
            SelectedItem.EndAddr = EditEndAddress?.Id == null ? null : EditEndAddress?.Name;
            SelectedItem.MaterialId = EditMaterial?.Id;
            SelectedItem.MaterialName = EditMaterial?.Id == null ? null : EditMaterial?.Name;
            SelectedItem.ProjectId = EditProject?.Id;
            SelectedItem.ProjectName = EditProject?.Id == null ? null : EditProject?.Name;
            SelectedItem.MemoKeyword = string.IsNullOrWhiteSpace(EditMemoKeyword) ? null : EditMemoKeyword.Trim();
            SelectedItem.Price = EditPrice;
            SelectedItem.Priority = EditPriority;
            SelectedItem.IsActive = EditIsActive ? 1 : 0;
            SelectedItem.Remark = string.IsNullOrWhiteSpace(EditRemark) ? null : EditRemark.Trim();
            SelectedItem.UpdatedAt = DateTime.Now;

            await _ruleService.UpdateAsync(SelectedItem);
        }

        await LoadDataAsync();
        IsEditing = false;
        IsNew = false;
    }

    private async Task DeleteAsync()
    {
        if (SelectedItem == null) return;

        if (MessageBox.Show("确定删除当前规则吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        await _ruleService.DeleteAsync(SelectedItem.RuleId);
        await LoadDataAsync();
        IsEditing = false;
        IsNew = false;
        SelectedItem = null;
    }

    private void CancelEdit()
    {
        IsEditing = false;
        IsNew = false;
        SelectedItem = null;
    }
}
