using System;
using System.Collections.Generic;
using System.Linq;
using HemaLeagueManager.Models;

namespace HemaLeagueManager.Services
{
    /// <summary>
    /// In-memory global fencer roster. Persistence is handled by ProjectStorage.
    /// </summary>
    public static class FencerRegistry
    {
        private static readonly List<Fencer> _all = new();
        public static List<Fencer> All => _all;

        public static IEnumerable<string> Names => _all.Select(f => f.Name).OrderBy(n => n);

        public static void Replace(IEnumerable<Fencer> fencers)
        {
            _all.Clear();
            _all.AddRange(fencers);
        }

        public static void MergeFrom(IEnumerable<Fencer> incoming)
        {
            foreach (var f in incoming)
            {
                if (string.IsNullOrWhiteSpace(f.Name)) continue;
                if (!_all.Any(x => x.Name.Equals(f.Name, StringComparison.OrdinalIgnoreCase)))
                    _all.Add(f);
            }
        }

        /// <summary>
        /// No-op for backward compatibility. Fencer data is now persisted by
        /// ProjectStorage via MainForm.OnDataChanged.
        /// </summary>
        public static void Save() { /* persisted by ProjectStorage */ }
    }
}