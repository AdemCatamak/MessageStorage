using System;
using System.Data;
using System.Globalization;
using System.Net;
using MessageStorage.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Samples.Db.WebApi.Data;
using Samples.Db.WebApi.Data.Models;
using Samples.Db.WebApi.Events;

namespace Samples.Db.WebApi.Controllers
{
    [Route("foo")]
    public class FooController : ControllerBase
    {
        private readonly IMessageStorageDbClient _messageStorageDbClient;
        private readonly FooDbContext _fooDbContext;

        public FooController(IMessageStorageDbClient messageStorageDbClient, FooDbContext fooDbContext)
        {
            _messageStorageDbClient = messageStorageDbClient;
            _fooDbContext = fooDbContext;
        }

        [HttpPost]
        public IActionResult PostSomething()
        {
            var dateTime = DateTime.UtcNow;

            using (var dbTransaction = _fooDbContext.Database.BeginTransaction(IsolationLevel.ReadCommitted).GetDbTransaction())
            {
                var fooModel = new FooModel()
                               {
                                   SomeField = dateTime.ToString(CultureInfo.InvariantCulture)
                               };
                _fooDbContext.Foo.Add(fooModel);
                _fooDbContext.SaveChanges();

                var somethingCreatedEvent = new FooCreatedEvent
                                            {
                                                Id = fooModel.Id.ToString(),
                                                SomeField = fooModel.SomeField
                                            };
                _messageStorageDbClient.Add(somethingCreatedEvent, dbTransaction);

                dbTransaction.Commit();
            }

            return StatusCode((int) HttpStatusCode.Created);
        }
    }
}