**MessageStorage**

MessageStorage sizin için sisteminizde oluşan 'Event' ve 'Command' gibi mesajları kaydeder. Eğer bu mesajların oluşmasından sonra çalışmasını istediğiniz görevleriniz varsa sistem üzerinde bunları da tanımlayabilirsiniz.

Tanımladığınız işlemler mesajın kaydolması ile birlikte sistemde kayıt altına alınır. Bu sebeple yürütülmesini istediğiniz işlemleri kaybetmez ve en az 1 kere yürütüleceğinden emin olabilirsiniz.
 
 **Kullanımı**
 
_MessageStorage_ nuget paketini indirmeniz halinde sadece InMemory desteğine sahip olursunuz. Bunu sadece test amaçlı kullanmanız önerilir.
_MessageStorage.DI_ nuget paketi ile Microsoft.DependencyInjection yapısını kullanırken size kolaylıklar sağlayacak Extension metodlara erişebilirsiniz.
_MessageStorage.Db.MsSql_ nuget paketini indirmeniz halinde mesajlarınızı ve görevlerinizi MsSql üzerinde saklamak için gereken bağımlılıklara sahip olacaksınız.
_MessageStorage.Db.MsSql.DI_ nuget paketi ile Microsoft.DependencyInjection yapısını kullanırken size kolaylıklar sağlayacak Extension metodlarına erişebilirsiniz.
  
 **Örnek Başlangıç Sınıfı** 
 
 ```
services.AddMessageStorage(builder =>
{
    builder.AddInMemoryMessageStorage()
           .AddHandlers(new[] {typeof(NoteCreatedEventHandler).Assembly})
           .AddJobProcessServer();
});
```

_MessageStorage_ ve _MessageStorage.DI_ paketlerine sahip olduğunuz durumda yukarıda kod bloğunu kullanarak MessageStorage paketinin özelliklerinden yararlanmaya başlayabilirsiniz.

`AddInMemoryMessageStorage` metodu ile sistemin veri depolama alanı olarak, çalışılan bilgisayarın belleğini kullanacağını belirtilir.
`AddJobProcessServer`metodu ile bir arka plan servisi sisteme tanımlanır. Tanımlanan arka plan servisi kayıt altına alınan mesajlar için tanımlanan görevleri çalıştıracaktır.
`AddHandlers` diyerek de tanımlanan görevlerin yer aldığı Assemblyleri MessageStorage paketinde gerekli sınıflara bildirmek için kullanılır. Bu şekilde bir mesajı sisteme kaydederken bu mesajın dağıtılacağı Handler sınıflarına ait bilgiler de MessageStorage paketi altında kayıt altına alınmış olur.

Bu adımların ardından `IMessageStorageClient` interface'i üzerinden bir obje elde ederek dilediğiniz yerde kullanabilirsiniz.

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

Eğer mesajlarınızı ve görevlerinizi kayıt altına almak istediğiniz ortam MsSql olacaksa, aşağıdaki kod örneğini kullanabilirsiniz.

 ```
MessageStorageDbConfiguration messageStorageDbConfiguration = MessageStorageDbConfigurationFactory.Create(connectionStr);
services.AddMessageStorage(messageStorageServiceCollection =>
{
    messageStorageServiceCollection.AddMsSqlMessageStorage(messageStorageDbConfiguration)
                                   .AddHandlers(new[] {typeof(Startup).Assembly})
                                   .AddJobProcessServer();
});
```

Bu örnekte Storage alanını belirlemek için kullandığımız satır değişmiştir. MessageStorageDbConfiguration sınıfına ait bir obje ile gerekli ayarları oluşturmamış gerekmektedir. `Connection String` bu ayarlardan olmazsa olmaz değerimizdir. Ayrıca dilerseniz `Db Schema` bilgisini de belirleyebilirsiniz.

Ayarlamaları tamamladıktan sonra `IMessageStorageClient` veya `IMessageStorageDbClient` interface tiplerinden birine ait objelerden istediğinizi kullanabilirsiniz. Eğer kendineze ait bir Transaction ile kayıt işlemi yürütmek istiyosanız `IMessageStorageDbClient` interface tipinin implementasyonu bir objeyi kullanmalısınız.

```
var noteCreatedEvent = new NoteCreatedEvent
    {
        Id = postNoteHttpRequest.Identifier,
        Note = postNoteHttpRequest.Content
    };

I ) _messageStorageDbClient.Add(noteCreatedEvent);
II) _messageStorageDbClient.Add(noteCreatedEvent, dbTransaction); 
```

