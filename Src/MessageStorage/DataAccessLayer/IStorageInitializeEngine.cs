using System.Threading;
using System.Threading.Tasks;

namespace MessageStorage.DataAccessLayer;

public interface IStorageInitializeEngine
{
    Task InitializeAsync(CancellationToken cancellationToken);
}