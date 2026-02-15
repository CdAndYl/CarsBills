using CarsBill.WPF.Models;
using Microsoft.EntityFrameworkCore;

namespace CarsBill.WPF.Data;

/// <summary>
/// EF Core 数据库上下文
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 车辆信息
    /// </summary>
    public DbSet<CarInfo> CarInfos { get; set; } = null!;

    /// <summary>
    /// 地点信息
    /// </summary>
    public DbSet<AddressInfo> AddressInfos { get; set; } = null!;

    /// <summary>
    /// 物料信息
    /// </summary>
    public DbSet<MaterialInfo> MaterialInfos { get; set; } = null!;

    /// <summary>
    /// 项目信息
    /// </summary>
    public DbSet<ProjectInfo> ProjectInfos { get; set; } = null!;

    /// <summary>
    /// 业务记录
    /// </summary>
    public DbSet<BusinessRecord> BusinessRecords { get; set; } = null!;

    /// <summary>
    /// 单价规则
    /// </summary>
    public DbSet<PriceRule> PriceRules { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 业务记录表 - 索引
        modelBuilder.Entity<BusinessRecord>(entity =>
        {
            entity.HasIndex(e => e.BusinessDate).HasDatabaseName("idx_business_date");
            entity.HasIndex(e => e.LicensePlate).HasDatabaseName("idx_license_plate");
        });

        // 单价规则表 - 索引
        modelBuilder.Entity<PriceRule>(entity =>
        {
            entity.HasIndex(e => e.IsActive).HasDatabaseName("idx_active");
        });

        // 车辆表 - 车牌号唯一
        modelBuilder.Entity<CarInfo>(entity =>
        {
            entity.HasIndex(e => e.LicensePlate).IsUnique();
        });
    }
}
