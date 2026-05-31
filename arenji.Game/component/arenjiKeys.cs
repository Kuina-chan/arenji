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
        private Sprite lightRay;
        private Sprite lightBulb;
        public Action<PianoKey, Vector2, float, Color4, int> OnKeyHit;
        private Box visualBox;
        private Color4 idleColor;
        private Color4 litColor;
        private List<VisualNoteData> activeNotes = new List<VisualNoteData>();

        public PianoKey(int pitch, bool isBlack)
        {
            MidiPitch = pitch;
            IsBlack = isBlack;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            idleColor = IsBlack ? Color4.Black : new Color4(220, 220, 220, 255); 

            lightRay = new Sprite
            {
                Texture = textures.Get("r"),
                RelativeSizeAxes = Axes.Both,
                Blending = BlendingParameters.Additive,
                Alpha = 0f
            };

            lightBulb = new Sprite
            {
                Texture = textures.Get("p"),
                Anchor = Anchor.TopCentre,
                Origin = Anchor.Centre,
                Blending = BlendingParameters.Additive,
                Alpha = 0f
            };
            InternalChildren = new Drawable[]
            {
                visualBox = new Box 
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = idleColor
                },
                lightRay,
                lightBulb,              
                new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 1,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Colour = Color4.Black,
                    Alpha = IsBlack ? 0 : 0.5f 
                }
            };
        }

        public void ClearNotes() => activeNotes.Clear();
        public void LoadNotes(IEnumerable<VisualNoteData> notes) => activeNotes.AddRange(notes);
        public void FlashGlow(Color4 noteColor, float rayOpacity, float bulbOpacity, float bulbSize)
        {
            Schedule(() =>
            {
                lightBulb.ClearTransforms();
            lightRay.ClearTransforms();
            lightRay.Colour = noteColor;
            lightBulb.Colour = noteColor;
            lightBulb.Size = new Vector2(bulbSize);

            lightRay.FadeTo(rayOpacity).Then().FadeOut(200, Easing.OutQuint);
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

                        //in case the user goes tripping mode
                        if (emitColor == Color4.Black)
                        {
                            emitColor = Color4.White;
                        }
                        OnKeyHit?.Invoke(this, topCentre, keyWidth, emitColor, velocity);
                    }

                    break; 
                }
                // 3. THE REWIND FIX: Reset the hit status if the user scrubs backward in the song!
                else if (currentTime < note.StartTimeMs)
                {
                    note.HasHit = false;
                }
            }

            visualBox.Colour = isCurrentlyLit ? currentColor : idleColor;
        }
    }
}