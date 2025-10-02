using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EvChargingAPI.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "USER"; // USER, BACKOFFICE, STATION_OPERATOR

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
