using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using workpoint.Application.DTOs;
using workpoint.Application.Interfaces;
using workpoint.Domain.Entities;
using workpoint.Domain.Interfaces.Repositories;


namespace workpoint.Application.Services;

public class AuthService : IAuthServices
{
    private readonly IRepository<User> _repository;
    private readonly IConfiguration _config;
    private readonly IMapper _mapper;

    
    // Duraciones configurables
    private readonly int _jwtMinutes = 60;
    private readonly int _refreshTokenDays = 7;
    
    
    public AuthService(IRepository<User> repository, IConfiguration config, IMapper mapper)
    {
        _repository = repository;
        _config = config;
        _mapper = mapper;
    }
    

    public async Task<UserRegisterResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        if (registerDto == null)
            throw new ArgumentNullException("El cuerpo de la peticion no puede estar vacio");

        var users = await _repository.GetAllAsync();
        var exist = users.FirstOrDefault(user => user.Email == registerDto.Email);
        
        if(exist != null) 
            throw new ArgumentException($"Ya existe un usuario con este correo {registerDto.Email}");

        var user = _mapper.Map<User>(registerDto);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _repository.CreateAsync(user);

        return _mapper.Map<UserRegisterResponseDto>(user);
    }

    public async Task<UserAuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var users = await _repository.GetAllAsync();
        var exist = users.FirstOrDefault(user => user.Email == loginDto.Email);

        if (exist == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, exist.PasswordHash))
            throw new SecurityException("Credenciales incorrectas");

        // Genera token + refresh y guarda el refresh en DB
        return await GenerateTokensAsync(exist);
    }

    
    public async Task<UserAuthResponseDto> RefreshAsync(RefreshDto refreshDto)
    {
        if (refreshDto == null || string.IsNullOrEmpty(refreshDto.Token) ||
            string.IsNullOrEmpty(refreshDto.RefreshToken))
            throw new ArgumentException("Token y refreshToken son requeridos.");
        
        // 1) Obtener claims aun cuando el token esté expirado.
        var principal = getPrincipalFromExpireToken(refreshDto.Token);
        // TODO: 
        var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!int.TryParse(userIdClaim, out var userId))
            throw new SecurityException("Token invalido");
        
        var user = await _repository.GetByIdAsync(userId);
        if (user == null)
            throw new SecurityException("Usuario no encontrado");

        // 2) Verificar que el refresh token coincide y no esté expirado
        if (user.RefreshToken != refreshDto.RefreshToken || user.RefreshTokenExpire <= DateTime.UtcNow)
            throw new SecurityException("Refresh token invalido o expirado");

        // 3) Generar nuevos tokens y guardar el nuevo refresh token
        return await GenerateTokensAsync(user);
    }

    
    
    public async Task<bool> RevokeAsync(RevokeTokenDto revokeTokenDto)
    {
        if(revokeTokenDto == null || string.IsNullOrEmpty(revokeTokenDto.Email))
            throw new ArgumentException("Email es requerido.");

        var users = await _repository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Email == revokeTokenDto.Email);
        if (user == null) return false;
        
        
        // Invalidate refresh token
        user.RefreshToken = null;
        user.RefreshTokenExpire = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(user);
        return true;
    }

    // Json Web Token
    private JwtSecurityToken GenerateToken(User user)
    {
        // 1. Key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        
        // 2. Algoritmo
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        // 3. Claims
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.RoleId.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        // Generamos elJwt
        return new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims:claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtMinutes),
            signingCredentials:credentials
            );
    }

    // RefreshToken
    private string GenerateRefresh()
    {
        var array = new byte[32];
        using var rgn = RandomNumberGenerator.Create();
        rgn.GetBytes(array);
        return Convert.ToBase64String(array);
    }

    private ClaimsPrincipal getPrincipalFromExpireToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
            ValidateLifetime = false
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,  StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityException("El token no es valido");

        return principal;
    }

    private async Task<UserAuthResponseDto> GenerateTokensAsync(User user)
    {
        var jwtToken = GenerateToken(user);
        var refresh =  GenerateRefresh();

        user.RefreshToken = refresh;
        user.RefreshTokenExpire = DateTime.UtcNow.AddDays(7);
        await _repository.UpdateAsync(user);

        var response = _mapper.Map<UserAuthResponseDto>(user);

        var handelr = new JwtSecurityTokenHandler();
        response.Token = handelr.WriteToken(jwtToken);
        
        return response;
    }
    
}