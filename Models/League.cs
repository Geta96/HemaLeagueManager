using System.Collections.Generic;

namespace HemaLeagueManager.Models
{
    public class League
    {
        public string Name { get; set; } = string.Empty;
        public List<Fencer> Fencers { get; set; } = new List<Fencer>();
        public List<Tournament> Tournaments { get; set; } = new List<Tournament>();
        public bool IsClosed { get; set; } = false;
    }
}