using CarsBill.WPF.Models;
using CarsBill.WPF.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Win32;
using OfficeOpenXml;
using Prism.Commands;
using Prism.Mvvm;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace CarsBill.WPF.ViewModels;

/// <summary>
/// Report & Statistics ViewModel
/// </summary>
public class ReportViewModel : BindableBase
{
    private readonly IBaseService<BusinessRecord> _bizService;
    private List<BusinessRecord> _currentRecords = new();

    public ReportViewModel(IBaseService<BusinessRecord> bizService)
    {
        _bizService = bizService;

        LoadCommand = new DelegateCommand(async () => await LoadReportAsync());
        ExportCommand = new DelegateCommand(async () => await ExportAsync());

        FilterStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        FilterEndDate = DateTime.Now.Date;

        _ = LoadReportAsync();
    }

    #region Filter

    private DateTime _filterStartDate;
    public DateTime FilterStartDate { get => _filterStartDate; set { if (SetProperty(ref _filterStartDate, value)) _ = LoadReportAsync(); } }

    private DateTime _filterEndDate;
    public DateTime FilterEndDate { get => _filterEndDate; set { if (SetProperty(ref _filterEndDate, value)) _ = LoadReportAsync(); } }

    private string _plateFilterText = "";
    public string PlateFilterText
    {
        get => _plateFilterText;
        set { if (SetProperty(ref _plateFilterText, value)) _ = LoadReportAsync(); }
    }

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
    public DelegateCommand ExportCommand { get; }

    private async Task LoadReportAsync()
    {
        var startDate = FilterStartDate.Date;
        var endDate = FilterEndDate.Date.AddDays(1);

        var allRecords = await _bizService.QueryAsync(b =>
            b.IsCancelled == 0 &&
            b.BusinessDate >= startDate &&
            b.BusinessDate < endDate);
        var records = ApplyPlateFilter(allRecords);
        _currentRecords = records
            .OrderBy(r => r.BusinessDate)
            .ThenBy(r => r.LicensePlate)
            .ToList();

        // Summary
        TotalRecords = _currentRecords.Count;
        TotalCarCount = _currentRecords.Sum(r => r.CarCount);
        TotalAmount = _currentRecords.Sum(r => r.TotalAmount);
        VehicleCount = _currentRecords
            .Select(r => r.LicensePlate?.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        // Daily chart
        var dailyData = _currentRecords
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
        var vehicleData = _currentRecords
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

    private async Task ExportAsync()
    {
        if (_currentRecords.Count == 0)
        {
            MessageBox.Show("当前筛选条件下没有可导出的数据。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var saveDialog = new SaveFileDialog
        {
            Filter = "Excel 文件 (*.xlsx)|*.xlsx",
            FileName = BuildDefaultExportFileName()
        };

        if (saveDialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            var plateTokens = ParsePlateTokens(PlateFilterText);
            if (plateTokens.Count > 1)
            {
                var groups = _currentRecords
                    .GroupBy(r => string.IsNullOrWhiteSpace(r.LicensePlate) ? "未填写车牌" : r.LicensePlate.Trim())
                    .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                AddVehicleSummarySheet(package, groups);
                foreach (var group in groups)
                {
                    AddDetailSheet(package, group.Key, group.ToList());
                }
            }
            else
            {
                var sheetName = plateTokens.Count == 1 ? plateTokens[0] : "业务明细";
                AddDetailSheet(package, sheetName, _currentRecords);
            }

            package.SaveAs(new FileInfo(saveDialog.FileName));

            MessageBox.Show($"导出成功：\n{saveDialog.FileName}", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        await Task.CompletedTask;
    }

    private List<BusinessRecord> ApplyPlateFilter(List<BusinessRecord> records)
    {
        var tokens = ParsePlateTokens(PlateFilterText);
        if (tokens.Count == 0)
        {
            return records;
        }

        if (tokens.Count == 1)
        {
            var keyword = tokens[0];
            return records.Where(r =>
                    !string.IsNullOrWhiteSpace(r.LicensePlate) &&
                    r.LicensePlate.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var plateSet = tokens.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return records.Where(r =>
                !string.IsNullOrWhiteSpace(r.LicensePlate) &&
                plateSet.Contains(r.LicensePlate.Trim()))
            .ToList();
    }

    private static List<string> ParsePlateTokens(string input)
    {
        return (input ?? string.Empty)
            .Split([',', '，', ';', '；', '\r', '\n', '\t', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(v => v.Trim())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private string BuildDefaultExportFileName()
    {
        var tokens = ParsePlateTokens(PlateFilterText);
        var suffix = tokens.Count switch
        {
            0 => "全部车牌",
            1 => tokens[0],
            _ => "多车牌"
        };
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeSuffix = new string(suffix.Where(c => !invalidChars.Contains(c)).ToArray());
        return $"业务报表_{FilterStartDate:yyyyMMdd}_{FilterEndDate:yyyyMMdd}_{safeSuffix}.xlsx";
    }

    private static void AddVehicleSummarySheet(ExcelPackage package, List<IGrouping<string, BusinessRecord>> groups)
    {
        var sheet = package.Workbook.Worksheets.Add(ToSafeSheetName(package, "汇总"));
        var headers = new[] { "车牌号", "记录数", "车数", "总金额" };
        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cells[1, i + 1].Value = headers[i];
        }

        var row = 2;
        foreach (var group in groups)
        {
            sheet.Cells[row, 1].Value = group.Key;
            sheet.Cells[row, 2].Value = group.Count();
            sheet.Cells[row, 3].Value = group.Sum(r => r.CarCount);
            sheet.Cells[row, 4].Value = group.Sum(r => r.TotalAmount);
            row++;
        }

        sheet.Cells[row, 1].Value = "合计";
        sheet.Cells[row, 2].Value = groups.Sum(g => g.Count());
        sheet.Cells[row, 3].Value = groups.Sum(g => g.Sum(r => r.CarCount));
        sheet.Cells[row, 4].Value = groups.Sum(g => g.Sum(r => r.TotalAmount));

        sheet.Cells[1, 1, 1, headers.Length].Style.Font.Bold = true;
        sheet.Cells[row, 1, row, headers.Length].Style.Font.Bold = true;
        sheet.Cells[2, 4, row, 4].Style.Numberformat.Format = "#,##0.00";
        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
    }

    private static void AddDetailSheet(ExcelPackage package, string sheetName, List<BusinessRecord> records)
    {
        var sheet = package.Workbook.Worksheets.Add(ToSafeSheetName(package, sheetName));
        var headers = new[] { "日期", "车牌号", "起点", "终点", "物料", "项目", "车数", "单价", "总额", "付款状态", "备注" };

        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cells[1, i + 1].Value = headers[i];
        }

        var row = 2;
        foreach (var record in records.OrderBy(r => r.BusinessDate).ThenBy(r => r.LicensePlate))
        {
            sheet.Cells[row, 1].Value = record.BusinessDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            sheet.Cells[row, 2].Value = record.LicensePlate;
            sheet.Cells[row, 3].Value = record.StartAddr;
            sheet.Cells[row, 4].Value = record.EndAddr;
            sheet.Cells[row, 5].Value = record.MaterialName;
            sheet.Cells[row, 6].Value = record.ProjectName;
            sheet.Cells[row, 7].Value = record.CarCount;
            sheet.Cells[row, 8].Value = record.Price;
            sheet.Cells[row, 9].Value = record.TotalAmount;
            sheet.Cells[row, 10].Value = record.IsPaid == 1 ? "已付" : "未付";
            sheet.Cells[row, 11].Value = record.Memo;
            row++;
        }

        sheet.Cells[row, 1].Value = "合计";
        sheet.Cells[row, 7].Value = records.Sum(r => r.CarCount);
        sheet.Cells[row, 9].Value = records.Sum(r => r.TotalAmount);

        sheet.Cells[1, 1, 1, headers.Length].Style.Font.Bold = true;
        sheet.Cells[row, 1, row, headers.Length].Style.Font.Bold = true;
        sheet.Cells[2, 8, row, 9].Style.Numberformat.Format = "#,##0.00";
        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
    }

    private static string ToSafeSheetName(ExcelPackage package, string rawName)
    {
        var invalidChars = new[] { '\\', '/', '*', '?', ':', '[', ']' };
        var cleaned = new string((rawName ?? "Sheet").Select(c => invalidChars.Contains(c) ? '_' : c).ToArray()).Trim();
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            cleaned = "Sheet";
        }

        if (cleaned.Length > 31)
        {
            cleaned = cleaned[..31];
        }

        var candidate = cleaned;
        var index = 1;
        while (package.Workbook.Worksheets.Any(s => string.Equals(s.Name, candidate, StringComparison.OrdinalIgnoreCase)))
        {
            var suffix = $"_{index++}";
            var maxPrefixLength = 31 - suffix.Length;
            var prefix = cleaned.Length > maxPrefixLength ? cleaned[..maxPrefixLength] : cleaned;
            candidate = $"{prefix}{suffix}";
        }

        return candidate;
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
