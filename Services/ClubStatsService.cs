using System.Collections.Generic;
using System.Linq;
using HemaLeagueManager.Models;

namespace HemaLeagueManager.Services
{
    public record ClubStats(
        string ClubName,
        string City,
        int FencerCount,
        int TotalPoints,
        double AveragePoints,
        int BestFencerPoints,
        string BestFencerName,
        int TournamentsParticipated);

    public static class ClubStatsService
    {
        public static List<ClubStats> Compute(IEnumerable<Club> clubs, League league)
        {
            var fencerByName = league.Fencers.ToDictionary(f => f.Name, f => f);
            var totals = ScoringSystem.CalculateStandings(league);

            var result = new List<ClubStats>();
            foreach (var club in clubs)
            {
                var members = league.Fencers
                    .Where(f => f.ClubName.Equals(club.Name, System.StringComparison.OrdinalIgnoreCase))
                    .Where(f => league.AllowsFencer(f))     // gender filter
                    .ToList();

                int total = members.Sum(f => ScoringSystem.GetTotalPointsForFencer(league, f.Name));
                int count = members.Count;
                double avg = count == 0 ? 0 : (double)total / count;

                var best = members
                    .Select(f => new { f.Name, Pts = ScoringSystem.GetTotalPointsForFencer(league, f.Name) })
                    .OrderByDescending(x => x.Pts)
                    .FirstOrDefault();

                int tournamentsParticipated = league.Tournaments
                    .Count(t => t.Placements.Any(p => members.Any(m => m.Name == p)));

                result.Add(new ClubStats(
                    club.Name, club.City, count, total, avg,
                    best?.Pts ?? 0, best?.Name ?? "", tournamentsParticipated));
            }
            return result;
        }
    }
}