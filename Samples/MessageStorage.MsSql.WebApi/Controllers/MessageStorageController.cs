using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace MessageStorage.MsSql.WebApi.Controllers
{
    [ApiController]
    [Route("message-storage")]
    public class MessageStorageController : ControllerBase
    {
        private readonly IMessageStorageMonitor _messageStorageMonitor;

        public MessageStorageController(IMessageStorageMonitor messageStorageMonitor)
        {
            _messageStorageMonitor = messageStorageMonitor;
        }

        [HttpGet("/jobs/{jobStatus}/count")]
        public IActionResult GetJobCountByStatus([FromRoute] JobStatuses jobStatus)
        {
            int result = _messageStorageMonitor.GetJobCountByStatus(jobStatus);
            return StatusCode((int) HttpStatusCode.OK, result);
        }

        [HttpGet("failed-job-count")]
        public IActionResult GetFailedJobCount()
        {
            int failedJobCount = _messageStorageMonitor.GetFailedJobCount();
            return StatusCode((int) HttpStatusCode.OK, failedJobCount);
        }

        [HttpGet("waiting-job-count")]
        public IActionResult GetWaitingJobCount()
        {
            int waitingJobCount = _messageStorageMonitor.GetWaitingJobCount();
            return StatusCode((int) HttpStatusCode.OK, waitingJobCount);
        }
    }
}