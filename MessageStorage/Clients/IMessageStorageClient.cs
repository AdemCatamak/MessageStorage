using System;
using System.Collections.Generic;
using System.Data;
using MessageStorage.DataAccessSection;
using MessageStorage.Models;

namespace MessageStorage.Clients
{
    public interface IMessageStorageClient
    {
        Tuple<Message, IEnumerable<Job>> Add<T>(T payload);
        Tuple<Message, IEnumerable<Job>> Add<T>(T payload, bool autoJobCreation);
        
        int GetJobCountByStatus(JobStatus jobStatus);
        
        IMessageStorageTransaction UseTransaction(IDbTransaction dbTransaction);
        IMessageStorageTransaction BeginTransaction(IsolationLevel isolationLevel);
    }
}