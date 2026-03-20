using System.Security.Claims;
using Auth.Application.Features.Auth.Commands.LoginUser;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(UserManager<AppUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var email = principal.FindFirstValue(ClaimTypes.Email);

        if (email == null)
            throw new UnauthorizedException("Geçersiz Access Token!");

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedException("Geçersiz veya süresi dolmuş Refresh Token! Lütfen tekrar giriş yapın.");

        var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new AuthResponse(newAccessToken, newRefreshToken);
    }
}