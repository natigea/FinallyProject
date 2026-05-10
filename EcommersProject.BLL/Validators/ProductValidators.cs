using EcommersProject.BLL.DTOs;
using FluentValidation;

namespace EcommersProject.BLL.Validators;

public class ProductCreateValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(250).WithMessage("Product name must not exceed 250 characters.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(100).WithMessage("SKU must not exceed 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must not be negative.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.BrandId)
            .NotEmpty().WithMessage("Brand is required.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description must not exceed 4000 characters.");
    }
}

public class ProductUpdateValidator : AbstractValidator<ProductUpdateDto>
{
    public ProductUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(250).WithMessage("Product name must not exceed 250 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must not be negative.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.BrandId)
            .NotEmpty().WithMessage("Brand is required.");
    }
}
