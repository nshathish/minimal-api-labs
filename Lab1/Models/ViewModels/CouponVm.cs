namespace Lab1.Models.ViewModels;

public class CouponVm
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Discount { get; set; }
    public int Total { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}