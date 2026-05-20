using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Input.Bindings;
using System.Collections.Generic;
using osu.Framework.Audio;
using System.Threading.Tasks;
using System;
using System.IO;
namespace arenji.Game
{
    public partial class arenjiVisualizer : Screen, IKeyBindingHandler<ArenjiAction>
    {
        private VirtualKeyboard keyboard;
        private Container noteCanvas;
        private arenjiSettings settingsPanel;
        //private osu.Framework.Timing.StopwatchClock manualClock;
        private arenjiPlaybackControl controlPanel;
        [Resolved]
        private AudioManager osuAudioManager { get; set; }
        private IArenjiAudioEngine activeAudioEngine;
        
        // ADD THIS LINE:
        private int currentLoadId = 0;
        [BackgroundDependencyLoader]
        private void load()
        {
            noteCanvas = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Padding = new MarginPadding { Bottom = 120 } 
            };

            keyboard = new VirtualKeyboard
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X,
                Height = 120
            };

            controlPanel = new arenjiPlaybackControl
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
            };

            settingsPanel = new arenjiSettings 
            { 
                State = { Value = Visibility.Hidden } 
            };

            InternalChildren = new Drawable[] 
            {
                new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(30, 30, 30, 255) },
                noteCanvas,
                keyboard,
                controlPanel,
                settingsPanel 
            };
        }

        public void LoadNewMidi(string midiPath, MidiFile midiFile)
        {
            // 1. Kill any existing audio gracefully
            if (activeAudioEngine != null)
            {
                activeAudioEngine.Pause();
                activeAudioEngine.Dispose();
            }

            int myLoadId = ++currentLoadId;
            noteCanvas.Clear();
            var tempoMap = midiFile.GetTempoMap();
            
            double lastNoteEndTime = 0; 
            var allVisualNotes = new List<VisualNoteData>();

            var trackChunks = midiFile.GetTrackChunks().ToList();

            for (int t = 0; t < trackChunks.Count; t++)
            {
                var trackNotes = trackChunks[t].GetNotes();
                
                foreach (var note in trackNotes)
                {
                    double startMs = note.TimeAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 1000.0;
                    double durMs = note.LengthAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 1000.0;
                    
                    if (startMs + durMs > lastNoteEndTime) lastNoteEndTime = startMs + durMs;

                    // This struct is now the single source of truth for both visuals AND the keyboard
                    var noteData = new VisualNoteData
                    {
                        Pitch = note.NoteNumber,
                        StartTimeMs = startMs,
                        DurationMs = durMs,
                        IsBlackKey = IsBlackKey(note.NoteNumber),
                        WhiteKeyIndex = CountWhiteKeysBefore(note.NoteNumber),
                        
                        TrackIndex = t,
                        PitchClass = note.NoteNumber % 12 
                    };

                    allVisualNotes.Add(noteData);
                    noteCanvas.Add(new DrawableMidiNote(noteData, settingsPanel));
                }
            }

            // 4. Boot up the audio engine
            activeAudioEngine = new ArenjiSoundFontEngine(osuAudioManager);

            Task.Run(() =>
            {
                // Ensure this path matches your setup!
                string mySoundFontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sf", "Touhou.sf2"); 
                
                activeAudioEngine.LoadFiles(midiPath, mySoundFontPath);
                
                Schedule(() =>
                {
                    // Race condition preventer
                    if (currentLoadId != myLoadId)
                    {
                        activeAudioEngine.Dispose();
                        return;
                    }

                    noteCanvas.Clock = new osu.Framework.Timing.FramedClock(activeAudioEngine.AudioClock);
                    keyboard.Clock = new osu.Framework.Timing.FramedClock(activeAudioEngine.AudioClock);
                    
                    keyboard.LoadNotes(allVisualNotes);
                    controlPanel.LinkEngine(activeAudioEngine);
                    
                    activeAudioEngine.Play(); 
                });
            });
        }

        private bool IsBlackKey(int pitch)
        {
            int[] blackKeys = { 1, 3, 6, 8, 10 };
            return blackKeys.Contains(pitch % 12);
        }

        private int CountWhiteKeysBefore(int targetPitch)
        {
            int count = 0;
            for (int i = 21; i <= targetPitch; i++) 
            {
                if (!IsBlackKey(i)) 
                {
                    if (i != targetPitch) count++; 
                }
            }
            return count;
        }

        public bool OnPressed(KeyBindingPressEvent<ArenjiAction> e)
        {
            if (e.Action == ArenjiAction.ToggleSetting) 
            {
                settingsPanel.ToggleVisibility();
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<ArenjiAction> e)
        {
        }
    }
    
    // --- KEYBOARD CONTAINER ---
    // --- KEYBOARD CONTAINER ---
    public partial class VirtualKeyboard : CompositeDrawable
    {
        private const int MIN_MIDI_PITCH = 21; 
        private const int MAX_MIDI_PITCH = 108; 
        private const int TOTAL_WHITE_KEYS = 52;

        // Keep track of which key belongs to which pitch
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

                keysByPitch[pitch] = key; // Store it in the dictionary
                AddInternal(key);
            }
        }

        // The visualizer calls this to distribute notes to the correct keys
        public void LoadNotes(List<VisualNoteData> allNotes)
        {
            // First, tell all keys to clear their previous notes
            foreach (var key in keysByPitch.Values) key.ClearNotes();

            // Group the notes by their pitch, then hand them to the matching key
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

    // --- INDIVIDUAL KEY ---
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
            // Define our colors (Cyan to perfectly match the falling notes!)
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
            Color4 currentColor = Color4.White; // Default
            double currentTime = Clock.CurrentTime;

            for (int i = 0; i < activeNotes.Count; i++)
            {
                var note = activeNotes[i];
                if (currentTime >= note.StartTimeMs && currentTime <= note.StartTimeMs + note.DurationMs)
                {
                    isCurrentlyLit = true;
                    // Ask the manager what color this specific key press should be!
                    currentColor = ArenjiColorManager.GetColorForNote(note); 
                    break; 
                }
            }

            visualBox.Colour = isCurrentlyLit ? currentColor : idleColor;
        }
    }
}