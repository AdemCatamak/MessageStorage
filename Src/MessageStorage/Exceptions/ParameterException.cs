namespace MessageStorage.Exceptions;

public class ParameterException : BaseMessageStorageException
{
    public ParameterException(string nameOfParameter) : base($"{nameOfParameter} is invalid")
    {
    }
}