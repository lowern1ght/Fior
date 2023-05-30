namespace FiorSearchService.Interfaces;

public abstract class SearchService {

    public SearchService(SearchServiceConfig serviceConfig)
    {
        ServiceConfig = serviceConfig;
    }
    public virtual SearchServiceConfig ServiceConfig { get; init; }

    public virtual Task GetResultAsync(string textSearch)
    {
        throw new NotImplementedException();
    }
}
