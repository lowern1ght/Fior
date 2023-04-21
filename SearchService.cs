namespace FiorSearchService;

public abstract record class SearchService {
    public virtual Object ServiceConfig { get; init; }

    public virtual Task SearchAsync(String textSearch)
        => throw new NotImplementedException();
}
