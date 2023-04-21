namespace FiorSearchService;

public abstract record class SearchService {
    public virtual SearchServiceConfig ServiceConfig { get; init; }

    public virtual Task SearchAsync(String textSearch)
        => throw new NotImplementedException();

    public SearchService(SearchServiceConfig serviceConfig) {
        ServiceConfig = serviceConfig;
    }
}

public record class SearchServiceConfig {
    public String? Cx { get; set; }
    public String? ApiKey { get; set; }
    public UInt16? ElementCount { get; set; }
}