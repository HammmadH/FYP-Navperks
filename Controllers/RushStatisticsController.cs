using FYP_Navperks.Models.Database;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace FYP_Navperks.Controllers
{
    public class RushStatisticsController : ControllerBase
    {
        private readonly DbHelper _dbHelper;

        public RushStatisticsController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        [HttpGet("api/Statistics/rush")]
        public async Task<IActionResult> GetRushStatistics()
        {
            var rushstatistic = await _dbHelper.GetRushStatistic();
            if (rushstatistic != null && rushstatistic.Any())
                return Ok(rushstatistic);

            return NotFound("No reservations found");
        }
    }
}
