using System.Collections.Generic;

namespace HemaLeagueManager.Models
{
    public class Tournament
    {
        public string Name { get; set; } = string.Empty;
        public System.DateTime Date { get; set; } = System.DateTime.Today;

        // Ordered list: index 0 = 1st place, 1 = 2nd, etc.
        public List<string> Placements { get; set; } = new List<string>();

        public override string ToString() => $"{Name} - {Date:yyyy-MM-dd}";
    }
}