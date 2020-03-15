namespace MessageStorage.Db
{
    public abstract class MessageStorageDbConfiguration
    {
        public string Schema { get; private set; } = "MessageStorage";
        public abstract string ConnectionStr { get; protected set; }
    }
}