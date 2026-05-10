using EcommersProject.BLL.DTOs;
using FluentValidation;

namespace EcommersProject.BLL.Validators;

public class CouponCreateValidator : AbstractValidator<CouponCreateDto>
{
    public CouponCreateValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(50).WithMessage("Coupon code must not exceed 50 characters.");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount must not be negative.");

        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(0, 100).WithMessage("Discount percent must be between 0 and 100.");

        RuleFor(x => x.ValidFrom)
            .LessThan(x => x.ValidTo).WithMessage("Valid from date must be before valid to date.");

        RuleFor(x => x.ValidTo)
            .GreaterThan(DateTimeOffset.UtcNow).WithMessage("Valid to date must be in the future.");

        RuleFor(x => x.UsageLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Usage limit must not be negative.");
    }
}

public class CouponUpdateValidator : AbstractValidator<CouponUpdateDto>
{
    public CouponUpdateValidator()
    {
        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount must not be negative.");

        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(0, 100).WithMessage("Discount percent must be between 0 and 100.");

        RuleFor(x => x.ValidFrom)
            .LessThan(x => x.ValidTo).WithMessage("Valid from date must be before valid to date.");

        RuleFor(x => x.ValidTo)
            .GreaterThan(DateTimeOffset.UtcNow).WithMessage("Valid to date must be in the future.");

        RuleFor(x => x.UsageLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Usage limit must not be negative.");
    }
}
