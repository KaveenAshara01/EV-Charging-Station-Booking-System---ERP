/*
 * File: Slot.cs
 * Description: Slot document stored in MongoDB
 */
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EvChargingAPI.Models
{
    public class Slot
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string SlotId { get; set; } = Guid.NewGuid().ToString();

        // Which station this slot belongs to
        public string StationId { get; set; } = string.Empty;

        // Slot start and end in UTC
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }

        // True if slot is available for booking
        public bool IsAvailable { get; set; } = true;

        // If reserved, link to ReservationId
        public string? ReservationId { get; set; } = null;

        // Optional human label (e.g. "Slot A1 09:00-10:00")
        public string? Label { get; set; } = null;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
