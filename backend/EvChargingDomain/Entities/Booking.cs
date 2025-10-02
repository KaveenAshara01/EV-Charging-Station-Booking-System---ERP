using System;
namespace Domain.Entities
{
    public class Booking
    {
        public string Id { get; set; } = default!;
        public string StationId { get; set; } = default!;
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public string Status { get; set; } = default!;
        // Add other properties as needed
    }
}
