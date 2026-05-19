using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Screens;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace arenji.Game
{
    public partial class arenjiGame : osu.Framework.Game
    {
        private string[] launchArguments;
        private ScreenStack screenStack;
        private arenjiVisualizer activeVisualizer;

        public arenjiGame(string[] args = null)
        {
            launchArguments = args ?? Array.Empty<string>();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new ArenjiKeyBindingContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = screenStack = new ScreenStack()
            };
            
            activeVisualizer = new arenjiVisualizer();
            

            screenStack.Push(activeVisualizer);
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);
            host.Window.DragDrop += handleDragDrop;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            string initialMidi = launchArguments.FirstOrDefault(arg => arg.EndsWith(".mid", StringComparison.OrdinalIgnoreCase));
            if (initialMidi != null) processMidiFile(initialMidi);
        }

        private void handleDragDrop(string filePath)
        {
            if (filePath.EndsWith(".mid", StringComparison.OrdinalIgnoreCase))
            {
                Schedule(() => processMidiFile(filePath));
            }
        }

        private void processMidiFile(string path)
        {
            osu.Framework.Logging.Logger.Log($"Loading MIDI: {path}");
            try
            {
                var midiFile = MidiFile.Read(path);
                Schedule(() => activeVisualizer.LoadNewMidi(midiFile));
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