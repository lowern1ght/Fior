namespace FiorSearchService.Entity;

public record struct AboutProduct
{
    public List<string> Description { get; set; }
    public Dictionary<string, IConvertible> Specifity { get; set; }
}