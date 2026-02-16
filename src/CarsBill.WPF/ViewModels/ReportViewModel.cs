using CarsBill.WPF.Models;
using CarsBill.WPF.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Prism.Commands;
using Prism.Mvvm;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace CarsBill.WPF.ViewModels;

/// <summary>
/// Report & Statistics ViewModel
/// </summary>
public class ReportViewModel : BindableBase
{
    private readonly IBaseService<BusinessRecord> _bizService;

    public ReportViewModel(IBaseService<BusinessRecord> bizService)
    {
        _bizService = bizService;

        LoadCommand = new DelegateCommand(async () => await LoadReportAsync());

        FilterStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        FilterEndDate = DateTime.Now.Date;

        _ = LoadReportAsync();
    }

    #region Filter

    private DateTime _filterStartDate;
    public DateTime FilterStartDate { get => _filterStartDate; set { if (SetProperty(ref _filterStartDate, value)) _ = LoadReportAsync(); } }

    private DateTime _filterEndDate;
    public DateTime FilterEndDate { get => _filterEndDate; set { if (SetProperty(ref _filterEndDate, value)) _ = LoadReportAsync(); } }

    #endregion

    #region Summary

    private int _totalRecords;
    public int TotalRecords { get => _totalRecords; set => SetProperty(ref _totalRecords, value); }

    private int _totalCarCount;
    public int TotalCarCount { get => _totalCarCount; set => SetProperty(ref _totalCarCount, value); }

    private decimal _totalAmount;
    public decimal TotalAmount { get => _totalAmount; set => SetProperty(ref _totalAmount, value); }

    private int _vehicleCount;
    public int VehicleCount { get => _vehicleCount; set => SetProperty(ref _vehicleCount, value); }

    #endregion

    #region Chart Data

    private ISeries[] _dailySeries = Array.Empty<ISeries>();
    public ISeries[] DailySeries { get => _dailySeries; set => SetProperty(ref _dailySeries, value); }

    private Axis[] _dailyXAxes = Array.Empty<Axis>();
    public Axis[] DailyXAxes { get => _dailyXAxes; set => SetProperty(ref _dailyXAxes, value); }

    private ISeries[] _vehicleSeries = Array.Empty<ISeries>();
    public ISeries[] VehicleSeries { get => _vehicleSeries; set => SetProperty(ref _vehicleSeries, value); }

    #endregion

    #region Detail Table

    private ObservableCollection<VehicleSummary> _vehicleSummaries = new();
    public ObservableCollection<VehicleSummary> VehicleSummaries { get => _vehicleSummaries; set => SetProperty(ref _vehicleSummaries, value); }

    #endregion

    public DelegateCommand LoadCommand { get; }

    private async Task LoadReportAsync()
    {
        var startDate = FilterStartDate.Date;
        var endDate = FilterEndDate.Date.AddDays(1);

        var records = await _bizService.QueryAsync(b =>
            b.IsCancelled == 0 &&
            b.BusinessDate >= startDate &&
            b.BusinessDate < endDate);

        // Summary
        TotalRecords = records.Count;
        TotalCarCount = records.Sum(r => r.CarCount);
        TotalAmount = records.Sum(r => r.TotalAmount);
        VehicleCount = records.Select(r => r.LicensePlate).Distinct().Count();

        // Daily chart
        var dailyData = records
            .Where(r => r.BusinessDate.HasValue)
            .GroupBy(r => r.BusinessDate!.Value.Date)
            .OrderBy(g => g.Key)
            .ToList();

        var labels = dailyData.Select(g => g.Key.ToString("MM-dd")).ToArray();
        var amounts = dailyData.Select(g => (double)g.Sum(r => r.TotalAmount)).ToArray();
        var counts = dailyData.Select(g => (double)g.Sum(r => r.CarCount)).ToArray();

        DailySeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = "Amount",
                Values = amounts,
                Fill = new SolidColorPaint(SKColors.CornflowerBlue)
            },
            new LineSeries<double>
            {
                Name = "Car Count",
                Values = counts,
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.OrangeRed) { StrokeThickness = 2 },
                ScalesYAt = 1
            }
        };

        DailyXAxes = new Axis[]
        {
            new Axis { Labels = labels, LabelsRotation = 45 }
        };

        // Vehicle summary
        var vehicleData = records
            .GroupBy(r => r.LicensePlate)
            .Select(g => new VehicleSummary
            {
                LicensePlate = g.Key,
                TripCount = g.Count(),
                CarCount = g.Sum(r => r.CarCount),
                TotalAmount = g.Sum(r => r.TotalAmount)
            })
            .OrderByDescending(v => v.TotalAmount)
            .ToList();

        VehicleSummaries = new ObservableCollection<VehicleSummary>(vehicleData);

        // Vehicle pie chart (top 8)
        var topVehicles = vehicleData.Take(8).ToList();
        VehicleSeries = topVehicles.Select(v =>
            new PieSeries<double>
            {
                Name = v.LicensePlate,
                Values = new[] { (double)v.TotalAmount }
            } as ISeries).ToArray();
    }
}

/// <summary>
/// Vehicle summary data for report table
/// </summary>
public class VehicleSummary
{
    public string LicensePlate { get; set; } = "";
    public int TripCount { get; set; }
    public int CarCount { get; set; }
    public decimal TotalAmount { get; set; }
}
