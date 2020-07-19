using System;

namespace MessageStorage.Configurations
{
    public class JobProcessorConfiguration
    {
        public TimeSpan WaitWhenMessageNotFound { get; set; } = TimeSpan.FromSeconds(value: 5);
        public TimeSpan WaitAfterMessageHandled { get; set; } = TimeSpan.Zero;

        public TimeSpan StopTaskTimeout { get; } = TimeSpan.FromSeconds(value: 30);
    }
}