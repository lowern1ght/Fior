namespace FiorSearchService;

public abstract record class SearchService {
    public String? ApiKey { get; set; }

    public abstract async Task Search(String textSearch, UInt16 matchPercentage = 80);
}
