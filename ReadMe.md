# MessageStorage

**Appveyor**
![AppVeyor](https://img.shields.io/appveyor/ci/ademcatamak/messagestorage/master.svg) ![AppVeyor tests](https://img.shields.io/appveyor/tests/ademcatamak/messagestorage/master.svg)

**Travis**
![Travis (.com)](https://travis-ci.com/AdemCatamak/MessageStorage.svg?branch=master)

**GitHub**
![.github/workflows/github.yml](https://github.com/AdemCatamak/MessageStorage/workflows/.github/workflows/github.yml/badge.svg?branch=master)

**Nuget Versions**

MessageStorage : ![Nuget](https://img.shields.io/nuget/v/MessageStorage.svg)  MessageStorage.DI.Extension : ![Nuget](https://img.shields.io/nuget/v/MessageStorage.DI.Extension.svg)

MessageStorage.Db.MsSql : ![Nuget](https://img.shields.io/nuget/v/MessageStorage.Db.MsSql.svg) MessageStorage.Db.MsSql.DI.Extension : ![Nuget](https://img.shields.io/nuget/v/MessageStorage.Db.MsSql.DI.Extension.svg)

**MessageStorage**

MessageStorage registers messages that are created on the system such as Event and Command. If there are any jobs that are wished to be handled after the creation of these messages, it is possible to define those jobs on the system as well.

Defined jobs are registered into the system along with a message being registered. Therefore, jobs to be handled are not lost and they are executed at least once.

 **Usage**
 
You have the required dependencies to register your messages and jobs on MsSql by downloading MessageStorage.Db.MsSql nuget package.
 
You can access extension methods that help you with Microsoft.DependencyInjection by using MessageStorage.Db.MsSql.DI nuget package. By using this nuget package, you can manage MessageStorage.Db.MsSql dependencies.
 
 **Sample Startup** 
 
  ```
 MessageStorageDbConfiguration messageStorageDbConfiguration = MessageStorageDbConfigurationFactory.Create(connectionStr);
 services.AddMessageStorage(messageStorageServiceCollection =>
 {
     messageStorageServiceCollection.AddMsSqlMessageStorage(messageStorageDbConfiguration)
                                    .AddHandlers(new[] {typeof(Startup).Assembly})
                                    .AddJobProcessServer();
 });
 ```

You can benefit from the MessageStorage package's features by having MessageStorage and MessageStorage.DI packages using the code block above.

`AddMsSqlMessageStorage` method lets you introduce MsSql Server is used for system's data storage.

`AddJobProcessServer` method lets you introduce a predefined background service to the system. This service executes defined jobs on the system.

`AddHandlers` method lets you introduce the jobs you coded to the system. In order to do that, Assembly object that the handler is registered is given as a parameter to the method.

After these steps, you can use the object that is an implementation of `IMessageStorageClient` interface.

```
public NoteController(IMessageStorageClient messageStorageClient)
{
    _messageStorageClient = messageStorageClient;
}

...

[HttpPost]
public IActionResult PostNote([FromBody] PostNoteHttpRequest postNoteHttpRequest)
{
    _messageStorageClient.Add(new NoteCreatedEvent
        {
            Id = postNoteHttpRequest.Identifier,
            Note = postNoteHttpRequest.Content
        });

    return StatusCode((int) HttpStatusCode.Created);
}
```

You can use objects of `IMessageStorageClient` or `IMessageStorageDbClient` interface after implementing necessary configurations. You should use an object of 'IMessageStorageDbClient' interface if you wish to save messages and jobs with a `IDbTransaction` of your own.

```
var noteCreatedEvent = new NoteCreatedEvent
    {
        Id = postNoteHttpRequest.Identifier,
        Note = postNoteHttpRequest.Content
    };

I ) _messageStorageDbClient.Add(noteCreatedEvent);
II) _messageStorageDbClient.Add(noteCreatedEvent, dbTransaction); 
```

