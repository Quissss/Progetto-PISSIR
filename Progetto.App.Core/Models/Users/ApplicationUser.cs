using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Models.Users;

public class ApplicationUser : IdentityUser
{
    public string? TelegramUsername { get; set; }
}
