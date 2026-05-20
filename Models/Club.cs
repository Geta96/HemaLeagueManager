namespace HemaLeagueManager.Models
{
    public class Club
    {
        /// <summary>Full club name — canonical identifier referenced by fencers.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Compact display name, max 10 characters.</summary>
        public string ShortName { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public override string ToString() =>
            string.IsNullOrWhiteSpace(ShortName) ? Name : ShortName;
    }
}