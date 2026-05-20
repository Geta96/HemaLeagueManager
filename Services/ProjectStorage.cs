using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HemaLeagueManager.Models;

namespace HemaLeagueManager.Services
{
    /// <summary>
    /// Reads / writes a whole Project as one CSV file with section headers.
    /// </summary>
    public static class ProjectStorage
    {
        public static void Save(Project project, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var w = new StreamWriter(path);

            w.WriteLine($"#PROJECT,{Escape(project.ActiveLeagueName)}");

            w.WriteLine("#FENCERS");
            foreach (var f in project.Fencers)
                w.WriteLine($"{Escape(f.Name)},{Escape(f.Sex)},{Escape(f.ClubName)}");

            w.WriteLine("#CLUBS");
            foreach (var c in project.Clubs)
                w.WriteLine($"{Escape(c.Name)},{Escape(c.ShortName)},{Escape(c.City)}");

            foreach (var league in project.Leagues)
            {
                w.WriteLine($"#LEAGUE,{Escape(league.Name)},{league.IsClosed}");
                foreach (var t in league.Tournaments)
                {
                    var placements = string.Join("|", t.Placements.Select(Escape));
                    w.WriteLine($"#TOURNAMENT,{Escape(t.Name)},{t.Date:yyyy-MM-dd},{t.IsGrandPrix},{placements}");
                }
            }
        }

        public static Project Load(string path)
        {
            var project = new Project();
            League? currentLeague = null;
            string section = string.Empty;

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.TrimEnd();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("#"))
                {
                    var head = line.Split(',');
                    switch (head[0])
                    {
                        case "#PROJECT":
                            project.ActiveLeagueName = head.Length > 1 ? Unescape(head[1]) : "";
                            section = "#PROJECT";
                            break;
                        case "#FENCERS":
                            section = "#FENCERS";
                            break;
                        case "#CLUBS":
                            section = "#CLUBS";
                            break;
                        case "#LEAGUE":
                            currentLeague = new League
                            {
                                Name = head.Length > 1 ? Unescape(head[1]) : "",
                                IsClosed = head.Length > 2 && bool.TryParse(head[2], out var c) && c
                            };
                            project.Leagues.Add(currentLeague);
                            section = "#LEAGUE";
                            break;
                        case "#TOURNAMENT" when currentLeague != null:
                            var t = new Tournament
                            {
                                Name = head.Length > 1 ? Unescape(head[1]) : "",
                                Date = head.Length > 2 && DateTime.TryParse(head[2], out var d) ? d : DateTime.Today,
                                IsGrandPrix = head.Length > 3 && bool.TryParse(head[3], out var gp) && gp
                            };
                            if (head.Length > 4 && !string.IsNullOrEmpty(head[4]))
                                t.Placements = head[4].Split('|').Select(Unescape).ToList();
                            currentLeague.Tournaments.Add(t);
                            break;
                    }
                    continue;
                }

                var parts = line.Split(',');
                switch (section)
                {
                    case "#FENCERS":
                        project.Fencers.Add(new Fencer
                        {
                            Name = Unescape(parts.ElementAtOrDefault(0) ?? ""),
                            Sex = Unescape(parts.ElementAtOrDefault(1) ?? ""),
                            ClubName = Unescape(parts.ElementAtOrDefault(2) ?? "")
                        });
                        break;
                    case "#CLUBS":
                        project.Clubs.Add(new Club
                        {
                            Name = Unescape(parts.ElementAtOrDefault(0) ?? ""),
                            ShortName = Unescape(parts.ElementAtOrDefault(1) ?? ""),
                            City = Unescape(parts.ElementAtOrDefault(2) ?? "")
                        });
                        break;
                }
            }

            return project;
        }

        private static string Escape(string s) =>
            (s ?? "").Replace(",", "\\c").Replace("|", "\\p").Replace("\r", " ").Replace("\n", " ");
        private static string Unescape(string s) =>
            (s ?? "").Replace("\\c", ",").Replace("\\p", "|");
    }
}