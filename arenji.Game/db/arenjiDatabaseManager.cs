using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using osu.Framework.Logging;

namespace arenji.Game.Database
{
    public static class arenjiDatabaseManager
    {
        private static SQLiteAsyncConnection db;
        
        private static readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string songsDir = Path.Combine(baseDir, "Songs");
        private static readonly string skinsDir = Path.Combine(baseDir, "Skins");
        private static readonly string dbPath = Path.Combine(baseDir, "arenji.db");

        public static async Task InitializeAsync()
        {
            Logger.Log("======= ARENJI DATABASE INITIALIZATION =======", LoggingTarget.Runtime, LogLevel.Important);

            try
            {
                // 1. Ensure physical directories exist next to the .exe
                Directory.CreateDirectory(songsDir);
                Directory.CreateDirectory(skinsDir);
                Logger.Log($"[DB] Verified Directories: \n -> Songs: {songsDir}\n -> Skins: {skinsDir}", LoggingTarget.Runtime, LogLevel.Debug);

                // 2. Establish connection and generate schemas
                db = new SQLiteAsyncConnection(dbPath);
                await db.CreateTablesAsync<SongModel, SkinModel>();
                Logger.Log($"[DB] Connected successfully to local file: {dbPath}", LoggingTarget.Runtime, LogLevel.Important);

                // 3. Sync local storage systems
                await syncSkinsAsync();
                await syncSongsAsync();

                Logger.Log("======= DATABASE INITIALIZED AND IN-SYNC =======", LoggingTarget.Runtime, LogLevel.Important);
            }
            catch (Exception ex)
            {
                Logger.Log($"[DB FATAL] Failed to initialize index registry: {ex.Message}", LoggingTarget.Runtime, LogLevel.Error);
                throw;
            }
        }

        private static async Task syncSkinsAsync()
        {
            Logger.Log("[DB] Synchronizing local skins directory...", LoggingTarget.Runtime, LogLevel.Debug);

            // Fetch physical folders currently sitting on disk
            var physicalFolders = Directory.GetDirectories(skinsDir)
                                           .Select(Path.GetFileName)
                                           .ToList();

            // Fetch what the DB remembers
            var dbSkins = await db.Table<SkinModel>().ToListAsync();

            // Check for orphaned records (folders deleted on disk but still in DB)
            foreach (var dbSkin in dbSkins)
            {
                if (!physicalFolders.Contains(dbSkin.FolderName))
                {
                    await db.DeleteAsync(dbSkin);
                    Logger.Log($"[DB PURGE] Removed missing skin registry: {dbSkin.FolderName}", LoggingTarget.Runtime, LogLevel.Important);
                }
            }

            // Check for new records (folders on disk but not in DB)
            foreach (var folder in physicalFolders)
            {
                if (!dbSkins.Any(s => s.FolderName == folder))
                {
                    var newSkin = new SkinModel
                    {
                        FolderName = folder,
                        SkinName = folder.Replace("Skin", ""), // Quick default formatting
                        Author = "Local User",
                        IsActive = false
                    };
                    await db.InsertAsync(newSkin);
                    Logger.Log($"[DB INDEX] Registered new skin folder: '{folder}'", LoggingTarget.Runtime, LogLevel.Important);
                }
            }
        }

        private static async Task syncSongsAsync()
        {
            Logger.Log("[DB] Synchronizing local songs directory...", LoggingTarget.Runtime, LogLevel.Debug);

            var physicalFolders = Directory.GetDirectories(songsDir)
                                           .Select(Path.GetFileName)
                                           .ToList();

            var dbSongs = await db.Table<SongModel>().ToListAsync();

            // Clean dead songs
            foreach (var dbSong in dbSongs)
            {
                if (!physicalFolders.Contains(dbSong.FolderName))
                {
                    await db.DeleteAsync(dbSong);
                    Logger.Log($"[DB PURGE] Removed missing song registry: {dbSong.FolderName}", LoggingTarget.Runtime, LogLevel.Important);
                }
            }

            // Catalog new songs
            foreach (var folder in physicalFolders)
            {
                if (!dbSongs.Any(s => s.FolderName == folder))
                {
                    string fullPath = Path.Combine(songsDir, folder);
                    
                    // Look for internal engine files inside the directory
                    string midiFile = Directory.GetFiles(fullPath, "*.mid").Select(Path.GetFileName).FirstOrDefault() ?? "";
                    string audioFile = Directory.GetFiles(fullPath, "*.mp3").Select(Path.GetFileName).FirstOrDefault() ?? "";

                    // Attempt simple metadata parsing from directory string format "Artist - Title"
                    string artist = "Unknown Artist";
                    string title = folder;
                    int dashIndex = folder.IndexOf('-');
                    if (dashIndex > 0)
                    {
                        artist = folder.Substring(0, dashIndex).Trim();
                        title = folder.Substring(dashIndex + 1).Trim();
                    }

                    var newSong = new SongModel
                    {
                        FolderName = folder,
                        Artist = artist,
                        Title = title,
                        MidiFileName = midiFile,
                        AudioFileName = audioFile,
                        DurationMs = 0 // Will populate dynamically during parser compilation passes later
                    };

                    await db.InsertAsync(newSong);
                    Logger.Log($"[DB INDEX] Registered new track: '{artist} - {title}' [MIDI: {midiFile}]", LoggingTarget.Runtime, LogLevel.Important);
                }
            }
        }

        // --- PUBLIC GETTERS FOR FUTURE FRONTEND ---
        
        public static AsyncTableQuery<SongModel> GetSongs()
        {
            return db.Table<SongModel>();
        }

        public static AsyncTableQuery<SkinModel> GetSkins()
        {
            return db.Table<SkinModel>();
        }
    }
}