using System;
using System.Collections.Generic;
using System.Linq;
using HemaLeagueManager.Models;

namespace HemaLeagueManager.Services
{
    /// <summary>
    /// In-memory global club registry. Persistence is handled by ProjectStorage.
    /// </summary>
    public static class ClubRegistry
    {
        private static readonly List<Club> _all = new();
        public static List<Club> All => _all;

        public static IEnumerable<string> Names => _all.Select(c => c.Name).OrderBy(n => n);

        public static void Replace(IEnumerable<Club> clubs)
        {
            _all.Clear();
            _all.AddRange(clubs);
        }

        public static bool Exists(string name) =>
            !string.IsNullOrWhiteSpace(name) &&
            _all.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public static Club? Find(string name) =>
            _all.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

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
            var club = new Club { Name = name.Trim(), ShortName = (shortName ?? "").Trim(), City = (city ?? "").Trim() };
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

        /// <summary>
        /// No-op for backward compatibility. Club data is now persisted by
        /// ProjectStorage via MainForm.OnDataChanged.
        /// </summary>
        public static void Save() { /* persisted by ProjectStorage */ }
    }
}