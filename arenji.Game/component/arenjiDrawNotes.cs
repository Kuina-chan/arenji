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
        
        // We need a specific reference to the body to update its tiling rectangle!
        private Sprite bodySprite; 

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

            var headTexture = arenjiSkinManager.SkinTextures?.Get("noteHead");
            
            // 1. THE MAGIC INGREDIENT: Ask for WrapMode.Repeat on the Y-Axis!
            var bodyTexture = arenjiSkinManager.SkinTextures?.Get(
                "noteBody", 
                WrapMode.ClampToEdge, // X-Axis (Don't repeat horizontally)
                WrapMode.Repeat       // Y-Axis (Tile vertically!)
            );
            
            var endTexture = arenjiSkinManager.SkinTextures?.Get("noteEnd");

            if (headTexture != null && bodyTexture != null && endTexture != null)
            {
                float initialHeight = (float)data.DurationMs * settings.ScrollSpeed.Value;
                float actualCapHeight = Math.Min(DESIRED_CAP_HEIGHT, initialHeight / 2f);

                // Initialize the body sprite and save it to our variable
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
                            Child = bodySprite // Add the tiling body here
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
            else if (arenjiSkinManager.SkinTextures?.Get("noteBody") != null)
            {
                // Note: We don't tile the 1-piece fallback, we just stretch it.
                noteVisual = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = arenjiSkinManager.SkinTextures.Get("noteBody")
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
            
            // Re-added the dynamic height binding here!
            settings.ScrollSpeed.BindValueChanged(e => Height = (float)data.DurationMs * e.NewValue, true);
        }

        protected override void Update()
        {
            base.Update();
            
            // Fixed the scroll speed variable here!
            Y = (float)((Clock.CurrentTime - data.StartTimeMs) * settings.ScrollSpeed.Value);
            noteVisual.Colour = ArenjiColorManager.GetColorForNote(data);

            if (Y > Height) this.Alpha = 0f; 
            else this.Alpha = ArenjiColorManager.GlobalOpacity; 
            
            if (bodySprite != null && bodySprite.Texture != null)
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