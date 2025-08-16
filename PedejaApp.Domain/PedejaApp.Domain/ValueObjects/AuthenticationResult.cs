namespace PedejaApp.Domain.ValueObjects;

public class AuthenticationResult
{
    public bool IsSuccess { get; init; }
    public string Token { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public IEnumerable<string> Roles { get; init; } = new List<string>();
    public string ErrorMessage { get; init; } = string.Empty;

    public static AuthenticationResult Success(
        string token, 
        string refreshToken, 
        DateTime expiresAt, 
        string userId, 
        string userName, 
        string email, 
        IEnumerable<string> roles)
    {
        return new AuthenticationResult
        {
            IsSuccess = true,
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            UserId = userId,
            UserName = userName,
            Email = email,
            Roles = roles
        };
    }

    public static AuthenticationResult Failure(string errorMessage)
    {
        return new AuthenticationResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
