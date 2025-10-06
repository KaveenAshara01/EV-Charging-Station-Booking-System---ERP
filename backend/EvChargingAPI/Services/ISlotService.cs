/*
 * File: ISlotService.cs
 * Description: Interface for slot business logic
 */
using EvChargingAPI.DTOs;
using EvChargingAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvChargingAPI.Services
{
    public interface ISlotService
    {
        Task<Slot> CreateSlotAsync(CreateSlotDto dto);
        Task<Slot?> GetByIdAsync(string slotId);
        Task<IEnumerable<Slot>> GetSlotsForStationAsync(string stationId, DateTime fromUtc, DateTime toUtc);
        Task<IEnumerable<Slot>> GetAvailableSlotsForStationAsync(string stationId, DateTime fromUtc, DateTime toUtc);
        Task<Slot> UpdateSlotAsync(string slotId, CreateSlotDto dto); // reuse Create DTO for update
        Task DeleteSlotAsync(string slotId);
    }
}
