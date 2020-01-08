using MessageStorage.WebApi.Handlers;
using MessageStorage.WebApi.HttpRequests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MessageStorage.WebApi.Controllers
{
    [ApiController]
    [Route("notes")]
    public class NoteController : ControllerBase
    {
        private readonly ILogger<NoteController> _logger;
        private readonly IMessageStorageClient _messageStorageClient;

        public NoteController(IMessageStorageClient messageStorageClient, ILogger<NoteController> logger)
        {
            _messageStorageClient = messageStorageClient;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult PostNote([FromBody] PostNoteHttpRequest postNoteHttpRequest)
        {
            _logger.LogInformation($"Note is stored into some storage");

            _messageStorageClient.Add(new NoteCreatedEvent
                                      {
                                          Id = postNoteHttpRequest.Identifier,
                                          Note = postNoteHttpRequest.Content
                                      });

            _logger.LogInformation($"Note Created Event is stored into MessageStorage");

            return Ok();
        }
    }
}