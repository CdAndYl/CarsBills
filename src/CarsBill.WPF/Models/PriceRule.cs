using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarsBill.WPF.Models;

/// <summary>
/// 单价规则表
/// </summary>
[Table("price_rule")]
public class PriceRule
{
    /// <summary>
    /// 规则ID
    /// </summary>
    [Key]
    [MaxLength(50)]
    [Column("rule_id")]
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// 起始地点ID
    /// </summary>
    [Column("start_addr_id")]
    public int? StartAddrId { get; set; }

    /// <summary>
    /// 起始地点名称
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
    /// 终止地点名称
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
    /// 备注关键字
    /// </summary>
    [MaxLength(200)]
    [Column("memo_keyword")]
    public string? MemoKeyword { get; set; }

    /// <summary>
    /// 单价
    /// </summary>
    [Required]
    [Column("price", TypeName = "decimal(12,2)")]
    public decimal Price { get; set; } = 0;

    /// <summary>
    /// 是否启用
    /// </summary>
    [Column("is_active")]
    public int IsActive { get; set; } = 1;

    /// <summary>
    /// 优先级
    /// </summary>
    [Column("priority")]
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 规则说明
    /// </summary>
    [MaxLength(500)]
    [Column("remark")]
    public string? Remark { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
