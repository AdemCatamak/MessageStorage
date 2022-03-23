using System;
using MessageStorage.Exceptions;

namespace MessageStorage.Processor;

public record JobExecutorOptionsFor<TMessageClient>
{
    private TimeSpan _jobExecutionMaxDuration = TimeSpan.FromSeconds(120);

    public TimeSpan JobExecutionMaxDuration
    {
        get => _jobExecutionMaxDuration;
        set
        {
            if (value < TimeSpan.Zero)
            {
                throw new ParameterException($"{nameof(JobExecutorOptionsFor<TMessageClient>)}.{nameof(JobExecutionMaxDuration)}");
            }

            _jobExecutionMaxDuration = value;
        }
    }
}