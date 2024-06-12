using FluentValidation;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Validators;

/// <summary>
/// Rules for ParkingSlot entity validation
/// </summary>
public class ParkingSlotValidator : AbstractValidator<ParkingSlot>
{
    public ParkingSlotValidator()
    {
        RuleFor(slot => slot.Number)
            .NotEmpty().WithMessage("Number is required.");
        RuleFor(slot => slot.Status)
            .IsInEnum().WithMessage("Status must be a valid ParkSlotStatus.");
        RuleFor(slot => slot.ParkingId)
            .NotEmpty().WithMessage("ParkingId is required.");
    }
}
