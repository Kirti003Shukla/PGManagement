namespace PGManagement.API.Services;

public sealed class MachineBusyException : Exception
{
    public MachineBusyException(string message) : base(message)
    {
    }
}
