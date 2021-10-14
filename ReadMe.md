# Message Storage

MessageStorage is a library prepared to be used in projects that want to apply the **Outbox** design pattern.

| Platform | Status |
| ------- | ----- |
| `Travis` | ![Travis](https://travis-ci.com/AdemCatamak/MessageStorage.svg?branch=master) |
| `GitHub` | ![.github/workflows/github.yml](https://github.com/AdemCatamak/MessageStorage/workflows/.github/workflows/github.yml/badge.svg?branch=master) |


| NuGet Package Name | Version |
| ------- | ----- |
| MessageStorage.AspNetCore | ![Nuget](https://img.shields.io/nuget/v/MessageStorage.AspNetCore.svg) | 
| MessageStorage.SqlServer | ![Nuget](https://img.shields.io/nuget/v/MessageStorage.SqlServer.svg) |
| MessageStorage.SqlServer.DependencyInjection | ![Nuget](https://img.shields.io/nuget/v/MessageStorage.SqlServer.DependencyInjection.svg) |
| MessageStorage.Postgres | ![Nuget](https://img.shields.io/nuget/v/MessageStorage.Postgres.svg) |
| MessageStorage.Postgres.DependencyInjection | ![Nuget](https://img.shields.io/nuget/v/MessageStorage.Postgres.DependencyInjection.svg) |
| MessageStorage.Integration.MassTransit | ![Nuget](https://img.shields.io/nuget/v/MessageStorage.Integration.MassTransit.svg) |

## Structure Overview

<img src="./Doc/MessageStorage.png" alt="message-storage structure overview">

## Getting Started

You can download the _MessageStorage.SqlServer.DependencyInjection_ or _MessageStorage.Postgres.DependencyInjection_ package
according to the storage environment you will use.

`UseSqlServer` / `UsePostgres` method lets you introduce SqlServer or Postgres is used for system's data storage.

`Register` method lets you introduce MessageHandlers that is used. When the message is recorded, the tasks that will be
executed in the background will be introduced through these classes.

`AddMessageStoragePrerequisiteExecutor` method lets you execute a predefined prerequisites like db migrations.

`AddMessageStorageJobProcessorHostedService` method lets you introduce a predefined background service to the system. This
service fetches tasks from db and executes.

### Sample Startup

 ```
 services.AddMessageStorage(configurator =>
 {
    configurator.UseSqlServer("SqlServerConnectionString");
    configurator.Register(messageHandlerAssemblies);
 })
 // order is important
 .AddMessageStoragePrerequisiteExecutor()
 .AddMessageStorageJobDispatcher();
 ```

After these steps, you can use the object that is an implementation of `IMessageStorageClient` interface.

### Sample Service

Example of registering SomeEntity and saving SomeEntityCreatedEvent message in the same transaction.

```
using (IDbConnection connection = _connectionFactory.CreateConnection())
{
    using IDbTransaction dbTransaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
    await connection.ExecuteAsync( sqlCommandText, sqlCommandParameters, dbTransaction);

    SomeEntityCreated someEntityCreated = new (someEntity.Id, someEntity.SomeProperty, someEntity.CreatedOn); 
    await _messageStorageClient.AddMessageAsync(someEntityCreated, dbTransaction);

    dbTransaction.Commit();
}
```
