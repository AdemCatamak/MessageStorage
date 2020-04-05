using MessageStorage.Db;

namespace Samples.Db.WebApi.MessageStorageSection
{
    public class MyMessageStorageDbConfiguration : MessageStorageDbConfiguration
    {
        public override string ConnectionStr { get; protected set; } = AppConst.DbConnectionStr;
    }
}