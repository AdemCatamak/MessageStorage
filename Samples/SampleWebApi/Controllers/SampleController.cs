using System.Collections.Generic;
using System.Net;
using MessageStorage.Db.Clients;
using Microsoft.AspNetCore.Mvc;
using SampleWebApi.WebApiMessageStorageSection.SampleHandlers;

namespace SampleWebApi.Controllers
{
    public class SampleController : ControllerBase
    {
        private readonly IMessageStorageDbClient _messageStorageDbClient;

        public SampleController(IMessageStorageDbClient messageStorageDbClient)
        {
            _messageStorageDbClient = messageStorageDbClient;
        }

        [HttpPost("samples")]
        public IActionResult Post([FromBody] List<string> sampleTexts)
        {
            using (var dbTransaction = _messageStorageDbClient.BeginTransaction())
            {
                foreach (string sampleText in sampleTexts)
                {
                    var sampleMessage = new SampleMessage
                                        {
                                            Text = sampleText
                                        };

                    _messageStorageDbClient.Add(sampleMessage);
                }

                dbTransaction.Commit();
            }

            return StatusCode((int) HttpStatusCode.Created);
        }
    }
}