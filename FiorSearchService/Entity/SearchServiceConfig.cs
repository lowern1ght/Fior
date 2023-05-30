namespace FiorSearchService.Interfaces;

public record class SearchServiceConfig {
    public string? Cx { get; set; }
    public string? ApiKey { get; set; }
    public ushort? ElementCount { get; set; }
}
