using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Channels;

namespace arenji.Game
{
    public enum NoteColorMode
    {
        Solid,
        ByTrack,
        ByNote,
        ByChannel
    }

    public static class ArenjiColorManager
    {
        public static NoteColorMode CurrentMode = NoteColorMode.Solid;
        public static string ToHex(Color4 color)
        {
            return $"#{(byte)(color.R * 255):X2}{(byte)(color.G * 255):X2}{(byte)(color.B * 255):X2}"; 
        }
        public static Color4 SolidColor = Color4.Cyan;
        public static int ActiveTrackCount = 1;
        public static Dictionary<int, Color4> TrackColors = new Dictionary<int, Color4>();
        public static Dictionary<int, Color4> NoteColors = new Dictionary<int, Color4>();
        public static Dictionary<int, Color4> ChannelColors = new Dictionary<int, Color4>();
        public static float GlobalOpacity = 1.0f;
        private static readonly Color4[] defaultPalette = {
            Color4.Red, Color4.Green, Color4.Blue, Color4.Yellow, 
            Color4.Magenta, Color4.Orange, Color4.Purple, Color4.Pink
        };

        // 1. NEW HELPER: Mathematically darkens a Color4 by multiplying its RGB values
        private static Color4 darkenColor(Color4 original, float darknessFactor = 0.65f)
        {
            return new Color4(
                original.R * darknessFactor,
                original.G * darknessFactor,
                original.B * darknessFactor,
                original.A
            );
        }

        public static Color4 GetColorForNote(VisualNoteData noteData)
        {
            Color4 finalColor;

            // 2. Figure out the base color first
            switch (CurrentMode)
            {
                case NoteColorMode.ByTrack:
                    if (!TrackColors.TryGetValue(noteData.TrackIndex, out finalColor))
                        finalColor = defaultPalette[noteData.TrackIndex % defaultPalette.Length];
                    break;

                case NoteColorMode.ByNote:
                    if (!NoteColors.TryGetValue(noteData.PitchClass, out finalColor))
                        finalColor = defaultPalette[noteData.PitchClass % defaultPalette.Length];
                    break;
                case NoteColorMode.ByChannel:
                    if (!ChannelColors.TryGetValue(noteData.ChannelIndex, out finalColor))
                        finalColor = defaultPalette[noteData.ChannelIndex % defaultPalette.Length];
                    break;
                    
                case NoteColorMode.Solid:
                default:
                    finalColor = SolidColor;
                    break;
            }

            if (noteData.IsBlackKey && CurrentMode != NoteColorMode.ByNote)
            {
                return darkenColor(finalColor, 0.65f);
            }
            finalColor.A = GlobalOpacity;
            return finalColor;
        }

        public static Color4 ParseString(string input, Color4 fallback)
        {
            if (string.IsNullOrWhiteSpace(input)) return fallback;
            input = input.Trim().ToUpper();

            if (input.Contains(',') || input.Contains(' '))
            {
                var parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3 &&
                    byte.TryParse(parts[0], out byte r) &&
                    byte.TryParse(parts[1], out byte g) &&
                    byte.TryParse(parts[2], out byte b))
                {
                    return new Color4(r, g, b, 255);
                }
            }

            if (input.StartsWith("#")) input = input.Substring(1);
            if (input.Length == 6)
            {
                if (byte.TryParse(input.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out byte hexR) &&
                    byte.TryParse(input.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out byte hexG) &&
                    byte.TryParse(input.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out byte hexB))
                {
                    return new Color4(hexR, hexG, hexB, 255);
                }
            }

            return fallback;
        }

        public static void InitializeDefaults(int TrackCount)
        {
            for (int i = 0; i < TrackCount; i++)
            {
                if (!TrackColors.ContainsKey(i))
                {
                    TrackColors[i] = defaultPalette[i % defaultPalette.Length];
                }
            }

            for (int i = 0; i < 12; i++)
            {
                if (!NoteColors.ContainsKey(i))
                {
                    NoteColors[i] = defaultPalette[i % defaultPalette.Length];
                }
            }

            for (int i = 0; i < 12; i++)
            {
                if (!ChannelColors.ContainsKey(i))
                {
                    ChannelColors[i] = defaultPalette[i % defaultPalette.Length];
                }
            }
        }
    }
}