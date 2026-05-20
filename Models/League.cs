using System.Collections.Generic;

namespace HemaLeagueManager.Models
{
    public class League
    {
        public string Name { get; set; } = string.Empty;
        public List<Fencer> Fencers { get; set; } = new();
        public List<Tournament> Tournaments { get; set; } = new();
        public bool IsClosed { get; set; }

        public override string ToString()
        {
            var suffix = IsClosed ? "  •  Closed" : "";
            return string.IsNullOrWhiteSpace(Name)
                ? "(unnamed league)"
                : $"{Name}   ({Tournaments.Count} tournaments){suffix}";
        }
    }
}