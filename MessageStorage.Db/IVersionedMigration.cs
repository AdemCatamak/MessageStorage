namespace MessageStorage.Db
{
    public interface IVersionedMigration : IMigration
    {
        int VersionNumber { get; }
    }
}