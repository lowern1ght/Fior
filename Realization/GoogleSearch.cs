using FiorSearchService.Interfaces;

namespace FiorSearchService.Realization;

public record class GoogleSearch : SearchService {
    public GoogleSearch(SearchService original) : base(original) { }
    public GoogleSearch(string uriSite, string? apiKey) : base(uriSite, apiKey) { }
}