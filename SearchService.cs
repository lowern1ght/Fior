namespace FiorSearchService;

public abstract record class SearchService {
    public virtual Object ServiceConfig { get; init; }
    public abstract Task Search(String textSearch, UInt16 matchPercentage = 80);
}
