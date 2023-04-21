
using Google.Apis.Services;
using Google.Apis.CustomSearchAPI;
using Google.Apis.CustomSearchAPI.v1;

namespace FiorSearchService.Realization;

public record class GoogleSearch : SearchService {
    public GoogleServiceConfig ServiceConfig { get; init; }
    public CustomSearchAPIService CustomSearch { get; init; }

    public GoogleSearch(String apiKey, GoogleServiceConfig serviceConfig) {
        ServiceConfig = serviceConfig;
        CustomSearch = new (
            new Google.Apis.Services.BaseClientService.Initializer() { 
                ApplicationName = "Fior",
                ApiKey = apiKey
            });
    }

    public override async Task Search(string textSearch, ushort matchPercentage = 80) {
        var listRequare = CustomSearch.Cse.List();
        listRequare.Num = this.ServiceConfig.NumPageSize;
        listRequare.Q = textSearch;

        var result = await listRequare.ExecuteAsync();
        if (result is null) 
            throw new ArgumentNullException(nameof(result));

        await Task.CompletedTask;
    }

    public record struct GoogleServiceConfig {
        public UInt16 NumPageSize { get; set; }
    }
}

public static class Tests {
    public static async Task Test() {
        SearchService googleService = new GoogleSearch(Resources.Resources.GoogleApiKey);
        await googleService.Search("хуй");
    }
}