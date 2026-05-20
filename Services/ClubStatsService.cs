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
                var clubFencers = league.Fencers
                    .Where(f => f.ClubName.Equals(club.Name, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();

                int total = clubFencers.Sum(f => totals.TryGetValue(f.Name, out var p) ? p : 0);
                int count = clubFencers.Count;
                double avg = count == 0 ? 0 : (double)total / count;

                var best = clubFencers
                    .Select(f => (f.Name, Pts: totals.TryGetValue(f.Name, out var p) ? p : 0))
                    .OrderByDescending(x => x.Pts)
                    .FirstOrDefault();

                int tournaments = league.Tournaments
                    .Count(t => t.Placements.Any(name =>
                        fencerByName.TryGetValue(name, out var f) &&
                        f.ClubName.Equals(club.Name, System.StringComparison.OrdinalIgnoreCase)));

                result.Add(new ClubStats(
                    club.Name, club.City, count, total, avg,
                    best.Pts, best.Name ?? "", tournaments));
            }
            return result;
        }
    }
}