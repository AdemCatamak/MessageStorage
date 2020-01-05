using System;

namespace MessageStorage.Exceptions
{
    public class JobAddOperationFailedException : Exception
    {
        public JobAddOperationFailedException(string message = null) : base(message)
        {
        }
    }
}