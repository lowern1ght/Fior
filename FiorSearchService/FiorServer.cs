using Serilog;
using Serilog.Core;
using Serilog.Events;
using FiorSearchService.Interfaces;
using FiorSearchService.Models;

namespace FiorSearchService;

internal static class DefaultFiorValues
{
    public static readonly TimeSpan DefaultDuration = TimeSpan.FromMinutes(1);
}

public class FiorServer
{
    private readonly Logger _logger;
    private readonly IParserService[] _parserServices;

    public event Action<IReadOnlyCollection<Event>>? HandlerEventsUpdate;

    public FiorServer(IParserService[] parserServices, 
        LogEventLevel eventLevel = LogEventLevel.Information)
    {
        if (parserServices.Length == 0)
        {
            throw new ArgumentException("Parser services is empty", nameof(parserServices));
        }
        _parserServices = parserServices;
        _logger = CreateDefaultLogger(eventLevel);
    }
    
    public async void Run(TimeSpan? duration = null)
    {
        while (true)
        {
            try
            {
                var eventsList = new List<Event>();
                foreach (var service in _parserServices)
                {
                    eventsList.Add(await service.GetEventAsync());
                }
                
                OnHandlerEventsUpdate(eventsList);
                Thread.Sleep(duration ?? DefaultFiorValues.DefaultDuration);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                return;
            }
        }
    }
    
    private Logger CreateDefaultLogger(LogEventLevel eventLevel)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Is(eventLevel)
            .WriteTo.Console()
            .CreateLogger();
    }

    protected virtual void OnHandlerEventsUpdate(IReadOnlyCollection<Event> collection)
    {
        HandlerEventsUpdate?.Invoke(collection);
    }
}