using FluentValidation;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.Validators;

/// <summary>
/// Rules for Car entity validation
/// </summary>
public class CarValidator : AbstractValidator<Car>
{
    public CarValidator()
    {
        RuleFor(c => c.Plate)
            .NotEmpty().WithMessage("Licence Plate is required")
            .Matches(@"^[A-Z]{2}\d{3}[A-Z]{2}$").WithMessage("Licence Plate must be in the format AA123AA");
        RuleFor(c => c.Brand)
            .NotEmpty().WithMessage("Brand is required")
            .MaximumLength(50).WithMessage("Brand must be less than 50 characters");
        RuleFor(c => c.Model)
            .NotEmpty().WithMessage("Model is required")
            .MaximumLength(50).WithMessage("Model must be less than 50 characters");
        RuleFor(c => c.OwnerId)
            .NotEmpty().WithMessage("Owner is required");
        RuleFor(c => c.Status)
            .IsInEnum().WithMessage("Invalid status");
    }
}