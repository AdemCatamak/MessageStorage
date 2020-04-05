using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db
{
    public interface IMigration
    {
        (string commandText, IEnumerable<IDbDataParameter>) Up(MessageStorageDbConfiguration messageStorageDbConfiguration);
    }
}