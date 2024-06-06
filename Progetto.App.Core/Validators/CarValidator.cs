using FluentValidation;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Validators;

public class CarValidator : AbstractValidator<Car>
{
    public CarValidator()
    {
        RuleFor(c => c.LicencePlate)
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
    }
}
