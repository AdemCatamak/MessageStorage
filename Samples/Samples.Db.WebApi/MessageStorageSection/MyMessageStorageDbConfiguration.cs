using MessageStorage.Db;

namespace Samples.Db.WebApi.MessageStorageSection
{
    public class MyMessageStorageDbConfiguration : MessageStorageDbConfiguration
    {
        public override string ConnectionStr { get; protected set; }
            = "Server=localhost,1433;Database=TestDb;User=sa;Password=kHyjGp7JH5;Trusted_Connection=False;";
    }
}