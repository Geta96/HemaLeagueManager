namespace HemaLeagueManager.Models
{
    public class Fencer
    {
        public string Name { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public string ClubName { get; set; } = string.Empty;

        public override string ToString() => $"{Name} ({ClubName})";
    }
}