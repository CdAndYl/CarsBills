using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarsBill.WPF.Models;

/// <summary>
/// 项目信息表
/// </summary>
[Table("project_info")]
public class ProjectInfo
{
    [Key]
    [Column("project_id")]
    public int ProjectId { get; set; }

    /// <summary>
    /// 项目名称
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("project_name")]
    public string ProjectName { get; set; } = string.Empty;

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
