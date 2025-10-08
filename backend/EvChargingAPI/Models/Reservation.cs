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

        // public DateTime? StartTime { get; set; } = null;
        // public DateTime? EndTime { get; set; } = null;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        // Reservation start time (local time at station)
        public DateTime? StartTime { get; set; } = null;

        // Reservation end time (local time at station)
        public DateTime? EndTime { get; set; } = null;

       
    }
}