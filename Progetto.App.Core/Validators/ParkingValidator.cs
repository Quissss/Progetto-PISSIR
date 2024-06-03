using FluentValidation;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Validators;

public class ParkingValidator : AbstractValidator<Parking>
{
    public ParkingValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name must be less than 50 characters");
        RuleFor(p => p.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(100).WithMessage("Address must be less than 100 characters");
        RuleFor(p => p.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(50).WithMessage("City must be less than 50 characters");
        RuleFor(p => p.Province)
            .NotEmpty().WithMessage("Province is required")
            .MaximumLength(50).WithMessage("Province must be less than 50 characters");
        RuleFor(p => p.PostalCode)
            .NotEmpty().WithMessage("PostalCode is required")
            .MaximumLength(10).WithMessage("PostalCode must be less than 10 characters");
        RuleFor(p => p.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(50).WithMessage("Country must be less than 50 characters");
        RuleFor(p => p.EnergyCostPerMinute)
            .NotEmpty().WithMessage("Energy cost is required")
            .GreaterThan(0).WithMessage("Energy cost must be greater than 0");
    }
}
