using System.Data;
using Dapper;
using MessageStorage.Configurations;
using MessageStorage.DataAccessSection.Repositories.Base;
using MessageStorage.Exceptions;
using MessageStorage.Models;

namespace MessageStorage.DataAccessSection.Repositories
{
    public interface IMessageRepository : IRepository<Message>
    {
    }

    public abstract class BaseMessageRepository : BaseRepository<Message>,
                                                  IMessageRepository
    {
        protected BaseMessageRepository(IDbTransaction dbTransaction, MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration) : base(dbTransaction, messageStorageRepositoryContextConfiguration)
        {
        }

        protected BaseMessageRepository(IDbConnection dbConnection, MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration) : base(dbConnection, messageStorageRepositoryContextConfiguration)
        {
        }

        protected abstract string AddStatement { get; }

        public override void Add(Message entity)
        {
            object payload = entity.GetPayload();

            int affectedRowCount = DbConnection.Execute(AddStatement,
                                                        new
                                                        {
                                                            MessageId = entity.Id,
                                                            SerializedPayload = entity.SerializedPayload,
                                                            PayloadClassName = payload.GetType().Name,
                                                            PayloadClassFullName = payload.GetType().FullName,
                                                            CreatedOn = entity.CreatedOn,
                                                        },
                                                        DbTransaction);

            if (affectedRowCount != 1)
            {
                throw new InsertFailedException($"[EntityId: {entity.Id}, EntityType: {nameof(Job)}] insert script return affected row count as {affectedRowCount}");
            }
        }
    }
}