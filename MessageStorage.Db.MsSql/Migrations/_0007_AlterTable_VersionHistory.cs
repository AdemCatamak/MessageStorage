using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MessageStorage.Db.MsSql.Migrations
{
    public class _0007_AlterTable_VersionHistory : IOneTimeMigration
    {
        public (string commandText, IEnumerable<IDbDataParameter>) Up(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            string commandText = $@"
DECLARE @table NVARCHAR(512),
@sql NVARCHAR(MAX);

SELECT @table = N'{messageStorageDbConfiguration.Schema}.{TableNames.VersionHistoryTable}';

SELECT @sql = 'ALTER TABLE ' + @table 
    + ' DROP CONSTRAINT ' + name + ';'
    FROM sys.key_constraints
    WHERE [type] = 'PK'
    AND [parent_object_id] = OBJECT_ID(@table);

EXEC sp_executeSQL @sql;

ALTER TABLE [{messageStorageDbConfiguration.Schema}].[{TableNames.VersionHistoryTable}] ALTER COLUMN [VersionNumber] INT NULL;
";
            return (commandText, new List<IDbDataParameter>());
        }
        
        public int VersionNumber => 7;
    }
}