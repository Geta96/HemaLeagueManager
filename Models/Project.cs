using System.Collections.Generic;

namespace HemaLeagueManager.Models
{
    /// <summary>
    /// The full workspace: every league with its tournaments, the global fencer
    /// roster, the global club registry, and which league is currently active.
    /// Persisted as a single CSV file.
    /// </summary>
    public class Project
    {
        public List<League> Leagues { get; set; } = new();
        public List<Fencer> Fencers { get; set; } = new();
        public List<Club> Clubs { get; set; } = new();
        public string ActiveLeagueName { get; set; } = string.Empty;
    }
}