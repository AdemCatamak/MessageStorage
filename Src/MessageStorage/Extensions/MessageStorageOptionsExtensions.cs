using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MessageStorage.MessageHandlers;

namespace MessageStorage.Extensions;

public static class MessageStorageOptionsExtensions
{
    public static void RegisterHandlers<TMessageStorageClient>(this MessageStorageOptionsFor<TMessageStorageClient> messageStorageOptions,
                                                               params Assembly[] assemblies)
        where TMessageStorageClient : IMessageStorageClient
    {
        RegisterHandlers(messageStorageOptions, 10, assemblies);
    }

    public static void RegisterHandlers<TMessageStorageClient>(this MessageStorageOptionsFor<TMessageStorageClient> messageStorageOptions,
                                                               int maxRetryCount,
                                                               params Assembly[] assemblies)
        where TMessageStorageClient : IMessageStorageClient
    {
        List<(Type, Type)>? types = GetAllBaseMessageHandlers(assemblies);
        foreach ((Type? messageHandlerType, Type? messageType) in types)
        {
            messageStorageOptions.RegisterHandler(messageHandlerType, messageType, maxRetryCount);
        }
    }

    private static List<(Type, Type)> GetAllBaseMessageHandlers(params Assembly[] assemblies)
    {
        var result = new ConcurrentBag<(Type, Type)>();

        List<Type>? types = assemblies.SelectMany(a => a.GetTypes())
                                      .Where(t => !t.IsInterface && !t.IsAbstract)
                                      .ToList();


        Parallel.ForEach(types, type =>
        {
            if (!IsMessageHandler(type, out Type? messageType)) return;

            if (messageType == null)
            {
                throw new ArgumentNullException(nameof(messageType));
            }

            result.Add(new ValueTuple<Type, Type>(type, messageType));
        });

        return result.ToList();
    }

    private static bool IsMessageHandler(Type examinedType, out Type? messageType)
    {
        Type? baseMessageHandlerDefinition = typeof(BaseMessageHandler<>).GetGenericTypeDefinition();
        messageType = null;

        while (true)
        {
            if (examinedType.IsGenericType && examinedType.GetGenericTypeDefinition() == baseMessageHandlerDefinition)
            {
                messageType = examinedType.GetGenericArguments().First();
                return true;
            }

            if (examinedType.BaseType != null)
            {
                examinedType = examinedType.BaseType;
                continue;
            }

            break;
        }

        return false;
    }
}