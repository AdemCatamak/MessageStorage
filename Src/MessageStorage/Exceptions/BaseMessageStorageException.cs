using System;

namespace MessageStorage.Exceptions;

public abstract class BaseMessageStorageException : Exception
{
    protected BaseMessageStorageException(string message)
        : base(message)
    {
    }
}