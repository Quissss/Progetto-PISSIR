using FluentValidation;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Validators;

/// <summary>
/// Rules for StopOver entity validation
/// </summary>
public class StopOverValidator : AbstractValidator<StopOver>
{
    public StopOverValidator()
    {
        RuleFor(x => x.StartStopOverTime).NotNull();
        RuleFor(x => x.UserId).NotNull().NotEmpty();
        RuleFor(x => x.CarPlate).NotNull().NotEmpty();
        RuleFor(x => x.ParkingId).NotNull().NotEmpty();
        RuleFor(x => x.ToPay).NotNull();
    }
}
