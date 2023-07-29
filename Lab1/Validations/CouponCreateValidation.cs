using FluentValidation;
using Lab1.Models.Dtos;

namespace Lab1.Validations;

public class CouponCreateValidation : AbstractValidator<CouponCreateDto>
{
    public CouponCreateValidation()
    {
        RuleFor(model => model.Code).NotEmpty().WithMessage("Code is required");
        RuleFor(model => model.Discount)
            .NotEmpty().WithMessage("Code is required")
            .InclusiveBetween(1, 100).WithMessage("Discount should be between 1 and 100");
        RuleFor(model => model.Total)
            .NotEmpty().WithMessage("Total is required")
            .GreaterThan(0).WithMessage("Total should be greater than 0");
    }
}