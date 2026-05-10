using EcommersProject.BLL.DTOs;
using FluentValidation;

namespace EcommersProject.BLL.Validators;

public class CartCreateValidator : AbstractValidator<CartCreateDto>
{
    public CartCreateValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User is required.");
    }
}

public class CartItemCreateValidator : AbstractValidator<CartItemCreateDto>
{
    public CartItemCreateValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Cart is required.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
    }
}

public class CartItemUpdateValidator : AbstractValidator<CartItemUpdateDto>
{
    public CartItemUpdateValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must not be negative.");
    }
}
