using FluentValidation;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Validators;

/// <summary>
/// Rules for ChargeHistory entity validation
/// </summary>
public class ChargeHistoryValidator : AbstractValidator<ChargeHistory>
{
    public ChargeHistoryValidator()
    {
        RuleFor(ch => ch.StartChargingTime).LessThanOrEqualTo(DateTime.Now).NotEmpty();
        RuleFor(ch => ch.EndChargingTime).NotEmpty();
        RuleFor(ch => ch.MwBotId).NotEmpty();
        RuleFor(ch => ch.StartChargePercentage).NotEmpty().Must(c => c >= 0 && c <= 100);
        RuleFor(ch => ch.TargetChargePercentage).NotEmpty().Must(c => c >= 0 && c <= 100);
    }
}
