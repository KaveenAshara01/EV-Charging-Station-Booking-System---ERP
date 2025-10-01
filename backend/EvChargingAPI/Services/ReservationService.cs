// /*
//  * File: ReservationService.cs
//  * Description: Implements business rules for reservations (7-day window, 12-hour update/cancel rule)
//  */
// using EvChargingAPI.DTOs;
// using EvChargingAPI.Models;
// using EvChargingAPI.Repositories;
// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;

// namespace EvChargingAPI.Services
// {
//     public class ReservationService : IReservationService
//     {
//         private readonly IReservationRepository _repo;
//         // If you have a Station repository/service, inject it here to verify station existence/slots
//         // private readonly IStationRepository _stationRepo;

//         public ReservationService(IReservationRepository repo /*, IStationRepository stationRepo */)
//         {
//             _repo = repo;
//             // _stationRepo = stationRepo;
//         }

//         // Purpose: Create a reservation after validating rules:
//         //  - reservation time must be within next 7 days
//         //  - slot must not be already booked (Pending/Confirmed)
//         public async Task<Reservation> CreateReservationAsync(string ownerId, CreateReservationDto dto)
//         {
//             // validate reservation window: within 7 days from now
//             var now = DateTime.UtcNow;
//             if (dto.ReservationTimeUtc < now)
//                 throw new InvalidOperationException("Reservation time cannot be in the past.");

//             if (dto.ReservationTimeUtc > now.AddDays(7))
//                 throw new InvalidOperationException("Reservation time must be within 7 days from now.");

//             // TODO: optionally verify station exists and slotId valid via stationRepo

//             // Check conflict: exact same slot/time at station
//             var conflict = await _repo.ExistsConflictAsync(dto.StationId, dto.SlotId, dto.ReservationTimeUtc);
//             if (conflict)
//                 throw new InvalidOperationException("Selected slot is already booked for that time.");

//             var reservation = new Reservation
//             {
//                 OwnerId = ownerId,
//                 StationId = dto.StationId,
//                 SlotId = dto.SlotId,
//                 ReservationTimeUtc = dto.ReservationTimeUtc,
//                 Status = "Pending",
//                 CreatedAtUtc = DateTime.UtcNow,
//                 UpdatedAtUtc = DateTime.UtcNow
//             };

//             await _repo.CreateAsync(reservation);
//             return reservation;
//         }

//         // Purpose: get reservation by id (used by controllers)
//         public async Task<Reservation?> GetByIdAsync(string reservationId)
//         {
//             return await _repo.GetByIdAsync(reservationId);
//         }

//         // Purpose: list reservations for an owner
//         public async Task<IEnumerable<Reservation>> GetByOwnerAsync(string ownerId)
//         {
//             return await _repo.GetByOwnerAsync(ownerId);
//         }

//         // Purpose: list reservations for a station (operator UI)
//         public async Task<IEnumerable<Reservation>> GetByStationAsync(string stationId)
//         {
//             return await _repo.GetByStationAsync(stationId);
//         }

//         // Purpose: update reservation time (reschedule) with 12-hour rule
//         public async Task<Reservation> UpdateReservationAsync(string ownerId, string reservationId, UpdateReservationDto dto)
//         {
//             var existing = await _repo.GetByIdAsync(reservationId);
//             if (existing == null)
//                 throw new KeyNotFoundException("Reservation not found.");

//             // only owner or admin should update — check owner matches
//             if (existing.OwnerId != ownerId)
//                 throw new UnauthorizedAccessException("You can only update your own reservations.");

//             // 12-hour cutoff check
//             var now = DateTime.UtcNow;
//             if (existing.ReservationTimeUtc <= now.AddHours(12))
//                 throw new InvalidOperationException("Cannot update reservation less than 12 hours before start time.");

//             // new time must be within 7 days
//             if (dto.ReservationTimeUtc < now || dto.ReservationTimeUtc > now.AddDays(7))
//                 throw new InvalidOperationException("New reservation time must be within next 7 days.");

//             // check conflict at new time
//             var conflict = await _repo.ExistsConflictAsync(existing.StationId, existing.SlotId, dto.ReservationTimeUtc);
//             if (conflict)
//                 throw new InvalidOperationException("Selected slot is already booked for the new time.");

//             existing.ReservationTimeUtc = dto.ReservationTimeUtc;
//             existing.UpdatedAtUtc = DateTime.UtcNow;
//             existing.Status = "Pending"; // keep Pending after reschedule
//             await _repo.UpdateAsync(existing);
//             return existing;
//         }

//         // Purpose: Cancel a reservation (respect 12-hour cutoff)
//         public async Task CancelReservationAsync(string ownerId, string reservationId)
//         {
//             var existing = await _repo.GetByIdAsync(reservationId);
//             if (existing == null)
//                 throw new KeyNotFoundException("Reservation not found.");

//             if (existing.OwnerId != ownerId)
//                 throw new UnauthorizedAccessException("You can only cancel your own reservations.");

//             var now = DateTime.UtcNow;
//             if (existing.ReservationTimeUtc <= now.AddHours(12))
//                 throw new InvalidOperationException("Cannot cancel reservation less than 12 hours before start time.");

//             existing.Status = "Cancelled";
//             existing.UpdatedAtUtc = DateTime.UtcNow;
//             await _repo.UpdateAsync(existing);
//         }
//     }
// }


/*
 * File: ReservationService.cs
 * Description: Implements business rules for reservations (7-day window, 12-hour update/cancel rule, approval, QR generation)
 */
using EvChargingAPI.DTOs;
using EvChargingAPI.Models;
using EvChargingAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using QRCoder;

namespace EvChargingAPI.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _repo;
        private readonly string _qrSigningSecret = "ReplaceWithLongSecretFromConfigOrEnv"; // move to config/env

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
                CreatedAtUtc = now,
                UpdatedAtUtc = now
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

            if (existing.OwnerId != ownerId)
                throw new UnauthorizedAccessException("You can only update your own reservations.");

            var now = DateTime.UtcNow;
            if (existing.ReservationTimeUtc <= now.AddHours(12))
                throw new InvalidOperationException("Cannot update reservation less than 12 hours before start time.");

            if (dto.ReservationTimeUtc < now || dto.ReservationTimeUtc > now.AddDays(7))
                throw new InvalidOperationException("New reservation time must be within next 7 days.");

            var conflict = await _repo.ExistsConflictAsync(existing.StationId, existing.SlotId, dto.ReservationTimeUtc);
            if (conflict)
                throw new InvalidOperationException("Selected slot is already booked for the new time.");

            existing.ReservationTimeUtc = dto.ReservationTimeUtc;
            existing.UpdatedAtUtc = now;
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
            existing.UpdatedAtUtc = now;
            await _repo.UpdateAsync(existing);
        }

        // Purpose: Approve a reservation (Backoffice or Operator role) and generate QR code
        public async Task<Reservation> ApproveReservationAsync(string approverId, string reservationId)
        {
            var reservation = await _repo.GetByIdAsync(reservationId);
            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found.");

            if (reservation.Status == "Approved")
                throw new InvalidOperationException("Reservation already approved.");

            if (reservation.Status == "Cancelled" || reservation.Status == "Completed")
                throw new InvalidOperationException("Cannot approve a cancelled/completed reservation.");

            // Build QR payload
            var payloadObj = new
            {
                reservationId = reservation.ReservationId,
                ownerId = reservation.OwnerId,
                stationId = reservation.StationId,
                slotId = reservation.SlotId,
                reservationTimeUtc = reservation.ReservationTimeUtc.ToString("o"),
                issuedAtUtc = DateTime.UtcNow.ToString("o")
            };
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payloadObj);

            // Create signature (HMAC-SHA256)
            var hmacKey = Encoding.UTF8.GetBytes(_qrSigningSecret);
            string signature;
            using (var hmac = new HMACSHA256(hmacKey))
            {
                var sigBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadJson));
                signature = Convert.ToBase64String(sigBytes);
            }

            // Compose QR envelope
            var qrEnvelope = new
            {
                payload = payloadObj,
                signature = signature
            };
            var qrEnvelopeJson = System.Text.Json.JsonSerializer.Serialize(qrEnvelope);

            // Generate QR PNG as Base64 using QRCoder
            string qrBase64;
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrData = qrGenerator.CreateQrCode(qrEnvelopeJson, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrData))
            {
                var qrBytes = qrCode.GetGraphic(20); // scale factor
                qrBase64 = Convert.ToBase64String(qrBytes);
            }

            // Update reservation
            reservation.Status = "Approved";
            reservation.QrCodeData = qrBase64;
            reservation.ApprovedBy = approverId;
            reservation.ApprovedAtUtc = DateTime.UtcNow;
            reservation.UpdatedAtUtc = DateTime.UtcNow;

            await _repo.UpdateAsync(reservation);
            return reservation;
        }
    }
}

