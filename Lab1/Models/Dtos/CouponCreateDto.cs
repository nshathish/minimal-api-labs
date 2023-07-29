using System.ComponentModel.DataAnnotations;

namespace Lab1.Models.Dtos;

public class CouponCreateDto
{
    [Required] public string Code { get; set; } = string.Empty;

    [Required] [Range(1, 100)] public decimal Discount { get; set; }

    [Required][Range(1, int.MaxValue)] public int Total { get; set; }

}