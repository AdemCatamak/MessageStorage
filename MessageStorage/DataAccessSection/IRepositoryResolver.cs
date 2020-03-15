namespace MessageStorage.DataAccessSection
{
    public interface IRepositoryResolver
    {
        T Resolve<T>() where T : IRepository;
    }
}