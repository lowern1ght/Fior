﻿namespace FiorSearchService.Interfaces;

public abstract class SearchService {
    public virtual SearchServiceConfig ServiceConfig { get; init; }

    public virtual Task GetReultAsync(string textSearch)
        => throw new NotImplementedException();

    public SearchService(SearchServiceConfig serviceConfig) {
        ServiceConfig = serviceConfig;
    }
}
