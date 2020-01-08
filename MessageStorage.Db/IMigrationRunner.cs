using System.Collections.Generic;

namespace MessageStorage.Db
{
    public interface IMigrationRunner
    {
        void Run(IEnumerable<IMigration> migrations, MessageStorageDbConfiguration messageStorageDbConfiguration);
    }
}