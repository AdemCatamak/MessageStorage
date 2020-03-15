namespace MessageStorage.DataAccessSection
{
    public interface IRepository
    {
    }

    public interface IRepository<in TEntity> : IRepository where TEntity : class
    {
        void Add(TEntity entity);
    }
}