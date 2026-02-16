using CarsBill.WPF.ViewModels;
using System.Windows.Controls;

namespace CarsBill.WPF.Views;

public partial class BaseInfoView : UserControl
{
    public BaseInfoView(
        CarInfoViewModel carVm,
        AddressInfoViewModel addressVm,
        MaterialInfoViewModel materialVm,
        ProjectInfoViewModel projectVm)
    {
        InitializeComponent();

        CarTab.DataContext = carVm;
        AddressTab.DataContext = addressVm;
        MaterialTab.DataContext = materialVm;
        ProjectTab.DataContext = projectVm;
    }
}
