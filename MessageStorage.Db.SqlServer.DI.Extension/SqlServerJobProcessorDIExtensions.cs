using System;
using System.Collections.Generic;
using MessageStorage.Clients;
using MessageStorage.Clients.Imp;
using MessageStorage.Configurations;
using MessageStorage.Db.Configurations;
using MessageStorage.Db.SqlServer.DataAccessSection;
using MessageStorage.DI.Extension;
using Microsoft.Extensions.Logging;

namespace MessageStorage.Db.SqlServer.DI.Extension
{
    public static class SqlServerJobProcessorDIExtensions
    {
        public static IMessageStorageServiceCollection AddSqlServerJobProcessor
            (this IMessageStorageServiceCollection messageStorageServiceCollection, DbRepositoryConfiguration dbRepositoryConfiguration, Func<IServiceProvider, IEnumerable<Handler>> handlerCollectionFactory, Func<IServiceProvider, ILogger<IJobProcessor>> loggerFactory = null, JobProcessorConfiguration jobProcessorConfiguration = null)
        {
            return messageStorageServiceCollection.AddJobProcessor<IJobProcessor>(provider =>
                                                                                  {
                                                                                      var sqlServerDbConnectionFactory = new SqlServerDbConnectionFactory();
                                                                                      var messageStorageDbClient
                                                                                          = new JobProcessor(() => new SqlServerDbRepositoryContext(dbRepositoryConfiguration, sqlServerDbConnectionFactory),
                                                                                                             new HandlerManager(handlerCollectionFactory.Invoke(provider)),
                                                                                                             loggerFactory?.Invoke(provider),
                                                                                                             jobProcessorConfiguration);
                                                                                      return messageStorageDbClient;
                                                                                  });
        }

        public static IMessageStorageServiceCollection AddSqlServerJobProcessor
            (this IMessageStorageServiceCollection messageStorageServiceCollection, DbRepositoryConfiguration dbRepositoryConfiguration, IEnumerable<Handler> handlers, ILogger<IJobProcessor> logger = null, JobProcessorConfiguration jobProcessorConfiguration = null)
        {
            return AddSqlServerJobProcessor(messageStorageServiceCollection, dbRepositoryConfiguration, new HandlerManager(handlers), logger, jobProcessorConfiguration);
        }

        public static IMessageStorageServiceCollection AddSqlServerJobProcessor
            (this IMessageStorageServiceCollection messageStorageServiceCollection, DbRepositoryConfiguration dbRepositoryConfiguration, IHandlerManager handlerManager, ILogger<IJobProcessor> logger = null, JobProcessorConfiguration jobProcessorConfiguration = null)
        {
            return messageStorageServiceCollection.AddJobProcessor<IJobProcessor>(provider =>
                                                                                  {
                                                                                      var sqlServerDbConnectionFactory = new SqlServerDbConnectionFactory();
                                                                                      var messageStorageDbClient
                                                                                          = new JobProcessor(() => new SqlServerDbRepositoryContext(dbRepositoryConfiguration, sqlServerDbConnectionFactory),
                                                                                                             handlerManager,
                                                                                                             logger,
                                                                                                             jobProcessorConfiguration);
                                                                                      return messageStorageDbClient;
                                                                                  });
        }
    }
}