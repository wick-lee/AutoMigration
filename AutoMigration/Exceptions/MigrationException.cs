namespace Wick.AutoMigration.Exceptions;

public class MigrationException : Exception
{
    public MigrationException() : base()
    {
    }

    public MigrationException(string? message) : base(message)
    {
    }

    public MigrationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}