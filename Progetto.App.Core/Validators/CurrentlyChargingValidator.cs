using FluentValidation;
using Progetto.App.Core.Models;

namespace Progetto.App.Core.Validators;

/// <summary>
/// Rules for CurrentlyCharging entity validation
/// </summary>
public class CurrentlyCharingValidator : AbstractValidator<CurrentlyCharging>
{
    public CurrentlyCharingValidator()
    {
        RuleFor(cc => cc.StartChargingTime).LessThanOrEqualTo(DateTime.Now).NotEmpty();
        RuleFor(cc => cc.EndChargingTime).LessThanOrEqualTo(DateTime.Now).NotEmpty();
        RuleFor(cc => cc.MwBotId).NotEmpty();
        RuleFor(cc => cc.StartChargePercentage).NotEmpty().Must(c => c >= 0 && c <= 100);
        RuleFor(cc => cc.CurrentChargePercentage).NotEmpty().Must(c => c >= 0 && c <= 100);
        RuleFor(cc => cc.TargetChargePercentage).NotEmpty().Must(c => c >= 0 && c <= 100);
        RuleFor(cc => cc.ImmediateRequestId).NotEmpty();
        //RuleFor(cc => cc.UserId).NotEmpty();
        //RuleFor(cc => cc.CarPlate).NotEmpty();
        //RuleFor(cc => cc.ParkingSlotId).NotEmpty();
        //RuleFor(cc => cc.EnergyConsumed).NotEmpty().Must(c => c >= 0);
        //RuleFor(cc => cc.TotalCost).NotEmpty().Must(c => c >= 0);
        //RuleFor(cc => cc.ToPay).NotEmpty();
    }
}
