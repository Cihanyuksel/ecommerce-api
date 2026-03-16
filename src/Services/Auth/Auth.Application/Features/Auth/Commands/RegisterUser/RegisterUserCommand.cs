using MediatR;

namespace Auth.Application.Features.Auth.Commands.RegisterUser;

public class RegisterUserCommand : IRequest<string>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}