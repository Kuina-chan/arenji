using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
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
        public bool HasHit { get; set; }
    }
    public partial class DrawableMidiNote : CompositeDrawable
    {
        private readonly VisualNoteData data;
        private readonly arenjiSettings settings;
        
        private const float SCROLL_SPEED = 0.5f; 
        private const int TOTAL_WHITE_KEYS = 52;
        private Drawable noteVisual;

        public DrawableMidiNote(VisualNoteData data, arenjiSettings settings)
        {
            this.data = data;
            this.settings = settings;

            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomCentre; 
            Depth = data.IsBlackKey ? -1 : 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AlwaysPresent = true; 
            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.X;
            Height = (float)data.DurationMs * SCROLL_SPEED;
            Masking = true; 

            Color4 myNoteColor = ArenjiColorManager.GetColorForNote(data);

            var noteTexture = arenjiSkinManager.SkinTextures?.Get("skin/note");

            if (noteTexture != null)
            {
                noteVisual = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = noteTexture,
                    Colour = myNoteColor 
                };
            }
            else
            {
                noteVisual = new Box 
                { 
                    RelativeSizeAxes = Axes.Both, 
                    Colour = myNoteColor 
                };
            }

            InternalChild = noteVisual;

            float whiteWidth = 1f / TOTAL_WHITE_KEYS;
            X = data.IsBlackKey 
                ? (data.WhiteKeyIndex * whiteWidth) 
                : (data.WhiteKeyIndex * whiteWidth) + (whiteWidth / 2f);

            if (!data.IsBlackKey)
            {
                settings.WhiteNoteWidth.BindValueChanged(e => Width = whiteWidth * e.NewValue, true);
            }
            else
            {
                settings.BlackNoteWidth.BindValueChanged(e => Width = whiteWidth * e.NewValue, true);
            }

            settings.NoteRoundness.BindValueChanged(e => CornerRadius = e.NewValue, true);
        }

        protected override void Update()
        {
            base.Update();
            
            // Move the note
            Y = (float)((Clock.CurrentTime - data.StartTimeMs) * SCROLL_SPEED);

            noteVisual.Colour = ArenjiColorManager.GetColorForNote(data);

            if (Y > Height) 
            {
                this.Alpha = 0f; 
            }
            else 
            {
                this.Alpha = ArenjiColorManager.GlobalOpacity; 
            }
        }
    }
}