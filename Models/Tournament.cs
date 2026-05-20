using System.Collections.Generic;

namespace HemaLeagueManager.Models
{
    public class Tournament
    {
        public string Name { get; set; } = string.Empty;
        public System.DateTime Date { get; set; } = System.DateTime.Today;

        /// <summary>Grand Prix tournaments award double points to every placement.</summary>
        public bool IsGrandPrix { get; set; } = false;

        // Ordered list: index 0 = 1st place, 1 = 2nd, etc.
        public List<string> Placements { get; set; } = new List<string>();

        public override string ToString()
        {
            var prefix = IsGrandPrix ? "★ " : "";
            return $"{prefix}{Name} - {Date:yyyy-MM-dd}";
        }
    }
}