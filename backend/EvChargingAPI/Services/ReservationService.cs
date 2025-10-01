/*
 * File: ReservationService.cs
 * Description: Implements business rules for reservations (7-day window, 12-hour update/cancel rule)
 */
using EvChargingAPI.DTOs;
using EvChargingAPI.Models;
using EvChargingAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvChargingAPI.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _repo;
        // If you have a Station repository/service, inject it here to verify station existence/slots
        // private readonly IStationRepository _stationRepo;

        public ReservationService(IReservationRepository repo /*, IStationRepository stationRepo */)
        {
            _repo = repo;
            // _stationRepo = stationRepo;
        }

        // Purpose: Create a reservation after validating rules:
        //  - reservation time must be within next 7 days
        //  - slot must not be already booked (Pending/Confirmed)
        public async Task<Reservation> CreateReservationAsync(string ownerId, CreateReservationDto dto)
        {
            // validate reservation window: within 7 days from now
            var now = DateTime.UtcNow;
            if (dto.ReservationTimeUtc < now)
                throw new InvalidOperationException("Reservation time cannot be in the past.");

            if (dto.ReservationTimeUtc > now.AddDays(7))
                throw new InvalidOperationException("Reservation time must be within 7 days from now.");

            // TODO: optionally verify station exists and slotId valid via stationRepo

            // Check conflict: exact same slot/time at station
            var conflict = await _repo.ExistsConflictAsync(dto.StationId, dto.SlotId, dto.ReservationTimeUtc);
            if (conflict)
                throw new InvalidOperationException("Selected slot is already booked for that time.");

            var reservation = new Reservation
            {
                OwnerId = ownerId,
                StationId = dto.StationId,
                SlotId = dto.SlotId,
                ReservationTimeUtc = dto.ReservationTimeUtc,
                Status = "Pending",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await _repo.CreateAsync(reservation);
            return reservation;
        }

        // Purpose: get reservation by id (used by controllers)
        public async Task<Reservation?> GetByIdAsync(string reservationId)
        {
            return await _repo.GetByIdAsync(reservationId);
        }

        // Purpose: list reservations for an owner
        public async Task<IEnumerable<Reservation>> GetByOwnerAsync(string ownerId)
        {
            return await _repo.GetByOwnerAsync(ownerId);
        }

        // Purpose: list reservations for a station (operator UI)
        public async Task<IEnumerable<Reservation>> GetByStationAsync(string stationId)
        {
            return await _repo.GetByStationAsync(stationId);
        }

        // Purpose: update reservation time (reschedule) with 12-hour rule
        public async Task<Reservation> UpdateReservationAsync(string ownerId, string reservationId, UpdateReservationDto dto)
        {
            var existing = await _repo.GetByIdAsync(reservationId);
            if (existing == null)
                throw new KeyNotFoundException("Reservation not found.");

            // only owner or admin should update — check owner matches
            if (existing.OwnerId != ownerId)
                throw new UnauthorizedAccessException("You can only update your own reservations.");

            // 12-hour cutoff check
            var now = DateTime.UtcNow;
            if (existing.ReservationTimeUtc <= now.AddHours(12))
                throw new InvalidOperationException("Cannot update reservation less than 12 hours before start time.");

            // new time must be within 7 days
            if (dto.ReservationTimeUtc < now || dto.ReservationTimeUtc > now.AddDays(7))
                throw new InvalidOperationException("New reservation time must be within next 7 days.");

            // check conflict at new time
            var conflict = await _repo.ExistsConflictAsync(existing.StationId, existing.SlotId, dto.ReservationTimeUtc);
            if (conflict)
                throw new InvalidOperationException("Selected slot is already booked for the new time.");

            existing.ReservationTimeUtc = dto.ReservationTimeUtc;
            existing.UpdatedAtUtc = DateTime.UtcNow;
            existing.Status = "Pending"; // keep Pending after reschedule
            await _repo.UpdateAsync(existing);
            return existing;
        }

        // Purpose: Cancel a reservation (respect 12-hour cutoff)
        public async Task CancelReservationAsync(string ownerId, string reservationId)
        {
            var existing = await _repo.GetByIdAsync(reservationId);
            if (existing == null)
                throw new KeyNotFoundException("Reservation not found.");

            if (existing.OwnerId != ownerId)
                throw new UnauthorizedAccessException("You can only cancel your own reservations.");

            var now = DateTime.UtcNow;
            if (existing.ReservationTimeUtc <= now.AddHours(12))
                throw new InvalidOperationException("Cannot cancel reservation less than 12 hours before start time.");

            existing.Status = "Cancelled";
            existing.UpdatedAtUtc = DateTime.UtcNow;
            await _repo.UpdateAsync(existing);
        }
    }
}
