using System;
using System.Collections.Generic;
using MessageStorage.Models;

namespace MessageStorage.Clients
{
    public interface IMessageStorageClient
    {
        Tuple<Message, IEnumerable<Job>> Add<T>(T payload);
        Tuple<Message, IEnumerable<Job>> Add<T>(T payload, bool autoJobCreation);
    }
}