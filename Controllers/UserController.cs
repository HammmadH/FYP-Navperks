using FYP_Navperks.Models.Database;
using FYP_Navperks.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FYP_Navperks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DbHelper _dbHelper;

        public UserController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] ConsentRequest request)
        {
            if (!request.Consent)
                return BadRequest("User did not give consent");

            var ipAddress = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList[1].ToString();
            if (string.IsNullOrWhiteSpace(ipAddress))
                return BadRequest("Unable to retrieve IP address");

            var isAdded = await _dbHelper.AddUser(ipAddress);

            var userId = await _dbHelper.GetUserByIPAddress(ipAddress);

            if (isAdded && userId != null)
                return Ok(new { Message = "User consent recorded and IP address stored successfully", UserId = userId });

            return StatusCode(500, "Failed to store user details");
        }
    }
}
