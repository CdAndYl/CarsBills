using CarsBill.WPF.Models;
using CarsBill.WPF.Services;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace CarsBill.WPF.ViewModels;

/// <summary>
/// Project Management ViewModel
/// </summary>
public class ProjectInfoViewModel : BindableBase
{
    private readonly IBaseService<ProjectInfo> _service;

    public ProjectInfoViewModel(IBaseService<ProjectInfo> service)
    {
        _service = service;
        LoadCommand = new DelegateCommand(async () => await LoadDataAsync());
        AddCommand = new DelegateCommand(() => { EditName = ""; EditCode = ""; IsEditing = true; IsNew = true; SelectedItem = null; });
        SaveCommand = new DelegateCommand(async () => await SaveAsync());
        DeleteCommand = new DelegateCommand(async () => await DeleteAsync(), () => SelectedItem != null).ObservesProperty(() => SelectedItem);
        CancelCommand = new DelegateCommand(() => { IsEditing = false; IsNew = false; SelectedItem = null; });
        LoadCommand.Execute();
    }

    private ObservableCollection<ProjectInfo> _items = new();
    public ObservableCollection<ProjectInfo> Items { get => _items; set => SetProperty(ref _items, value); }

    private ProjectInfo? _selectedItem;
    public ProjectInfo? SelectedItem
    {
        get => _selectedItem;
        set { if (SetProperty(ref _selectedItem, value) && value != null) { EditName = value.ProjectName; EditCode = value.LookupCode ?? ""; IsEditing = true; IsNew = false; } }
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

    private async Task LoadDataAsync() => Items = new ObservableCollection<ProjectInfo>(await _service.GetAllAsync());

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText)) { await LoadDataAsync(); return; }
        var kw = SearchText.Trim();
        Items = new ObservableCollection<ProjectInfo>(await _service.QueryAsync(x => x.ProjectName.Contains(kw) || (x.LookupCode != null && x.LookupCode.Contains(kw))));
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName)) return;
        if (IsNew) await _service.AddAsync(new ProjectInfo { ProjectName = EditName.Trim(), LookupCode = EditCode.Trim() });
        else if (SelectedItem != null) { SelectedItem.ProjectName = EditName.Trim(); SelectedItem.LookupCode = EditCode.Trim(); SelectedItem.UpdatedAt = DateTime.Now; await _service.UpdateAsync(SelectedItem); }
        await LoadDataAsync(); IsEditing = false; IsNew = false;
    }

    private async Task DeleteAsync()
    {
        if (SelectedItem == null) return;
        await _service.DeleteAsync(SelectedItem.ProjectId);
        await LoadDataAsync(); IsEditing = false; SelectedItem = null;
    }
}
