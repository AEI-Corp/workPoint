using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Experimental;
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

        //TODO:
        _repository.CreateAsync();
        
        return new UserRegisterResponseDto
        {
            Name = registerDto.Name,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            UserName = registerDto.UserName,
            NumDocument = registerDto.NumDocument,
            Role = registerDto.Role,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public async Task<UserAuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var users = await _repository.GetAllAsync();
        var exist = users.FirstOrDefault(user => user.Email == loginDto.Email);

        if (exist == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, exist.PasswordHash))
            throw new SecurityException("Credenciales incorrectas");

        return GenerateTokens(exist);
    }

    // TODO:
    public Task<UserAuthResponseDto> RefreshAsync(RefreshDto refreshDto)
    {
        
    }

    
    // TODO:
    public Task<bool> RevokeAsync(RevokeTokenDto revokeTokenDto)
    {
        
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
            new Claim(ClaimTypes.Role, user.RoleID),
            new Claim(ClaimTypes.Email, user.Email)
        };

        // Generamos elJwt
        return new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims:claims,
            expires: DateTime.UtcNow.AddMinutes(20),
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

    private ClaimsPrincipal getPrincipalFromExpire(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]))
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, parameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
            throw new SecurityException("El token no es valido");

        return principal;
    }

    private UserAuthResponseDto GenerateTokens(User user)
    {
        var token = GenerateToken(user);
        var refresh = GenerateRefresh();

        return new UserAuthResponseDto()
        {
            Id = user.Id,
            Name = user.Name,
            LastName = user.LastName,
            Email = user.Email,
            UserName = user.UserName,
            NumDocument = user.NumDocument,
            Role = user.RoleID,
            Token = token.ToString(),
            UpdatedAt = DateTime.UtcNow
        };
    }
}