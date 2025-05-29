using FYP_Navperks.Models.Admin;
using FYP_Navperks.Models.Database;
using Microsoft.AspNetCore.Mvc;

namespace FYP_Navperks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DbHelper _dbHelper;

        public AdminController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        [HttpPost("api/admin/login")]
        public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest request)
        {
            if (await _dbHelper.ValidateCredentials(request.Username, request.Password))
                return Ok("Logged in successfully");

            return Unauthorized("Enter correct username and password");
        }

        [HttpPut("api/admin/update-password")]
        public async Task<IActionResult> UpdateAdminPassword([FromBody] AdminPasswordUpdateRequest request)
        {
            await _dbHelper.UpdatePassword(request.Username, request.NewPassword);
            return Ok("Password updated successfully");
        }
    }
}
