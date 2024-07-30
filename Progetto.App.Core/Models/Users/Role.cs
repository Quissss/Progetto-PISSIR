namespace Progetto.App.Core.Models.Users;

public enum Role
{
    Admin,
    PremiumUser,
    User
}

/// <summary>
/// User claim names
/// </summary>
public static class ClaimName
{
    public const string Role = "Role";

    public const string FirstName = "FirstName";

    public const string LastName = "LastName";
}
