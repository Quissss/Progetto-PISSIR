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
        RuleFor(p => p.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(p => p.Address).NotEmpty().WithMessage("Address is required");
        RuleFor(p => p.City).NotEmpty().WithMessage("City is required");
        RuleFor(p => p.Province).NotEmpty().WithMessage("Province is required");
        RuleFor(p => p.PostalCode).NotEmpty().WithMessage("PostalCode is required");
        RuleFor(p => p.Country).NotEmpty().WithMessage("Country is required");
        RuleFor(p => p.EnergyCostPerMinute).NotEmpty().WithMessage("Energy cost is required");
    }
}
