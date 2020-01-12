using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db.MsSql.Migrations
{
    public class _0005_CreateTable_Message : IVersionedMigration
    {
        public (string commandText, IEnumerable<IDbDataAdapter>) Up(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            string commandText = $@"
CREATE TABLE [{messageStorageDbConfiguration.Schema}].[{TableNames.MessageTable}] (
    Id bigint NOT NULL PRIMARY KEY identity(1,1),
    TraceId nvarchar(MAX),
    PayloadClassName nvarchar(MAX) not null,
    PayloadClassNamespace nvarchar(MAX) not null,
    SerializedPayload nvarchar(MAX) not null,
    CreatedOn DATETIME2(3) not null,
    
);";
            return (commandText, new List<IDbDataAdapter>());
        }

        public (string commandText, IEnumerable<IDbDataAdapter>) Down(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            string commandText = $"IF OBJECT_ID('{messageStorageDbConfiguration.Schema}.{TableNames.MessageTable}', 'U') IS NOT NULL DROP TABLE {messageStorageDbConfiguration.Schema}.{TableNames.MessageTable}; ";
            return (commandText, new List<IDbDataAdapter>());
        }

        public int VersionNumber => 5;
    }
}