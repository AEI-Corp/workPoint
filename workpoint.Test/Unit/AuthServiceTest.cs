using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using workpoint.Application.DTOs;
using workpoint.Application.Services;
using workpoint.Domain.Entities;
using workpoint.Domain.Interfaces.Repositories;
using Xunit;
using System.IdentityModel.Tokens.Jwt;

namespace workpoint.Test.Unit;

public class AuthServiceTests
{
    private readonly Mock<IRepository<User>> _mockRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockRepository = new Mock<IRepository<User>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockMapper = new Mock<IMapper>();

        // Configuración NECESARIA para que funcione el JWT (la clave secreta)
        var jwtKey = "EstaClaveDebeSerSuficientementeLargaParaFirmarJWT";
        
        // Simular que Configuration tiene la clave secreta y otros valores
        var mockJwtKeySection = new Mock<IConfigurationSection>();
        mockJwtKeySection.Setup(x => x.Value).Returns(jwtKey);
        
        _mockConfiguration.Setup(c => c.GetSection("Jwt:Key")).Returns(mockJwtKeySection.Object);
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns(jwtKey);
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("workpoint.io");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("workpoint.users");
        
        // Simular que el refresh token dura 7 días
        var mockRefreshTokenDaysSection = new Mock<IConfigurationSection>();
        mockRefreshTokenDaysSection.Setup(x => x.Value).Returns("7");
        _mockConfiguration.Setup(c => c.GetSection("Jwt:RefreshTokenDays")).Returns(mockRefreshTokenDaysSection.Object);

        _authService = new AuthService(
            _mockRepository.Object,
            _mockConfiguration.Object,
            _mockMapper.Object
        );
    }
    
    
    // PRUEBA 1: Registro Exitoso
    [Fact]
    public async Task RegisterAsync_ShouldReturnUserResponseDto_WhenRegistrationIsSuccessful()
    {
        // ARRANGE: Preparar datos
        var registerDto = new RegisterDto 
        { 
            Email = "test@example.com", 
            Password = "Password123", 
            Name = "Test User" 
        };
        
        // Simular: Nadie en la DB tiene ese correo
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());
        
        // Simular: AutoMapper convierte DTO a Entity
        var userEntity = new User { Id = 1, Email = registerDto.Email, Name = registerDto.Name };
        _mockMapper
            .Setup(m => m.Map<User>(registerDto))
            .Returns(userEntity);
        
        // Simular: La DB guarda al usuario
        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns<User>(u => Task.FromResult(u)); 

        // Simular: AutoMapper convierte Entity a Response DTO
        var expectedResponse = new UserRegisterResponseDto { Id = 1, Email = registerDto.Email, Name = registerDto.Name };
        _mockMapper
            .Setup(m => m.Map<UserRegisterResponseDto>(userEntity))
            .Returns(expectedResponse);
        
        // ACT: Llamar a la función
        var result = await _authService.RegisterAsync(registerDto);
        
        // ASSERT: Verificar resultados
        result.Should().NotBeNull();
        
        // Verificar: Se llamó a CreateAsync 1 vez
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        
        // Verificar: El usuario guardado tiene un PasswordHash (se hashea correctamente)
        _mockRepository.Verify(r => r.CreateAsync(
            It.Is<User>(u => !string.IsNullOrEmpty(u.PasswordHash))), Times.Once);
    }


    // PRUEBA 2: Registro Falla porque el usuario ya existe
    [Fact]
    public async Task RegisterAsync_ShouldThrowArgumentException_WhenUserAlreadyExists()
    {
        // ARRANGE: Preparar un usuario que ya existe
        var registerDto = new RegisterDto { Email = "existente@example.com", Password = "Password123" };
        var existingUser = new User { Id = 2, Email = registerDto.Email };
        
        // Simular: El GetAllAsync devuelve al usuario existente
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User> { existingUser });
        
        // ACT & ASSERT: Verificar que lanza ArgumentException
        await _authService.Invoking(s => s.RegisterAsync(registerDto))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Ya existe un usuario con este correo {registerDto.Email}");
        
        // Verificar: El método de guardar NUNCA fue llamado
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }
    
    // PRUEBA 3: Login Exitoso
    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResponseDto_WhenCredentialsAreCorrect()
    {
        // ARRANGE: Preparar credenciales
        var loginDto = new LoginDto { Email = "user@test.com", Password = "CorrectPassword123" };
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(loginDto.Password);
        
        var userEntity = new User { Id = 1, Email = loginDto.Email, PasswordHash = hashedPassword };

        // Simular: El repositorio encuentra al usuario
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User> { userEntity });
        
        // Simular: La respuesta que se mapearía
        var expectedResponse = new UserAuthResponseDto { Token = "fake.jwt.token", RefreshToken = "fake_refresh_token" };
        _mockMapper
            .Setup(m => m.Map<UserAuthResponseDto>(It.IsAny<User>()))
            .Returns(expectedResponse);

        // Simular: La DB guarda el nuevo refresh token (Corrige el error CS1503 si UpdateAsync es 'void')
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        
        // ACT
        var result = await _authService.LoginAsync(loginDto);

        // ASSERT
        result.Should().NotBeNull();
        
        // Verificar: Se llamó a UpdateAsync 1 vez para guardar el Refresh Token
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    }
    
    // PRUEBA 4: Login Falla por credenciales malas
    [Theory]
    [InlineData("nonexistent@test.com", "anypassword")] 
    [InlineData("user@test.com", "WrongPassword123")] 
    public async Task LoginAsync_ShouldThrowSecurityException_WhenCredentialsAreIncorrect(string email, string password)
    {
        // ARRANGE: Preparar DTO con datos malos
        var loginDto = new LoginDto { Email = email, Password = password };
        var existingUser = new User 
        { 
            Email = "user@test.com", 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123")
        };

        // Simular: El repositorio tiene el usuario correcto (pero el loginDto no lo es)
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User> { existingUser });
        
        // ACT & ASSERT: Esperar SecurityException
        await _authService.Invoking(s => s.LoginAsync(loginDto))
            .Should().ThrowAsync<SecurityException>()
            .WithMessage("Credenciales incorrectas");

        // Verificar: No se actualizó la DB
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    // PRUEBA 5: Revoke Exitoso
    [Fact]
    public async Task RevokeAsync_ShouldReturnTrueAndClearToken_WhenUserExists()
    {
        // ARRANGE
        var revokeDto = new RevokeTokenDto { Email = "user@test.com" };
        var userWithToken = new User 
        { 
            Id = 1, 
            Email = revokeDto.Email, 
            RefreshToken = "valid_refresh_token",
            RefreshTokenExpire = DateTime.UtcNow.AddDays(7)
        };

        // Simular: El repositorio encuentra al usuario
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User> { userWithToken });
        
        // Simular: La DB guarda la actualización
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // ACT
        var result = await _authService.RevokeAsync(revokeDto);

        // ASSERT
        result.Should().BeTrue();
        
        // Verificar: Se llamó a UpdateAsync con el RefreshToken = null
        _mockRepository.Verify(r => r.UpdateAsync(
            It.Is<User>(u => u.RefreshToken == null && u.RefreshTokenExpire <= DateTime.UtcNow)), Times.Once);
    }

    // PRUEBA 6: Revoke Falla cuando el usuario no existe
    [Fact]
    public async Task RevokeAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // ARRANGE
        var revokeDto = new RevokeTokenDto { Email = "nonexistent@test.com" };
        
        // Simular: El repositorio devuelve una lista vacía
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        // ACT
        var result = await _authService.RevokeAsync(revokeDto);

        // ASSERT
        result.Should().BeFalse();
        
        // Verificar: NUNCA se llama a la actualización
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    // PRUEBA 7: Revoke Falla cuando el DTO es nulo o vacío
    [Fact]
    public async Task RevokeAsync_ShouldThrowArgumentException_WhenDtoIsInvalid()
    {
        // ARRANGE
        // Uso de string.Empty en lugar de null para evitar la advertencia CS8625
        var invalidDto = new RevokeTokenDto { Email = string.Empty }; 
        
        // ACT & ASSERT
        await _authService.Invoking(s => s.RevokeAsync(invalidDto))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email es requerido.");

        // Verificar que la búsqueda nunca fue llamada
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Never);
    }
}