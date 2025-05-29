using FYP_Navperks.Models.Admin;
using FYP_Navperks.Models.Database;
using Microsoft.AspNetCore.Mvc;

namespace FYP_Navperks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserAController : ControllerBase
    {
        private readonly DbHelper _dbHelper;

        public UserAController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAUser([FromBody] UserADetails request)
        {
            if (string.IsNullOrWhiteSpace(request.CarNumber))
                return BadRequest("Car number is required");

            var isAdded = await _dbHelper.AddAUser(request.CarNumber);

            var userId = await _dbHelper.GetUserByCarNumber(request.CarNumber);

            if (isAdded && userId != null)
                return Ok(new { Message = "User details recorded successfully", UserId = userId });

            return StatusCode(500, "Failed to store user details");
        }
    }
}
