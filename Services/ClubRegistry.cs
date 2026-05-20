using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HemaLeagueManager.Models;

namespace HemaLeagueManager.Services
{
    public static class ClubRegistry
    {
        private static readonly List<Club> _all = new();

        public static string FilePath { get; } = Path.Combine(LeagueLibrary.RootFolder, "clubs.csv");

        public static List<Club> All => _all;

        public static IEnumerable<string> Names =>
            _all.Select(c => c.Name).OrderBy(n => n);

        public static void Load()
        {
            _all.Clear();
            if (!File.Exists(FilePath)) return;
            try
            {
                foreach (var raw in File.ReadAllLines(FilePath))
                {
                    var line = raw.TrimEnd();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    var parts = line.Split(',');
                    var name = Unescape(parts.ElementAtOrDefault(0) ?? "");
                    var shortName = Unescape(parts.ElementAtOrDefault(1) ?? "");
                    var city = Unescape(parts.ElementAtOrDefault(2) ?? "");

                    // Backward compatibility: older files only had Name,City.
                    // Heuristic: if shortName is longer than 10 chars it's actually the city.
                    if (shortName.Length > 10 && string.IsNullOrEmpty(city))
                    {
                        city = shortName;
                        shortName = "";
                    }

                    _all.Add(new Club
                    {
                        Name = name,
                        ShortName = shortName,
                        City = city
                    });
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Club registry load failed: " + ex); }
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                using var w = new StreamWriter(FilePath);
                w.WriteLine("# Global club registry: Name,ShortName,City");
                foreach (var c in _all)
                    w.WriteLine($"{Escape(c.Name)},{Escape(c.ShortName)},{Escape(c.City)}");
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Club registry save failed: " + ex); }
        }

        public static bool Exists(string name) =>
            !string.IsNullOrWhiteSpace(name) &&
            _all.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public static Club? Find(string name) =>
            _all.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        /// <summary>Returns the short name for a club, falling back to a 10-char truncation
        /// of the long name if no short name is configured. Safe to call with anything.</summary>
        public static string GetShortName(string longName)
        {
            if (string.IsNullOrWhiteSpace(longName)) return "";
            var club = Find(longName);
            if (club != null && !string.IsNullOrWhiteSpace(club.ShortName))
                return club.ShortName;
            return longName.Length <= 10 ? longName : longName.Substring(0, 10);
        }

        public static Club AddIfMissing(string name, string shortName = "", string city = "")
        {
            var existing = Find(name);
            if (existing != null) return existing;
            var club = new Club
            {
                Name = name.Trim(),
                ShortName = (shortName ?? "").Trim(),
                City = (city ?? "").Trim()
            };
            _all.Add(club);
            return club;
        }

        public static bool Remove(string name)
        {
            var c = Find(name);
            if (c == null) return false;
            _all.Remove(c);
            return true;
        }

        public static void EnsureFromFencers(IEnumerable<Fencer> fencers)
        {
            foreach (var f in fencers)
                if (!string.IsNullOrWhiteSpace(f.ClubName))
                    AddIfMissing(f.ClubName);
        }

        private static string Escape(string s) => (s ?? "").Replace(",", "\\c").Replace("|", "\\p");
        private static string Unescape(string s) => (s ?? "").Replace("\\c", ",").Replace("\\p", "|");
    }
}