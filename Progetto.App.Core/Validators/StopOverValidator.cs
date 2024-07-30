using FluentValidation;
using Progetto.App.Core.Models;

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
