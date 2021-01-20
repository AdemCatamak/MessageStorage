using System.Net;
using MessageStorage.Clients;
using MessageStorage.Models;
using Microsoft.AspNetCore.Mvc;

namespace AccountWebApi.Controllers
{
    [Route("message-storage")]
    public class MessageStorageController : ControllerBase
    {
        private readonly IMessageStorageClient _messageStorageClient;

        public MessageStorageController(IMessageStorageClient messageStorageClient)
        {
            _messageStorageClient = messageStorageClient;
        }

        [HttpGet("jobs/{jobStatus}/count")]
        public IActionResult Get([FromRoute] JobStatus jobStatus)
        {
            int jobCount = _messageStorageClient.GetJobCountByStatus(jobStatus);
            return StatusCode((int) HttpStatusCode.OK, jobCount);
        }
    }
}