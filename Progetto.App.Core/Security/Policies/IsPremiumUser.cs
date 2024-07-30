using Microsoft.AspNetCore.Authorization;
using Progetto.App.Core.Models.Users;

namespace Progetto.App.Core.Security.Policies;

/// <summary>
/// Requirement for premium users
/// </summary>
public class IsPremiumUser : IAuthorizationRequirement { }

public class IsPremiumUserAuthorizationHandler : AuthorizationHandler<IsPremiumUser>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsPremiumUser requirement)
    {
        if (context.User.HasClaim(ClaimName.Role, $"{((int)Role.PremiumUser)}"))
            context.Succeed(requirement);

        if (context.User.HasClaim(ClaimName.Role, $"{((int)Role.Admin)}"))
            context.Succeed(requirement);

        return Task.FromResult(context);
    }
}
