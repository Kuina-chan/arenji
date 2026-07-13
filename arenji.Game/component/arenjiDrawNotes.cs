using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
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
        
        private const int TOTAL_WHITE_KEYS = 52;
        private const float DESIRED_CAP_HEIGHT = 20f; 
        
        private Drawable noteVisual;
        private Sprite bodySprite; 
        
        // Flag to determine if we should execute the tiling math in the Update loop
        private bool shouldTileBody;

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
            Masking = true; 

            Color4 myNoteColor = ArenjiColorManager.GetColorForNote(data);

            var headTexture = arenjiSkinManager.SkinTextures?.Get("Skins/noteHead");
            
            // 1. Peek at the body texture first to evaluate its height
            var baseBodyTexture = arenjiSkinManager.SkinTextures?.Get("Skins/noteBody");
            
            // 2. Decide if it meets the criteria to be tiled
            shouldTileBody = baseBodyTexture != null && baseBodyTexture.Height <= 128;

            // 3. Request the texture with the appropriate WrapMode based on our check
            var bodyTexture = arenjiSkinManager.SkinTextures?.Get(
                "Skins/noteBody", 
                WrapMode.ClampToEdge, 
                shouldTileBody ? WrapMode.Repeat : WrapMode.ClampToEdge
            );
            
            var endTexture = arenjiSkinManager.SkinTextures?.Get("Skins/noteEnd");

            if (headTexture != null && bodyTexture != null && endTexture != null)
            {
                float initialHeight = (float)data.DurationMs * settings.ScrollSpeed.Value;
                float actualCapHeight = Math.Min(DESIRED_CAP_HEIGHT, initialHeight / 2f);

                bodySprite = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = bodyTexture
                };

                noteVisual = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = actualCapHeight, Bottom = actualCapHeight },
                            Child = bodySprite
                        },
                        new Sprite
                        {
                            RelativeSizeAxes = Axes.X, Height = actualCapHeight,
                            Anchor = Anchor.TopCentre, Origin = Anchor.TopCentre,
                            Texture = endTexture
                        },
                        new Sprite
                        {
                            RelativeSizeAxes = Axes.X, Height = actualCapHeight,
                            Anchor = Anchor.BottomCentre, Origin = Anchor.BottomCentre,
                            Texture = headTexture
                        }
                    }
                };
            }
            else if (arenjiSkinManager.SkinTextures?.Get("Skins/noteBody") != null)
            {
                noteVisual = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = arenjiSkinManager.SkinTextures.Get("Skins/noteBody")
                };
            }
            else
            {
                noteVisual = new Box { RelativeSizeAxes = Axes.Both };
            }

            noteVisual.Colour = myNoteColor;
            InternalChild = noteVisual;

            float whiteWidth = 1f / TOTAL_WHITE_KEYS;
            X = data.IsBlackKey 
                ? (data.WhiteKeyIndex * whiteWidth) 
                : (data.WhiteKeyIndex * whiteWidth) + (whiteWidth / 2f);

            if (!data.IsBlackKey) settings.WhiteNoteWidth.BindValueChanged(e => Width = whiteWidth * e.NewValue, true);
            else settings.BlackNoteWidth.BindValueChanged(e => Width = whiteWidth * e.NewValue, true);

            settings.NoteRoundness.BindValueChanged(e => CornerRadius = e.NewValue, true);
            
            settings.ScrollSpeed.BindValueChanged(e => Height = (float)data.DurationMs * e.NewValue, true);
        }

        protected override void Update()
        {
            base.Update();
            
            Y = (float)((Clock.CurrentTime - data.StartTimeMs) * settings.ScrollSpeed.Value);
            noteVisual.Colour = ArenjiColorManager.GetColorForNote(data);

            if (Y > Height) this.Alpha = 0f; 
            else this.Alpha = ArenjiColorManager.GlobalOpacity; 
            
            // Only apply the dynamic tiling rectangle if the texture met the <= 128px condition
            if (shouldTileBody && bodySprite != null && bodySprite.Texture != null)
            {
                bodySprite.TextureRectangle = new RectangleF(
                    0, 
                    0, 
                    bodySprite.Texture.Width, 
                    bodySprite.DrawHeight 
                );
            }
        }
    }
}