using System.Net;
using MessageStorage.Clients;
using MessageStorage.Models;
using Microsoft.AspNetCore.Mvc;

namespace AccountWebApi.Controllers
{
    [Route("jobs")]
    public class MessageStorageController : ControllerBase
    {
        private readonly IMessageStorageMonitor _messageStorageMonitor;

        public MessageStorageController(IMessageStorageMonitor messageStorageMonitor)
        {
            _messageStorageMonitor = messageStorageMonitor;
        }

        [HttpGet("{jobStatus}/count")]
        public IActionResult Get([FromRoute] JobStatus jobStatus)
        {
            int jobCount = _messageStorageMonitor.GetJobCountByStatus(jobStatus);
            return StatusCode((int) HttpStatusCode.OK, jobCount);
        }
    }
}