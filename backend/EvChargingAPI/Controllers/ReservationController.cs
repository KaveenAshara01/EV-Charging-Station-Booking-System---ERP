/*
 * File: ReservationsController.cs
 * Description: Reservations controller - exposes booking endpoints; requires JWT authentication.
 */
using EvChargingAPI.DTOs;
using EvChargingAPI.Models;
using EvChargingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EvChargingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _service;

        public ReservationsController(IReservationService service)
        {
            _service = service;
        }

        // Create a reservation for the authenticated user
        // [HttpPost]
        // [AllowAnonymous]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(ownerId))
                return Unauthorized("Invalid token: owner id is missing in token.");

            try
            {
                var reservation = await _service.CreateReservationAsync(ownerId, dto);
                var resp = new ReservationResponseDto
                {
                    ReservationId = reservation.ReservationId,
                    OwnerId = reservation.OwnerId,
                    StationId = reservation.StationId,
                    SlotId = reservation.SlotId,
                    ReservationTimeUtc = reservation.ReservationTimeUtc,
                    Status = reservation.Status,
                    CreatedAtUtc = reservation.CreatedAtUtc,
                    UpdatedAtUtc = reservation.UpdatedAtUtc
                };
                return CreatedAtAction(nameof(GetById), new { id = reservation.ReservationId }, resp);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Get reservation by id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(string id)
        {
            var reservation = await _service.GetByIdAsync(id);
            if (reservation == null) return NotFound();

            return Ok(new ReservationResponseDto
            {
                ReservationId = reservation.ReservationId,
                OwnerId = reservation.OwnerId,
                StationId = reservation.StationId,
                SlotId = reservation.SlotId,
                ReservationTimeUtc = reservation.ReservationTimeUtc,
                Status = reservation.Status,
                CreatedAtUtc = reservation.CreatedAtUtc,
                UpdatedAtUtc = reservation.UpdatedAtUtc,
                QrCodeData = reservation.QrCodeData,
                ApprovedBy = reservation.ApprovedBy,
                ApprovedAtUtc = reservation.ApprovedAtUtc
            });
        }

        // Get reservations for current user
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(ownerId))
                return Unauthorized();

            var list = await _service.GetByOwnerAsync(ownerId);
            var resp = list.Select(r => new ReservationResponseDto
            {
                ReservationId = r.ReservationId,
                OwnerId = r.OwnerId,
                StationId = r.StationId,
                SlotId = r.SlotId,
                ReservationTimeUtc = r.ReservationTimeUtc,
                Status = r.Status,
                CreatedAtUtc = r.CreatedAtUtc,
                UpdatedAtUtc = r.UpdatedAtUtc,
                QrCodeData = r.QrCodeData,
                ApprovedBy = r.ApprovedBy,
                ApprovedAtUtc = r.ApprovedAtUtc
            });
            return Ok(resp);
        }

        // Update/reschedule a reservation (owner only)
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateReservationDto dto)
        {
            var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(ownerId))
                return Unauthorized();

            try
            {
                var updated = await _service.UpdateReservationAsync(ownerId, id, dto);
                return Ok(new ReservationResponseDto
                {
                    ReservationId = updated.ReservationId,
                    OwnerId = updated.OwnerId,
                    StationId = updated.StationId,
                    SlotId = updated.SlotId,
                    ReservationTimeUtc = updated.ReservationTimeUtc,
                    Status = updated.Status,
                    CreatedAtUtc = updated.CreatedAtUtc,
                    UpdatedAtUtc = updated.UpdatedAtUtc,
                    QrCodeData = updated.QrCodeData,
                    ApprovedBy = updated.ApprovedBy,
                    ApprovedAtUtc = updated.ApprovedAtUtc
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // Cancel a reservation
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Cancel(string id)
        {
            var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(ownerId))
                return Unauthorized();

            try
            {
                await _service.CancelReservationAsync(ownerId, id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // Get reservations for a station (operator/backoffice)
        [HttpGet("station/{stationId}")]
        [Authorize(Roles = "BACKOFFICE,STATION_OPERATOR")]
        public async Task<IActionResult> GetByStation(string stationId)
        {
            var list = await _service.GetByStationAsync(stationId);
            var resp = list.Select(r => new ReservationResponseDto
            {
                ReservationId = r.ReservationId,
                OwnerId = r.OwnerId,
                StationId = r.StationId,
                SlotId = r.SlotId,
                ReservationTimeUtc = r.ReservationTimeUtc,
                Status = r.Status,
                CreatedAtUtc = r.CreatedAtUtc,
                UpdatedAtUtc = r.UpdatedAtUtc,
                QrCodeData = r.QrCodeData,
                ApprovedBy = r.ApprovedBy,
                ApprovedAtUtc = r.ApprovedAtUtc
            });
            return Ok(resp);
        }

        // Approve reservation and generate QR (Backoffice only)
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "BACKOFFICE")]
        public async Task<IActionResult> Approve(string id)
        {
            var approverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(approverId))
                return Unauthorized("Invalid token.");

            try
            {
                var updated = await _service.ApproveReservationAsync(approverId, id);
                var resp = new ReservationResponseDto
                {
                    ReservationId = updated.ReservationId,
                    OwnerId = updated.OwnerId,
                    StationId = updated.StationId,
                    SlotId = updated.SlotId,
                    ReservationTimeUtc = updated.ReservationTimeUtc,
                    Status = updated.Status,
                    CreatedAtUtc = updated.CreatedAtUtc,
                    UpdatedAtUtc = updated.UpdatedAtUtc,
                    QrCodeData = updated.QrCodeData,
                    ApprovedBy = updated.ApprovedBy,
                    ApprovedAtUtc = updated.ApprovedAtUtc
                };
                return Ok(resp);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet]
        [Authorize(Roles = "BACKOFFICE,STATION_OPERATOR")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            var resp = list.Select(r => new ReservationResponseDto
            {
                ReservationId = r.ReservationId,
                OwnerId = r.OwnerId,
                StationId = r.StationId,
                SlotId = r.SlotId,
                ReservationTimeUtc = r.ReservationTimeUtc,
                Status = r.Status,
                CreatedAtUtc = r.CreatedAtUtc,
                UpdatedAtUtc = r.UpdatedAtUtc,
                QrCodeData = r.QrCodeData,
                ApprovedBy = r.ApprovedBy,
                ApprovedAtUtc = r.ApprovedAtUtc
            });
            return Ok(resp);
        }
    }
}