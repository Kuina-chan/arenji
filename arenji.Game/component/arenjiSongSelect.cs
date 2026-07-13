using System;
using System.IO;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osuTK.Graphics;
using arenji.Game.Database;

namespace arenji.Game
{
    public partial class arenjiSongSelectScreen : Screen
    {
        private FillFlowContainer songListContainer;
        private readonly string songsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Songs");

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                // A basic background to make it look like a menu
                new osu.Framework.Graphics.Shapes.Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(30, 30, 30, 255)
                },
                // The scrollable area for our song buttons
                new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(20),
                    Child = songListContainer = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new osuTK.Vector2(0, 10)
                    }
                }
            };

            // Fetch the songs from the database and build the UI
            Task.Run(async () => await populateSongListAsync());
        }

        private async Task populateSongListAsync()
        {
            // Make sure the DB is initialized first (in case this screen loads faster than the DB syncs)
            // It's safe to await an already-completed task if the DB finished booting in arenjiGame.cs
            
            var songs = await arenjiDatabaseManager.GetSongs().ToListAsync();

            // We must schedule UI creation back to the main thread!
            Schedule(() =>
            {
                if (songs.Count == 0)
                {
                    songListContainer.Add(new SpriteText
                    {
                        Text = "No songs found in the local database! Drop some folders in the Songs directory.",
                        Font = FontUsage.Default.With(size: 24)
                    });
                    return;
                }

                foreach (var song in songs)
                {
                    // Ensure we actually have a MIDI file to play
                    if (string.IsNullOrEmpty(song.MidiFileName)) continue;

                    string fullMidiPath = Path.Combine(songsDir, song.FolderName, song.MidiFileName);

                    songListContainer.Add(new BasicButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 50,
                        Text = $"{song.Artist} - {song.Title}",
                        BackgroundColour = new Color4(50, 50, 50, 255),
                        Action = () => loadSong(fullMidiPath)
                    });
                }
            });
        }

        private void loadSong(string midiPath)
        {
            // Create a fresh visualizer and push it to the screen stack
            var visualizer = new arenjiVisualizer();

            this.Push(visualizer);
            
            // Once it's pushed, hand it the file
            visualizer.HandleDroppedFile(midiPath);
        }
    }
}