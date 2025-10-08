using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EvChargingAPI.Models
{
    public class Station
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string StationUniqueId { get; set; } = Guid.NewGuid().ToString();

        // Non-unique display name or code
        public string StationId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public LocationData Location { get; set; } = new LocationData();

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public class LocationData
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
    }
}
