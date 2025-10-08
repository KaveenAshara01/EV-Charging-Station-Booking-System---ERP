// using EvChargingAPI.DTOs;
// using EvChargingAPI.Models;
// using EvChargingAPI.Repositories;
// using System;
// using System.Collections.Generic;
// using System.Security.Cryptography;
// using System.Text;
// using System.Threading.Tasks;
// using QRCoder;

// namespace EvChargingAPI.Services
// {
//     public class ReservationService : IReservationService
//     {
//         private readonly IReservationRepository _repo;
//         private readonly string _qrSigningSecret = "ReplaceWithLongSecretFromConfigOrEnv"; // move to config/env
//         private readonly ISlotRepository _slotRepo;

//         // If you have a Station repository/service, inject it here to verify station existence/slots
//         // private readonly IStationRepository _stationRepo;

//         public ReservationService(IReservationRepository repo ,ISlotRepository slotRepo)
//         {
//             _repo = repo;
//             _slotRepo = slotRepo;
//         }

//         // Purpose: Create a reservation after validating rules:
//         //  - reservation time must be within next 7 days
//         //  - slot must not be already booked (Pending/Confirmed)
//         public async Task<Reservation> CreateReservationAsync(string ownerId, CreateReservationDto dto)
//         {
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
//                 CreatedAtUtc = now,
//                 UpdatedAtUtc = now
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

//             if (existing.OwnerId != ownerId)
//                 throw new UnauthorizedAccessException("You can only update your own reservations.");

//             var now = DateTime.UtcNow;
//             if (existing.ReservationTimeUtc <= now.AddHours(12))
//                 throw new InvalidOperationException("Cannot update reservation less than 12 hours before start time.");

//             if (dto.ReservationTimeUtc < now || dto.ReservationTimeUtc > now.AddDays(7))
//                 throw new InvalidOperationException("New reservation time must be within next 7 days.");

//             var conflict = await _repo.ExistsConflictAsync(existing.StationId, existing.SlotId, dto.ReservationTimeUtc);
//             if (conflict)
//                 throw new InvalidOperationException("Selected slot is already booked for the new time.");

//             existing.ReservationTimeUtc = dto.ReservationTimeUtc;
//             existing.UpdatedAtUtc = now;
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
//             existing.UpdatedAtUtc = now;
//             await _repo.UpdateAsync(existing);
//         }

//         // Purpose: Approve a reservation (Backoffice or Operator role) and generate QR code
//         public async Task<Reservation> ApproveReservationAsync(string approverId, string reservationId)
//         {
//             var reservation = await _repo.GetByIdAsync(reservationId);
//             if (reservation == null)
//                 throw new KeyNotFoundException("Reservation not found.");

//             if (reservation.Status == "Approved")
//                 throw new InvalidOperationException("Reservation already approved.");

//             if (reservation.Status == "Cancelled" || reservation.Status == "Completed")
//                 throw new InvalidOperationException("Cannot approve a cancelled/completed reservation.");

//             // Build QR payload
//             var payloadObj = new
//             {
//                 reservationId = reservation.ReservationId,
//                 ownerId = reservation.OwnerId,
//                 stationId = reservation.StationId,
//                 slotId = reservation.SlotId,
//                 reservationTimeUtc = reservation.ReservationTimeUtc.ToString("o"),
//                 issuedAtUtc = DateTime.UtcNow.ToString("o")
//             };
//             var payloadJson = System.Text.Json.JsonSerializer.Serialize(payloadObj);

//             // Create signature (HMAC-SHA256)
//             var hmacKey = Encoding.UTF8.GetBytes(_qrSigningSecret);
//             string signature;
//             using (var hmac = new HMACSHA256(hmacKey))
//             {
//                 var sigBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadJson));
//                 signature = Convert.ToBase64String(sigBytes);
//             }

//             // Compose QR envelope
//             var qrEnvelope = new
//             {
//                 payload = payloadObj,
//                 signature = signature
//             };
//             var qrEnvelopeJson = System.Text.Json.JsonSerializer.Serialize(qrEnvelope);

//             // Generate QR PNG as Base64 using QRCoder
//             string qrBase64;
//             using (var qrGenerator = new QRCodeGenerator())
//             using (var qrData = qrGenerator.CreateQrCode(qrEnvelopeJson, QRCodeGenerator.ECCLevel.Q))
//             using (var qrCode = new PngByteQRCode(qrData))
//             {
//                 var qrBytes = qrCode.GetGraphic(20); // scale factor
//                 qrBase64 = Convert.ToBase64String(qrBytes);
//             }

//             // Update reservation
//             reservation.Status = "Approved";
//             reservation.QrCodeData = qrBase64;
//             reservation.ApprovedBy = approverId;
//             reservation.ApprovedAtUtc = DateTime.UtcNow;
//             reservation.UpdatedAtUtc = DateTime.UtcNow;

//             await _repo.UpdateAsync(reservation);
//             return reservation;
//         }
//     }
// }



// using EvChargingAPI.DTOs;
// using EvChargingAPI.Models;
// using EvChargingAPI.Repositories;
// using System;
// using System.Collections.Generic;
// using System.Security.Cryptography;
// using System.Text;
// using System.Threading.Tasks;
// using QRCoder;

// namespace EvChargingAPI.Services
// {
//     public class ReservationService : IReservationService
//     {
//         private readonly IReservationRepository _repo;
//         private readonly ISlotRepository _slotRepo;
//         private readonly string _qrSigningSecret = "ReplaceWithLongSecretFromConfigOrEnv"; // move to config/env

//         public ReservationService(IReservationRepository repo, ISlotRepository slotRepo)
//         {
//             _repo = repo;
//             _slotRepo = slotRepo;
//         }

//         // -----------------------------
//         // Create Reservation
//         // -----------------------------
//         public async Task<Reservation> CreateReservationAsync(string ownerId, CreateReservationDto dto)
//         {
//             var now = DateTime.UtcNow;

//             if (dto.ReservationTimeUtc < now)
//                 throw new InvalidOperationException("Reservation time cannot be in the past.");

//             if (dto.ReservationTimeUtc > now.AddDays(7))
//                 throw new InvalidOperationException("Reservation time must be within 7 days from now.");

//             // ✅ Verify slot exists
//             var slot = await _slotRepo.GetByIdAsync(dto.SlotId);
//             if (slot == null)
//                 throw new InvalidOperationException("Selected slot does not exist.");

//             if (slot.StationId != dto.StationId)
//                 throw new InvalidOperationException("Slot does not belong to the specified station.");

//             // ✅ Check conflict: same slot/time already reserved
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
//                 CreatedAtUtc = now,
//                 UpdatedAtUtc = now
//             };

//             await _repo.CreateAsync(reservation);
//             return reservation;
//         }

//         // -----------------------------
//         // Get by Id
//         // -----------------------------
//         public async Task<Reservation?> GetByIdAsync(string reservationId)
//         {
//             return await _repo.GetByIdAsync(reservationId);
//         }

//         // -----------------------------
//         // Get by Owner
//         // -----------------------------
//         public async Task<IEnumerable<Reservation>> GetByOwnerAsync(string ownerId)
//         {
//             return await _repo.GetByOwnerAsync(ownerId);
//         }

//         // -----------------------------
//         // Get by Station
//         // -----------------------------
//         public async Task<IEnumerable<Reservation>> GetByStationAsync(string stationId)
//         {
//             return await _repo.GetByStationAsync(stationId);
//         }

//         // -----------------------------
//         // Update Reservation (Reschedule)
//         // -----------------------------
//         public async Task<Reservation> UpdateReservationAsync(string ownerId, string reservationId, UpdateReservationDto dto)
//         {
//             var existing = await _repo.GetByIdAsync(reservationId);
//             if (existing == null)
//                 throw new KeyNotFoundException("Reservation not found.");

//             if (existing.OwnerId != ownerId)
//                 throw new UnauthorizedAccessException("You can only update your own reservations.");

//             var now = DateTime.UtcNow;
//             if (existing.ReservationTimeUtc <= now.AddHours(12))
//                 throw new InvalidOperationException("Cannot update reservation less than 12 hours before start time.");

//             if (dto.ReservationTimeUtc < now || dto.ReservationTimeUtc > now.AddDays(7))
//                 throw new InvalidOperationException("New reservation time must be within next 7 days.");

//             // ✅ Ensure slot still exists
//             var slot = await _slotRepo.GetByIdAsync(existing.SlotId);
//             if (slot == null)
//                 throw new InvalidOperationException("Slot no longer exists.");

//             // ✅ Check conflict
//             var conflict = await _repo.ExistsConflictAsync(existing.StationId, existing.SlotId, dto.ReservationTimeUtc);
//             if (conflict)
//                 throw new InvalidOperationException("Selected slot is already booked for the new time.");

//             existing.ReservationTimeUtc = dto.ReservationTimeUtc;
//             existing.UpdatedAtUtc = now;
//             existing.Status = "Pending";
//             await _repo.UpdateAsync(existing);
//             return existing;
//         }

//         // -----------------------------
//         // Cancel Reservation
//         // -----------------------------
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
//             existing.UpdatedAtUtc = now;

//             await _repo.UpdateAsync(existing);
//             // ✅ No need to "free slot" explicitly because
//             // conflict checks always ignore Cancelled reservations
//         }

//         // -----------------------------
//         // Approve Reservation (Backoffice)
//         // -----------------------------
//         public async Task<Reservation> ApproveReservationAsync(string approverId, string reservationId)
//         {
//             var reservation = await _repo.GetByIdAsync(reservationId);
//             if (reservation == null)
//                 throw new KeyNotFoundException("Reservation not found.");

//             if (reservation.Status == "Approved")
//                 throw new InvalidOperationException("Reservation already approved.");

//             if (reservation.Status == "Cancelled" || reservation.Status == "Completed")
//                 throw new InvalidOperationException("Cannot approve a cancelled/completed reservation.");

//             // ✅ Double-check slot still exists
//             var slot = await _slotRepo.GetByIdAsync(reservation.SlotId);
//             if (slot == null)
//                 throw new InvalidOperationException("Slot no longer exists.");

//             // Build QR payload
//             var payloadObj = new
//             {
//                 reservationId = reservation.ReservationId,
//                 ownerId = reservation.OwnerId,
//                 stationId = reservation.StationId,
//                 slotId = reservation.SlotId,
//                 reservationTimeUtc = reservation.ReservationTimeUtc.ToString("o"),
//                 issuedAtUtc = DateTime.UtcNow.ToString("o")
//             };
//             var payloadJson = System.Text.Json.JsonSerializer.Serialize(payloadObj);

//             // Create signature
//             var hmacKey = Encoding.UTF8.GetBytes(_qrSigningSecret);
//             string signature;
//             using (var hmac = new HMACSHA256(hmacKey))
//             {
//                 var sigBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadJson));
//                 signature = Convert.ToBase64String(sigBytes);
//             }

//             // Generate QR code
//             var qrEnvelope = new { payload = payloadObj, signature = signature };
//             var qrEnvelopeJson = System.Text.Json.JsonSerializer.Serialize(qrEnvelope);

//             string qrBase64;
//             using (var qrGenerator = new QRCodeGenerator())
//             using (var qrData = qrGenerator.CreateQrCode(qrEnvelopeJson, QRCodeGenerator.ECCLevel.Q))
//             using (var qrCode = new PngByteQRCode(qrData))
//             {
//                 var qrBytes = qrCode.GetGraphic(20);
//                 qrBase64 = Convert.ToBase64String(qrBytes);
//             }

//             reservation.Status = "Approved";
//             reservation.QrCodeData = qrBase64;
//             reservation.ApprovedBy = approverId;
//             reservation.ApprovedAtUtc = DateTime.UtcNow;
//             reservation.UpdatedAtUtc = DateTime.UtcNow;

//             await _repo.UpdateAsync(reservation);
//             return reservation;
//         }
//     }
// }



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
        private readonly ISlotRepository _slotRepo;
        private readonly string _qrSigningSecret = "ReplaceWithLongSecretFromConfigOrEnv"; // move to config/env

        public ReservationService(IReservationRepository repo, ISlotRepository slotRepo)
        {
            _repo = repo;
            _slotRepo = slotRepo;
        }

        // -----------------------------
        // Create Reservation
        // -----------------------------
        public async Task<Reservation> CreateReservationAsync(string ownerId, CreateReservationDto dto)
        {
            var now = DateTime.UtcNow;

            if (dto.ReservationTimeUtc < now)
                throw new InvalidOperationException("Reservation time cannot be in the past.");

            if (dto.ReservationTimeUtc > now.AddDays(7))
                throw new InvalidOperationException("Reservation time must be within 7 days from now.");

            // ✅ Verify slot exists
            var slot = await _slotRepo.GetByIdAsync(dto.SlotId);
            if (slot == null)
                throw new InvalidOperationException("Selected slot does not exist.");

            if (slot.StationId != dto.StationId)
                throw new InvalidOperationException("Slot does not belong to the specified station.");

            if (!slot.IsAvailable)
                throw new InvalidOperationException("Slot is currently not available.");

            // ✅ Check conflict: same slot/time already reserved
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

        // -----------------------------
        // Get by Id
        // -----------------------------
        public async Task<Reservation?> GetByIdAsync(string reservationId)
        {
            return await _repo.GetByIdAsync(reservationId);
        }

        // -----------------------------
        // Get by Owner
        // -----------------------------
        public async Task<IEnumerable<Reservation>> GetByOwnerAsync(string ownerId)
        {
            return await _repo.GetByOwnerAsync(ownerId);
        }

        // -----------------------------
        // Get by Station
        // -----------------------------
        public async Task<IEnumerable<Reservation>> GetByStationAsync(string stationId)
        {
            return await _repo.GetByStationAsync(stationId);
        }

        // -----------------------------
        // Update Reservation (Reschedule)
        // -----------------------------
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

            var slot = await _slotRepo.GetByIdAsync(existing.SlotId);
            if (slot == null)
                throw new InvalidOperationException("Slot no longer exists.");

            var conflict = await _repo.ExistsConflictAsync(existing.StationId, existing.SlotId, dto.ReservationTimeUtc);
            if (conflict)
                throw new InvalidOperationException("Selected slot is already booked for the new time.");

            existing.ReservationTimeUtc = dto.ReservationTimeUtc;
            existing.UpdatedAtUtc = now;
            existing.Status = "Pending";
            await _repo.UpdateAsync(existing);
            return existing;
        }

        // -----------------------------
        // Cancel Reservation
        // -----------------------------
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

            // ✅ Free slot again
            var slot = await _slotRepo.GetByIdAsync(existing.SlotId);
            if (slot != null)
            {
                slot.IsAvailable = true;
                slot.ReservationId = null;
                await _slotRepo.UpdateAsync(slot);
            }
        }

        // -----------------------------
        // Approve Reservation (Backoffice)
        // -----------------------------
        public async Task<Reservation> ApproveReservationAsync(string approverId, string reservationId)
        {
            var reservation = await _repo.GetByIdAsync(reservationId);
            if (reservation == null)
                throw new KeyNotFoundException("Reservation not found.");

            if (reservation.Status == "Approved")
                throw new InvalidOperationException("Reservation already approved.");

            if (reservation.Status == "Cancelled" || reservation.Status == "Completed")
                throw new InvalidOperationException("Cannot approve a cancelled/completed reservation.");

            var slot = await _slotRepo.GetByIdAsync(reservation.SlotId);
            if (slot == null)
                throw new InvalidOperationException("Slot no longer exists.");

            if (!slot.IsAvailable)
                throw new InvalidOperationException("Slot is already in use.");

            // Build QR payload (includes slotId)
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

            var hmacKey = Encoding.UTF8.GetBytes(_qrSigningSecret);
            string signature;
            using (var hmac = new HMACSHA256(hmacKey))
            {
                var sigBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadJson));
                signature = Convert.ToBase64String(sigBytes);
            }

            var qrEnvelope = new { payload = payloadObj, signature = signature };
            var qrEnvelopeJson = System.Text.Json.JsonSerializer.Serialize(qrEnvelope);

            string qrBase64;
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrData = qrGenerator.CreateQrCode(qrEnvelopeJson, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrData))
            {
                var qrBytes = qrCode.GetGraphic(20);
                qrBase64 = Convert.ToBase64String(qrBytes);
            }

            // ✅ Mark slot as occupied
            slot.IsAvailable = false;
            slot.ReservationId = reservation.ReservationId;
            await _slotRepo.UpdateAsync(slot);

            // ✅ Update reservation
            reservation.Status = "Approved";
            reservation.QrCodeData = qrBase64;
            reservation.ApprovedBy = approverId;
            reservation.ApprovedAtUtc = DateTime.UtcNow;
            reservation.UpdatedAtUtc = DateTime.UtcNow;
            await _repo.UpdateAsync(reservation);

            return reservation;
        }


        // -----------------------------
        // Get All Reservations
        // -----------------------------
        public async Task<IEnumerable<Reservation>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

    }
}

