using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace arenji.Game
{
    public struct VisualNoteData
    {
        public int Pitch;
        public double StartTimeMs;
        public double DurationMs;
        public bool IsBlackKey;
        public int WhiteKeyIndex; 
        public int TrackIndex; 
        public int PitchClass;
        public int ChannelIndex;
    }

    public partial class DrawableMidiNote : CompositeDrawable
    {
        private readonly VisualNoteData data;
        private readonly arenjiSettings settings; // 1. Save a reference to the settings
        
        private const float SCROLL_SPEED = 0.5f; 
        private const int TOTAL_WHITE_KEYS = 52;
        private Box visualBox; 

        // 2. Add arenjiSettings to the constructor
        public DrawableMidiNote(VisualNoteData data, arenjiSettings settings)
        {
            this.data = data;
            this.settings = settings;
            Depth = data.IsBlackKey ? -1 : 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AlwaysPresent = true; 

            // 3. Changed Origin to BottomCentre so notes shrink inward instead of leftward!
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomCentre; 
            
            Height = (float)data.DurationMs * SCROLL_SPEED;
            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.X;
            
            // 4. Updated X Math to target the exact center of the keys
            float whiteWidth = 1f / TOTAL_WHITE_KEYS;
            if (!data.IsBlackKey)
            {
                X = (data.WhiteKeyIndex * whiteWidth) + (whiteWidth / 2f);
            }
            else
            {
                X = (data.WhiteKeyIndex * whiteWidth);
            }

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true, // REQUIRED for corner radius to work!
                Child = visualBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = data.IsBlackKey ? Color4.DarkCyan : Color4.Cyan, 
                    Alpha = 0.8f 
                }
            };

            if (!data.IsBlackKey)
            {
                settings.WhiteNoteWidth.BindValueChanged(e => Width = whiteWidth * e.NewValue, true);
            }
            else
            {
                settings.BlackNoteWidth.BindValueChanged(e => Width = whiteWidth * e.NewValue, true);
            }

            // We apply the corner radius to the Container wrapping the Box
            settings.NoteRoundness.BindValueChanged(e => ((Container)InternalChild).CornerRadius = e.NewValue, true);
        }

        protected override void Update()
        {
            base.Update();
            Y = (float)((Clock.CurrentTime - data.StartTimeMs) * SCROLL_SPEED);
            visualBox.Colour = ArenjiColorManager.GetColorForNote(data);
            if (Y > Height) visualBox.Alpha = 0; 
            else visualBox.Alpha = 0.8f; 
        }
    }
}