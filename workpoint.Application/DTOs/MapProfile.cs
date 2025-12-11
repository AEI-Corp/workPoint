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
        CreateMap<CreateBookingDto, Booking>()
            .ForMember(dest => dest.StartHour, opt => opt.MapFrom(src => src.Start))
            .ForMember(dest => dest.EndHour,   opt => opt.MapFrom(src => src.End));

        CreateMap<Booking, BookingListItemDto>()
            .ForMember(dest => dest.SpaceId, opt => opt.MapFrom(src => src.SpaceId ?? 0))
            .ForMember(dest => dest.Start,   opt => opt.MapFrom(src => src.StartHour))
            .ForMember(dest => dest.End,     opt => opt.MapFrom(src => src.EndHour));
        // CreateMap<ResponseSpaceDto, SpaceCreateDto>();
        // CreateMap<SpaceCreateDto, ResponseSpaceDto>();

    }
}