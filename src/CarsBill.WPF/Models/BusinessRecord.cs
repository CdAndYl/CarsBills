using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarsBill.WPF.Models;

/// <summary>
/// 业务记录表
/// </summary>
[Table("business_record")]
public class BusinessRecord
{
    /// <summary>
    /// 业务ID
    /// </summary>
    [Key]
    [MaxLength(15)]
    [Column("business_id")]
    public string BusinessId { get; set; } = string.Empty;

    /// <summary>
    /// 车辆ID
    /// </summary>
    [Column("car_id")]
    public int CarId { get; set; }

    /// <summary>
    /// 车牌号
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("license_plate")]
    public string LicensePlate { get; set; } = string.Empty;

    /// <summary>
    /// 业务日期
    /// </summary>
    [Column("business_date")]
    public DateTime? BusinessDate { get; set; }

    /// <summary>
    /// 起始地点ID
    /// </summary>
    [Column("start_addr_id")]
    public int? StartAddrId { get; set; }

    /// <summary>
    /// 起始地点
    /// </summary>
    [MaxLength(200)]
    [Column("start_addr")]
    public string? StartAddr { get; set; }

    /// <summary>
    /// 终止地点ID
    /// </summary>
    [Column("end_addr_id")]
    public int? EndAddrId { get; set; }

    /// <summary>
    /// 终止地点
    /// </summary>
    [MaxLength(200)]
    [Column("end_addr")]
    public string? EndAddr { get; set; }

    /// <summary>
    /// 物料ID
    /// </summary>
    [Column("material_id")]
    public int? MaterialId { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    [MaxLength(200)]
    [Column("material_name")]
    public string? MaterialName { get; set; }

    /// <summary>
    /// 项目ID
    /// </summary>
    [Column("project_id")]
    public int? ProjectId { get; set; }

    /// <summary>
    /// 项目名称
    /// </summary>
    [MaxLength(200)]
    [Column("project_name")]
    public string? ProjectName { get; set; }

    /// <summary>
    /// 车数
    /// </summary>
    [Column("car_count")]
    public int CarCount { get; set; } = 0;

    /// <summary>
    /// 单价
    /// </summary>
    [Column("price", TypeName = "decimal(12,2)")]
    public decimal Price { get; set; } = 0;

    /// <summary>
    /// 总金额
    /// </summary>
    [Column("total_amount", TypeName = "decimal(12,2)")]
    public decimal TotalAmount { get; set; } = 0;

    /// <summary>
    /// 是否付款(0:未付,1:已付)
    /// </summary>
    [Column("is_paid")]
    public int IsPaid { get; set; } = 0;

    /// <summary>
    /// 是否作废(0:正常,1:作废)
    /// </summary>
    [Column("is_cancelled")]
    public int IsCancelled { get; set; } = 0;

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(200)]
    [Column("memo")]
    public string? Memo { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
