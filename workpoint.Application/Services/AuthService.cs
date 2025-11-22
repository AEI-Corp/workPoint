using AutoMapper;
using Microsoft.Extensions.Configuration;
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
    

    public Task<UserRegisterResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        throw new NotImplementedException();
    }

    public Task<UserAuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        throw new NotImplementedException();
    }

    public Task<UserAuthResponseDto> RefreshAsync(RefreshDto refreshDto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RevokeAsync(RevokeTokenDto revokeTokenDto)
    {
        throw new NotImplementedException();
    }
}