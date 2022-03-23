using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MessageStorage.Processor;

public interface IJobQueue
{
    void Enqueue(Job job);
    Task StartDequeue(CancellationToken cancellationToken);
}

public interface IJobQueueFor<TMessageStorageClient> : IJobQueue
    where TMessageStorageClient : IMessageStorageClient
{
}

internal class JobQueueFor<TMessageStorageClient> : IJobQueueFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    private readonly ChannelWriter<Job> _channelWriter;
    private readonly ChannelReader<Job> _channelReader;
    private readonly IJobExecutorFor<TMessageStorageClient> _jobExecutor;

    public JobQueueFor(ChannelWriter<Job> channelWriter, ChannelReader<Job> channelReader, IJobExecutorFor<TMessageStorageClient> jobExecutor)
    {
        _channelWriter = channelWriter;
        _channelReader = channelReader;
        _jobExecutor = jobExecutor;
    }

    public void Enqueue(Job job)
    {
        _channelWriter.TryWrite(job);
    }

    public async Task StartDequeue(CancellationToken cancellationToken)
    {
        while (await _channelReader.WaitToReadAsync(cancellationToken))
        {
            Job job = await _channelReader.ReadAsync(cancellationToken);
            await _jobExecutor.ExecuteAsync(job, cancellationToken);
        }
    }
}