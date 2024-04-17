using TryBets.Odds.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Globalization;

namespace TryBets.Odds.Repository;

public class OddRepository : IOddRepository
{
    protected readonly ITryBetsContext _context;
    public OddRepository(ITryBetsContext context)
    {
        _context = context;
    }

    public Match Patch(int MatchId, int TeamId, string BetValue)
    {
        var match = _context.Matches
            .FirstOrDefault(m => m.MatchId == MatchId);

        if (match == null)
        {
            throw new Exception("Match not found");
        }
        string converted = BetValue.Replace(",", ".");
        decimal oddValue = decimal.Parse(converted, CultureInfo.InvariantCulture);
        _context.Matches.Update(match);
        if (match.MatchTeamAId != TeamId && match.MatchTeamBId != TeamId)
        {
            throw new Exception("Not Found Teams");
        }
        if (match.MatchTeamAId == TeamId)
        {
            match.MatchTeamAValue += oddValue;
        }
        else
        {
            match.MatchTeamBValue += oddValue;
        }

        _context.SaveChanges();

        return match;
    }
}