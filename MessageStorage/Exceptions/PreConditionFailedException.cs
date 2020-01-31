using System;

namespace MessageStorage.Exceptions
{
    public class PreConditionFailedException : Exception
    {
        public PreConditionFailedException(string message, Exception innerEx = null) : base(message, innerEx)
        {
        }
    }
}