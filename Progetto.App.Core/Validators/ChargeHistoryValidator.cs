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
        RuleFor(ch => ch.ParkStartDate)
            .NotEmpty().WithMessage("Start Date is required");
        RuleFor(ch => ch.ParkEndDate)
            .NotEmpty().WithMessage("End Date is required");
        RuleFor(ch => ch.StartChargeLevel)
            .NotEmpty().WithMessage("Start Charge Level is required")
            .LessThan(100).WithMessage("Start Charge Level must be less than 100")
            .GreaterThanOrEqualTo(0).WithMessage("Start Charge Level must be greater than or equal to 0");
        RuleFor(ch => ch.EndChargeLevel)
            .NotEmpty().WithMessage("End Charge Level is required")
            .GreaterThan(0).WithMessage("End Charge Level must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("End Charge Level must be less than or equal to 100")
            .GreaterThanOrEqualTo(ch => ch.StartChargeLevel).WithMessage("End Charge Level must be greater than or equal to Start Charge Level");
        RuleFor(ch => ch.MWBotId)
            .NotEmpty().WithMessage("MWBot is required");
        RuleFor(ch => ch.UserId)
            .NotEmpty().WithMessage("User is required");
    }
}
