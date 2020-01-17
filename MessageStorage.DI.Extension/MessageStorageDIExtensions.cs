﻿using System;
using Microsoft.Extensions.DependencyInjection;

namespace MessageStorage.DI.Extension
{
    public static class MessageStorageDIExtensions
    {
        public static void AddMessageStorage(this IServiceCollection serviceCollection, Action<IMessageStorageServiceCollection> messageStorageServiceCollection)
        {
            var m = new MessageStorageServiceCollection(serviceCollection);
            messageStorageServiceCollection.Invoke(m);
        }
    }
}