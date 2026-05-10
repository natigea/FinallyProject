using EcommersProject.BLL.DTOs;
using FluentValidation;

namespace EcommersProject.BLL.Validators;

public class ReviewCreateValidator : AbstractValidator<ReviewCreateDto>
{
    public ReviewCreateValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters.");
    }
}

public class ReviewUpdateValidator : AbstractValidator<ReviewUpdateDto>
{
    public ReviewUpdateValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters.");
    }
}
