using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace arenji.Game
{
    public partial class PianoKey : CompositeDrawable
    {
        public readonly int MidiPitch;
        public readonly bool IsBlack;
        
        // Changed to Drawable so we can swap between a Sprite (skin) or a Circle (fallback)
        private Drawable lightBulb; 
        
        public Action<PianoKey, Vector2, float, Color4, int> OnKeyHit;
        
        private Color4 idleColor;
        private readonly Texture customTexture;
        
        // This fully replaces visualBox!
        private Drawable keyVisual; 
        
        private List<VisualNoteData> activeNotes = new List<VisualNoteData>();

        public PianoKey(int pitch, bool isBlack, Texture customTexture = null)
        {
            MidiPitch = pitch;
            IsBlack = isBlack; // Store the lowercase parameter into the uppercase property
            this.customTexture = customTexture;
        }

        [BackgroundDependencyLoader]
        private void load() // Removed the injected TextureStore so we can use our Skin Manager
        {
            // Fixed the capitalization to use IsBlack
            idleColor = IsBlack ? Color4.Black : new Color4(220, 220, 220, 255); 

            var bulbTexture = arenjiSkinManager.SkinTextures?.Get("skin/b"); // Change to "skin/p" if you prefer!
            
            if (bulbTexture != null)
            {
                lightBulb = new Sprite
                {
                    Texture = bulbTexture,
                    Anchor = Anchor.TopCentre, Origin = Anchor.Centre,
                    Blending = BlendingParameters.Additive, Alpha = 0f
                };
            }
            else
            {
                // Fallback math circle if the user has no custom bulb
                lightBulb = new Circle
                {
                    Anchor = Anchor.TopCentre, Origin = Anchor.Centre,
                    Blending = BlendingParameters.Additive, Alpha = 0f
                };
            }

            // 2. Key Skinning Logic
            if (customTexture != null)
            {
                keyVisual = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = customTexture,
                    Colour = idleColor 
                };
            }
            else
            {
                // Fallback box
                keyVisual = new Box 
                { 
                    RelativeSizeAxes = Axes.Both, 
                    Colour = idleColor 
                };
            }

            // 3. Proper Hierarchy setup (No more overwriting!)
            InternalChildren = new Drawable[]
            {
                keyVisual, // Draws the skin (or box) at the very bottom
                lightBulb, // Draws the bulb on top of the key
                new Box    // Draws the border last
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 1,
                    Anchor = Anchor.TopRight, Origin = Anchor.TopRight,
                    Colour = Color4.Black,
                    Alpha = IsBlack ? 0 : 0.5f 
                }
            };
        }

        public void ClearNotes() => activeNotes.Clear();
        public void LoadNotes(IEnumerable<VisualNoteData> notes) => activeNotes.AddRange(notes);
        
        public void FlashGlow(Color4 noteColor, float bulbOpacity, float bulbSize)
        {
            Schedule(() =>
            {
                lightBulb.ClearTransforms();
                lightBulb.Colour = noteColor;
                lightBulb.Size = new Vector2(bulbSize);
                lightBulb.FadeTo(bulbOpacity).Then().FadeOut(200, Easing.OutQuint);
            });
        }
        
        protected override void Update()
        {
            base.Update();
            bool isCurrentlyLit = false;
            Color4 currentColor = Color4.White; 
            double currentTime = Clock.CurrentTime;

            for (int i = 0; i < activeNotes.Count; i++)
            {
                var note = activeNotes[i];
                
                if (currentTime >= note.StartTimeMs && currentTime <= note.StartTimeMs + note.DurationMs)
                {
                    isCurrentlyLit = true;
                    currentColor = ArenjiColorManager.GetColorForNote(note); 
                    
                    if (!note.HasHit)
                    {
                        note.HasHit = true;
                        int velocity = 100; 
                        float keyWidth = this.DrawWidth;
                        Vector2 topCentre = (this.ScreenSpaceDrawQuad.TopLeft + this.ScreenSpaceDrawQuad.TopRight) / 2f;
                        Color4 emitColor = currentColor;

                        if (emitColor == Color4.Black)
                        {
                            emitColor = Color4.White;
                        }
                        OnKeyHit?.Invoke(this, topCentre, keyWidth, emitColor, velocity);
                    }

                    break; 
                }
                else if (currentTime < note.StartTimeMs)
                {
                    note.HasHit = false;
                }
            }

            // 4. Tint the actual key visual instead of the deleted visualBox
            keyVisual.Colour = isCurrentlyLit ? currentColor : idleColor;
        }
    }
}