using System;

namespace MessageStorage.Configurations
{
    public class JobProcessorConfiguration
    {
        public TimeSpan WaitWhenJobNotFound { get; set; } = TimeSpan.FromMilliseconds(value: 5000);
        public TimeSpan WaitAfterJobHandled { get; set; } = TimeSpan.FromMilliseconds(value: 5);
        public TimeSpan JobProcessDeadline { get; set; } = TimeSpan.FromSeconds(value: 3600);
    }
}