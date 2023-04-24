﻿using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using FiorSearchService.Interfaces;
using Google.Apis.CustomSearchAPI.v1;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace FiorSearchService;

public record class GoogleSearch : SearchService {
    private HttpClient HttpClient { get; init; }
    public CustomSearchAPIService CustomSearch { get; init; }

    private const string PatternImgSrc = @"<img\s[^>]*?src\s*=\s*['\""]([^'\""]*?)['\""][^>]*?>";

    public GoogleSearch(SearchServiceConfig serviceConfig) : base(serviceConfig) {
        HttpClient = new HttpClient();
        CustomSearch = new(
            new Google.Apis.Services.BaseClientService.Initializer() {
                ApiKey = ServiceConfig.ApiKey,
            });
    }

    /// <summary>Get item list before search in google service</summary>
    /// <param name="textSearch"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public override async Task<IList<Google.Apis.CustomSearchAPI.v1.Data.Result>?> GetReultAsync(string textSearch) {
        var listRequare = CustomSearch.Cse.List();
        listRequare.Num = ServiceConfig.ElementCount;
        listRequare.Cx = ServiceConfig.Cx ?? throw new ArgumentNullException(nameof(ServiceConfig.Cx));
        listRequare.Q = textSearch;

        var result = await listRequare.ExecuteAsync();
        if (result is null)
            throw new ArgumentNullException(nameof(result));
        return result.Items;
    }

    public async Task<IEnumerable<PossibleAttributesProduct?>> GetPossibleAttributesProductAsync(IList<Google.Apis.CustomSearchAPI.v1.Data.Result> items) {
        foreach (var item in items) {
            var result = await CompletePossibleAttributesProductAsync(item);
        }

        throw new NotImplementedException();
    }

    private async Task<PossibleAttributesProduct?> CompletePossibleAttributesProductAsync(Google.Apis.CustomSearchAPI.v1.Data.Result item) {
        PossibleAttributesProduct possibleAttributes = new() {
            SiteName = item.Title 
        };

        String response = await GetResponseHtmlFromWebsiteAsync(item.Link);
        var matchCollection = Regex.Matches(response, PatternImgSrc);
        possibleAttributes.UriImages = matchCollection.Select(s
            => new Uri(s.Value))
                .ToList();

        return new PossibleAttributesProduct();
    }

    private async Task<String> GetResponseHtmlFromWebsiteAsync(String uriWebsite) {
        var response = await HttpClient.GetStringAsync(uriWebsite);
        if (response is null)
            throw new ArgumentNullException(nameof(response));
        return response;
    }

    private Task<IEnumerable<String>> JTokenToStrings(JArray array) {
        throw new NotImplementedException();
    }

    public record struct PossibleAttributesProduct {
        public String SiteName { get; init; }
        public List<String> Text { get; set; }
        public List<Uri> UriImages { get; set; }
    }
}