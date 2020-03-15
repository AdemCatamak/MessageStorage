using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db.MsSql.Migrations
{
    public class _0005_CreateTable_Message : IOneTimeMigration
    {
        public (string commandText, IEnumerable<IDbDataAdapter>) Up(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            string commandText = $@"
CREATE TABLE [{messageStorageDbConfiguration.Schema}].[{TableNames.MessageTable}] (
    MessageId nvarchar(255) NOT NULL PRIMARY KEY,
    TraceId nvarchar(MAX),
    PayloadClassName nvarchar(MAX) not null,
    PayloadClassNamespace nvarchar(MAX) not null,
    SerializedPayload nvarchar(MAX) not null,
    CreatedOn DATETIME2(3) not null,
    
);";
            return (commandText, new List<IDbDataAdapter>());
        }

        public int VersionNumber => 5;
    }
}