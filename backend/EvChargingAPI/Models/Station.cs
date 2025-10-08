using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EvChargingAPI.Models
{
    public class Station
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string StationId { get; set; } = string.Empty; // like name (not unique)
        public string UniqueIdentifier { get; set; } = string.Empty; // unique ID
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
    }
}
