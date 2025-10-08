using EvChargingAPI.Models;
using EvChargingAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvChargingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly IStationService _stationService;

        public StationsController(IStationService stationService)
        {
            _stationService = stationService;
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
    }
}
