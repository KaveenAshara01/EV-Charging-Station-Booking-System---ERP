/*
 * File: UpdateReservationDto.cs
 * Description: DTO for updating a reservation (reschedule)
 */
using System;

namespace EvChargingAPI.DTOs
{
    public class UpdateReservationDto
    {
        public DateTime ReservationTimeUtc { get; set; }
    }
}