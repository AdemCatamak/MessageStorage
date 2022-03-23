using System;

namespace MessageStorage.Exceptions;

public class ArgumentNotCompatibleException : BaseMessageStorageException
{
    public ArgumentNotCompatibleException(Type actualType, Type expectedType)
        : base($"{expectedType.FullName} is expected. Supplied type is {actualType.FullName}")
    {
    }
}