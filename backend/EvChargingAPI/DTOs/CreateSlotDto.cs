/*
 * File: CreateSlotDto.cs
 * Description: DTO for creating a slot
 */
using System;

namespace EvChargingAPI.DTOs
{
    public class CreateSlotDto
    {
        public string StationId { get; set; } = string.Empty;
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public string? Label { get; set; }
    }
}
