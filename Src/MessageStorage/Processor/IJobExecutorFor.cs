using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageStorage.DataAccessLayer;
using MessageStorage.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MessageStorage.Processor;

public interface IJobExecutorFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    Task ExecuteAsync(Job job, CancellationToken? cancellationToken = null);
}

internal class JobExecutorFor<TMessageStorageClient> : IJobExecutorFor<TMessageStorageClient>
    where TMessageStorageClient : IMessageStorageClient
{
    private readonly IServiceProvider _serviceProvider;

    public JobExecutorFor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(Job job, CancellationToken? cancellationToken = null)
    {
        IServiceScope? scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<JobExecutorOptionsFor<TMessageStorageClient>>>();
        var messageHandlerMetaDataHolder = scope.ServiceProvider.GetRequiredService<IMessageHandlerMetaDataHolderFor<TMessageStorageClient>>();
        var repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContextFor<TMessageStorageClient>>();
        var jobExecutorOptionsFor = scope.ServiceProvider.GetRequiredService<JobExecutorOptionsFor<TMessageStorageClient>>();
        Type? messageHandlerType = messageHandlerMetaDataHolder.MetaDataCollection
                                                               .FirstOrDefault(metaData => metaData.MessageHandlerType.FullName == job.MessageHandlerTypeName)
                                                              ?.MessageHandlerType;

        using CancellationTokenSource? cancellationTokenSource = CancellationTokenSource(cancellationToken, jobExecutorOptionsFor);

        if (messageHandlerType == null)
        {
            job.SetFailed($"{job.MessageHandlerTypeName} could not found");
            await TryUpdateJobStatusAsync(job, repositoryContext, logger, cancellationTokenSource.Token);
            return;
        }

        if (scope.ServiceProvider.GetService(messageHandlerType) is not IMessageHandler messageHandler)
        {
            job.SetFailed($"{job.MessageHandlerTypeName} could not resolve");
            await TryUpdateJobStatusAsync(job, repositoryContext, logger, cancellationTokenSource.Token);
            return;
        }

        try
        {
            var messageContext = new MessageContext<object>(job.Id.ToString(), job.Message.Payload);
            await messageHandler.BaseHandleOperationAsync(messageContext, cancellationTokenSource.Token);
            job.SetCompleted();
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.Message);
            job.SetFailed(e.Message);
        }

        await TryUpdateJobStatusAsync(job, repositoryContext, logger, cancellationTokenSource.Token);
    }

    private static CancellationTokenSource CancellationTokenSource(CancellationToken? cancellationToken, JobExecutorOptionsFor<TMessageStorageClient> jobExecutorOptionsFor)
    {
        var jobExecutionCancellationToken = new CancellationTokenSource(jobExecutorOptionsFor.JobExecutionMaxDuration);
        CancellationTokenSource? linkedCancellationToken = cancellationToken != null
                                                               ? System.Threading.CancellationTokenSource.CreateLinkedTokenSource(jobExecutionCancellationToken.Token, cancellationToken.Value)
                                                               : System.Threading.CancellationTokenSource.CreateLinkedTokenSource(jobExecutionCancellationToken.Token);

        return linkedCancellationToken;
    }

    private async Task TryUpdateJobStatusAsync(Job job, IRepositoryContextFor<TMessageStorageClient> repositoryContext, ILogger<JobExecutorOptionsFor<TMessageStorageClient>> logger, CancellationToken cancellationToken)
    {
        try
        {
            IJobRepository? jobRepository = repositoryContext.GetJobRepository();
            await jobRepository.UpdateStatusAsync(job, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Message}", e.Message);
        }
    }
}