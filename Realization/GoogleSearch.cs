
using Google.Apis.Services;
using Google.Apis.CustomSearchAPI;
using Google.Apis.CustomSearchAPI.v1;

namespace FiorSearchService.Realization;

public record class GoogleSearch : SearchService {
    public CustomSearchAPIService CustomSearch { get; init; }

    public GoogleSearch(String apiKey) {
        CustomSearch = new (
            new Google.Apis.Services.BaseClientService.Initializer() { ApiKey = apiKey });
    }

    public override async Task Search(string textSearch, ushort matchPercentage = 80) {
        CustomSearch.Cse.List();
    }
}

public static class Tests {
    public static async Task Test() {
        SearchService googleService = new GoogleSearch(Resources.Resources.GoogleApiKey);
        await googleService.Search("");
    }
}