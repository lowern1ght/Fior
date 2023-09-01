namespace FiorSearchService.Models;

public class Event
{
    public string? Title { get; }
    public string? Description { get; }
    public DateTime BeginEvent { get; }
    public Opponent FirstOpponent { get; }
    public Opponent SecondOpponent { get; }
    
    public Event(string? title, string? description, Opponent firstOpponent, Opponent secondOpponent,
        DateTime beginEvent, IReadOnlyCollection<Bet> bets)
    {
        FirstOpponent = firstOpponent;
        SecondOpponent = secondOpponent;
        BeginEvent = beginEvent;
        Bets = bets;
        Title = title;
        Description = description;
    }
    
    public IReadOnlyCollection<Bet> Bets { get; }
}