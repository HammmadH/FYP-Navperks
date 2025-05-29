using FYP_Navperks.Models.Database;
using Microsoft.AspNetCore.Mvc;
using System;

namespace FYP_Navperks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementsController : ControllerBase
    {
        private readonly DbHelper _dbHelper;

        public AnnouncementsController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnnouncements()
        {
            var announcements = await _dbHelper.GetAnnouncementsAsync();
            return Ok(announcements);
        }

        [HttpPost]
        public async Task<IActionResult> AddAnnouncement([FromBody] string announcementText)
        {
            var newId = await _dbHelper.AddAnnouncementAsync(announcementText);
            return Ok(newId);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnnouncement(int id, [FromBody] string announcementText)
        {
            var success = await _dbHelper.UpdateAnnouncementAsync(id, announcementText);
            if (!success) return NotFound();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var success = await _dbHelper.DeleteAnnouncementAsync(id);
            if (!success) return NotFound();
            return Ok();
        }
    }
}
