using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarsBill.WPF.Models;

/// <summary>
/// 地点信息表
/// </summary>
[Table("address_info")]
public class AddressInfo
{
    [Key]
    [Column("space_id")]
    public int SpaceId { get; set; }

    /// <summary>
    /// 地点名称
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("space_name")]
    public string SpaceName { get; set; } = string.Empty;

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
