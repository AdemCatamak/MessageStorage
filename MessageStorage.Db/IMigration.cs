using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db
{
    public interface IMigration
    {
        (string commandText, IEnumerable<IDbDataAdapter>) Up(MessageStorageDbConfiguration messageStorageDbConfiguration);
        (string commandText, IEnumerable<IDbDataAdapter>) Down(MessageStorageDbConfiguration messageStorageDbConfiguration);
    }
}