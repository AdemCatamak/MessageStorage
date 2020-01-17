using System;

namespace MessageStorage.Exceptions
{
    public class JobAlreadyExistException : Exception
    {
        public JobAlreadyExistException(string identifier) : base($"Job id already registered #{identifier}")
        {
        }
    }
}