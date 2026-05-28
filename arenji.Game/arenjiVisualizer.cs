using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Input.Bindings;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Audio;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using arenji.Game.particles;

namespace arenji.Game
{
    public partial class arenjiVisualizer : Screen, IKeyBindingHandler<ArenjiAction>
    {
        
        private VirtualKeyboard keyboard;
        private Container noteCanvas;
        private arenjiSettings settingsPanel;
        private arenjiAdvancedColorOverlay advancedColorOverlay;
        private arenjiProjectPrompt projectPrompt;
        private arenjiPlaybackControl controlPanel;
        private arenjiLoadingOverlay loadingOverlay;
        private arenjiImportPrompt importPrompt;
        private arenjiBackgroundSelector bgSelector;
        private Container backgroundLayer;
        private IArenjiAudioEngine activeAudioEngine;
        private int currentLoadId = 0;
        private osu.Framework.Timing.OffsetClock backgroundClock;
        private arenjiAudioSelector audioSelector;
        private osu.Framework.Audio.Track.Track backingTrack;
        private arenjiProjectSelector projectSelector;
        private ParticleEmitter particleLayer;
        [osu.Framework.Allocation.Resolved]
        private AudioManager osuAudioManager { get; set; }
        [osu.Framework.Allocation.Resolved]
        private osu.Framework.Platform.GameHost host { get; set; }
        [osu.Framework.Allocation.Resolved]
        private AudioManager globalAudioManager { get; set; }
        private void applyMasterVolumes()
        {
            // Apply Soundfont Volume
            if (activeAudioEngine != null)
            {
                activeAudioEngine.Volume = settingsPanel.MuteSoundfont.Value ? 0 : settingsPanel.SoundFontVolume.Value;
            }

            // Apply Backing Track Volume
            if (backingTrack != null)
            {
                backingTrack.Volume.Value = settingsPanel.MuteBackingAudio.Value ? 0 : settingsPanel.BackingAudioVolume.Value;
            }
        }
        [BackgroundDependencyLoader]    
        private void load()
        {
            noteCanvas = new Container
            {
                RelativeSizeAxes = Axes.Both, Anchor = Anchor.BottomLeft, Origin = Anchor.BottomLeft,
                Padding = new MarginPadding { Bottom = 120 } 
            };

            keyboard = new VirtualKeyboard
            {
                Anchor = Anchor.BottomLeft, Origin = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X, Height = 120
            };
            particleLayer = new ParticleEmitter();
            keyboard.OnKeyHit = (position, color, velocity) => 
            {
                int count = (int)(velocity / 10f);
                if (count < 5) count = 5;
                double userLifetime = settingsPanel.ParticleLifeTime.Value * 1000;
                float userSpeed = settingsPanel.ParticleSpeed.Value;
                float userTurbulence = settingsPanel.ParticleTurbulance.Value;

                particleLayer.Emit(position, color, count, userLifetime, userSpeed, userTurbulence);
            };
            controlPanel = new arenjiPlaybackControl { Anchor = Anchor.TopLeft, Origin = Anchor.TopLeft };
            
            settingsPanel = new arenjiSettings { State = { Value = Visibility.Hidden } };
            advancedColorOverlay = new arenjiAdvancedColorOverlay();
            projectPrompt = new arenjiProjectPrompt();
            importPrompt = new arenjiImportPrompt();
            loadingOverlay = new arenjiLoadingOverlay();
            backgroundLayer = new Container { RelativeSizeAxes = Axes.Both, Depth = float.MaxValue };
            bgSelector = new arenjiBackgroundSelector();
            projectSelector = new arenjiProjectSelector();
            audioSelector = new arenjiAudioSelector();
            settingsPanel.OnRequestAudioImport = () => audioSelector.Show();
            audioSelector.OnFileSelected = (filePath) => 
            {
                string savedPath = arenjiProjectManager.ImportBackingAudio(filePath);
                ApplyBackingAudio(savedPath);
                arenjiProjectManager.SaveCurrentProject(settingsPanel);
            };

            settingsPanel.MuteSoundfont.BindValueChanged(_ => applyMasterVolumes(), true);
            settingsPanel.MuteBackingAudio.BindValueChanged(_ => applyMasterVolumes(), true);
            settingsPanel.SoundFontVolume.BindValueChanged(_ => applyMasterVolumes(), true);
            settingsPanel.BackingAudioVolume.BindValueChanged(_ => applyMasterVolumes(), true);
            settingsPanel.OnRequestImport = () => projectSelector.Show();
            settingsPanel.OnRequestAdvancedColors = (mode) => advancedColorOverlay.OpenForMode(mode);
            settingsPanel.OnRequestImport = () => importPrompt.Show();
            settingsPanel.OnRequestBackgroundChange = () => bgSelector.Show();
            settingsPanel.BackgroundOpacity.ValueChanged += e => 
            {
                backgroundLayer.Alpha = e.NewValue;
            };
            bgSelector.OnFileSelected = (filePath) => 
            {
                string safeName = arenjiProjectManager.ImportBackground(filePath);
                arenjiProjectManager.SaveCurrentProject(settingsPanel);
                ApplyBackground(safeName);
            };
            settingsPanel.NoteOpacity.ValueChanged += e => 
            {
                ArenjiColorManager.GlobalOpacity = e.NewValue;
            };
            settingsPanel.BackgroundOffset.ValueChanged += e => 
            {
                if (backgroundClock != null) 
                    backgroundClock.Offset = e.NewValue * 1000f; 
            };

            importPrompt.OnConfirm = (folderPath) =>
            {
                string iniPath = Path.Combine(folderPath, "project.ini");
                
                string loadedMidiPath = arenjiProjectManager.LoadProject(iniPath, settingsPanel);
                settingsPanel.RefreshUIAfterLoad(); 

                Melanchall.DryWetMidi.Core.MidiFile midiFile;
                using (var stream = new FileStream(loadedMidiPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    midiFile = Melanchall.DryWetMidi.Core.MidiFile.Read(stream);
                }    
                LoadNewMidi(loadedMidiPath, midiFile);
                ApplyBackingAudio(arenjiProjectManager.CurrentBackingAudioPath);

                if (backingTrack != null)
                {
                    backingTrack.Volume.Value = settingsPanel.MuteBackingAudio.Value ? 0 : settingsPanel.BackingAudioVolume.Value;
                }
                ApplyBackground(arenjiProjectManager.CurrentBackgroundPath);
            };
            projectSelector.OnFolderConfirmed = (folderPath) =>
            {
                try
                {
                    string iniPath = Path.Combine(folderPath, "project.ini");
                    
                    if (!File.Exists(iniPath))
                    {
                        osu.Framework.Logging.Logger.Log($"IMPORT FAILED: No project.ini found in {folderPath}", osu.Framework.Logging.LoggingTarget.Runtime, osu.Framework.Logging.LogLevel.Error);
                        return;
                    }

                    arenjiProjectManager.CurrentProjectFolder = folderPath; 

                    string loadedMidiPath = arenjiProjectManager.LoadProject(iniPath, settingsPanel);
                    settingsPanel.RefreshUIAfterLoad(); 

                    if (!File.Exists(loadedMidiPath))
                    {
                        osu.Framework.Logging.Logger.Log($"IMPORT FAILED: Midi file missing at {loadedMidiPath}", osu.Framework.Logging.LoggingTarget.Runtime, osu.Framework.Logging.LogLevel.Error);
                        return;
                    }

                    Melanchall.DryWetMidi.Core.MidiFile midiFile;
                    using (var stream = new FileStream(loadedMidiPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        midiFile = Melanchall.DryWetMidi.Core.MidiFile.Read(stream);
                    }
                    
                    LoadNewMidi(loadedMidiPath, midiFile);
                    ApplyBackground(arenjiProjectManager.CurrentBackgroundPath); 
                    ApplyBackingAudio(arenjiProjectManager.CurrentBackingAudioPath);
                }
                catch (Exception ex)
                {
                    osu.Framework.Logging.Logger.Log($"CRITICAL IMPORT ERROR: {ex.Message}", osu.Framework.Logging.LoggingTarget.Runtime, osu.Framework.Logging.LogLevel.Error);
                }
            };

            // 5. ADD EVERYTHING TO THE SCREEN (Layered back to front)
            InternalChildren = new Drawable[] 
            {
                //new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Black },
                backgroundLayer,
                noteCanvas,
                keyboard,
                particleLayer,
                controlPanel,
                settingsPanel,
                advancedColorOverlay,
                projectPrompt,
                importPrompt,
                bgSelector,
                audioSelector,
                projectSelector,
                loadingOverlay
            };
        }

        public void ApplyBackground(string filename)
        {
            backgroundLayer.Clear();
            var defaultBox = new Box { RelativeSizeAxes = Axes.Both, Colour = new Color4(30, 30, 30, 255) };
            
            if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(arenjiProjectManager.CurrentProjectFolder))
            {
                backgroundLayer.Add(defaultBox);
                return;
            }

            string fullPath = Path.Combine(arenjiProjectManager.CurrentProjectFolder, "bg", filename);
            if (!File.Exists(fullPath))
            {
                backgroundLayer.Add(defaultBox);
                return;
            }

            string ext = Path.GetExtension(fullPath).ToLower();

            try
            {
                if (ext == ".mp4" || ext == ".avi")
                {
                    // THE FIX 1: Use safe FileStreams to bypass Windows Defender/Indexer locks!
                    var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var video = new osu.Framework.Graphics.Video.Video(stream, false)
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fill, 
                        Loop = true,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    };
                    backgroundLayer.Add(video);
                }
                else if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                {
                    // THE FIX 1: Safe FileStreams for images too!
                    using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var texture = osu.Framework.Graphics.Textures.Texture.FromStream(host.Renderer, stream);
                        backgroundLayer.Add(new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fill, 
                            Texture = texture,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                backgroundLayer.Add(defaultBox);
                backgroundLayer.Add(new osu.Framework.Graphics.Sprites.SpriteText 
                { 
                    Text = $"BG LOAD ERROR: {ex.Message}", 
                    Colour = Color4.Red,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
                
                osu.Framework.Logging.Logger.Log($"Background failed to load: {ex.Message}", osu.Framework.Logging.LoggingTarget.Runtime, osu.Framework.Logging.LogLevel.Error);
            }
        }
        
        public void HandleDroppedFile(string filePath)
        {
            if (filePath.EndsWith(".mid", StringComparison.OrdinalIgnoreCase) || filePath.EndsWith(".midi", StringComparison.OrdinalIgnoreCase))
            {
                projectPrompt.OnConfirm = (parentPath, projectName) =>
                {
                    string newIniPath = arenjiProjectManager.CreateProject(filePath, parentPath, projectName, settingsPanel);
                    string targetMidi = Path.Combine(Path.GetDirectoryName(newIniPath), Path.GetFileName(filePath));
                    
                    Melanchall.DryWetMidi.Core.MidiFile midiFile;
                    using (var stream = new FileStream(targetMidi, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        midiFile = Melanchall.DryWetMidi.Core.MidiFile.Read(stream);
                    }
                    //ApplyBackground(arenjiProjectManager.CurrentBackgroundPath);
                    LoadNewMidi(targetMidi, midiFile);
                };
                
                projectPrompt.Show();
            }
        }

        public void ApplyBackingAudio(string filePath)  
        {
            // 1. Clean up the old track before loading a new one
            if (backingTrack != null)
            {
                backingTrack.Stop();
                backingTrack.Dispose();
                backingTrack = null;
            }

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

            try
            {
                if (globalAudioManager == null)
                {
                    osu.Framework.Logging.Logger.Log("ApplyBackingAudio failed: globalAudioManager is null.", osu.Framework.Logging.LoggingTarget.Runtime, osu.Framework.Logging.LogLevel.Error);
                    return;
                }

                string folder = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);

                var storage = new osu.Framework.Platform.NativeStorage(folder);
                var trackStore = globalAudioManager.GetTrackStore(new osu.Framework.IO.Stores.StorageBackedResourceStore(storage));

                backingTrack = trackStore.Get(fileName);
                applyMasterVolumes();
                if (backingTrack != null)
                {
                    backingTrack.Volume.Value = settingsPanel.MuteBackingAudio.Value ? 0 : settingsPanel.BackingAudioVolume.Value;

                    if (activeAudioEngine != null && activeAudioEngine.AudioClock != null)
                    {
                        backingTrack.Seek(activeAudioEngine.AudioClock.CurrentTime);
                        
                        if (activeAudioEngine.AudioClock.IsRunning)
                        {
                            backingTrack.Start();
                        }
                    }
                    controlPanel.LinkBackingTrack(backingTrack);
                }
            }
            catch (Exception ex)
            {
                osu.Framework.Logging.Logger.Log($"Audio failed to load: {ex.Message}", osu.Framework.Logging.LoggingTarget.Runtime, osu.Framework.Logging.LogLevel.Error);
            }
        }
        public void LoadNewMidi(string midiPath, MidiFile midiFile)
        {
            if (activeAudioEngine != null)
            {
                activeAudioEngine.Pause();
                activeAudioEngine.Dispose();
                activeAudioEngine = null;
            }

            int myLoadId = ++currentLoadId;
            noteCanvas.Clear();
            var tempoMap = midiFile.GetTempoMap();
            
            double lastNoteEndTime = 0; 
            var allVisualNotes = new List<VisualNoteData>();

            var trackChunks = midiFile.GetTrackChunks().ToList();
            ArenjiColorManager.ActiveTrackCount = trackChunks.Count;
            ArenjiColorManager.InitializeDefaults(trackChunks.Count);

            for (int t = 0; t < trackChunks.Count; t++)
            {
                var trackNotes = trackChunks[t].GetNotes();
                
                foreach (var note in trackNotes)
                {
                    if (note.NoteNumber < 21 || note.NoteNumber > 108) continue;

                    double startMs = note.TimeAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 1000.0;
                    double durMs = note.LengthAs<MetricTimeSpan>(tempoMap).TotalMicroseconds / 1000.0;
                    
                    if (startMs + durMs > lastNoteEndTime) lastNoteEndTime = startMs + durMs;

                    var noteData = new VisualNoteData
                    {
                        Pitch = note.NoteNumber,
                        StartTimeMs = startMs,
                        DurationMs = durMs,
                        IsBlackKey = IsBlackKey(note.NoteNumber),
                        WhiteKeyIndex = CountWhiteKeysBefore(note.NoteNumber),
                        TrackIndex = t,
                        PitchClass = note.NoteNumber % 12,
                        ChannelIndex = note.Channel
                    };

                    allVisualNotes.Add(noteData);
                    noteCanvas.Add(new DrawableMidiNote(noteData, settingsPanel));
                }
            }

            noteCanvas.Clock = new osu.Framework.Timing.FramedClock(new osu.Framework.Timing.ManualClock());
            keyboard.Clock = new osu.Framework.Timing.FramedClock(new osu.Framework.Timing.ManualClock());
            backgroundLayer.Clock = new osu.Framework.Timing.FramedClock(new osu.Framework.Timing.ManualClock());
            loadingOverlay.Show(); 

            activeAudioEngine = new ArenjiSoundFontEngine(osuAudioManager);

            Task.Run(() =>
            {
                try
                {
                    string sfDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sf");
                    string mySoundFontPath = string.Empty;

                    if (Directory.Exists(sfDirectory))
                    {
                        string[] sfFiles = Directory.GetFiles(sfDirectory, "*.sf2");
                        if (sfFiles.Length > 0)
                        {
                            mySoundFontPath = sfFiles[0];
                        }
                    }

                    if (string.IsNullOrEmpty(mySoundFontPath))
                    {
                        osu.Framework.Logging.Logger.Log("CRITICAL: No .sf2 soundfont found in the ./sf/ directory! Please add one.", osu.Framework.Logging.LoggingTarget.Runtime, osu.Framework.Logging.LogLevel.Error);
                    }
                    activeAudioEngine.LoadFiles(midiPath, mySoundFontPath);
                    
                    Schedule(() =>
                    {
                        if (currentLoadId != myLoadId)
                        {
                            activeAudioEngine.Dispose();
                            loadingOverlay.Hide(); 
                            return;
                        }

                        noteCanvas.Clock = new osu.Framework.Timing.FramedClock(activeAudioEngine.AudioClock);
                        keyboard.Clock = new osu.Framework.Timing.FramedClock(activeAudioEngine.AudioClock);
                        backgroundClock = new osu.Framework.Timing.OffsetClock(activeAudioEngine.AudioClock) 
                        { 
                            Offset = settingsPanel.BackgroundOffset.Value * 1000f
                        };
                        backgroundLayer.Clock = new osu.Framework.Timing.FramedClock(backgroundClock);
                        keyboard.LoadNotes(allVisualNotes);
                        controlPanel.LinkEngine(activeAudioEngine);
                        
                        activeAudioEngine.Play();
                        applyMasterVolumes();
                        if (backingTrack != null)
                        {
                            backingTrack.Seek(0);
                            backingTrack.Start();
                        }
                        loadingOverlay.Hide(); 
                    });
                }
                catch (Exception ex)
                {
                    Schedule(() => 
                    {
                        loadingOverlay.Hide();
                        osu.Framework.Logging.Logger.Log($"AUDIO ENGINE CRASH: {ex.Message}", osu.Framework.Logging.LoggingTarget.Runtime, osu.Framework.Logging.LogLevel.Error);
                    });
                }
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

        public void OnReleased(KeyBindingReleaseEvent<ArenjiAction> e) { }
    }
}