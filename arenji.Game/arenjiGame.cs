using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Screens;
using arenji.Game.Database;

namespace arenji.Game
{
    public partial class arenjiGame : osu.Framework.Game
    {
        private string[] launchArguments;
        private ScreenStack screenStack;

        public arenjiGame(string[] args = null)
        {
            launchArguments = args ?? Array.Empty<string>();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // Boot the database immediately
            Task.Run(async () => await arenjiDatabaseManager.InitializeAsync());
            
            Resources.AddStore(new DllResourceStore(typeof(arenji.Resources.arenjiResources).Assembly));
            
            Child = new TooltipContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new ArenjiKeyBindingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = screenStack = new ScreenStack()
                }         
            };

            // Boot directly into the new Song Select Screen!
            screenStack.Push(new arenjiSongSelectScreen());
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);
            if (host.Window != null) host.Window.DragDrop += handleDragDrop;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            string initialMidi = launchArguments.FirstOrDefault(arg => arg.EndsWith(".mid", StringComparison.OrdinalIgnoreCase));
            if (initialMidi != null) processFile(initialMidi);
        }

        private void handleDragDrop(string filePath)
        {
            if (filePath.EndsWith(".mid", StringComparison.OrdinalIgnoreCase))
            {
                Schedule(() => processFile(filePath));
            }
        }

        private void processFile(string path)
        {
            try
            {
                // If the user drops a file, bypass the menu and launch the visualizer immediately
                var visualizer = new arenjiVisualizer();
                screenStack.Push(visualizer);
                visualizer.HandleDroppedFile(path);
            }
            catch (Exception ex)
            {
                osu.Framework.Logging.Logger.Log($"Error: {ex.Message}", osu.Framework.Logging.LoggingTarget.Runtime, osu.Framework.Logging.LogLevel.Error);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (Host?.Window != null) Host.Window.DragDrop -= handleDragDrop;
        }
    }
}