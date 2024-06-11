using FluentValidation;
using Progetto.App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Validators;

/// <summary>
/// Rules for Reservation entity validation
/// </summary>
public class ReservationValidator : AbstractValidator<Reservation>
{
    public ReservationValidator()
    {
        RuleFor(r => r.RequestDate)
            .NotEmpty().WithMessage("Request Date is required")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Request Date must be in the past");
        RuleFor(r => r.RequestedChargeLevel)
            .NotEmpty().WithMessage("Charge level is required")
            .Must(c => c >= 0 && c <= 100).WithMessage("Charge level must be between 0 and 100");
        RuleFor(r => r.UserId)
            .NotEmpty().WithMessage("User is required");
        RuleFor(r => r.ParkingSlotId)
            .NotEmpty().WithMessage("Parking Slot is required");
    }
}
