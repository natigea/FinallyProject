using EcommersProject.BLL.DTOs;
using FluentValidation;

namespace EcommersProject.BLL.Validators;

public class PaymentCreateValidator : AbstractValidator<PaymentCreateDto>
{
    public PaymentCreateValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than 0.");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Payment provider is required.")
            .MaximumLength(100).WithMessage("Provider name must not exceed 100 characters.");
    }
}

public class PaymentUpdateValidator : AbstractValidator<PaymentUpdateDto>
{
    public PaymentUpdateValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Payment status is required.");
    }
}
