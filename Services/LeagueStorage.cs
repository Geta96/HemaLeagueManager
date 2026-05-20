using System.Collections.Generic;
using System.IO;
using System.Linq;
using HemaLeagueManager.Models;

namespace HemaLeagueManager.Services
{
    public static class LeagueStorage
    {
        // Simple CSV format with section headers.
        public static void Save(League league, string path)
        {
            using var w = new StreamWriter(path);
            w.WriteLine("#LEAGUE");
            w.WriteLine($"{Escape(league.Name)},{league.IsClosed}");

            w.WriteLine("#FENCERS");
            foreach (var f in league.Fencers)
                w.WriteLine($"{Escape(f.Name)},{Escape(f.Sex)},{Escape(f.ClubName)}");

            w.WriteLine("#TOURNAMENTS");
            foreach (var t in league.Tournaments)
            {
                var placements = string.Join("|", t.Placements.Select(Escape));
                w.WriteLine($"{Escape(t.Name)},{t.Date:yyyy-MM-dd},{placements}");
            }
        }

        public static League Load(string path)
        {
            var league = new League();
            var lines = File.ReadAllLines(path);
            string section = string.Empty;

            foreach (var raw in lines)
            {
                var line = raw.TrimEnd();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("#")) { section = line; continue; }

                var parts = line.Split(',');
                switch (section)
                {
                    case "#LEAGUE":
                        league.Name = Unescape(parts[0]);
                        if (parts.Length > 1 && bool.TryParse(parts[1], out var closed))
                            league.IsClosed = closed;
                        break;
                    case "#FENCERS":
                        league.Fencers.Add(new Fencer
                        {
                            Name = Unescape(parts[0]),
                            Sex = parts.Length > 1 ? Unescape(parts[1]) : "",
                            ClubName = parts.Length > 2 ? Unescape(parts[2]) : ""
                        });
                        break;
                    case "#TOURNAMENTS":
                        var t = new Tournament
                        {
                            Name = Unescape(parts[0]),
                            Date = System.DateTime.TryParse(parts.ElementAtOrDefault(1), out var d) ? d : System.DateTime.Today
                        };
                        if (parts.Length > 2 && !string.IsNullOrEmpty(parts[2]))
                            t.Placements = parts[2].Split('|').Select(Unescape).ToList();
                        league.Tournaments.Add(t);
                        break;
                }
            }

            return league;
        }

        private static string Escape(string s) => (s ?? "").Replace(",", "\\c").Replace("|", "\\p");
        private static string Unescape(string s) => (s ?? "").Replace("\\c", ",").Replace("\\p", "|");
    }
}