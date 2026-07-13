using osu.Framework.Timing;

namespace arenji.Game
{
    public interface IArenjiAudioEngine
    {
        // The visualizer will read this clock to move the notes
        IClock AudioClock { get; }
        
        // Information about the track
        double DurationMs { get; }
        bool IsReady { get; }

        // The core commands
        void LoadFiles(string midiPath, string instrumentPath);
        void Play();
        void Pause();
        void Seek(double timeMs);
        void Dispose();
        double Volume {get; set;}
    }
}