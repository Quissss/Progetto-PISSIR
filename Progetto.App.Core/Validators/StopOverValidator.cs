using FluentValidation;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Validators;

/// <summary>
/// Rules for Stopover entity validation
/// </summary>
public class StopoverValidator : AbstractValidator<Stopover>
{
    public StopoverValidator()
    {
        RuleFor(x => x.StartStopoverTime).NotNull();
        RuleFor(x => x.UserId).NotNull().NotEmpty();
        RuleFor(x => x.CarPlate).NotNull().NotEmpty();
        RuleFor(x => x.ParkingSlotId).NotNull().NotEmpty();
        RuleFor(x => x.ToPay).NotNull();
    }
}
