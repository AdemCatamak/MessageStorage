using System;

namespace MessageStorage.Exceptions
{
    public class RepositoryNotFoundException : Exception
    {
        public RepositoryNotFoundException(Type repositoryType) : base($"{repositoryType.FullName} could not found")
        {
        }
    }
}