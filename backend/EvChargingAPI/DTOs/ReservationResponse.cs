/*
 * File: ReservationResponseDto.cs
 * Description: DTO returned to clients to represent a reservation
 */
using System;

namespace EvChargingAPI.DTOs
{
    public class ReservationResponseDto
    {
        public string ReservationId { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string SlotId { get; set; } = string.Empty;
        public DateTime ReservationTimeUtc { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
