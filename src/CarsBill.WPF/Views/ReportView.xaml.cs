using CarsBill.WPF.ViewModels;
using System.Windows.Controls;

namespace CarsBill.WPF.Views;

public partial class ReportView : UserControl
{
    public ReportView(ReportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
