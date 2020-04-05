using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db.MsSql.Migrations
{
    public class _0001_CreateSchema : IMigration
    {
        public (string commandText, IEnumerable<IDbDataParameter>) Up(MessageStorageDbConfiguration messageStorageDbConfiguration)
        {
            string commandText = $@"
IF NOT EXISTS(
    SELECT  * FROM sys.schemas
        WHERE name = N'{messageStorageDbConfiguration.Schema}'
)
    EXEC('CREATE SCHEMA [{messageStorageDbConfiguration.Schema}]');
";

            return (commandText, new List<IDbDataParameter>());
        }
    }
}