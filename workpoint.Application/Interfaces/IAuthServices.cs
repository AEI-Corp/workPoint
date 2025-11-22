
using workpoint.Application.DTOs;
using workpoint.Domain.Entities;

namespace workpoint.Application.Interfaces;

public interface IAuthServices
{
    Task<UserRegisterResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<UserAuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<UserAuthResponseDto> RefreshAsync(RefreshDto refreshDto);
    Task<bool> RevokeAsync(RevokeTokenDto revokeTokenDto);
}