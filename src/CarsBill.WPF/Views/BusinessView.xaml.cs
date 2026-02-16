using CarsBill.WPF.ViewModels;
using System.Windows.Controls;

namespace CarsBill.WPF.Views;

public partial class BusinessView : UserControl
{
    public BusinessView(BusinessViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
