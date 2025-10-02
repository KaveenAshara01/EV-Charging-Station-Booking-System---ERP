using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EvChargingAPI.Models
{
    public class Reservation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string OwnerId { get; set; }
        public string StationId { get; set; }
        public string Status { get; set; }   // Pending, Approved, Cancelled, Completed
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
