using System.Collections.Generic;
using System.Linq;
using HemaLeagueManager.Models;

namespace HemaLeagueManager.Services
{
    public static class ScoringSystem
    {
        // F1-style points by finishing position (index 0 = 1st place).
        private static readonly int[] Points = { 25, 18, 15, 12, 10, 8, 6, 4, 2, 1 };

        /// <summary>Grand Prix tournaments double every point earned.</summary>
        public const int GrandPrixMultiplier = 2;

        public static int GetPointsForPlacement(int placementIndex)
        {
            if (placementIndex < 0) return 0;
            return placementIndex < Points.Length ? Points[placementIndex] : 0;
        }

        /// <summary>Points for a placement in a specific tournament (applies Grand Prix multiplier).</summary>
        public static int GetPointsForPlacement(int placementIndex, Tournament tournament)
        {
            var basePoints = GetPointsForPlacement(placementIndex);
            return tournament.IsGrandPrix ? basePoints * GrandPrixMultiplier : basePoints;
        }

        public static int GetTotalPointsForFencer(League league, string fencerName)
        {
            int total = 0;
            foreach (var t in league.Tournaments)
            {
                int idx = t.Placements.IndexOf(fencerName);
                if (idx >= 0) total += GetPointsForPlacement(idx, t);
            }
            return total;
        }

        public static Dictionary<string, int> CalculateStandings(League league)
        {
            var standings = league.Fencers.ToDictionary(f => f.Name, f => 0);

            foreach (var t in league.Tournaments)
            {
                for (int i = 0; i < t.Placements.Count; i++)
                {
                    var name = t.Placements[i];
                    var pts = GetPointsForPlacement(i, t);
                    if (standings.ContainsKey(name))
                        standings[name] += pts;
                    else
                        standings[name] = pts;
                }
            }

            return standings;
        }
    }
}