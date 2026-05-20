using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HemaLeagueManager.Models;

namespace HemaLeagueManager.Services
{
    /// <summary>
    /// Global, league-independent list of fencers. Persisted to fencers.csv
    /// next to the league library. All leagues share this single list.
    /// </summary>
    public static class FencerRegistry
    {
        private static readonly List<Fencer> _all = new();

        public static string FilePath { get; } = Path.Combine(LeagueLibrary.RootFolder, "fencers.csv");

        /// <summary>The single shared list — pass this directly into League.Fencers.</summary>
        public static List<Fencer> All => _all;

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
                    _all.Add(new Fencer
                    {
                        Name = Unescape(parts.ElementAtOrDefault(0) ?? ""),
                        Sex = Unescape(parts.ElementAtOrDefault(1) ?? ""),
                        ClubName = Unescape(parts.ElementAtOrDefault(2) ?? "")
                    });
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Fencer registry load failed: " + ex); }
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                using var w = new StreamWriter(FilePath);
                w.WriteLine("# Global fencer registry");
                foreach (var f in _all)
                    w.WriteLine($"{Escape(f.Name)},{Escape(f.Sex)},{Escape(f.ClubName)}");
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Fencer registry save failed: " + ex); }
        }

        /// <summary>Merge fencers from a loaded league into the global registry by name.</summary>
        public static void MergeFrom(IEnumerable<Fencer> incoming)
        {
            foreach (var f in incoming)
            {
                if (string.IsNullOrWhiteSpace(f.Name)) continue;
                if (!_all.Any(x => x.Name.Equals(f.Name, StringComparison.OrdinalIgnoreCase)))
                    _all.Add(f);
            }
        }

        private static string Escape(string s) => (s ?? "").Replace(",", "\\c").Replace("|", "\\p");
        private static string Unescape(string s) => (s ?? "").Replace("\\c", ",").Replace("\\p", "|");
    }
}