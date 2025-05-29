using FYP_Navperks.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Azure.Core;
using FYP_Navperks.Models.Database;
using FYP_Navperks.Models.Parking;
using FYP_Navperks.Models.Reservations;

namespace FYP_Navperks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationAController : ControllerBase
    {
        private readonly DbHelper _dbHelper;
        private readonly IHubContext<ParkingHub> _hubContext;
        private readonly ISlotHardwareService _slotHardwareService; 

        public ReservationAController(DbHelper dbHelper, IHubContext<ParkingHub> hubContext, ISlotHardwareService slotHardwareService)
        {
            _dbHelper = dbHelper;
            _hubContext = hubContext;
            _slotHardwareService = slotHardwareService; 
        }

        [HttpPost]
        public async Task<IActionResult> ReserveASlot([FromBody] ReservationRequest request2)
        {
            if (string.IsNullOrWhiteSpace(request2.SlotCode) || string.IsNullOrWhiteSpace(request2.CarType) || request2.UserAId <= 0)
                return BadRequest("Invalid slot code or user ID");

            DateTime reservedTime = DateTime.Now;

            var existingReservation = await _dbHelper.GetReservationByUserAID(request2.UserAId);
            if (existingReservation != null)
            {
                return Conflict("User already has an active reservation.");
            }

            var isAdded = await _dbHelper.ReserveASlot(request2.UserAId, request2.SlotCode, request2.CarType, reservedTime);
            var reservationId = await _dbHelper.GetReservationByUserAID(request2.UserAId);

            if (isAdded != null && reservationId != null)
            {
                try
                {
                    //read from config if its true then dont read if false then read
                    //await _slotHardwareService.ReserveSlotHardware(request2.SlotCode);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Slot reserved in system but failed to communicate with hardware: {ex.Message}");
                }

                var parkingSlots = await _dbHelper.GetParkingSlotsAsync();
                await _hubContext.Clients.All.SendAsync("UpdateParkingSlots", parkingSlots);

                return Ok(new { Message = "Slot reserved successfully", ReservationId = reservationId });
            }

            return StatusCode(500, "Failed to reserve slot");
        }
    }
}