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
            var reservation = await _reservationRepository.GetReservationById(reservationId);
            if (reservation == null) return null;

            if (reservation.Status != "Approved") return null;

            if (reservation.StationId != stationId) return null;

            return reservation;
        }

        public async Task<Reservation> FinalizeReservation(string reservationId)
        {
            var reservation = await _reservationRepository.GetReservationById(reservationId);
            if (reservation == null) return null;

            if (reservation.Status != "Approved") return null;

            reservation.Status = "Completed";
            reservation.CompletedAt = DateTime.UtcNow;

            await _reservationRepository.UpdateReservation(reservation);

            return reservation;
        }
    }
}
