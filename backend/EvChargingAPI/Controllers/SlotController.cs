/*
 * File: SlotsController.cs
 * Description: Slot management endpoints (Backoffice)
 */
using EvChargingAPI.DTOs;
using EvChargingAPI.Models;
using EvChargingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EvChargingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlotsController : ControllerBase
    {
        private readonly ISlotService _service;

        public SlotsController(ISlotService service)
        {
            _service = service;
        }

        // Create a slot (BACKOFFICE only)
        [HttpPost]
        [Authorize(Roles = "BACKOFFICE")]
        public async Task<IActionResult> Create([FromBody] CreateSlotDto dto)
        {
            try
            {
                var slot = await _service.CreateSlotAsync(dto);
                var resp = new SlotResponseDto
                {
                    SlotId = slot.SlotId,
                    StationId = slot.StationId,
                    StartUtc = slot.StartUtc,
                    EndUtc = slot.EndUtc,
                    IsAvailable = slot.IsAvailable,
                    ReservationId = slot.ReservationId,
                    Label = slot.Label,
                    CreatedAtUtc = slot.CreatedAtUtc,
                    UpdatedAtUtc = slot.UpdatedAtUtc
                };
                return CreatedAtAction(nameof(GetById), new { id = slot.SlotId }, resp);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Get a slot by id (auth required)
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(string id)
        {
            var slot = await _service.GetByIdAsync(id);
            if (slot == null) return NotFound();
            return Ok(new SlotResponseDto
            {
                SlotId = slot.SlotId,
                StationId = slot.StationId,
                StartUtc = slot.StartUtc,
                EndUtc = slot.EndUtc,
                IsAvailable = slot.IsAvailable,
                ReservationId = slot.ReservationId,
                Label = slot.Label,
                CreatedAtUtc = slot.CreatedAtUtc,
                UpdatedAtUtc = slot.UpdatedAtUtc
            });
        }

    // List all slots (auth required)
            [HttpGet]
            [Authorize]
            public async Task<IActionResult> GetAll()
            {
                var slots = await _service.GetAllSlotsAsync();
                var resp = slots.Select(s => new SlotResponseDto
                {
                SlotId = s.SlotId,
                StationId = s.StationId,
                StartUtc = s.StartUtc,
                EndUtc = s.EndUtc,
                IsAvailable = s.IsAvailable,
                ReservationId = s.ReservationId,
                Label = s.Label,
                CreatedAtUtc = s.CreatedAtUtc,
                UpdatedAtUtc = s.UpdatedAtUtc
                });
                return Ok(resp);
            }
        // List slots for a station in a window (auth required)
        [HttpGet("station/{stationId}")]
        [Authorize]
        public async Task<IActionResult> GetByStation(string stationId, [FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc)
        {
            var list = await _service.GetSlotsForStationAsync(stationId, fromUtc, toUtc);
            var resp = list.Select(s => new SlotResponseDto
            {
                SlotId = s.SlotId,
                StationId = s.StationId,
                StartUtc = s.StartUtc,
                EndUtc = s.EndUtc,
                IsAvailable = s.IsAvailable,
                ReservationId = s.ReservationId,
                Label = s.Label,
                CreatedAtUtc = s.CreatedAtUtc,
                UpdatedAtUtc = s.UpdatedAtUtc
            });
            return Ok(resp);
        }

        // List available slots for a station in a window (auth required)
        [HttpGet("station/{stationId}/available")]
        [Authorize]
        public async Task<IActionResult> GetAvailableByStation(string stationId, [FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc)
        {
            var list = await _service.GetAvailableSlotsForStationAsync(stationId, fromUtc, toUtc);
            var resp = list.Select(s => new SlotResponseDto
            {
                SlotId = s.SlotId,
                StationId = s.StationId,
                StartUtc = s.StartUtc,
                EndUtc = s.EndUtc,
                IsAvailable = s.IsAvailable,
                ReservationId = s.ReservationId,
                Label = s.Label,
                CreatedAtUtc = s.CreatedAtUtc,
                UpdatedAtUtc = s.UpdatedAtUtc
            });
            return Ok(resp);
        }

        // Update slot (BACKOFFICE)
        [HttpPut("{id}")]
        [Authorize(Roles = "BACKOFFICE")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateSlotDto dto)
        {
            try
            {
                var updated = await _service.UpdateSlotAsync(id, dto);
                return Ok(new SlotResponseDto
                {
                    SlotId = updated.SlotId,
                    StationId = updated.StationId,
                    StartUtc = updated.StartUtc,
                    EndUtc = updated.EndUtc,
                    IsAvailable = updated.IsAvailable,
                    ReservationId = updated.ReservationId,
                    Label = updated.Label,
                    CreatedAtUtc = updated.CreatedAtUtc,
                    UpdatedAtUtc = updated.UpdatedAtUtc
                });
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

        // Delete slot (BACKOFFICE)
        [HttpDelete("{id}")]
        [Authorize(Roles = "BACKOFFICE")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _service.DeleteSlotAsync(id);
                return NoContent();
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
    }
}
