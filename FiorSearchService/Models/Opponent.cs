namespace FiorSearchService.Models;

public class Opponent
{
    public string? Id { get; }
    public string? Name { get; }

    public Opponent(string? id, string? name)
    {
        Id = id;
        Name = name;
    }

    public override bool Equals(object? obj)
    {
        return obj is Opponent opponent && opponent.Id == Id;
    }

    protected bool Equals(Opponent other)
    {
        return Id == other.Id && Name == other.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name);
    }
}