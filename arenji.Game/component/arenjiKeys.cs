using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;

namespace arenji.Game
{
    public partial class PianoKey : CompositeDrawable
    {
        public readonly int MidiPitch;
        public readonly bool IsBlack;

        public Action<Vector2, Color4, int> OnKeyHit;

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
        private void load()
        {
            idleColor = IsBlack ? Color4.Black : Color4.White;
            litColor = IsBlack ? Color4.DarkCyan : Color4.Cyan;

            InternalChildren = new Drawable[]
            {
                visualBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = idleColor
                },
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
                    
                    // 2. THE TRIGGER: Fire particles the exact frame the note starts!
                    if (!note.HasHit)
                    {
                        note.HasHit = true; // Mark it so it doesn't fire again tomorrow
                        
                        // Use a default velocity of 100 for now. 
                        int velocity = 100; 
                        
                        // Pass the top-center of this exact piano key to the particle emitter!
                        Vector2 topCentre = (this.ScreenSpaceDrawQuad.TopLeft + this.ScreenSpaceDrawQuad.TopRight) / 2f;
                        OnKeyHit?.Invoke(topCentre, currentColor, velocity);
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