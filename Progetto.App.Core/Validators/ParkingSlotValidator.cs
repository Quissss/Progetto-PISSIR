using FluentValidation;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Validators;

public class ParkingSlotValidator : AbstractValidator<ParkingSlot>
{
    public ParkingSlotValidator()
    {
        RuleFor(slot => slot.Status).IsInEnum().WithMessage("Status must be a valid ParkSlotStatus.");
    }
}
