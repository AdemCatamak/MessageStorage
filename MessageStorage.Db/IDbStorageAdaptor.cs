using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db
{
    public interface IDbStorageAdaptor : IStorageAdaptor
    {
        Message Add(Message message, IEnumerable<Job> jobs, IDbTransaction dbTransaction);
    }
}