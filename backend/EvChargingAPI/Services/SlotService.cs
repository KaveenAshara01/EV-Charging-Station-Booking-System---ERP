/*
 * File: SlotService.cs
 * Description: Implements business logic for slots (create, update, delete, queries)
 */
using EvChargingAPI.DTOs;
using EvChargingAPI.Models;
using EvChargingAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvChargingAPI.Services
{
    public class SlotService : ISlotService
    {
        private readonly ISlotRepository _repo;

        public SlotService(ISlotRepository repo)
        {
            _repo = repo;
        }

        public async Task<Slot> CreateSlotAsync(CreateSlotDto dto)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(dto.StationId))
                throw new ArgumentException("StationId is required.");

            if (dto.EndUtc <= dto.StartUtc)
                throw new ArgumentException("EndUtc must be after StartUtc.");

            // Enforce 7-day future window: only allow creating slots up to 7 days ahead
            var now = DateTime.UtcNow;
            if (dto.StartUtc < now)
                throw new InvalidOperationException("Cannot create a slot in the past.");
            if (dto.StartUtc > now.AddDays(7))
                throw new InvalidOperationException("Cannot create slots beyond 7 days from now.");

            // Check overlap
            var overlap = await _repo.ExistsOverlapAsync(dto.StationId, dto.StartUtc, dto.EndUtc);
            if (overlap)
                throw new InvalidOperationException("Slot overlaps with existing slot.");

            var slot = new Slot
            {
                StationId = dto.StationId,
                StartUtc = dto.StartUtc,
                EndUtc = dto.EndUtc,
                Label = dto.Label,
                IsAvailable = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await _repo.CreateAsync(slot);
            return slot;
        }

        public async Task<Slot?> GetByIdAsync(string slotId) => await _repo.GetByIdAsync(slotId);

        public async Task<IEnumerable<Slot>> GetSlotsForStationAsync(string stationId, DateTime fromUtc, DateTime toUtc)
            => await _repo.GetByStationAndWindowAsync(stationId, fromUtc, toUtc);

        public async Task<IEnumerable<Slot>> GetAvailableSlotsForStationAsync(string stationId, DateTime fromUtc, DateTime toUtc)
            => await _repo.GetAvailableByStationAndWindowAsync(stationId, fromUtc, toUtc);

        public async Task<Slot> UpdateSlotAsync(string slotId, CreateSlotDto dto)
        {
            var existing = await _repo.GetByIdAsync(slotId);
            if (existing == null) throw new KeyNotFoundException("Slot not found.");

            // cannot update if reserved
            if (!existing.IsAvailable && !string.IsNullOrEmpty(existing.ReservationId))
                throw new InvalidOperationException("Cannot modify a slot with an active reservation.");

            // validate times (same rules as create)
            if (dto.EndUtc <= dto.StartUtc) throw new ArgumentException("EndUtc must be after StartUtc.");
            var now = DateTime.UtcNow;
            if (dto.StartUtc < now) throw new InvalidOperationException("Cannot set slot start in the past.");
            if (dto.StartUtc > now.AddDays(7)) throw new InvalidOperationException("Cannot set slot beyond 7 days from now.");

            // check overlap with other slots (exclude this slot)
            var overlap = await _repo.ExistsOverlapAsync(dto.StationId, dto.StartUtc, dto.EndUtc);
            if (overlap && !(existing.StationId == dto.StationId && existing.StartUtc == dto.StartUtc && existing.EndUtc == dto.EndUtc))
                throw new InvalidOperationException("Slot overlaps with existing slot.");

            existing.StationId = dto.StationId;
            existing.StartUtc = dto.StartUtc;
            existing.EndUtc = dto.EndUtc;
            existing.Label = dto.Label;
            existing.UpdatedAtUtc = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return existing;
        }

        public async Task DeleteSlotAsync(string slotId)
        {
            var existing = await _repo.GetByIdAsync(slotId);
            if (existing == null) throw new KeyNotFoundException("Slot not found.");

            if (!existing.IsAvailable && !string.IsNullOrEmpty(existing.ReservationId))
                throw new InvalidOperationException("Cannot delete a slot that has an active reservation.");

            await _repo.DeleteAsync(slotId);
        }

        public async Task<IEnumerable<Slot>> GetAllSlotsAsync()
        {
            return await _repo.GetAllAsync();
        }
    }
}
