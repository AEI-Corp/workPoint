using AutoMapper;
using workpoint.Domain.Entities;

namespace workpoint.Application.DTOs;

public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<RegisterDto, User>();
        CreateMap<User, RegisterDto>();
        
        CreateMap<User, UserRegisterResponseDto>();
        CreateMap<UserRegisterResponseDto, User>();
        
        CreateMap<UserAuthResponseDto, User>();
        CreateMap<User, UserAuthResponseDto>();

        CreateMap<SpaceCreateDto, Space>();
        CreateMap<Space, SpaceCreateDto>();
        CreateMap<Space, ResponseSpaceDto>();

        CreateMap<ResponseSpaceDto, SpaceCreateDto>();
        CreateMap<SpaceCreateDto, ResponseSpaceDto>();

    }
}