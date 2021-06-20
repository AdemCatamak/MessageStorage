using System;

namespace MessageStorage.Exceptions
{
    public class DuplicateDependencyInjectionException : MessageStorageCustomException
    {
        public DuplicateDependencyInjectionException(Type dependencyType)
            : base($"{dependencyType.FullName} is already injected")
        {
        }
    }
}