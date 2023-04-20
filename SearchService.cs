using FiorSearchService.Interfaces;

namespace FiorSearchService;

public abstract record class SearchService : ISearchService {
    public String URISite { get; init; }
    public String? ApiKey { get; set; }

    public SearchService(String uriSite, String? apiKey) {
        this.URISite = uriSite; this.ApiKey = apiKey;
    }

    public Task Search(string textSearch, ushort matchPercentage) {
        throw new NotImplementedException();
    }
}
