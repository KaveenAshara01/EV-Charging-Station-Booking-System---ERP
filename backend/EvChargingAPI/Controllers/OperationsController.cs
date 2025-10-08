using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EvChargingAPI.DTOs;
using EvChargingAPI.Services;

namespace EvChargingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OperationsController : ControllerBase
    {
        private readonly OperationService _operationService;

        public OperationsController(OperationService operationService)
        {
            _operationService = operationService;
        }

        // POST: api/operations/validate
        [Authorize(Roles = "STATION_OPERATOR")]
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateQr([FromBody] QrValidationRequest request)
        {
            var reservation = await _operationService.ValidateQr(request.ReservationId, request.StationId);
            if (reservation == null) return BadRequest(new { message = "Invalid or expired QR reservation" });

            return Ok(new { message = "QR code valid", reservation });
        }

        // POST: api/operations/finalize
        [Authorize(Roles = "STATION_OPERATOR")]
        [HttpPost("finalize")]
        public async Task<IActionResult> FinalizeReservation([FromBody] FinalizeRequest request)
        {
            var reservation = await _operationService.FinalizeReservation(request.ReservationId);
            if (reservation == null) return BadRequest(new { message = "Unable to finalize reservation" });

            return Ok(new { message = "Charging session completed", reservation });
        }
    }
}
