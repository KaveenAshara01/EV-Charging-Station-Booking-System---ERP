/*
 * File: SlotResponseDto.cs
 * Description: DTO returned to clients to represent a slot
 */
using System;

namespace EvChargingAPI.DTOs
{
    public class SlotResponseDto
    {
        public string SlotId { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public bool IsAvailable { get; set; }
        public string? ReservationId { get; set; }
        public string? Label { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
