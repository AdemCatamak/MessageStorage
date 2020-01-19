**MessageStorage**

MessageStorage sizin için sisteminizde oluşan 'Event' ve 'Command' gibi mesajları kaydeder. Eğer bu mesajların oluşmasından sonra çalışmasını istediğiniz işlemleriniz varsa sistem üzerinde bunları da tanımlayabilirsiniz.

Tanımladığınız işlemler mesajın veri tabanına kaydolması ile birlikte sistemde kayıt altına alınır. Bu sebeple yürütülmesini istediğiniz işlemleri kaybetmez ve en az 1 kere yürütüldüğünden emin olabilirsiniz.
 
 **Kullanımı**
 
MessageStorage nuget paketini indirmeniz halinde sadece InMemory desteğine sahip olursunuz. Bunu sadece test amaçlı kullanmaızı öneririz.
MessageStorage.DI nuget paketi ile Microsoft.DependencyInjection yapısını kullanırken size kulaylıklar sağlayacak Extension metodlarına erişebilirsiniz.
MessageStorage.Db.MsSql nuget paketini indirmeniz halinde mesajlarınızı ve görevlerinizi MsSql üzerinde saklamak için gereken bağımlılıklara sahip olacaksınız.
MessageStorage.Db.MsSql.DI nuget paketi ile Microsoft.DependencyInjection yapısını kullanırken size kolaylıklar sağlayacak Extension metodlarına erişebilirsiniz.
  
 **Örnek Başlangıç Sınıfı** 
 
 ```
services.AddMessageStorage(builder =>
{
    builder.AddInMemoryMessageStorage()
           .AddHandlers(new[] {typeof(NoteCreatedEventHandler).Assembly})
           .AddMessageProcessServer();
});
```

MessageStorage ve MessageStorage.DI paketlerine sahip olduğunuz durumda yukarıda kod bloğunu kullanarak MessageStorage sisteminin özelliklerinden yararlanabilirsiniz.

`AddInMemoryMessageStorage` metodu ile sistemin veri depolama alanı olarak, çalışılan bilgisayarın belleğini kullanacağını belirtmiş oluyoruz.
`AddMessageProcessServer`metodu ile tanımlanan görevlerin bu bilgisayarda çalıştırılacağı bir arka plan servisi tanımlıyoruz.
`AddHandlers` diyerek de tanımladığımız görevlerin yer aldığı Assemblyleri MessageStorage paketinde gerekli sınıflara bildirmiş olacağız. Bu şekilde bir mesajı sisteme kaydederken bu mesajın dağıtılacağı Handler sınıflarına ait bilgiler de sisteme kayıt edilmiş olacak.

Bu adımların ardından `IMessageStorageClient` interface'i üzerinden bir obje elde ederek dilediğiniz yerde kullanabilirsiniz.

```
var noteCreatedEvent = new NoteCreatedEvent
    {
        Id = postNoteHttpRequest.Identifier,
        Note = postNoteHttpRequest.Content
    };
_messageStorageClient.Add(noteCreatedEvent);
```

Eğer mesajlarınızı ve görevlerinizi kayıt altına almak istediğiniz ortam MsSql olacaksa aşağıdaki kod örneğini kullanabilirsiniz.

 ```
MessageStorageDbConfiguration messageStorageDbConfiguration = MessageStorageDbConfigurationFactory.Create(connectionStr);
services.AddMessageStorage(messageStorageServiceCollection =>
{
    messageStorageServiceCollection.AddMessageProcessServer()
                                   .AddMsSqlMessageStorage(messageStorageDbConfiguration)
                                   .AddHandlers(new[] {typeof(Startup).Assembly});
});
```

Bu örnekte Storage alanını belirlemek için kullandığımız satır değişmiştir. MessageStorageDbConfiguration sınıfına ait bir obje ile gerekli ayarları oluşturmamış gerekmektedir. conection string bu ayarlardan olmazsa olmaz değerimizdir. Ayrıca dilerseniz schema bilgisini de belirleyebilirsiniz.

Ayarlamaları tamamladıktan sonra `IMessageStorageDbClient` veya `_IMessageStorageClient` interface implementsyonu olan sınıflardan istediğinizi kullanabilirsiniz. Eğer kendineze ait bir Transaction ile kayıt işlemi yürütmek istiyosanız `IMessageStorageDbClient` interface implementasyonu bir objeyi kullanmalısınız.

```
var noteCreatedEvent = new NoteCreatedEvent
    {
        Id = postNoteHttpRequest.Identifier,
        Note = postNoteHttpRequest.Content
    };

I ) _messageStorageDbClient.Add(noteCreatedEvent);
II) _messageStorageDbClient.Add(noteCreatedEvent, dbTransaction); 
```

