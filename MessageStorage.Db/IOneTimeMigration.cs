namespace MessageStorage.Db
{
    public interface IOneTimeMigration : IMigration
    {
        int VersionNumber { get; }
    }
}