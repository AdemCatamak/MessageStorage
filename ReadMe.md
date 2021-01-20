# MessageStorage

**Appveyor**
![AppVeyor](https://img.shields.io/appveyor/ci/ademcatamak/messagestorage/master.svg) ![AppVeyor tests](https://img.shields.io/appveyor/tests/ademcatamak/messagestorage/master.svg)

**Travis**
![Travis (.com)](https://travis-ci.com/AdemCatamak/MessageStorage.svg?branch=master)

**GitHub**
![.github/workflows/github.yml](https://github.com/AdemCatamak/MessageStorage/workflows/.github/workflows/github.yml/badge.svg?branch=master)

**Nuget Versions**

~~MessageStorage.Db.MsSql~~ : ![Nuget](https://img.shields.io/nuget/v/MessageStorage.Db.MsSql.svg)

~~MessageStorage.Db.MsSql.DI.Extension~~ : ![Nuget](https://img.shields.io/nuget/v/MessageStorage.Db.MsSql.DI.Extension.svg)

MessageStorage.SqlServer : ![Nuget](https://img.shields.io/nuget/v/MessageStorage.SqlServer.svg)

MessageStorage.SqlServer.DI.Extension : ![Nuget](https://img.shields.io/nuget/v/MessageStorage.SqlServer.DI.Extension.svg)

MessageStorage.AspNetCore : ![Nuget](https://img.shields.io/nuget/v/MessageStorage.AspNetCore.svg)

**MessageStorage**

MessageStorage registers messages that are created on the system such as Event and Command. If there are any jobs that are wished to be handled after the creation of these messages, it is possible to define those jobs on the system as well.

Defined jobs are registered into the system along with a message being registered. Therefore, jobs to be handled are not lost and they are executed at least once.

 **Usage**
 
You have the required dependencies to register your messages and jobs on SqlServer by downloading MessageStorage.SqlServer nuget package.
 
You can access extension methods that help you with Microsoft.DependencyInjection by using MessageStorage.SqlServer.DI.Extension nuget package. By using this nuget package, you can manage MessageStorage.SqlServer dependencies.
 
 **Sample Startup** 
 

 ```

services.AddMessageStorageHostedService();

services.AddMessageStorage(messageStorage =>
{
    messageStorage.UseSqlServer(connectionStr)
                  .UseHandlers((handlerManager, provider) =>
                               {
                                   // Handler without dependency
                                   handlerManager.TryAddHandler(new HandlerDescription<AccountEventHandler>
                                                                    (() => new AccountEventHandler()));

                                   // Handler with dependency
                                   handlerManager.TryAddHandler(new HandlerDescription<AccountCreatedEventHandler>
                                                                    (() =>
                                                                     {
                                                                         var x = provider.GetRequiredService<AccountDbContext>();
                                                                         return new AccountCreatedEventHandler(x);
                                                                     })
                                                               );
                               });
})
        .WithJobProcessor();

 ```


`UseSqlServer` method lets you introduce SqlServer is used for system's data storage.

`UseHandlers` method lets you introduce Handlers that is used.

`WithJobProcessor` method lets you introduce a predefined background service to the system. This service fetches tasks from db and executes.

`AddMessageStorageHostedService` method inject one hosted service which execute all injected background services.


After these steps, you can use the object that is an implementation of `IMessageStorageClient` interface.

__Example of registering user (with entity-framework) and saving AccountCreatedEvent message in the same transaction.__

```
public AccountController(IMessageStorageClient messageStorageClient, AccountDbContext accountDbContext)
{
    _messageStorageClient = messageStorageClient;
    _accountDbContext = accountDbContext;
}

...

[HttpPost("")]
public IActionResult PostAccount([FromBody] string email)
{
    var accountModel = new AccountModel(email);
    using (DbTransaction transaction = _accountDbContext.Database.BeginTransaction(IsolationLevel.ReadCommitted).GetDbTransaction())
    {
        IMessageStorageTransaction messageStorageTransaction = _messageStorageClient.UseTransaction(transaction);

        _accountDbContext.Accounts.Add(accountModel);
        _accountDbContext.SaveChanges();

        var accountCreatedEvent = new AccountCreatedEvent(accountModel.Id, accountModel.Email);
        _messageStorageClient.Add(accountCreatedEvent);

        messageStorageTransaction.Commit();
    }

    return StatusCode((int) HttpStatusCode.Created);
}
```

__Example of returning job count__

```
public MessageStorageController(IMessageStorageClient messageStorageClient)
{
    _messageStorageClient = messageStorageClient;
}

[HttpGet("{jobStatus}/count")]
public IActionResult Get([FromRoute] JobStatus jobStatus)
{
    int jobCount = _messageStorageClient.GetJobCount(jobStatus);
    return StatusCode((int) HttpStatusCode.OK, jobCount);
}
```

