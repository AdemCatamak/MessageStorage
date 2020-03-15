using System;
using System.Globalization;
using System.Net;
using MessageStorage.Db;
using Microsoft.AspNetCore.Mvc;
using Samples.Db.WebApi.Events;

namespace Samples.Db.WebApi.Controllers
{
    [Route("something")]
    public class SomethingController : ControllerBase
    {
        private readonly IMessageStorageDbClient _messageStorageDbClient;

        public SomethingController(IMessageStorageDbClient messageStorageDbClient)
        {
            _messageStorageDbClient = messageStorageDbClient;
        }

        [HttpPost]
        public IActionResult PostSomething()
        {
            var dateTime = DateTime.UtcNow;
            var somethingCreatedEvent = new SomethingCreatedEvent()
                                        {
                                            Id = dateTime.ToString(CultureInfo.InvariantCulture),
                                            SomeField = dateTime.ToString(CultureInfo.InvariantCulture)
                                        };
            _messageStorageDbClient.Add(somethingCreatedEvent);
            
            return StatusCode((int) HttpStatusCode.Created);
        }
    }
}