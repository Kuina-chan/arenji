using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;
using System.Collections.Generic;

namespace arenji.Game
{
    public partial class PianoKey : CompositeDrawable
    {
        public readonly int MidiPitch;
        public readonly bool IsBlack;

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
                    break; 
                }
            }

            visualBox.Colour = isCurrentlyLit ? currentColor : idleColor;
        }
    }
}