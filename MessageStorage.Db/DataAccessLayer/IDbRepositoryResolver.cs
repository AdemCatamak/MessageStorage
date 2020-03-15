using System.Collections.Generic;
using System.Linq;
using MessageStorage.DataAccessSection;
using MessageStorage.Exceptions;

namespace MessageStorage.Db.DataAccessLayer
{
    public interface IDbRepositoryResolver : IRepositoryResolver
    {
    }

    public class DbRepositoryResolver : IDbRepositoryResolver
    {
        private readonly IEnumerable<IDbRepository> _dbRepositories;

        public DbRepositoryResolver(IEnumerable<IDbRepository> dbRepositories)
        {
            _dbRepositories = dbRepositories;
        }

        public T Resolve<T>() where T : IRepository
        {
            var dbRepository = (T) _dbRepositories.FirstOrDefault(repository => repository is T);
            if (dbRepository == null)
                throw new RepositoryNotFoundException(typeof(T));

            return dbRepository;
        }
    }
}