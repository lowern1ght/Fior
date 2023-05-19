namespace FiorSearchService.Entity;

public record struct AboutProduct {
    public IList<String> Name { get; set; }
    public IList<String> UriImages { get; set; }
    public List<string> Description { get; set; }
    public Dictionary<string, IConvertible> Specifity { get; set; }
}