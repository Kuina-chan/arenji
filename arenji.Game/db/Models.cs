using SQLite;

namespace arenji.Game.Database
{
    [Table("Songs")]
    public class SongModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed, Unique]
        public string FolderName { get; set; } // e.g., "Camellia - Ghost"

        public string Title { get; set; }
        public string Artist { get; set; }
        public double DurationMs { get; set; }
        public string MidiFileName { get; set; } // Relative path to the .mid inside the folder
        public string AudioFileName { get; set; } // Relative path to the .mp3/.wav
    }

    [Table("Skins")]
    public class SkinModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed, Unique]
        public string FolderName { get; set; } // e.g., "RetroNeonSkin"

        public string SkinName { get; set; }
        public string Author { get; set; }
        public bool IsActive { get; set; }
    }
}