using EcommersProject.BLL.DTOs;
using FluentValidation;

namespace EcommersProject.BLL.Validators;

public class OrderCreateValidator : AbstractValidator<OrderCreateDto>
{
    public OrderCreateValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product is required.");
            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Item quantity must be greater than 0.");
        });
    }
}

public class OrderUpdateValidator : AbstractValidator<OrderUpdateDto>
{
    private static readonly string[] ValidStatuses =
    [
        "Pending", "Paid", "Processing", "Shipped", "Delivered", "Cancelled"
    ];

    public OrderUpdateValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.");
    }
}
