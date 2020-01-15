using System;

namespace MessageStorage.Exceptions
{
    public class JobNotFoundException : Exception
    {
        public JobNotFoundException(string identifier) : base($"Job id could not found #{identifier}")
        {
        }
    }
}