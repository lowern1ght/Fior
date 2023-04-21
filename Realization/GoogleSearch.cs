
using Google.Apis.Services;
using Google.Apis.CustomSearchAPI;
using Google.Apis.CustomSearchAPI.v1;

namespace FiorSearchService.Realization;

public record class GoogleSearch : SearchService {
    public new GoogleServiceConfig ServiceConfig { get; init; }
    public CustomSearchAPIService CustomSearch { get; init; }

    public GoogleSearch(GoogleServiceConfig serviceConfig) {
        ServiceConfig = serviceConfig;
        CustomSearch = new (
            new Google.Apis.Services.BaseClientService.Initializer() { 
                ApiKey = ServiceConfig.ApiKey,
            });
    }

    public override async Task<IEnumerable<Google.Apis.CustomSearchAPI.v1.Data.Result>?> Search(string textSearch, ushort matchPercentage = 80) {
        var listRequare = CustomSearch.Cse.List();
        listRequare.Num = this.ServiceConfig.NumPageSize;
        listRequare.Cx = ServiceConfig.CxId ?? throw new ArgumentNullException(nameof(ServiceConfig.CxId));
        listRequare.Q = textSearch;

        var result = await listRequare.ExecuteAsync();
        if (result is null) 
            throw new ArgumentNullException(nameof(result));
        return result.Items;
    }

    public record struct GoogleServiceConfig {
        public String? CxId { get; set; }
        public String? OAuth2 { get; set; } 
        public String? ApiKey { get; set; }
        public UInt16? NumPageSize { get; set; }
    }
}