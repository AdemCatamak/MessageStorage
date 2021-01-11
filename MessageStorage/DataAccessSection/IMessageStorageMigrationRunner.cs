using MessageStorage.Configurations;

namespace MessageStorage.DataAccessSection
{
    public interface IMessageStorageMigrationRunner
    {
        void MigrateUp(MessageStorageRepositoryContextConfiguration messageStorageRepositoryContextConfiguration);
    }
}