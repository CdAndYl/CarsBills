using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarsBill.WPF.Models;

/// <summary>
/// 车辆信息表
/// </summary>
[Table("car_info")]
public class CarInfo
{
    [Key]
    [Column("car_id")]
    public int CarId { get; set; }

    /// <summary>
    /// 车牌号
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("license_plate")]
    public string LicensePlate { get; set; } = string.Empty;

    /// <summary>
    /// 车主姓名
    /// </summary>
    [MaxLength(50)]
    [Column("owner_name")]
    public string? OwnerName { get; set; }

    /// <summary>
    /// 电话号码
    /// </summary>
    [MaxLength(50)]
    [Column("phone_number")]
    public string? PhoneNumber { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
