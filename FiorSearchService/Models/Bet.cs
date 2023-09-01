namespace FiorSearchService.Models;

public class Bet
{
    public BetType BetType { get; }
    
    public Opponent Opponent { get; }
    
    public Bet(BetType betType, Opponent opponent)
    {
        BetType = betType;
        Opponent = opponent;
    }
}