using EcommersProject.BLL.DTOs;
using FluentValidation;

namespace EcommersProject.BLL.Validators;

public class BrandCreateValidator : AbstractValidator<BrandCreateDto>
{
    public BrandCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required.")
            .MaximumLength(250).WithMessage("Brand name must not exceed 250 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}

public class BrandUpdateValidator : AbstractValidator<BrandUpdateDto>
{
    public BrandUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required.")
            .MaximumLength(250).WithMessage("Brand name must not exceed 250 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}
