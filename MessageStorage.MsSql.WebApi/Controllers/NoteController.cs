using System.Data.Common;
using MessageStorage.Db;
using MessageStorage.MsSql.WebApi.DbContextSection;
using MessageStorage.MsSql.WebApi.DbContextSection.Models;
using MessageStorage.MsSql.WebApi.Handlers;
using MessageStorage.MsSql.WebApi.HttpRequests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;

namespace MessageStorage.MsSql.WebApi.Controllers
{
    [ApiController]
    [Route("notes")]
    public class NoteController : ControllerBase
    {
        private readonly IMessageStorageDbClient _messageStorageDbClient;
        private readonly NoteDbContext _noteDbContext;

        public NoteController(IMessageStorageDbClient messageStorageDbClient, NoteDbContext noteDbContext)
        {
            _messageStorageDbClient = messageStorageDbClient;
            _noteDbContext = noteDbContext;
        }

        [HttpPost]
        public IActionResult PostNote([FromBody] PostNoteHttpRequest postNoteHttpRequest)
        {
            using (DbTransaction dbTransaction = _noteDbContext.Database.BeginTransaction().GetDbTransaction())
            {
                var noteModel = new NoteModel(postNoteHttpRequest.Title, postNoteHttpRequest.Content);
                _noteDbContext.NoteModel.Add(noteModel);
                _noteDbContext.SaveChanges();

                var noteCreatedEvent = new NoteCreatedEvent
                                       {
                                           Id = noteModel.Id.ToString(),
                                           Content = noteModel.NoteContent,
                                           Title = noteModel.NoteTitle
                                       };

                _messageStorageDbClient.Add(noteCreatedEvent, dbTransaction);

                dbTransaction.Commit();
            }


            return Ok();
        }
    }
}