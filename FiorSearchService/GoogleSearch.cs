using FiorSearchService.Interfaces;
using Google.Apis.CustomSearchAPI.v1;

namespace FiorSearchService;

public record class GoogleSearch : SearchService {
    public CustomSearchAPIService CustomSearch { get; init; }

    public GoogleSearch(SearchServiceConfig serviceConfig) : base(serviceConfig) {
        CustomSearch = new(
            new Google.Apis.Services.BaseClientService.Initializer() {
                ApiKey = ServiceConfig.ApiKey,
            });
    }

    public override async Task<IEnumerable<Google.Apis.CustomSearchAPI.v1.Data.Result>?> SearchAsync(string textSearch) {
        var listRequare = CustomSearch.Cse.List();
        listRequare.Num = ServiceConfig.ElementCount;
        listRequare.Cx = ServiceConfig.Cx ?? throw new ArgumentNullException(nameof(ServiceConfig.Cx));
        listRequare.Q = textSearch;

        var result = await listRequare.ExecuteAsync();
        if (result is null)
            throw new ArgumentNullException(nameof(result));
        return result.Items;
    }
}