using FluentValidation;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Validators;

/// <summary>
/// Rules for StopoverHistory entity validation
/// </summary>
public class StopoverHistoryValidator : AbstractValidator<Stopover>
{
    public StopoverHistoryValidator()
    {
        RuleFor(x => x.StartStopoverTime).NotNull();
        RuleFor(x => x.UserId).NotNull().NotEmpty();
        RuleFor(x => x.CarPlate).NotNull().NotEmpty();
        RuleFor(x => x.ParkingSlotId).NotNull().NotEmpty();
        RuleFor(x => x.ToPay).NotNull();
    }
}
