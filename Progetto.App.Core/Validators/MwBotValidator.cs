using FluentValidation;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Validators;

/// <summary>
/// Rules for MwBot entity validation
/// </summary>
public class MwBotValidator : AbstractValidator<MwBot>
{
    public MwBotValidator()
    {
        RuleFor(mw => mw.BatteryPercentage)
            .NotEmpty().WithMessage("Battery Percentage is required")
            .LessThanOrEqualTo(100).WithMessage("Battery Percentage must be less than or equal to 100")
            .GreaterThanOrEqualTo(0).WithMessage("Battery Percentage must be greater than or equal to 0");
        RuleFor(mw => mw.Status)
            .NotEmpty().WithMessage("Status is required")
            .IsInEnum().WithMessage("Status must be a valid MwBotStatus");
    }
}
