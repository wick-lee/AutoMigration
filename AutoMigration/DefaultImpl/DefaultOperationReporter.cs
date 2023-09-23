using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.Extensions.Logging;

namespace Wick.AutoMigration.DefaultImpl;

public class DefaultOperationReporter : IOperationReporter
{
    private readonly ILogger _logger;

    public DefaultOperationReporter(ILogger logger)
    {
        _logger = logger;
    }

    void IOperationReporter.WriteError(string message) => _logger.LogError(message);

    void IOperationReporter.WriteWarning(string message) => _logger.LogWarning(message);

    void IOperationReporter.WriteInformation(string message) => _logger.LogInformation(message);

    void IOperationReporter.WriteVerbose(string message) => _logger.LogTrace(message);
}