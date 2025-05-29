using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using FYP_Navperks.Models.Database;
using FYP_Navperks.Models.Parking;

namespace FYP_Navperks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParkingSlotsController : ControllerBase
    {
        private readonly DbHelper _dbHelper;

        public ParkingSlotsController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        [HttpPost]
        public async Task<IActionResult> AddParkingSlots([FromBody] List<ParkingSlotDTO> parkingSlots)
        {
            if (parkingSlots == null || parkingSlots.Count == 0)
                return BadRequest("No parking slots provided");

            var isAdded = await _dbHelper.AddParkingSlots(parkingSlots);
            if (isAdded)
                return Ok("Parking slots added successfully");

            return StatusCode(500, "Failed to add parking slots");

        }
    }
}
