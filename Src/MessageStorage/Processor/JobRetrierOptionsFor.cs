using System;
using MessageStorage.Exceptions;

namespace MessageStorage.Processor;

public record JobRetrierOptionsFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    private TimeSpan _waitAfterFullFetch = new TimeSpan(hours: 0, minutes: 0, seconds: 5);

    public TimeSpan WaitAfterFullFetch
    {
        get => _waitAfterFullFetch;
        set
        {
            if (value < TimeSpan.Zero)
            {
                throw new ParameterException($"{nameof(JobRetrierOptionsFor<TMessageStorageClient>)}.{nameof(WaitAfterFullFetch)}");
            }

            _waitAfterFullFetch = value;
        }
    }

    private int _fetchCount = 20;

    public int FetchCount
    {
        get => _fetchCount;
        set
        {
            if (value < 0)
            {
                throw new ParameterException($"{nameof(JobRetrierOptionsFor<TMessageStorageClient>)}.{nameof(FetchCount)}");
            }

            _fetchCount = value;
        }
    }

    private int _concurrency = 4;

    public int Concurrency
    {
        get => _concurrency;
        set
        {
            if (value < 1)
            {
                throw new ParameterException($"{nameof(JobRetrierOptionsFor<TMessageStorageClient>)}.{nameof(Concurrency)}");
            }

            _concurrency = value;
        }
    }
}