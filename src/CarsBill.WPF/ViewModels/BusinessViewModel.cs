using CarsBill.WPF.Models;
using CarsBill.WPF.Services;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace CarsBill.WPF.ViewModels;

/// <summary>
/// Business Record Management ViewModel
/// </summary>
public class BusinessViewModel : BindableBase
{
    private readonly IBaseService<BusinessRecord> _bizService;
    private readonly IBaseService<CarInfo> _carService;
    private readonly IBaseService<AddressInfo> _addrService;
    private readonly IBaseService<MaterialInfo> _matService;
    private readonly IBaseService<ProjectInfo> _projService;
    private readonly IBaseService<PriceRule> _priceService;

    public BusinessViewModel(
        IBaseService<BusinessRecord> bizService,
        IBaseService<CarInfo> carService,
        IBaseService<AddressInfo> addrService,
        IBaseService<MaterialInfo> matService,
        IBaseService<ProjectInfo> projService,
        IBaseService<PriceRule> priceService)
    {
        _bizService = bizService;
        _carService = carService;
        _addrService = addrService;
        _matService = matService;
        _projService = projService;
        _priceService = priceService;

        LoadCommand = new DelegateCommand(async () => await LoadDataAsync());
        AddCommand = new DelegateCommand(PrepareNew);
        SaveCommand = new DelegateCommand(async () => await SaveAsync());
        DeleteCommand = new DelegateCommand(async () => await DeleteAsync(), () => SelectedItem != null)
            .ObservesProperty(() => SelectedItem);
        CancelCommand = new DelegateCommand(Cancel);
        AutoPriceCommand = new DelegateCommand(async () => await AutoFillPriceAsync());

        FilterStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        FilterEndDate = DateTime.Now.Date;

        _ = InitAsync();
    }

    #region Collections (Dropdowns)

    private ObservableCollection<CarInfo> _cars = new();
    public ObservableCollection<CarInfo> Cars { get => _cars; set => SetProperty(ref _cars, value); }

    private ObservableCollection<AddressInfo> _addresses = new();
    public ObservableCollection<AddressInfo> Addresses { get => _addresses; set => SetProperty(ref _addresses, value); }

    private ObservableCollection<MaterialInfo> _materials = new();
    public ObservableCollection<MaterialInfo> Materials { get => _materials; set => SetProperty(ref _materials, value); }

    private ObservableCollection<ProjectInfo> _projects = new();
    public ObservableCollection<ProjectInfo> Projects { get => _projects; set => SetProperty(ref _projects, value); }

    #endregion

    #region List & Selection

    private ObservableCollection<BusinessRecord> _items = new();
    public ObservableCollection<BusinessRecord> Items { get => _items; set => SetProperty(ref _items, value); }

    private BusinessRecord? _selectedItem;
    public BusinessRecord? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value) && value != null)
            {
                EditBusinessDate = value.BusinessDate ?? DateTime.Now;
                EditSelectedCar = Cars.FirstOrDefault(c => c.CarId == value.CarId);
                EditSelectedStartAddr = Addresses.FirstOrDefault(a => a.SpaceId == value.StartAddrId);
                EditSelectedEndAddr = Addresses.FirstOrDefault(a => a.SpaceId == value.EndAddrId);
                EditSelectedMaterial = Materials.FirstOrDefault(m => m.MaterialId == value.MaterialId);
                EditSelectedProject = Projects.FirstOrDefault(p => p.ProjectId == value.ProjectId);
                EditCarCount = value.CarCount;
                EditPrice = value.Price;
                EditMemo = value.Memo ?? "";
                IsEditing = true;
                IsNew = false;
            }
        }
    }

    #endregion

    #region Filter Properties

    private DateTime _filterStartDate;
    public DateTime FilterStartDate { get => _filterStartDate; set { if (SetProperty(ref _filterStartDate, value)) _ = LoadDataAsync(); } }

    private DateTime _filterEndDate;
    public DateTime FilterEndDate { get => _filterEndDate; set { if (SetProperty(ref _filterEndDate, value)) _ = LoadDataAsync(); } }

    private string _filterKeyword = "";
    public string FilterKeyword
    {
        get => _filterKeyword;
        set { if (SetProperty(ref _filterKeyword, value)) _ = LoadDataAsync(); }
    }

    #endregion

    #region Edit Properties

    private DateTime _editBusinessDate = DateTime.Now;
    public DateTime EditBusinessDate { get => _editBusinessDate; set => SetProperty(ref _editBusinessDate, value); }

    private CarInfo? _editSelectedCar;
    public CarInfo? EditSelectedCar
    {
        get => _editSelectedCar;
        set => SetProperty(ref _editSelectedCar, value);
    }

    private AddressInfo? _editSelectedStartAddr;
    public AddressInfo? EditSelectedStartAddr { get => _editSelectedStartAddr; set => SetProperty(ref _editSelectedStartAddr, value); }

    private AddressInfo? _editSelectedEndAddr;
    public AddressInfo? EditSelectedEndAddr { get => _editSelectedEndAddr; set => SetProperty(ref _editSelectedEndAddr, value); }

    private MaterialInfo? _editSelectedMaterial;
    public MaterialInfo? EditSelectedMaterial { get => _editSelectedMaterial; set => SetProperty(ref _editSelectedMaterial, value); }

    private ProjectInfo? _editSelectedProject;
    public ProjectInfo? EditSelectedProject { get => _editSelectedProject; set => SetProperty(ref _editSelectedProject, value); }

    private int _editCarCount = 1;
    public int EditCarCount
    {
        get => _editCarCount;
        set { if (SetProperty(ref _editCarCount, value)) CalcTotal(); }
    }

    private decimal _editPrice;
    public decimal EditPrice
    {
        get => _editPrice;
        set { if (SetProperty(ref _editPrice, value)) CalcTotal(); }
    }

    private decimal _editTotal;
    public decimal EditTotal { get => _editTotal; set => SetProperty(ref _editTotal, value); }

    private string _editMemo = "";
    public string EditMemo { get => _editMemo; set => SetProperty(ref _editMemo, value); }

    private bool _isEditing;
    public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }

    private bool _isNew;
    public bool IsNew { get => _isNew; set => SetProperty(ref _isNew, value); }

    // Summary
    private int _totalCount;
    public int TotalCount { get => _totalCount; set => SetProperty(ref _totalCount, value); }

    private decimal _totalAmount;
    public decimal TotalAmount { get => _totalAmount; set => SetProperty(ref _totalAmount, value); }

    #endregion

    #region Commands

    public DelegateCommand LoadCommand { get; }
    public DelegateCommand AddCommand { get; }
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand DeleteCommand { get; }
    public DelegateCommand CancelCommand { get; }
    public DelegateCommand AutoPriceCommand { get; }

    #endregion

    #region Methods

    private async Task InitAsync()
    {
        Cars = new ObservableCollection<CarInfo>(await _carService.GetAllAsync());
        Addresses = new ObservableCollection<AddressInfo>(await _addrService.GetAllAsync());
        Materials = new ObservableCollection<MaterialInfo>(await _matService.GetAllAsync());
        Projects = new ObservableCollection<ProjectInfo>(await _projService.GetAllAsync());
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var startDate = FilterStartDate.Date;
        var endDate = FilterEndDate.Date.AddDays(1);
        var kw = FilterKeyword?.Trim() ?? "";

        var list = await _bizService.QueryAsync(b =>
            b.IsCancelled == 0 &&
            b.BusinessDate >= startDate &&
            b.BusinessDate < endDate &&
            (string.IsNullOrEmpty(kw) ||
             b.LicensePlate.Contains(kw) ||
             (b.StartAddr != null && b.StartAddr.Contains(kw)) ||
             (b.EndAddr != null && b.EndAddr.Contains(kw)) ||
             (b.MaterialName != null && b.MaterialName.Contains(kw)) ||
             (b.ProjectName != null && b.ProjectName.Contains(kw))));

        Items = new ObservableCollection<BusinessRecord>(list.OrderByDescending(b => b.BusinessDate));
        TotalCount = Items.Count;
        TotalAmount = Items.Sum(b => b.TotalAmount);
    }

    private void PrepareNew()
    {
        EditBusinessDate = DateTime.Now;
        EditSelectedCar = null;
        EditSelectedStartAddr = null;
        EditSelectedEndAddr = null;
        EditSelectedMaterial = null;
        EditSelectedProject = null;
        EditCarCount = 1;
        EditPrice = 0;
        EditTotal = 0;
        EditMemo = "";
        IsEditing = true;
        IsNew = true;
        SelectedItem = null;
    }

    private async Task SaveAsync()
    {
        if (EditSelectedCar == null) return;

        if (IsNew)
        {
            var id = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99);
            var entity = new BusinessRecord
            {
                BusinessId = id,
                CarId = EditSelectedCar.CarId,
                LicensePlate = EditSelectedCar.LicensePlate,
                BusinessDate = EditBusinessDate,
                StartAddrId = EditSelectedStartAddr?.SpaceId,
                StartAddr = EditSelectedStartAddr?.SpaceName,
                EndAddrId = EditSelectedEndAddr?.SpaceId,
                EndAddr = EditSelectedEndAddr?.SpaceName,
                MaterialId = EditSelectedMaterial?.MaterialId,
                MaterialName = EditSelectedMaterial?.MaterialName,
                ProjectId = EditSelectedProject?.ProjectId,
                ProjectName = EditSelectedProject?.ProjectName,
                CarCount = EditCarCount,
                Price = EditPrice,
                TotalAmount = EditCarCount * EditPrice,
                Memo = EditMemo.Trim()
            };
            await _bizService.AddAsync(entity);
        }
        else if (SelectedItem != null)
        {
            SelectedItem.CarId = EditSelectedCar.CarId;
            SelectedItem.LicensePlate = EditSelectedCar.LicensePlate;
            SelectedItem.BusinessDate = EditBusinessDate;
            SelectedItem.StartAddrId = EditSelectedStartAddr?.SpaceId;
            SelectedItem.StartAddr = EditSelectedStartAddr?.SpaceName;
            SelectedItem.EndAddrId = EditSelectedEndAddr?.SpaceId;
            SelectedItem.EndAddr = EditSelectedEndAddr?.SpaceName;
            SelectedItem.MaterialId = EditSelectedMaterial?.MaterialId;
            SelectedItem.MaterialName = EditSelectedMaterial?.MaterialName;
            SelectedItem.ProjectId = EditSelectedProject?.ProjectId;
            SelectedItem.ProjectName = EditSelectedProject?.ProjectName;
            SelectedItem.CarCount = EditCarCount;
            SelectedItem.Price = EditPrice;
            SelectedItem.TotalAmount = EditCarCount * EditPrice;
            SelectedItem.Memo = EditMemo.Trim();
            SelectedItem.UpdatedAt = DateTime.Now;
            await _bizService.UpdateAsync(SelectedItem);
        }

        await LoadDataAsync();
        IsEditing = false;
        IsNew = false;
    }

    private async Task DeleteAsync()
    {
        if (SelectedItem == null) return;
        // Soft delete
        SelectedItem.IsCancelled = 1;
        SelectedItem.UpdatedAt = DateTime.Now;
        await _bizService.UpdateAsync(SelectedItem);
        await LoadDataAsync();
        IsEditing = false;
        SelectedItem = null;
    }

    private void Cancel()
    {
        IsEditing = false;
        IsNew = false;
        SelectedItem = null;
    }

    private void CalcTotal()
    {
        EditTotal = EditCarCount * EditPrice;
    }

    private async Task AutoFillPriceAsync()
    {
        var rules = await _priceService.QueryAsync(r => r.IsActive == 1);
        var matched = rules
            .Where(r =>
                (r.StartAddrId == null || r.StartAddrId == EditSelectedStartAddr?.SpaceId) &&
                (r.EndAddrId == null || r.EndAddrId == EditSelectedEndAddr?.SpaceId) &&
                (r.MaterialId == null || r.MaterialId == EditSelectedMaterial?.MaterialId) &&
                (r.ProjectId == null || r.ProjectId == EditSelectedProject?.ProjectId))
            .OrderBy(r => r.Priority)
            .FirstOrDefault();

        if (matched != null)
        {
            EditPrice = matched.Price;
        }
    }

    #endregion
}
