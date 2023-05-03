using Microsoft.VisualBasic;
using Serilog;

namespace FiorSearchService.Modules;

public class LogService {
    public LogService(LoggingTo loggingTo = 0)
        => Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
}
