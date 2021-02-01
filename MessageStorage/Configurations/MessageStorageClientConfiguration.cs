namespace MessageStorage.Configurations
{
    public class MessageStorageClientConfiguration
    {
        public bool AutoJobCreation { get; private set; }

        public MessageStorageClientConfiguration(bool autoJobCreation = true)
        {
            AutoJobCreation = autoJobCreation;
        }
    }
}