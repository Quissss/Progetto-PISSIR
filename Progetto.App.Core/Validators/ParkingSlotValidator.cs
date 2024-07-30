using FluentValidation;
using Progetto.App.Core.Models;

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
