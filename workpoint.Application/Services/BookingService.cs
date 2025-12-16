using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
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
        private readonly IWebhookService _webhookService; // ← Added
        private readonly ILogger<BookingService> _logger; // ← Added

        public BookingService(
            IBookingRepository bookingRepository, 
            IMapper mapper,
            IWebhookService webhookService, // ← Added
            ILogger<BookingService> logger) // ← Added
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
            _webhookService = webhookService; // ← Added
            _logger = logger; // ← Added
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

            // Publish event RABBITMQ
            try
            {
                await _webhookService.SendWebhookAsync("booking.created", new
                {
                    bookingId = booking.Id,
                    userId = booking.UserId,
                    spaceId = booking.SpaceId,
                    startHour = booking.StartHour,
                    endHour = booking.EndHour,
                    available = booking.Available,
                    createdAt = booking.CreatedAt
                });

                _logger.LogInformation("Evento booking.created publicado para reserva {BookingId}", booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publicando webhook para reserva {BookingId}", booking.Id);
                // Do not throw exception to do not affect a new booking
            }

            return booking.Id;
        }

        public async Task CancelBookingAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id)
                          ?? throw new KeyNotFoundException("Reserva no encontrada");

            booking.Available = false;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookingRepository.UpdateAsync(booking);

            // Publish event RABBITMQ
            try
            {
                await _webhookService.SendWebhookAsync("booking.cancelled", new
                {
                    bookingId = booking.Id,
                    userId = booking.UserId,
                    spaceId = booking.SpaceId,
                    cancelledAt = booking.UpdatedAt
                });

                _logger.LogInformation("Evento booking.cancelled publicado para reserva {BookingId}", booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publicando webhook para cancelación de reserva {BookingId}", booking.Id);
            }
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

            var hasConflict = await _bookingRepository.HasConflictForUpdateAsync(
                bookingId: id,
                spaceId: dto.SpaceId,
                start: dto.Start,
                end: dto.End);

            if (hasConflict)
                throw new InvalidOperationException("Ya existe una reserva en ese horario para ese espacio.");

            booking.SpaceId   = dto.SpaceId;
            booking.StartHour = dto.Start;
            booking.EndHour   = dto.End;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookingRepository.UpdateAsync(booking);

            // Publish event RABBITMQ
            try
            {
                await _webhookService.SendWebhookAsync("booking.updated", new
                {
                    bookingId = booking.Id,
                    userId = booking.UserId,
                    spaceId = booking.SpaceId,
                    startHour = booking.StartHour,
                    endHour = booking.EndHour,
                    updatedAt = booking.UpdatedAt
                });

                _logger.LogInformation("Evento booking.updated publicado para reserva {BookingId}", booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publicando webhook para actualización de reserva {BookingId}", booking.Id);
            }
        }
    }
}