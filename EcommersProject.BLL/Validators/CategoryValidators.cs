using EcommersProject.BLL.DTOs;
using FluentValidation;

namespace EcommersProject.BLL.Validators;

public class CategoryCreateValidator : AbstractValidator<CategoryCreateDto>
{
    public CategoryCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(250).WithMessage("Category name must not exceed 250 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}

public class CategoryUpdateValidator : AbstractValidator<CategoryUpdateDto>
{
    public CategoryUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(250).WithMessage("Category name must not exceed 250 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}
