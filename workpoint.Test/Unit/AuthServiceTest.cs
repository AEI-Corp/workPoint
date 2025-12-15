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

        // Configuration required for the JWT (secret key) to work
        var jwtKey = "EstaClaveDebeSerSuficientementeLargaParaFirmarJWT";
        
        // Simulate that Configuration has the secret key and other values
        var mockJwtKeySection = new Mock<IConfigurationSection>();
        mockJwtKeySection.Setup(x => x.Value).Returns(jwtKey);
        
        _mockConfiguration.Setup(c => c.GetSection("Jwt:Key")).Returns(mockJwtKeySection.Object);
        _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns(jwtKey);
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("workpoint.io");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("workpoint.users");
        
        // Simulate that the refresh token lasts 7 days
        var mockRefreshTokenDaysSection = new Mock<IConfigurationSection>();
        mockRefreshTokenDaysSection.Setup(x => x.Value).Returns("7");
        _mockConfiguration.Setup(c => c.GetSection("Jwt:RefreshTokenDays")).Returns(mockRefreshTokenDaysSection.Object);

        _authService = new AuthService(
            _mockRepository.Object,
            _mockConfiguration.Object,
            _mockMapper.Object
        );
    }
    
    
    // TEST 1: Successful Registration
    [Fact]
    public async Task RegisterAsync_ShouldReturnUserResponseDto_WhenRegistrationIsSuccessful()
    {
        // ARRANGE: preparing data
        var registerDto = new RegisterDto 
        { 
            Email = "test@example.com", 
            Password = "Password123", 
            Name = "Test User" 
        };
        
        // Simulate: Nobody in the database has that email address
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());
        
        // Simulate: AutoMapper converts DTO to Entity
        var userEntity = new User { Id = 1, Email = registerDto.Email, Name = registerDto.Name };
        _mockMapper
            .Setup(m => m.Map<User>(registerDto))
            .Returns(userEntity);
        
        // Simulate: The DB saves the user
        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns<User>(u => Task.FromResult(u)); 

        // Simulate: AutoMapper converts Entity to Response DTO
        var expectedResponse = new UserRegisterResponseDto { Id = 1, Email = registerDto.Email, Name = registerDto.Name };
        _mockMapper
            .Setup(m => m.Map<UserRegisterResponseDto>(userEntity))
            .Returns(expectedResponse);
        
        // ACT: Call the function
        var result = await _authService.RegisterAsync(registerDto);
        
        // ASSERT: Check results
        result.Should().NotBeNull();
        
        // Verify: CreateAsync was called 1 time
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        
        // Verify: The saved user has a PasswordHash (hashed correctly)
        _mockRepository.Verify(r => r.CreateAsync(
            It.Is<User>(u => !string.IsNullOrEmpty(u.PasswordHash))), Times.Once);
    }


    // TEST 2: Registration Fails because the user already exists
    [Fact]
    public async Task RegisterAsync_ShouldThrowArgumentException_WhenUserAlreadyExists()
    {
        // ARRANGE: Prepare an existing user
        var registerDto = new RegisterDto { Email = "existente@example.com", Password = "Password123" };
        var existingUser = new User { Id = 2, Email = registerDto.Email };
        
        // Simular: GetAllAsync returns the existing user
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User> { existingUser });
        
        // ACT & ASSERT: Verify that it throws ArgumentException
        await _authService.Invoking(s => s.RegisterAsync(registerDto))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Ya existe un usuario con este correo {registerDto.Email}");
        
        // Verify: The save method was NEVER called
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }
    
    // TEST 3: Successful Login
    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResponseDto_WhenCredentialsAreCorrect()
    {
        // ARRANGE: Prepare credentials
        var loginDto = new LoginDto { Email = "user@test.com", Password = "CorrectPassword123" };
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(loginDto.Password);
        
        var userEntity = new User { Id = 1, Email = loginDto.Email, PasswordHash = hashedPassword };

        // Simulate: The repository finds the user
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User> { userEntity });
        
        // Simulate: The response that would be mapped
        var expectedResponse = new UserAuthResponseDto { Token = "fake.jwt.token", RefreshToken = "fake_refresh_token" };
        _mockMapper
            .Setup(m => m.Map<UserAuthResponseDto>(It.IsAny<User>()))
            .Returns(expectedResponse);

        // Simulate: The DB saves the new refresh token (Corrects error CS1503 if UpdateAsync is 'void')
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        
        // ACT
        var result = await _authService.LoginAsync(loginDto);

        // ASSERT
        result.Should().NotBeNull();
        
        // Verification: UpdateAsync was called once to save the Refresh Token
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    }
    
    // TEST 4: Login Fails due to bad credentials
    [Theory]
    [InlineData("nonexistent@test.com", "anypassword")] 
    [InlineData("user@test.com", "WrongPassword123")] 
    public async Task LoginAsync_ShouldThrowSecurityException_WhenCredentialsAreIncorrect(string email, string password)
    {
        // ARRANGE: Prepare DTO with bad data
        var loginDto = new LoginDto { Email = email, Password = password };
        var existingUser = new User 
        { 
            Email = "user@test.com", 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123")
        };

        // Simulate: The repository has the correct user (but the loginDto is not)
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User> { existingUser });
        
        // ACT & ASSERT: Waiting SecurityException
        await _authService.Invoking(s => s.LoginAsync(loginDto))
            .Should().ThrowAsync<SecurityException>()
            .WithMessage("Credenciales incorrectas");

        // Check: The database was not updated
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    // TEST 5: Successful Revoke
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

        // Simulate: The repository finds the user
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User> { userWithToken });
        
        // Simulate: The DB saves the update
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        // ACT
        var result = await _authService.RevokeAsync(revokeDto);

        // ASSERT
        result.Should().BeTrue();
        
        //Verify: UpdateAsync was called with RefreshToken = null
        _mockRepository.Verify(r => r.UpdateAsync(
            It.Is<User>(u => u.RefreshToken == null && u.RefreshTokenExpire <= DateTime.UtcNow)), Times.Once);
    }

    //TEST 6: Revoke Fails when user does not exist
    [Fact]
    public async Task RevokeAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // ARRANGE
        var revokeDto = new RevokeTokenDto { Email = "nonexistent@test.com" };
        
        // Simulate: The repository returns an empty list
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        // ACT
        var result = await _authService.RevokeAsync(revokeDto);

        // ASSERT
        result.Should().BeFalse();
        
        // Verify: The update is NEVER called
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    // TEST 7: Revoke Fails when the DTO is null or empty
    [Fact]
    public async Task RevokeAsync_ShouldThrowArgumentException_WhenDtoIsInvalid()
    {
        // ARRANGE
        // Using string.Empty instead of null to avoid the CS8625 warning
        var invalidDto = new RevokeTokenDto { Email = string.Empty }; 
        
        // ACT & ASSERT
        await _authService.Invoking(s => s.RevokeAsync(invalidDto))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email es requerido.");

        // Verify that the search was never called
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Never);
    }
}