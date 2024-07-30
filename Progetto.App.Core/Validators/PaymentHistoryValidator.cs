using FluentValidation;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.Validators;

/// <summary>
/// Rules for PaymentHistory entity validation
/// </summary>
public class PaymentHistoryValidator : AbstractValidator<PaymentHistory>
{
    public PaymentHistoryValidator()
    {
        RuleFor(x => x.StartTime).NotNull();
        RuleFor(x => x.EndTime).NotNull();
        RuleFor(x => x.UserId).NotNull().NotEmpty();
        RuleFor(x => x.CarPlate).NotNull().NotEmpty();
        RuleFor(x => x.TotalCost).NotNull().NotEmpty();
        RuleFor(x => x.IsCharge).NotNull().NotEmpty();
    }
}
