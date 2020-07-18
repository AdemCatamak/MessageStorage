using System;
using System.Collections.Generic;
using System.Linq;
using MessageStorage.Configurations;
using MessageStorage.Models;

namespace MessageStorage.Clients.Imp
{
    public abstract class MessageStorageClient : IMessageStorageClient
    {
        private readonly IHandlerManager _handlerManager;
        private readonly MessageStorageConfiguration _messageStorageConfiguration;

        protected MessageStorageClient(IHandlerManager handlerManager, MessageStorageConfiguration messageStorageConfiguration = null)
        {
            _handlerManager = handlerManager ?? throw new ArgumentNullException(nameof(handlerManager));
            _messageStorageConfiguration = messageStorageConfiguration ?? new MessageStorageConfiguration();
        }

        protected Tuple<Message, IEnumerable<Job>> PrepareMessageAndJobs(object payload, bool autoJobCreator)
        {
            var message = new Message(payload);
            var jobs = new List<Job>();
            if (autoJobCreator)
            {
                IEnumerable<string> availableHandlerNames = _handlerManager.GetAvailableHandlerNames(payload);

                jobs = availableHandlerNames.Select(handlerName => new Job(handlerName, message))
                                            .ToList();
            }

            return new Tuple<Message, IEnumerable<Job>>(message, jobs);
        }

        public Tuple<Message, IEnumerable<Job>> Add<T>(T payload)
        {
            return Add(payload, _messageStorageConfiguration.AutoJobCreation);
        }

        public abstract Tuple<Message, IEnumerable<Job>> Add<T>(T payload, bool autoJobCreation);
    }
}