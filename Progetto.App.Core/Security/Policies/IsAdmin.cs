using Microsoft.AspNetCore.Authorization;
using Progetto.App.Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Progetto.App.Core.Security.Policies;

/// <summary>
/// Requirement for admin users
/// </summary>
public class IsAdmin : IAuthorizationRequirement { }

public class IsAdminAuthorizationHandler : AuthorizationHandler<IsAdmin>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAdmin requirement)
    {
        if (context.User.HasClaim(ClaimName.Role, $"{((int)Role.Admin)}"))
            context.Succeed(requirement);
        
        return Task.FromResult(context);
    }
}
