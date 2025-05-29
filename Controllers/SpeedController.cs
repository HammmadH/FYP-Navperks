using FYP_Navperks.Models.Database;
using FYP_Navperks.Models.Statistics;
using Microsoft.AspNetCore.Mvc;

namespace FYP_Navperks.Controllers
{
    public class SpeedController : ControllerBase
    {
        private readonly DbHelper _dbHelper;

        public SpeedController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        [HttpPut("api/Speed/{reservationId}")]
        public async Task<IActionResult> NoteCarSpeed(int reservationId, [FromBody] SpeedRequest request)
        {
            if (reservationId <= 0)
                return BadRequest("Invalid reservation ID");

            var isNote = await _dbHelper.NoteCarSpeed(reservationId, request.CarSpeed);

            if (isNote)
            {
                return Ok(new { Message = "Car Speed Noted successfully" });
            }

            return StatusCode(500, "Failed to Note Speed");
        }
    }
}
