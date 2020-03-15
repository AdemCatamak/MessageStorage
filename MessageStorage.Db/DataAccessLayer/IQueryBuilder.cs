using System.Collections.Generic;
using System.Data;

namespace MessageStorage.Db.DataAccessLayer
{
    public interface IQueryBuilder
    {
    }

    public interface IQueryBuilder<in T> : IQueryBuilder
    {
        (string, IEnumerable<IDbDataParameter>) Add(T entity);
    }
}