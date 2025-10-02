/*
 * File: CreateReservationDto.cs
 * Description: DTO for creating a reservation
 */
using System;

namespace EvChargingAPI.DTOs
{
    public class CreateReservationDto
    {
        public string StationId { get; set; } = string.Empty;
        public string SlotId { get; set; } = string.Empty;
        public DateTime ReservationTimeUtc { get; set; } // client should send UTC
    }
}