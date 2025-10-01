/*
 * File: Reservation.cs
 * Author: <YourName>
 * Description: Reservation document stored in MongoDB
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

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

        // PendingSync, Pending, Confirmed, Cancelled, Completed, Failed
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
