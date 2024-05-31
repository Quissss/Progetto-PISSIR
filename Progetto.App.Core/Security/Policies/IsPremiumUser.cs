using Microsoft.AspNetCore.Authorization;
using Progetto.App.Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Security.Policies
{
    public class IsPremiumUser : IAuthorizationRequirement { }

    public class IsPremiumUserAuthorizationHandler : AuthorizationHandler<IsPremiumUser>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsPremiumUser requirement)
        {
            if (context.User.HasClaim(ClaimName.Role, $"{Role.PremiumUser}"))
                context.Succeed(requirement);

            return Task.FromResult(context);
        }
    }
}
