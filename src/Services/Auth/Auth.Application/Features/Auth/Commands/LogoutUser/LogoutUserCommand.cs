using MediatR;

namespace Auth.Application.Features.Auth.Commands.LogoutUser;

public class LogoutUserCommand : IRequest<bool>
{
    public string AccessToken { get; set; } = string.Empty;
}