using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osuTK.Graphics;

namespace arenji.Game
{
    public partial class PlaybackControlPanel : FillFlowContainer
    {
        private osu.Framework.Timing.StopwatchClock linkedClock;
        
        public readonly BindableDouble SeekBindable = new BindableDouble { MinValue = 0, MaxValue = 10000 };
        private BasicButton playPauseButton;

        // --- 1. THE MAGIC FLAG ---
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
                    Width = 500,
                    Height = 30,
                    Current = SeekBindable,
                    BackgroundColour = Color4.Black,
                    SelectionColour = Color4.Cyan
                }
            };

            // --- 2. THE SEEK LISTENER ---
            // This event fires anytime the slider moves (either by the clock or by your mouse)
            SeekBindable.ValueChanged += e =>
            {
                // If the clock is missing, or if the Update() loop is currently moving the slider, ignore this!
                if (linkedClock == null || isUpdatingFromClock) return;

                // If we get past the line above, it means the USER clicked or dragged the slider!
                linkedClock.Seek(e.NewValue);
            };
        }

        public void LinkClock(osu.Framework.Timing.StopwatchClock newClock, double maxSongTime)
        {
            linkedClock = newClock;
            SeekBindable.MaxValue = maxSongTime;
            SeekBindable.Value = 0;
            
            playPauseButton.Text = "Pause";
            playPauseButton.BackgroundColour = Color4.DarkRed;
        }

        private void TogglePlayback()
        {
            if (linkedClock == null) return;

            if (linkedClock.IsRunning)
            {
                linkedClock.Stop();
                playPauseButton.Text = "Play";
                playPauseButton.BackgroundColour = Color4.DarkGreen;
            }
            else
            {
                // We no longer need to seek here, the ValueChanged event handles it!
                linkedClock.Start();
                playPauseButton.Text = "Pause";
                playPauseButton.BackgroundColour = Color4.DarkRed;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (linkedClock != null && linkedClock.IsRunning)
            {
                // 3. THE LOCK MANEUVER
                isUpdatingFromClock = true;                // Lock the event listener
                SeekBindable.Value = linkedClock.CurrentTime; // Move the slider visually
                isUpdatingFromClock = false;               // Unlock the event listener immediately
            }
            
            // Notice we completely deleted the "else" block. 
            // When paused, the Update loop does nothing, leaving total control to the mouse!
        }
    }
}