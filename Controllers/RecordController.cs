using CorrelationIdUpdateAPI.Models;
using CorrelationIdUpdateAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorrelationIdUpdateAPI.Controllers
{

    [Authorize] // Requires authentication
    [Route("api/[controller]")]
    [ApiController]
    public class RecordController : ControllerBase
    {
        private readonly RecordService _recordService;

        public RecordController(RecordService recordService)
        {
            _recordService = recordService;
        }

        // GET: api/Record/{correlationId}
        [HttpGet("{correlationId}")]
        public async Task<IActionResult> GetRecord(string correlationId)
        {
            var record = await _recordService.GetRecordByCorrelationIdAsync(correlationId);
            if (record == null)
            {
                return NotFound(new { message = "Record not found." });
            }
            return Ok(record);
        }

        // PUT: api/Record/update/{correlationId}
        [Authorize(Roles = "Admin")] // Restrict this action to Admin role
        [HttpPut("update/{correlationId}")]
        public async Task<IActionResult> UpdateRecord(string correlationId, [FromBody] Record updatedRecord)
        {
            var record = await _recordService.GetRecordByCorrelationIdAsync(correlationId);
            if (record == null)
            {
                return NotFound(new { message = "Record not found." });
            }

            var success = await _recordService.UpdateRecordStatusAsync(correlationId, updatedRecord.Status);
            if (!success)
            {
                return StatusCode(500, new { message = "Failed to update the record." });
            }

            return Ok(new { message = "Record updated successfully." });
        }
    }
}
