using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osuTK.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Input.Bindings;
using osu.Framework.Audio.Track;

namespace arenji.Game
{
    public partial class arenjiPlaybackControl : FillFlowContainer, IKeyBindingHandler<ArenjiAction>
    {
        private IArenjiAudioEngine audioEngine;
        private Track backingTrack; 
        public readonly BindableDouble SeekBindable = new BindableDouble { MinValue = 0, MaxValue = 10000 };
        private BasicButton playPauseButton;

        private bool isUpdatingFromClock;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Height = 50;
            Direction = FillDirection.Horizontal;
            Padding = new MarginPadding(10);
            Spacing = new osuTK.Vector2(10, 0);

            Children = new Drawable[]
            {
                playPauseButton = new BasicButton
                {
                    Width = 100,
                    Height = 30,
                    Text = "Pause",
                    BackgroundColour = Color4.DarkRed,
                    Action = TogglePlayback
                },
                new BasicSliderBar<double>
                {
                    RelativeSizeAxes = Axes.None,
                    Width = 1000,
                    Height = 30,
                    Current = SeekBindable,
                    BackgroundColour = Color4.Lavender,
                    SelectionColour = Color4.DarkBlue
                }
            };

            SeekBindable.ValueChanged += e =>
            {
                if (audioEngine == null || isUpdatingFromClock) return;
                
                audioEngine.Seek(e.NewValue);
                backingTrack?.Seek(e.NewValue); 
            };
        }

        public void LinkEngine(IArenjiAudioEngine newEngine)
        {
            audioEngine = newEngine;
            SeekBindable.MaxValue = audioEngine.DurationMs;
            SeekBindable.Value = 0;
            
            playPauseButton.Text = "Pause";
            playPauseButton.BackgroundColour = Color4.DarkRed;
        }

        public void LinkBackingTrack(Track track)
        {
            backingTrack = track;
        }

        private void TogglePlayback()
        {
            if (audioEngine == null || !audioEngine.IsReady) return;

            if (audioEngine.AudioClock.IsRunning)
            {
                audioEngine.Pause();
                backingTrack?.Stop(); 
                
                playPauseButton.Text = "Play";
                playPauseButton.BackgroundColour = Color4.DarkGreen;
            }
            else
            {
                audioEngine.Play();
                backingTrack?.Start(); 
                
                playPauseButton.Text = "Pause";
                playPauseButton.BackgroundColour = Color4.DarkRed;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (audioEngine != null && audioEngine.IsReady && audioEngine.AudioClock.IsRunning)
            {
                isUpdatingFromClock = true;                
                SeekBindable.Value = audioEngine.AudioClock.CurrentTime; 
                isUpdatingFromClock = false;               
            }
        }

        public bool OnPressed(KeyBindingPressEvent<ArenjiAction> e)
        {
            if (audioEngine == null) return false; 

            switch (e.Action)
            {
                case ArenjiAction.TogglePlayback:
                    TogglePlayback();
                    return true;

                case ArenjiAction.SeekForward:
                    SeekBindable.Value += 5000;
                    return true;

                case ArenjiAction.SeekBackward:
                    SeekBindable.Value -= 5000;
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<ArenjiAction> e)
        {
        }
    }
}