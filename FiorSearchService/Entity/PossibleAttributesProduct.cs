namespace FiorSearchService.Entity;

public record struct PossibleAttributesProduct
{
    public Uri WebAddress { get; set; }
    public string SiteName { get; init; }
    public AboutProduct AboutProduct { get; set; }
}
