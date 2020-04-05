using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db.MsSql.Migrations
{
    public class _0002_CreateTable_VersionHistory : IMigration
    {
        public (string commandText, IEnumerable<IDbDataParameter>) Up(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            string commandText = $@"
IF (NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_SCHEMA = '{messageStorageDbConfiguration.Schema}'
            AND  TABLE_NAME = '{TableNames.VersionHistoryTable}'))
BEGIN
    CREATE TABLE [{messageStorageDbConfiguration.Schema}].[{TableNames.VersionHistoryTable}] (
        VersionNumber int NOT NULL PRIMARY KEY,
        VersionName nvarchar(max),
        ExecutionTime datetime default GETDATE()
    );
    INSERT INTO [{messageStorageDbConfiguration.Schema}].[{TableNames.VersionHistoryTable}] (VersionName) VALUES ('{nameof(_0002_CreateTable_VersionHistory)}')
END
";
            return (commandText, new List<IDbDataParameter>());
        }
    }
}