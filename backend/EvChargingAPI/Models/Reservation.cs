// /*
//  * File: Reservation.cs
//  * Author: <YourName>
//  * Description: Reservation document stored in MongoDB
//  */

// using MongoDB.Bson;
// using MongoDB.Bson.Serialization.Attributes;
// using System;
// using QRCoder;
// using System.Security.Cryptography;
// using System.Text;

// namespace EvChargingAPI.Models
// {
//     public class Reservation
//     {
//         [BsonId]
//         [BsonRepresentation(BsonType.String)]
//         public string ReservationId { get; set; } = Guid.NewGuid().ToString();

//         // Owner's identifier (NIC or user id) - link to EV Owner
//         public string OwnerId { get; set; } = string.Empty;

//         // Station identifier - link to station document
//         public string StationId { get; set; } = string.Empty;

//         // Slot identifier (slot number or id)
//         public string SlotId { get; set; } = string.Empty;

//         // Reservation start time stored in UTC (ISO 8601)
//         public DateTime ReservationTimeUtc { get; set; }

//         // PendingSync, Pending, Confirmed, Cancelled, Completed, Failed
//         public string Status { get; set; } = "Pending";

//         public string? QrCodeData { get; set; } = null; // Base64 PNG data (without data:image prefix)
//         public string? ApprovedBy { get; set; } = null; // operator/backoffice user id
//         public DateTime? ApprovedAtUtc { get; set; } = null;

//         public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
//         public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;



        

// // QR code generation


// private readonly string _qrSigningSecret = "ReplaceWithLongSecretFromConfigOrEnv"; // move to config

// public async Task<Reservation> ApproveReservationAsync(string approverId, string reservationId)
// {
//     var reservation = await _repo.GetByIdAsync(reservationId);
//     if (reservation == null)
//         throw new KeyNotFoundException("Reservation not found.");

//     if (reservation.Status == "Approved")
//         throw new InvalidOperationException("Reservation already approved.");

//     if (reservation.Status == "Cancelled" || reservation.Status == "Completed")
//         throw new InvalidOperationException("Cannot approve a cancelled/completed reservation.");

//     // Enforce: optionally ensure approval happens only if reservation time >= now (or some business rule).
//     // Here we simply allow approve if pending/confirmed.

//     // Build QR payload (include minimal info and signature)
//     // Example payload: JSON string with reservationId, stationId, ownerId, issuedAtUtc
//     var payloadObj = new
//     {
//         reservationId = reservation.ReservationId,
//         ownerId = reservation.OwnerId,
//         stationId = reservation.StationId,
//         slotId = reservation.SlotId,
//         reservationTimeUtc = reservation.ReservationTimeUtc.ToString("o"),
//         issuedAtUtc = DateTime.UtcNow.ToString("o")
//     };
//     var payloadJson = System.Text.Json.JsonSerializer.Serialize(payloadObj);

//     // Create signature (HMAC-SHA256) so operator can validate that QR wasn't forged
//     var hmacKey = Encoding.UTF8.GetBytes(_qrSigningSecret);
//     string signature;
//     using (var hmac = new HMACSHA256(hmacKey))
//     {
//         var sigBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadJson));
//         signature = Convert.ToBase64String(sigBytes);
//     }

//     // Compose final QR content (we store JSON with signature)
//     var qrEnvelope = new
//     {
//         payload = payloadObj,
//         signature = signature
//     };
//     var qrEnvelopeJson = System.Text.Json.JsonSerializer.Serialize(qrEnvelope);

//     // Generate QR PNG as Base64 using QRCoder
//     string qrBase64;
//     using (var qrGenerator = new QRCodeGenerator())
//     using (var qrData = qrGenerator.CreateQrCode(qrEnvelopeJson, QRCodeGenerator.ECCLevel.Q))
//     using (var qrCode = new PngByteQRCode(qrData))
//     {
//         var qrBytes = qrCode.GetGraphic(20); // scale factor
//         qrBase64 = Convert.ToBase64String(qrBytes);
//     }

//     // Update reservation fields
//     reservation.Status = "Approved";
//     reservation.QrCodeData = qrBase64;
//     reservation.ApprovedBy = approverId;
//     reservation.ApprovedAtUtc = DateTime.UtcNow;
//     reservation.UpdatedAtUtc = DateTime.UtcNow;

//     await _repo.UpdateAsync(reservation);
//     return reservation;
// }

//     }
// }



/*
 * File: Reservation.cs
 * Author: <YourName>
 * Description: Reservation document stored in MongoDB with QR generation logic
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using QRCoder;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EvChargingAPI.Models
{
    public class Reservation
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string ReservationId { get; set; } = Guid.NewGuid().ToString();

        // Owner's identifier (NIC or user id) - link to EV Owner
        public string OwnerId { get; set; } = string.Empty;

        // Station identifier - link to station document
        public string StationId { get; set; } = string.Empty;

        // Slot identifier (slot number or id)
        public string SlotId { get; set; } = string.Empty;

        // Reservation start time stored in UTC (ISO 8601)
        public DateTime ReservationTimeUtc { get; set; }

        // Pending, Approved, Cancelled, Completed, Failed
        public string Status { get; set; } = "Pending";

        // Base64 PNG data for QR code
        public string? QrCodeData { get; set; } = null;

        // Backoffice user who approved
        public string? ApprovedBy { get; set; } = null;

        public DateTime? ApprovedAtUtc { get; set; } = null;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        

       
    }
}
