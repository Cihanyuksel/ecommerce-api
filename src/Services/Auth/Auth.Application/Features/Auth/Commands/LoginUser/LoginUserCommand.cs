using MediatR;

namespace Auth.Application.Features.Auth.Commands.LoginUser;

public class LoginUserCommand : IRequest<AuthResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public record AuthResponse(string AccessToken, string RefreshToken);