namespace FiorSearchService.Interfaces;

public abstract record class SearchService {
    public virtual SearchServiceConfig ServiceConfig { get; init; }

    public virtual Task SearchAsync(string textSearch)
        => throw new NotImplementedException();

    public SearchService(SearchServiceConfig serviceConfig) {
        ServiceConfig = serviceConfig;
    }
}
