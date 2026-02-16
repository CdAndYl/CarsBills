using CarsBill.WPF.Models;
using CarsBill.WPF.Services;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace CarsBill.WPF.ViewModels;

/// <summary>
/// Material Management ViewModel
/// </summary>
public class MaterialInfoViewModel : BindableBase
{
    private readonly IBaseService<MaterialInfo> _service;

    public MaterialInfoViewModel(IBaseService<MaterialInfo> service)
    {
        _service = service;
        LoadCommand = new DelegateCommand(async () => await LoadDataAsync());
        AddCommand = new DelegateCommand(() => { EditName = ""; EditCode = ""; IsEditing = true; IsNew = true; SelectedItem = null; });
        SaveCommand = new DelegateCommand(async () => await SaveAsync());
        DeleteCommand = new DelegateCommand(async () => await DeleteAsync(), () => SelectedItem != null).ObservesProperty(() => SelectedItem);
        CancelCommand = new DelegateCommand(() => { IsEditing = false; IsNew = false; SelectedItem = null; });
        LoadCommand.Execute();
    }

    private ObservableCollection<MaterialInfo> _items = new();
    public ObservableCollection<MaterialInfo> Items { get => _items; set => SetProperty(ref _items, value); }

    private MaterialInfo? _selectedItem;
    public MaterialInfo? SelectedItem
    {
        get => _selectedItem;
        set { if (SetProperty(ref _selectedItem, value) && value != null) { EditName = value.MaterialName; EditCode = value.LookupCode ?? ""; IsEditing = true; IsNew = false; } }
    }

    private string _editName = "";
    public string EditName { get => _editName; set => SetProperty(ref _editName, value); }

    private string _editCode = "";
    public string EditCode { get => _editCode; set => SetProperty(ref _editCode, value); }

    private bool _isEditing;
    public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }

    private bool _isNew;
    public bool IsNew { get => _isNew; set => SetProperty(ref _isNew, value); }

    private string _searchText = "";
    public string SearchText { get => _searchText; set { if (SetProperty(ref _searchText, value)) _ = SearchAsync(); } }

    public DelegateCommand LoadCommand { get; }
    public DelegateCommand AddCommand { get; }
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand DeleteCommand { get; }
    public DelegateCommand CancelCommand { get; }

    private async Task LoadDataAsync() => Items = new ObservableCollection<MaterialInfo>(await _service.GetAllAsync());

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText)) { await LoadDataAsync(); return; }
        var kw = SearchText.Trim();
        Items = new ObservableCollection<MaterialInfo>(await _service.QueryAsync(x => x.MaterialName.Contains(kw) || (x.LookupCode != null && x.LookupCode.Contains(kw))));
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName)) return;
        if (IsNew) await _service.AddAsync(new MaterialInfo { MaterialName = EditName.Trim(), LookupCode = EditCode.Trim() });
        else if (SelectedItem != null) { SelectedItem.MaterialName = EditName.Trim(); SelectedItem.LookupCode = EditCode.Trim(); SelectedItem.UpdatedAt = DateTime.Now; await _service.UpdateAsync(SelectedItem); }
        await LoadDataAsync(); IsEditing = false; IsNew = false;
    }

    private async Task DeleteAsync()
    {
        if (SelectedItem == null) return;
        await _service.DeleteAsync(SelectedItem.MaterialId);
        await LoadDataAsync(); IsEditing = false; SelectedItem = null;
    }
}
