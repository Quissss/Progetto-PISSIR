using FluentValidation;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Validators;

/// <summary>
/// Rules for CurrentlyCharging entity validation
/// </summary>
public class CurrentlyCharingValidator : AbstractValidator<CurrentlyCharging>
{
    public CurrentlyCharingValidator()
    {
        RuleFor(cc => cc.StartChargingTime).LessThanOrEqualTo(DateTime.Now).NotEmpty();
        RuleFor(cc => cc.MwBotId).NotEmpty();
        RuleFor(cc => cc.StartChargePercentage).NotEmpty().Must(c => c >= 0 && c <= 100);
        RuleFor(cc => cc.TargetChargePercentage).NotEmpty().Must(c => c >= 0 && c <= 100);
    }
}
