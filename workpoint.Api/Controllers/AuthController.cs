using Microsoft.AspNetCore.Mvc;
using workpoint.Application.DTOs;
using workpoint.Application.Interfaces;

namespace workpoint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServices _authService;

    public AuthController(IAuthServices authService)
    {
        _authService = authService;
    }

    // --------------------------------------------------------------
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var result = await _authService.LoginAsync(request);

        if (result == null)
            return Unauthorized("Credenciales incorrectas.");

        return Ok(result); // <-- Debe devolver UserAuthResponseDto
    }
    
    

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result == null)
            return BadRequest("No se pudo registrar el usuario.");

        return Ok(result); // <-- Debe devolver UserRegisterResponseDto
    }

    
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshDto request)
    {
        var result = await _authService.RefreshAsync(request);

        if (result == null)
            return Unauthorized("Refresh Token inválido.");

        return Ok(result); // <-- Debe devolver UserAuthResponseDto de nuevo
    }

    
    
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenDto request)
    {
        var result = await _authService.RevokeAsync(request);

        if (!result)
            return BadRequest("No se pudo revocar el token.");

        return Ok("Token revocado correctamente.");
    }
}