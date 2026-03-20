using System.Security.Claims;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Features.Auth.Commands.LogoutUser;

public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, bool>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;

    public LogoutUserCommandHandler(UserManager<AppUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<bool> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var email = principal.FindFirstValue(ClaimTypes.Email);

        if (email == null)
            throw new UnauthorizedException("Geçersiz Access Token!");

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            throw new UnauthorizedException("Kullanıcı bulunamadı!");

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = DateTime.MinValue;
        await _userManager.UpdateAsync(user);

        return true;
    }
}