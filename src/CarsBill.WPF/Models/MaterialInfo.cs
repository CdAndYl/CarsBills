using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarsBill.WPF.Models;

/// <summary>
/// 物料信息表
/// </summary>
[Table("material_info")]
public class MaterialInfo
{
    [Key]
    [Column("material_id")]
    public int MaterialId { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("material_name")]
    public string MaterialName { get; set; } = string.Empty;

    /// <summary>
    /// 助记码
    /// </summary>
    [MaxLength(200)]
    [Column("lookup_code")]
    public string? LookupCode { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
