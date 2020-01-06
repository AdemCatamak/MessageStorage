using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db
{
    public interface IDbStorageAdaptor : IStorageAdaptor
    {
        void Add(Message message, IEnumerable<Job> jobs, IDbTransaction dbTransaction);
    }
}