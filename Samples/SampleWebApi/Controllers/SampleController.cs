using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Net;
using MessageStorage.Db.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SampleWebApi.EntityFrameworkSection;
using SampleWebApi.EntityFrameworkSection.Models;
using SampleWebApi.WebApiMessageStorageSection.SampleHandlers;

namespace SampleWebApi.Controllers
{
    public class SampleController : ControllerBase
    {
        private readonly IMessageStorageDbClient _messageStorageDbClient;
        private readonly SampleDbContext _sampleDbContext;

        public SampleController(IMessageStorageDbClient messageStorageDbClient, SampleDbContext sampleDbContext)
        {
            _messageStorageDbClient = messageStorageDbClient;
            _sampleDbContext = sampleDbContext;
        }

        [HttpPost("samples")]
        public IActionResult PostSample([FromBody] string sampleText)
        {
            var sampleModel = new SampleModel(sampleText);
            using (DbTransaction transaction = _sampleDbContext.Database.BeginTransaction(IsolationLevel.ReadCommitted).GetDbTransaction())
            {
                _sampleDbContext.Samples.Add(sampleModel);
                _sampleDbContext.SaveChanges();

                _messageStorageDbClient.UseTransaction(transaction);
                var sampleMessage = new SampleCreatedMessage
                                    {
                                        SampleModelId = sampleModel.Id,
                                        Text = sampleText
                                    };

                _messageStorageDbClient.Add(sampleMessage);

                transaction.Commit();
                // after transaction-commit, if you do not use message-storage-client, you do not have to call ClearTransaction
                _messageStorageDbClient.ClearTransaction();
            }

            return StatusCode((int) HttpStatusCode.Created);
        }

        [HttpPost("samples-by-one-by")]
        public IActionResult PostByOneBy([FromBody] List<string> sampleTexts)
        {
            foreach (string sampleText in sampleTexts)
            {
                var sampleMessage = new SampleMessage
                                    {
                                        Text = sampleText
                                    };

                _messageStorageDbClient.Add(sampleMessage);
            }

            return StatusCode((int) HttpStatusCode.Created);
        }

        [HttpPost("samples-transactional")]
        public IActionResult PostTransactional([FromBody] List<string> sampleTexts)
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