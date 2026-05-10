using EcommersProject.BLL.DTOs;
using FluentValidation;

namespace EcommersProject.BLL.Validators;

public class AddressCreateValidator : AbstractValidator<AddressCreateDto>
{
    public AddressCreateValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User is required.");

        RuleFor(x => x.Line1)
            .NotEmpty().WithMessage("Address line 1 is required.")
            .MaximumLength(250).WithMessage("Address line 1 must not exceed 250 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required.")
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");
    }
}

public class AddressUpdateValidator : AbstractValidator<AddressUpdateDto>
{
    public AddressUpdateValidator()
    {
        RuleFor(x => x.Line1)
            .NotEmpty().WithMessage("Address line 1 is required.")
            .MaximumLength(250).WithMessage("Address line 1 must not exceed 250 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required.")
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");
    }
}
