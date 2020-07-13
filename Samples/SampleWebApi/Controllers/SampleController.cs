using System;
using System.Collections.Generic;
using System.Net;
using MessageStorage.Db.Clients;
using MessageStorage.Models;
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

        [HttpPost("sample")]
        public IActionResult Post([FromBody] string sampleText)
        {
            var sampleMessage = new SampleMessage()
                                {
                                    Text = sampleText
                                };

            Tuple<Message, IEnumerable<Job>> tuple = _messageStorageDbClient.Add(sampleMessage);

            return StatusCode((int) HttpStatusCode.Created, tuple);
        }
    }
}