# MessageStorage

**Appveyor**
![AppVeyor](https://img.shields.io/appveyor/ci/ademcatamak/messagestorage/master.svg) ![AppVeyor tests](https://img.shields.io/appveyor/tests/ademcatamak/messagestorage/master.svg)

**Travis**
![Travis (.com)](https://travis-ci.com/AdemCatamak/MessageStorage.svg?branch=master)

**GitHub**
![.github/workflows/github.yml](https://github.com/AdemCatamak/MessageStorage/workflows/.github/workflows/github.yml/badge.svg?branch=master)

**Nuget Versions**

MessageStorage.Db.MsSql : ![Nuget](https://img.shields.io/nuget/v/MessageStorage.Db.MsSql.svg)

MessageStorage.Db.MsSql.DI.Extension : ![Nuget](https://img.shields.io/nuget/v/MessageStorage.Db.MsSql.DI.Extension.svg)

MessageStorage.AspNetCore : ![Nuget](https://img.shields.io/nuget/v/MessageStorage.AspNetCore.svg)

**MessageStorage**

MessageStorage registers messages that are created on the system such as Event and Command. If there are any jobs that are wished to be handled after the creation of these messages, it is possible to define those jobs on the system as well.

Defined jobs are registered into the system along with a message being registered. Therefore, jobs to be handled are not lost and they are executed at least once.

 **Usage**
 
You have the required dependencies to register your messages and jobs on MsSql by downloading MessageStorage.Db.MsSql nuget package.
 
You can access extension methods that help you with Microsoft.DependencyInjection by using MessageStorage.Db.MsSql.DI nuget package. By using this nuget package, you can manage MessageStorage.Db.MsSql dependencies.
 
 **Sample Startup** 
 
  ```
var dbRepositoryConfiguration = new DbRepositoryConfiguration(connectionStr);

var handlers = new List<Handler>
                 {
                     new AccountEventHandler(),
                     new AccountCreatedEventHandler()
                 };

services.AddMessageStorage(collection =>
    {
        collection.AddMessageStorageSqlServerClient(dbRepositoryConfiguration, handlers);
        collection.AddSqlServerJobProcessor(dbRepositoryConfiguration,handlers);
        collection.AddMessageStorageSqlServerMonitor(dbRepositoryConfiguration);
    });

services.AddJobProcessorHostedService();
 ```


`AddMessageStorageSqlServerClient` method lets you introduce MsSql Server is used for system's data storage.

`AddSqlServerJobProcessor` method lets you introduce a predefined background service to the system. This service executes defined jobs on the system.

`AddMessageStorageSqlServerMonitor` method lets you introduce monitor object for Sql Server. Monitor allow you take count of jobs that registered.

After these steps, you can use the object that is an implementation of `IMessageStorageDbClient` interface.


__Example of registering user and saving AccountCreatedEvent message in the same transaction.__

```
public AccountController(IMessageStorageDbClient messageStorageDbClient, AccountDbContext accountDbContext)
{
    _messageStorageDbClient = messageStorageDbClient;
    _accountDbContext = accountDbContext;
}

...

[HttpPost("")]
public IActionResult PostAccount([FromBody] string email)
{
    var accountModel = new AccountModel(email);
    using (DbTransaction transaction = _accountDbContext.Database.BeginTransaction(IsolationLevel.ReadCommitted).GetDbTransaction())
    {
        _accountDbContext.Accounts.Add(accountModel);
        _accountDbContext.SaveChanges();

        _messageStorageDbClient.UseTransaction(transaction);
        var accountCreatedEvent = new AccountCreatedEvent
                                  {
                                      SampleModelId = accountModel.Id,
                                      Email = email
                                  };

        _messageStorageDbClient.Add(accountCreatedEvent);

        transaction.Commit();
    }

    return StatusCode((int) HttpStatusCode.Created);
}
```

