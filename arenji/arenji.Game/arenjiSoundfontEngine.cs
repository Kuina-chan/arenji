using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MeltySynth;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.IO.Stores;
using osu.Framework.Timing;

namespace arenji.Game
{
    // The RAM-based fake hard drive
    public class MemoryAudioStore : IResourceStore<byte[]>
    {
        private readonly byte[] audioData;
        public MemoryAudioStore(byte[] audioData) { this.audioData = audioData; }
        public byte[] Get(string name) => audioData;
        public Task<byte[]> GetAsync(string name, CancellationToken token = default) => Task.FromResult(audioData);
        public Stream GetStream(string name) => new MemoryStream(audioData);
        public IEnumerable<string> GetAvailableResources() => new[] { "audio" };
        public void Dispose() { }
    }

    // The Interface Implementation!
    public class ArenjiSoundFontEngine : IArenjiAudioEngine
    {
        private readonly AudioManager audioManager;
        private Track internalTrack;

        public IClock AudioClock => internalTrack; // The track acts as our clock!
        public double DurationMs => internalTrack?.Length ?? 0;
        public bool IsReady => internalTrack != null && internalTrack.IsLoaded;

        public ArenjiSoundFontEngine(AudioManager manager)
        {
            audioManager = manager;
        }

        public void LoadFiles(string midiPath, string instrumentPath)
        {
            // Clean up the old track if we load a new one
            Dispose();

            // Render the audio in RAM
            var wavBytes = renderMidiToWavBytes(midiPath, instrumentPath);
            
            // Load it into the framework
            var trackStore = audioManager.GetTrackStore(new MemoryAudioStore(wavBytes));
            internalTrack = trackStore.Get("audio");
        }

        public void Play() => internalTrack?.Start();
        public void Pause() => internalTrack?.Stop();
        public void Seek(double timeMs) => internalTrack?.Seek(timeMs);
        
        public void Dispose()
        {
            internalTrack?.Stop();
            internalTrack?.Dispose();
            internalTrack = null;
        }

        // --- The MeltySynth logic remains untouched down here ---
        private byte[] renderMidiToWavBytes(string midiPath, string sf2Path)
        {
            int sampleRate = 44100;
            var synthesizer = new Synthesizer(sf2Path, sampleRate);
            var midiFile = new MeltySynth.MidiFile(midiPath);
            var sequencer = new MidiFileSequencer(synthesizer);

            sequencer.Play(midiFile, false);

            // 1. Calculate the exact number of samples needed based on the MIDI length
            int totalSamples = (int)(sampleRate * midiFile.Length.TotalSeconds);

            // 2. Allocate the arrays for the entire song up front
            var leftBuffer = new float[totalSamples];
            var rightBuffer = new float[totalSamples];

            // 3. Render the entire track in one blazing-fast pass!
            sequencer.Render(leftBuffer, rightBuffer);

            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);

            // 4. Because we know the size up front, we can write the final header instantly
            int dataSize = totalSamples * 4;

            writer.Write("RIFF".ToCharArray()); 
            writer.Write(36 + dataSize); 
            writer.Write("WAVE".ToCharArray()); 
            writer.Write("fmt ".ToCharArray());
            writer.Write(16); 
            writer.Write((short)1); 
            writer.Write((short)2);
            writer.Write(sampleRate); 
            writer.Write(sampleRate * 4); 
            writer.Write((short)4); 
            writer.Write((short)16); 
            writer.Write("data".ToCharArray()); 
            writer.Write(dataSize); 

            // 5. Convert the float buffers to standard 16-bit PCM audio
            for (int i = 0; i < totalSamples; i++)
            {
                writer.Write((short)(Math.Clamp(leftBuffer[i], -1f, 1f) * 32767));
                writer.Write((short)(Math.Clamp(rightBuffer[i], -1f, 1f) * 32767));
            }

            return memoryStream.ToArray();
        }
    }
}