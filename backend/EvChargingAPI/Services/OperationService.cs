using System;
using System.Threading.Tasks;
using EvChargingAPI.Models;
using EvChargingAPI.Repositories;

namespace EvChargingAPI.Services
{
    public class OperationService
    {
        private readonly IReservationRepository _reservationRepository;

        public OperationService(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public async Task<Reservation> ValidateQr(string reservationId, string stationId)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null) return null;

            if (reservation.Status != "Approved") return null;

            if (reservation.StationId != stationId) return null;

            return reservation;
        }

        public async Task<Reservation> FinalizeReservation(string reservationId)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null) return null;

            if (reservation.Status != "Approved") return null;

            reservation.Status = "Completed";
            reservation.EndTime = DateTime.UtcNow;

            await _reservationRepository.UpdateAsync(reservation);

            return reservation;
        }
    }
}
