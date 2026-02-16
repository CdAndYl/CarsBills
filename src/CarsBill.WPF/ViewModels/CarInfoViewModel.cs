using CarsBill.WPF.Models;
using CarsBill.WPF.Services;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace CarsBill.WPF.ViewModels;

/// <summary>
/// Vehicle Management ViewModel
/// </summary>
public class CarInfoViewModel : BindableBase
{
    private readonly IBaseService<CarInfo> _service;

    public CarInfoViewModel(IBaseService<CarInfo> service)
    {
        _service = service;

        // Commands
        LoadCommand = new DelegateCommand(async () => await LoadDataAsync());
        AddCommand = new DelegateCommand(async () => await AddAsync());
        SaveCommand = new DelegateCommand(async () => await SaveAsync());
        DeleteCommand = new DelegateCommand(async () => await DeleteAsync(), () => SelectedItem != null)
            .ObservesProperty(() => SelectedItem);
        CancelCommand = new DelegateCommand(Cancel);

        // Load data on init
        LoadCommand.Execute();
    }

    #region Properties

    private ObservableCollection<CarInfo> _items = new();
    public ObservableCollection<CarInfo> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    private CarInfo? _selectedItem;
    public CarInfo? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value) && value != null)
            {
                // Copy to edit fields
                EditLicensePlate = value.LicensePlate;
                EditOwnerName = value.OwnerName ?? "";
                EditPhoneNumber = value.PhoneNumber ?? "";
                IsEditing = true;
                IsNew = false;
            }
        }
    }

    private string _editLicensePlate = "";
    public string EditLicensePlate
    {
        get => _editLicensePlate;
        set => SetProperty(ref _editLicensePlate, value);
    }

    private string _editOwnerName = "";
    public string EditOwnerName
    {
        get => _editOwnerName;
        set => SetProperty(ref _editOwnerName, value);
    }

    private string _editPhoneNumber = "";
    public string EditPhoneNumber
    {
        get => _editPhoneNumber;
        set => SetProperty(ref _editPhoneNumber, value);
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

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                _ = SearchAsync();
        }
    }

    #endregion

    #region Commands

    public DelegateCommand LoadCommand { get; }
    public DelegateCommand AddCommand { get; }
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand DeleteCommand { get; }
    public DelegateCommand CancelCommand { get; }

    #endregion

    #region Methods

    private async Task LoadDataAsync()
    {
        var list = await _service.GetAllAsync();
        Items = new ObservableCollection<CarInfo>(list);
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadDataAsync();
            return;
        }

        var keyword = SearchText.Trim();
        var list = await _service.QueryAsync(c =>
            c.LicensePlate.Contains(keyword) ||
            (c.OwnerName != null && c.OwnerName.Contains(keyword)));
        Items = new ObservableCollection<CarInfo>(list);
    }

    private async Task AddAsync()
    {
        EditLicensePlate = "";
        EditOwnerName = "";
        EditPhoneNumber = "";
        IsEditing = true;
        IsNew = true;
        SelectedItem = null;
        await Task.CompletedTask;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditLicensePlate)) return;

        if (IsNew)
        {
            var entity = new CarInfo
            {
                LicensePlate = EditLicensePlate.Trim(),
                OwnerName = EditOwnerName.Trim(),
                PhoneNumber = EditPhoneNumber.Trim()
            };
            await _service.AddAsync(entity);
        }
        else if (SelectedItem != null)
        {
            SelectedItem.LicensePlate = EditLicensePlate.Trim();
            SelectedItem.OwnerName = EditOwnerName.Trim();
            SelectedItem.PhoneNumber = EditPhoneNumber.Trim();
            SelectedItem.UpdatedAt = DateTime.Now;
            await _service.UpdateAsync(SelectedItem);
        }

        await LoadDataAsync();
        IsEditing = false;
        IsNew = false;
    }

    private async Task DeleteAsync()
    {
        if (SelectedItem == null) return;

        await _service.DeleteAsync(SelectedItem.CarId);
        await LoadDataAsync();
        IsEditing = false;
        SelectedItem = null;
    }

    private void Cancel()
    {
        IsEditing = false;
        IsNew = false;
        SelectedItem = null;
        EditLicensePlate = "";
        EditOwnerName = "";
        EditPhoneNumber = "";
    }

    #endregion
}
