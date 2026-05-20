using System;
using System.IO;

namespace HemaLeagueManager.Services
{
    /// <summary>
    /// Paths and helpers for the single-file project autosave.
    /// All leagues, fencers and clubs are persisted together via ProjectStorage.
    /// </summary>
    public static class LeagueLibrary
    {
        public static string RootFolder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HemaLeagueManager");

        public static string AutosavePath { get; } = Path.Combine(RootFolder, "project.csv");

        static LeagueLibrary() => Directory.CreateDirectory(RootFolder);

        public static bool AutosaveExists() => File.Exists(AutosavePath);

        public static void DeleteAutosave()
        {
            try { if (File.Exists(AutosavePath)) File.Delete(AutosavePath); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("DeleteAutosave failed: " + ex); }
        }
    }
}