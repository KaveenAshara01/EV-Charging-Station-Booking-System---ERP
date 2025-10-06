/*
 * File: ISlotRepository.cs
 * Description: Interface for slot data access operations
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EvChargingAPI.Models;

namespace EvChargingAPI.Repositories
{
    public interface ISlotRepository
    {
        Task CreateAsync(Slot slot);
        Task<Slot?> GetByIdAsync(string slotId);
        Task<IEnumerable<Slot>> GetByStationAndWindowAsync(string stationId, DateTime fromUtc, DateTime toUtc);
        Task<IEnumerable<Slot>> GetAvailableByStationAndWindowAsync(string stationId, DateTime fromUtc, DateTime toUtc);
        Task<bool> ExistsOverlapAsync(string stationId, DateTime startUtc, DateTime endUtc);
        Task UpdateAsync(Slot slot);
        Task DeleteAsync(string slotId);
        Task EnsureIndexesAsync();
    }
}
