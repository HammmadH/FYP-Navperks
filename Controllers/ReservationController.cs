using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using FYP_Navperks.Services;
using FYP_Navperks.Models.Database;
using FYP_Navperks.Models.Parking;
using FYP_Navperks.Models.Reservations;
using FYP_Navperks.Models.Statistics;

namespace FYP_Navperks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly DbHelper _dbHelper;
        private readonly IHubContext<ParkingHub> _hubContext;
        private readonly SlotHardwareService _slotHardwareService;

        public ReservationController(DbHelper dbHelper, IHubContext<ParkingHub> hubContext, SlotHardwareService slotHardwareService)
        {
            _dbHelper = dbHelper;
            _hubContext = hubContext;
            _slotHardwareService = slotHardwareService;
        }

        [HttpPost]
        public async Task<IActionResult> ReserveSlot([FromBody] ReservationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SlotCode) || string.IsNullOrWhiteSpace(request.CarType) || request.UserId <= 0)
                return BadRequest("Invalid slot code or user ID");

            DateTime reservedTime = DateTime.Now;

            var reservations = await _dbHelper.GetReservationsByUserID(request.UserId);

            if (reservations.Any(r => r.ReleasedTime == null))
            {
                var activeReservationId = reservations.First(r => r.ReleasedTime == null).ReservationId;
                return Conflict(new { Message = "User already has an active reservation.", ReservationId = activeReservationId });
            }

            var isAdded = await _dbHelper.ReserveSlot(request.UserId, request.SlotCode, request.CarType, reservedTime);
            var reservationId = await _dbHelper.GetReservationByUserID(request.UserId);

            if (isAdded != null && reservationId != null)
            {
                //await _slotHardwareService.ReserveSlotHardware(request.SlotCode);
                var parkingSlots = await _dbHelper.GetParkingSlotsAsync();
                await _hubContext.Clients.All.SendAsync("UpdateParkingSlots", parkingSlots);

                return Ok(new { Message = "Slot reserved successfully", ReservationId = reservationId });
            }

            return StatusCode(500, "Failed to reserve slot");
        }

        [HttpGet("api/reservationsdetails")]
        public async Task<IActionResult> GetReservations()
        {
            var reservations = await _dbHelper.GetReservationsWithSlotCode();
            if (reservations != null && reservations.Any())
                return Ok(reservations);

            return NotFound("No reservations found");
        }

        [HttpPut("{reservationId}")]
        public async Task<IActionResult> ReleaseSlot(int reservationId, [FromBody] ReservationRequest request)
        {
            if (reservationId <= 0 || string.IsNullOrWhiteSpace(request.SlotCode))
                return BadRequest("Invalid reservation ID or slot code");

            DateTime releasedTime = DateTime.Now;
            var isReleased = await _dbHelper.ReleaseSlot(reservationId, releasedTime);

            if (isReleased)
            {
                //await _slotHardwareService.ReleaseSlotHardware(request.SlotCode);
                var parkingSlots = await _dbHelper.GetParkingSlotsAsync();
                await _hubContext.Clients.All.SendAsync("UpdateParkingSlots", parkingSlots);
                return Ok(new { Message = "Slot released successfully"});
            }

            return StatusCode(500, "Failed to release slot");
        }

        [HttpGet("real-time")]
        public async Task<IActionResult> GetParkingSlotsRealTime()
        {
            var parkingSlots = await _dbHelper.GetParkingSlotsAsync();

            if (parkingSlots != null && parkingSlots.Any())
            {
                await _hubContext.Clients.All.SendAsync("UpdateParkingSlots", parkingSlots);

                return Ok(parkingSlots);
            }

            return NotFound("No parking slots found");
        }
    }
}