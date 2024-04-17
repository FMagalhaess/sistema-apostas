using TryBets.Bets.DTO;
using TryBets.Bets.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TryBets.Bets.Repository;

public class BetRepository : IBetRepository
{
    protected readonly ITryBetsContext _context;
    public BetRepository(ITryBetsContext context)
    {
        _context = context;
    }

    public BetDTOResponse Post(BetDTORequest betRequest, string email)
    {
        Match match = _context.Matches
            .FirstOrDefault(x => x.MatchId == betRequest.MatchId);

        if (match == null)
            throw new Exception("Match not founded");

        User user = _context.Users
            .FirstOrDefault(x => x.Email == email);
        if (user == null)
            throw new Exception("User not founded");
        
        Team team = _context.Teams
            .FirstOrDefault(x => x.TeamId == betRequest.TeamId);
        if (team == null)
            throw new Exception("Team not founded");
        if (match.MatchFinished)
            throw new Exception("Match finished");
        if (match.MatchTeamAId != betRequest.TeamId && match.MatchTeamBId != betRequest.TeamId)
            throw new Exception("Team is not in this match");

        Bet newBet = new Bet
        {
            UserId = user.UserId,
            MatchId = betRequest.MatchId,
            TeamId = betRequest.TeamId,
            BetValue = betRequest.BetValue,
        };

        _context.Bets.Add(newBet);
        _context.SaveChanges();

        Bet createdBet = _context.Bets
            .Include(x => x.Match)
            .Include(x => x.Team)
            .FirstOrDefault(x => x.BetId == newBet.BetId);
        if (match.MatchTeamAId == betRequest.TeamId)
        {
            match.MatchTeamAValue += betRequest.BetValue;
        }
        else
        {
            match.MatchTeamBValue += betRequest.BetValue;
        }
        _context.Matches.Update(match);
        _context.SaveChanges();

        return new BetDTOResponse
        {
            BetId = newBet.BetId,
            MatchId = newBet.MatchId,
            TeamId = newBet.TeamId,
            BetValue = newBet.BetValue,
            MatchDate = newBet.Match!.MatchDate,
            TeamName = newBet.Team!.TeamName,
            Email = newBet.User!.Email
        };
    }
    public BetDTOResponse Get(int BetId, string email)
    {
        Bet bet = _context.Bets
            .Include(x => x.Match)
            .Include(x => x.Team)
            .Include(x => x.User)
            .FirstOrDefault(x => x.BetId == BetId);
        if (bet == null)
            throw new Exception("Bet not founded");

        if (bet.User!.Email != email)
            throw new Exception("User not authorized");

        return new BetDTOResponse
        {
            BetId = bet.BetId,
            MatchId = bet.MatchId,
            TeamId = bet.TeamId,
            BetValue = bet.BetValue,
            MatchDate = bet.Match!.MatchDate,
            TeamName = bet.Team!.TeamName,
            Email = bet.User!.Email
        };
    }
}