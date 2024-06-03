using FluentValidation;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Validators;

public class ChargeHistoryValidator : AbstractValidator<ChargeHistory>
{
    public ChargeHistoryValidator()
    {
        RuleFor(ch => ch.StartDate)
            .NotEmpty().WithMessage("Start Date is required");
        RuleFor(ch => ch.EndDate)
            .NotEmpty().WithMessage("End Date is required");
        RuleFor(ch => ch.StartChargeLevel)
            .NotEmpty().WithMessage("Start Charge Level is required");
        RuleFor(ch => ch.EndChargeLevel)
            .NotEmpty().WithMessage("End Charge Level is required");
        RuleFor(ch => ch.MWBotId)
            .NotEmpty().WithMessage("MWBot is required");
        RuleFor(ch => ch.UserId)
            .NotEmpty().WithMessage("User is required");
    }
}
