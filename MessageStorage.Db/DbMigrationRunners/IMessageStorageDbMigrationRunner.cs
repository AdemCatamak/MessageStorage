using MessageStorage.Db.Configurations;

namespace MessageStorage.Db.DbMigrationRunners
{
    public interface IMessageStorageDbMigrationRunner
    {
        void MigrateUp(DbRepositoryConfiguration dbRepositoryConfiguration);
    }
}