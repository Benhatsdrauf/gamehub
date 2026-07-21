using GameHub.Application.Common.Errors;

namespace GameHub.Application.Authentication;

public static class AuthErrors
{
    // Deliberately vague: the SAME error whether the email is unknown or the
    // password is wrong. Revealing "no such email" lets an attacker enumerate
    // which accounts exist. Maps to 401 via ApiController.Problem.
    public static Error InvalidCredentials => new(
        "Auth.InvalidCredentials",
        "The email or password is incorrect.",
        ErrorType.Unauthorized);
}
