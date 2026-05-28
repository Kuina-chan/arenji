using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace arenji.Game
{
    public partial class VirtualKeyboard : CompositeDrawable
    {
        private const int MIN_MIDI_PITCH = 21; 
        private const int MAX_MIDI_PITCH = 108; 
        private const int TOTAL_WHITE_KEYS = 52;
        
        public Action<Vector2, float, Color4, int> OnKeyHit;

        private Dictionary<int, PianoKey> keysByPitch = new Dictionary<int, PianoKey>();

        [BackgroundDependencyLoader]
        private void load()
        {
            int whiteKeyIndex = 0;

            for (int pitch = MIN_MIDI_PITCH; pitch <= MAX_MIDI_PITCH; pitch++)
            {
                bool isBlack = isBlackKey(pitch);

                PianoKey key = new PianoKey(pitch, isBlack)
                {
                    Depth = isBlack ? -1 : 0 
                };

                key.OnKeyHit = (pos, width, color, velocity) => OnKeyHit?.Invoke(pos, width, color, velocity);

                if (!isBlack)
                {
                    key.RelativeSizeAxes = Axes.Both;
                    key.Width = 1f / TOTAL_WHITE_KEYS;
                    key.RelativePositionAxes = Axes.X;
                    key.X = whiteKeyIndex * key.Width;
                    whiteKeyIndex++;
                }
                else
                {
                    key.RelativeSizeAxes = Axes.Both;
                    key.Width = (1f / TOTAL_WHITE_KEYS) * 0.6f; 
                    key.Height = 0.65f; 
                    key.RelativePositionAxes = Axes.X;
                    key.X = (whiteKeyIndex * (1f / TOTAL_WHITE_KEYS)) - (key.Width / 2);
                }

                keysByPitch[pitch] = key; 
                AddInternal(key);
            }
        }

        public void LoadNotes(List<VisualNoteData> allNotes)
        {
            foreach (var key in keysByPitch.Values) key.ClearNotes();

            var groupedNotes = allNotes.GroupBy(n => n.Pitch);
            foreach (var group in groupedNotes)
            {
                if (keysByPitch.TryGetValue(group.Key, out var pianoKey))
                {
                    pianoKey.LoadNotes(group);
                }
            }
        }

        private bool isBlackKey(int pitch)
        {
            int[] blackKeys = { 1, 3, 6, 8, 10 }; 
            return blackKeys.Contains(pitch % 12);
        }
    }
}