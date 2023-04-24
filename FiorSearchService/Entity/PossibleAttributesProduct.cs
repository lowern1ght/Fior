namespace FiorSearchService.Entity;

public record struct PossibleAttributesProduct
{
    public Uri WebAddress { get; set; }
    public string SiteName { get; init; }
    public List<Uri> UriImages { get; set; }
    public AboutProduct AboutProduct { get; set; }
}
