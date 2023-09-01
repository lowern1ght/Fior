using FiorSearchService.Models;

namespace FiorSearchService.Interfaces;

public interface IParserService
{
    string UrlGet { get; init; }
    Task<Event> GetEventAsync();
}