using FYP_Navperks.Models.Database;
using FYP_Navperks.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Net;
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

            var ip = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList[1].ToString();
            if (string.IsNullOrWhiteSpace(ip))
                return BadRequest("Unable to retrieve IP address");

            string ipAddress;
            if (!Request.Cookies.TryGetValue("ipAddress", out ipAddress))
            {
                var guid = Guid.NewGuid().ToString();
                ipAddress = $"{ip}-{guid}"; 

                Response.Cookies.Append("ipAddress", ipAddress, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    HttpOnly = true,
                    IsEssential = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });
            }

            var statusMessage = await _dbHelper.AddUser(ipAddress);

            var userId = await _dbHelper.GetUserByIPAddress(ipAddress);

            if (userId != null && statusMessage != null)
                if (statusMessage == "User already exists")
                    return Ok(new { Message = statusMessage, UserId = userId });

                if (statusMessage == "User inserted successfully")
                    return Ok(new { Message = "User consent recorded and IP address stored successfully", UserId = userId });

            return StatusCode(500, "Failed to store user details");
        }
    }
}
