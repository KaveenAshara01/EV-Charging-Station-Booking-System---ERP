using EvChargingAPI.Models;
using EvChargingAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;


namespace EvChargingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly IStationService _stationService;
        private readonly ISlotService _slotService;

        public StationsController(IStationService stationService, ISlotService slotService)
        {
            _stationService = stationService;
            _slotService = slotService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var stations = await _stationService.GetAllStationsAsync();
            return Ok(stations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var station = await _stationService.GetStationByIdAsync(id);
            if (station == null)
                return NotFound();

            return Ok(station);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Station station)
        {
            await _stationService.CreateStationAsync(station);
            return CreatedAtAction(nameof(GetById), new { id = station.Id }, station);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Station station)
        {
            var existing = await _stationService.GetStationByIdAsync(id);
            if (existing == null)
                return NotFound();

            station.Id = id;
            await _stationService.UpdateStationAsync(id, station);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _stationService.GetStationByIdAsync(id);
            if (existing == null)
                return NotFound();

            await _stationService.DeleteStationAsync(id);
            return NoContent();
        }



        [HttpGet("{stationId}/slots")]
        public async Task<IActionResult> GetAvailableSlots(string stationId)
        {
            var response = await _slotService.GetAllSlotsAsync();
            if (response == null || !response.Any())
            {
                return NotFound(new { Message = "No available slots found for the specified station." });
            }

            var availableSlots = response
                .Where(slot => slot.StationId == stationId && slot.IsAvailable)
                .ToList();

            if (!availableSlots.Any())
            {
                return NotFound(new { Message = "No available slots found for the specified station." });
            }

            return Ok(availableSlots);
        //     var response = await _slotService.GetAllSlotsAsync();
        //     Console.WriteLine($"Response {response}");
        // if (response == null || !response.Any())
        // {
        //     return NotFound(new { Message = "No available slots found for the specified station." });
        // }
        //     return Ok(response);
        }
    }
}
