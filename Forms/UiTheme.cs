using System.Drawing;

namespace HemaLeagueManager.Forms
{
    /// <summary>Central place for colors/fonts so all screens stay in sync.</summary>
    public static class UiTheme
    {
        // Palette — warm dark "parchment & leather" with gold accents.
        public static readonly Color Background    = Color.FromArgb(28, 24, 22);   // app background
        public static readonly Color Surface       = Color.FromArgb(40, 34, 30);   // panels
        public static readonly Color SurfaceAlt    = Color.FromArgb(50, 42, 36);   // raised surfaces (lists)
        public static readonly Color Header        = Color.FromArgb(34, 28, 24);   // top header bar
        public static readonly Color Divider       = Color.FromArgb(70, 58, 48);

        public static readonly Color TextPrimary   = Color.FromArgb(240, 230, 210);
        public static readonly Color TextMuted     = Color.FromArgb(170, 158, 138);

        public static readonly Color Accent        = Color.FromArgb(212, 170, 90);  // warm gold
        public static readonly Color AccentHover   = Color.FromArgb(232, 190, 110);
        public static readonly Color AccentMuted   = Color.FromArgb(90, 70, 40);

        public static readonly Color ButtonIdle    = Color.FromArgb(58, 48, 40);
        public static readonly Color ButtonHover   = Color.FromArgb(78, 64, 52);
        public static readonly Color ButtonActive  = Color.FromArgb(120, 90, 50);

        public static readonly Color Danger        = Color.FromArgb(170, 70, 60);
        public static readonly Color DangerHover   = Color.FromArgb(195, 90, 78);

        // Fonts — Segoe UI for body (clean & readable), Garamond reserved for titles.
        public static readonly Font TitleLarge  = new("Garamond", 22F, FontStyle.Bold);
        public static readonly Font TitleMedium = new("Garamond", 16F, FontStyle.Bold);
        public static readonly Font Subtitle    = new("Segoe UI Semibold", 11F);
        public static readonly Font Body        = new("Segoe UI", 10.5F);
        public static readonly Font BodyBold    = new("Segoe UI Semibold", 10.5F);
        public static readonly Font Small       = new("Segoe UI", 9F);
    }
}