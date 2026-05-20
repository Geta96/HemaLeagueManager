using System;
using System.Collections.Generic;

namespace HemaLeagueManager.Models
{
    public enum LeagueGender { Open, Male, Female }

    public class League
    {
        public string Name { get; set; } = string.Empty;
        public LeagueGender Gender { get; set; } = LeagueGender.Open;
        public List<Fencer> Fencers { get; set; } = new();
        public List<Tournament> Tournaments { get; set; } = new();
        public bool IsClosed { get; set; }

        public string GenderLabel => Gender switch
        {
            LeagueGender.Male => "Male only",
            LeagueGender.Female => "Female only",
            _ => "Open"
        };

        /// <summary>
        /// Rule: female fencers compete in Female-only + Open leagues; male and
        /// unspecified (empty / "Other" / anything else) compete in Male-only +
        /// Open leagues.
        /// </summary>
        public bool AllowsFencer(Fencer? f)
        {
            var sex = (f?.Sex ?? "").Trim();
            bool isFemale = sex.Equals("Female", StringComparison.OrdinalIgnoreCase);

            return Gender switch
            {
                LeagueGender.Female => isFemale,
                LeagueGender.Male   => !isFemale,
                _                   => true,
            };
        }

        public override string ToString()
        {
            var closed = IsClosed ? "  •  Closed" : "";
            var g = Gender == LeagueGender.Open ? "" : $"  •  {GenderLabel}";
            return string.IsNullOrWhiteSpace(Name)
                ? "(unnamed league)"
                : $"{Name}   ({Tournaments.Count} tournaments){g}{closed}";
        }
    }
}