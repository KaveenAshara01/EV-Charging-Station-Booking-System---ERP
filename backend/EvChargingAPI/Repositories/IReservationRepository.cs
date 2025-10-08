/*
 * File: IReservationRepository.cs
 * Description: Interface for reservation data access operations
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EvChargingAPI.Models;

namespace EvChargingAPI.Repositories
{
    public interface IReservationRepository
    {
        Task CreateAsync(Reservation reservation);
        Task<Reservation?> GetByIdAsync(string reservationId);
        Task<IEnumerable<Reservation>> GetByOwnerAsync(string ownerId);
        Task<IEnumerable<Reservation>> GetByStationAsync(string stationId, DateTime? from = null, DateTime? to = null);
        Task<bool> ExistsConflictAsync(string stationId, string slotId, DateTime reservationTimeUtc);
        Task UpdateAsync(Reservation reservation);
        Task DeleteAsync(string reservationId);
        Task EnsureIndexesAsync();

        Task<IEnumerable<Reservation>> GetAllAsync();
    }
}