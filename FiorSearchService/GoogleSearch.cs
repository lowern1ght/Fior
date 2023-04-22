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

    /// <summary>Get item list before search in google service</summary>
    /// <param name="textSearch"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public override async Task<IEnumerable<Google.Apis.CustomSearchAPI.v1.Data.Result>?> GetReultAsync(string textSearch) {
        var listRequare = CustomSearch.Cse.List();
        listRequare.Num = ServiceConfig.ElementCount;
        listRequare.Cx = ServiceConfig.Cx ?? throw new ArgumentNullException(nameof(ServiceConfig.Cx));
        listRequare.Q = textSearch;

        var result = await listRequare.ExecuteAsync();
        if (result is null)
            throw new ArgumentNullException(nameof(result));
        return result.Items;
    }

    public async Task<PossibleAttributesProduct> GetPossibleAttributesProductAsync() {
        //TODO: реализовать создание структуры
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Uri>> GetUriImagesAsync(IEnumerable<Google.Apis.CustomSearchAPI.v1.Data.Result> items) {
        List<Uri> images = new List<Uri>();
        foreach (var item in items) {
            
        }

        throw new NotImplementedException();
    }

    public record struct PossibleAttributesProduct {
        public IEnumerable<String> Text { get; init; }
        public IEnumerable<Uri> UriImages { get; init; }
    }
}