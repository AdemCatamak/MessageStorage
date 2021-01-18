using System.Data;
using System.Net;
using MessageStorage;
using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.Configurations;
using MessageStorage.Models;
using MessageStorage.SqlServer.DataAccessSection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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

        [HttpPost("messages")]
        public IActionResult CreateMessage([FromBody] int count)
        {
            for (int index = 0; index < count; index++)
            {
                using (var repositoryContext = new SqlServerMessageStorageRepositoryContext(new MessageStorageRepositoryContextConfiguration(Startup.ConnectionStr)))
                {
                    using (var messageStorageClient = new MessageStorageClient(repositoryContext, new HandlerManager()))
                    {
                        messageStorageClient.Add(index);
                    }
                }
            }

            return StatusCode((int) HttpStatusCode.OK);
        }

        [HttpPost("messages-with-transaction")]
        public IActionResult CreateMessageWithTransaction([FromBody] int count)
        {
            for (int index = 0; index < count; index++)
            {
                using (var repositoryContext = new SqlServerMessageStorageRepositoryContext(new MessageStorageRepositoryContextConfiguration(Startup.ConnectionStr)))
                {
                    using (var messageStorageClient = new MessageStorageClient(repositoryContext, new HandlerManager()))
                    {
                        using (var transaction = messageStorageClient.BeginTransaction(IsolationLevel.ReadCommitted))
                        {
                            messageStorageClient.Add(index);
                            transaction.Commit();
                        }
                    }
                }
            }

            return StatusCode((int) HttpStatusCode.OK);
        }

        [HttpPost("messages-in-transaction")]
        public IActionResult CreateMessageInTransaction([FromBody] int count)
        {
            for (int index = 0; index < count; index++)
            {
                using (var connection = new SqlConnection(Startup.ConnectionStr))
                {
                    connection.Open();
                    using (var dbTransaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        using (var repositoryContext = new SqlServerMessageStorageRepositoryContext(new MessageStorageRepositoryContextConfiguration(Startup.ConnectionStr)))
                        {
                            using (var messageStorageClient = new MessageStorageClient(repositoryContext, new HandlerManager()))
                            {
                                using (var transaction = messageStorageClient.UseTransaction(dbTransaction))
                                {
                                    messageStorageClient.Add(index);
                                    transaction.Commit();
                                }
                            }
                        }
                    }
                }
            }

            return StatusCode((int) HttpStatusCode.OK);
        }
    }
}