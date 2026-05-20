using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HemaLeagueManager.Models;

namespace HemaLeagueManager.Services
{
    /// <summary>
    /// Manages the on-disk collection of leagues (one .csv per league)
    /// under %AppData%/HemaLeagueManager/Leagues, plus a dedicated autosave file.
    /// </summary>
    public static class LeagueLibrary
    {
        public static string RootFolder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HemaLeagueManager");

        public static string LibraryFolder { get; } = Path.Combine(RootFolder, "Leagues");

        /// <summary>Single file the app always writes to on every change.</summary>
        public static string AutosavePath { get; } = Path.Combine(RootFolder, "autosave.csv");

        static LeagueLibrary()
        {
            Directory.CreateDirectory(LibraryFolder);
        }

        public static IEnumerable<string> ListLeagueFiles() =>
            Directory.EnumerateFiles(LibraryFolder, "*.csv")
                     .OrderByDescending(File.GetLastWriteTime);

        public static string PathFor(string leagueName)
        {
            var safe = MakeSafeFileName(leagueName);
            return Path.Combine(LibraryFolder, safe + ".csv");
        }

        public static League Load(string path) => LeagueStorage.Load(path);

        public static string Save(League league, string? existingPath = null)
        {
            var path = existingPath ?? PathFor(league.Name);
            LeagueStorage.Save(league, path);
            return path;
        }

        public static void Delete(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        // ---- Autosave ----

        public static void SaveAutosave(League league)
        {
            try { LeagueStorage.Save(league, AutosavePath); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Autosave failed: " + ex); }
        }

        public static League? LoadAutosaveIfExists()
        {
            if (!File.Exists(AutosavePath)) return null;
            try { return LeagueStorage.Load(AutosavePath); }
            catch { return null; }
        }

        public static bool AutosaveExists() => File.Exists(AutosavePath);

        private static string MakeSafeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "League";
            foreach (var ch in Path.GetInvalidFileNameChars())
                name = name.Replace(ch, '_');
            return name.Trim();
        }
    }
}