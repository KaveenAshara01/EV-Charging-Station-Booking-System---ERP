/*
 * File: IReservationService.cs
 * Description: Interface for reservation business logic
 */
using EvChargingAPI.DTOs;
using EvChargingAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvChargingAPI.Services
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservationAsync(string ownerId, CreateReservationDto dto);
        Task<Reservation?> GetByIdAsync(string reservationId);
        Task<IEnumerable<Reservation>> GetByOwnerAsync(string ownerId);
        Task<IEnumerable<Reservation>> GetByStationAsync(string stationId);
        Task<Reservation> UpdateReservationAsync(string ownerId, string reservationId, UpdateReservationDto dto);
        Task CancelReservationAsync(string ownerId, string reservationId);
        Task<Reservation> ApproveReservationAsync(string approverId, string reservationId);


        Task<IEnumerable<Reservation>> GetAllAsync();

    }
}