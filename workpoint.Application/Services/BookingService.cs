using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using workpoint.Application.DTOs;
using workpoint.Application.Interfaces;
using workpoint.Domain.Entities;
using workpoint.Domain.Interfaces.Repositories;

namespace workpoint.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;

        public BookingService(IBookingRepository bookingRepository, IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
        }

        public async Task<DailyAvailabilityDto> GetDailyAvailabilityAsync(
            int spaceId,
            DateTime date,
            int slotMinutes)
        {
           
            var bookings = await _bookingRepository.GetBySpaceAndDateAsync(spaceId, date);

            
            var dayStart = date.Date;
            var dayEnd   = date.Date.AddHours(1);

            var response = new DailyAvailabilityDto
            {
                SpaceId = spaceId,
                Date = date.Date,
                SlotMinutes = slotMinutes,
                Slots = new List<TimeSlotDto>()
            };

            for (var start = dayStart; start < dayEnd; start = start.AddMinutes(slotMinutes))
            {
                var end = start.AddMinutes(slotMinutes);

                // Verificamos si este bloque se cruza con alguna reserva
                var reserved = bookings.Any(b =>
                    b.StartHour < end &&
                    start < b.EndHour);

                response.Slots.Add(new TimeSlotDto
                {
                    Start = start,
                    End = end,
                    Status = reserved ? "reserved" : "free"
                });
            }

            return response;
        }

        public async Task<int> CreateBookingAsync(CreateBookingDto dto)
        {
            if (dto.Start >= dto.End)
                throw new ArgumentException("La hora de inicio debe ser menor a la hora final.");

            var hasConflict = await _bookingRepository.HasConflictAsync(
                dto.SpaceId, dto.Start, dto.End);

            if (hasConflict)
                throw new InvalidOperationException("Ya existe una reserva en ese horario.");

            var booking = _mapper.Map<Booking>(dto);
            booking.Available = true;
            booking.CreatedAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookingRepository.AddAsync(booking);

            return booking.Id;
        }

        public async Task CancelBookingAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id)
                          ?? throw new KeyNotFoundException("Reserva no encontrada");

            booking.Available = false;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookingRepository.UpdateAsync(booking);
        }

        public async Task<List<BookingListItemDto>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();

            return _mapper.Map<List<BookingListItemDto>>(bookings);
        }

        public async Task<BookingListItemDto?> GetBookingByIdAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return null;

            return _mapper.Map<BookingListItemDto>(booking);        
        }

        public async Task UpdateBookingAsync(int id, UpdateBookingDto dto)
        {
            if (dto.Start >= dto.End)
                throw new ArgumentException("La hora de inicio debe ser menor a la hora final.");

            var booking = await _bookingRepository.GetByIdAsync(id)
                          ?? throw new KeyNotFoundException("Reserva no encontrada");

            // Verificar si hay conflicto con otras reservas
            var hasConflict = await _bookingRepository.HasConflictForUpdateAsync(
                bookingId: id,
                spaceId: dto.SpaceId,
                start: dto.Start,
                end: dto.End);

            if (hasConflict)
                throw new InvalidOperationException("Ya existe una reserva en ese horario para ese espacio.");

            // Actualizamos
            booking.SpaceId   = dto.SpaceId;
            booking.StartHour = dto.Start;
            booking.EndHour   = dto.End;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookingRepository.UpdateAsync(booking);        }
    }
}
