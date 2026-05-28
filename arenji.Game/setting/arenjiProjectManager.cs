using osuTK.Graphics;
using System;
using System.IO;
using System.Collections.Generic;

namespace arenji.Game
{
    public static class arenjiProjectManager
    {
        // 1. Remember the active project folder!
        public static string CurrentProjectFolder = string.Empty;
        public static string CurrentMidiFileName = string.Empty;
        public static string CurrentBackgroundPath = string.Empty;
        public static string CurrentBackingAudioPath = string.Empty;
        public static void SetBackgroundPath(string absolutePath)
        {
            CurrentBackgroundPath = absolutePath;
        }

        // 2. THE NEW AUTO-SAVE METHOD
        public static void SaveCurrentProject(arenjiSettings settingsPanel)
        {
            // If we haven't loaded or created a project yet, don't try to save!
            if (string.IsNullOrEmpty(CurrentProjectFolder) || string.IsNullOrEmpty(CurrentMidiFileName)) 
                return;

            var lines = new List<string>
            {
                "[General]",
                $"MidiFile={CurrentMidiFileName}",
                "",
                "[Note Setting]",
                $"WhiteNoteWidth={settingsPanel.WhiteNoteWidth.Value}",
                $"NoteRoundness={settingsPanel.NoteRoundness.Value}",
                $"NoteOpacity={settingsPanel.NoteOpacity.Value}",
                $"ColorMode={ArenjiColorManager.CurrentMode}",
                $"SolidColor={ArenjiColorManager.ToHex(ArenjiColorManager.SolidColor)}",
                $"BlackNoteWidth={settingsPanel.BlackNoteWidth.Value}",
                "",
                "[Audio Setting]",
                $"BackingAudioPath={CurrentBackingAudioPath}",
                $"SoundfontVolume={settingsPanel.SoundFontVolume.Value}",
                $"BackingAudioVolume={settingsPanel.BackingAudioVolume.Value}",
                $"MuteSoundfont={settingsPanel.MuteSoundfont.Value}",
                $"MuteBackingAudio={settingsPanel.MuteBackingAudio.Value}",
                "",
                "[Background Setting]",
                $"BackgroundFile={CurrentBackgroundPath}",
                $"BackgroundOpacity={settingsPanel.BackgroundOpacity.Value}",
                $"BackgroundOffset={settingsPanel.BackgroundOffset.Value}",
                "",
                "[Particle Setting]",
                $"ParticleLifetime={settingsPanel.ParticleLifeTime.Value}",
                $"ParticleSpeed={settingsPanel.ParticleSpeed.Value}",
                $"ParticleTurbulance={settingsPanel.ParticleTurbulance.Value}",
                $"ParticleSize={settingsPanel.ParticleSize.Value}",
                $"ParticleCount={settingsPanel.ParticleCount.Value}",
                "",
                "[TrackColors]"
            };

            foreach (var kvp in ArenjiColorManager.TrackColors) lines.Add($"{kvp.Key}={ArenjiColorManager.ToHex(kvp.Value)}");

            lines.Add(""); lines.Add("[NoteColors]");
            foreach (var kvp in ArenjiColorManager.NoteColors) lines.Add($"{kvp.Key}={ArenjiColorManager.ToHex(kvp.Value)}");

            // 3. Add Channel Colors to the save file!
            lines.Add(""); lines.Add("[ChannelColors]");
            foreach (var kvp in ArenjiColorManager.ChannelColors) lines.Add($"{kvp.Key}={ArenjiColorManager.ToHex(kvp.Value)}");

            string iniPath = Path.Combine(CurrentProjectFolder, "project.ini");
            File.WriteAllLines(iniPath, lines);
        }

        public static string CreateProject(string sourceMidiPath, string parentFolder, string projectName, arenjiSettings settingsPanel)
        {
            CurrentProjectFolder = Path.Combine(parentFolder, projectName);
            Directory.CreateDirectory(CurrentProjectFolder);

            CurrentMidiFileName = Path.GetFileName(sourceMidiPath);
            string targetMidiPath = Path.Combine(CurrentProjectFolder, CurrentMidiFileName);
            File.Copy(sourceMidiPath, targetMidiPath, true);

            // Just call our new save method to build the INI!
            SaveCurrentProject(settingsPanel);

            return Path.Combine(CurrentProjectFolder, "project.ini");
        }

        public static string ImportBackground(string sourceFilePath)
        {
            if (string.IsNullOrEmpty(CurrentProjectFolder)) return string.Empty;

            string bgDir = Path.Combine(CurrentProjectFolder, "bg");
            Directory.CreateDirectory(bgDir);

            string safeName = Path.GetFileName(sourceFilePath);
            string destPath = Path.Combine(bgDir, safeName);
            
            // Copy it so it travels with the project!
            if (!File.Exists(destPath) || sourceFilePath != destPath)
                File.Copy(sourceFilePath, destPath, true);

            CurrentBackgroundPath = safeName;
            return safeName; // Return just the relative name
        }

        public static string ImportBackingAudio(string sourceFilePath)
        {
            if (string.IsNullOrEmpty(CurrentProjectFolder) || string.IsNullOrEmpty(sourceFilePath)) 
                return string.Empty;

            // Create an "audio" folder inside the project
            string audioDir = Path.Combine(CurrentProjectFolder, "audio");
            Directory.CreateDirectory(audioDir);

            string safeName = Path.GetFileName(sourceFilePath);
            string destPath = Path.Combine(audioDir, safeName);
            
            // Copy the file so it travels with the project!
            if (!File.Exists(destPath) || sourceFilePath != destPath)
                File.Copy(sourceFilePath, destPath, true);

            CurrentBackingAudioPath = destPath;
            return destPath; 
        }

        public static string LoadProject(string iniPath, arenjiSettings settingsPanel)
        {
            CurrentProjectFolder = Path.GetDirectoryName(iniPath);
            string[] lines = File.ReadAllLines(iniPath);
            string currentSection = "";

            ArenjiColorManager.TrackColors.Clear();
            ArenjiColorManager.NoteColors.Clear();
            ArenjiColorManager.ChannelColors.Clear();
            

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("[") && line.EndsWith("]")) { currentSection = line; continue; }

                var parts = line.Split('=', 2);
                if (parts.Length != 2) continue;
                string key = parts[0].Trim(); string value = parts[1].Trim();

                if (currentSection == "[General]")
                {
                    if (key == "MidiFile") CurrentMidiFileName = value;      
                }

                else if (currentSection == "[Note Setting]")
                {
                    if (key == "WhiteNoteWidth") settingsPanel.WhiteNoteWidth.Value = float.Parse(value);
                    else if (key == "BlackNoteWidth") settingsPanel.BlackNoteWidth.Value = float.Parse(value);
                    else if (key == "NoteRoundness") settingsPanel.NoteRoundness.Value = float.Parse(value);
                    else if (key == "NoteOpacity")
                    {
                        float parsedOpacity = float.Parse(value);
                        settingsPanel.NoteOpacity.Value = parsedOpacity;
                        ArenjiColorManager.GlobalOpacity = parsedOpacity;
                    }
                    else if (key == "ColorMode") Enum.TryParse(value, out ArenjiColorManager.CurrentMode);
                    else if (key == "SolidColor") ArenjiColorManager.SolidColor = ArenjiColorManager.ParseString(value, Color4.Cyan);
                }

                else if (currentSection == "[Background Setting]")
                {
                    if (key == "BackgroundFile") CurrentBackgroundPath = value; 
                    else if (key == "BackgroundOffset") settingsPanel.BackgroundOffset.Value = float.Parse(value);              
                    else if (key == "BackgroundOpacity")
                    {
                        settingsPanel.BackgroundOpacity.Value = float.Parse(value);
                    }
                }

                else if (currentSection == "[Audio Setting]")
                {
                    if (key == "BackingAudioPath") CurrentBackingAudioPath = value;
                    else if (key == "SoundfontVolume") settingsPanel.SoundFontVolume.Value = float.Parse(value);
                    else if (key == "BackingAudioVolume") settingsPanel.BackingAudioVolume.Value = float.Parse(value);
                    else if (key == "MuteSoundfont") settingsPanel.MuteSoundfont.Value = bool.Parse(value);
                    else if (key == "MuteBackingAudio") settingsPanel.MuteBackingAudio.Value = bool.Parse(value);
                }

                else if (currentSection == "[Particle Setting]")
                {
                    if (key == "ParticleLifetime") settingsPanel.ParticleLifeTime.Value = float.Parse(value);
                    else if (key == "ParticleSpeed") settingsPanel.ParticleSpeed.Value = float.Parse(value);
                    else if (key == "ParticleTurbulance") settingsPanel.ParticleTurbulance.Value = float.Parse(value);
                    else if (key == "ParticleSize") settingsPanel.ParticleSize.Value = float.Parse(value);
                    else if (key == "ParticleCount") settingsPanel.ParticleCount.Value = int.Parse(value);
                }
                
                else if (currentSection == "[TrackColors]" && int.TryParse(key, out int tId))
                    ArenjiColorManager.TrackColors[tId] = ArenjiColorManager.ParseString(value, Color4.White);
                else if (currentSection == "[NoteColors]" && int.TryParse(key, out int nId))
                    ArenjiColorManager.NoteColors[nId] = ArenjiColorManager.ParseString(value, Color4.White);
                else if (currentSection == "[ChannelColors]" && int.TryParse(key, out int cId)) // Load Channels!
                    ArenjiColorManager.ChannelColors[cId] = ArenjiColorManager.ParseString(value, Color4.White);
            }

            return Path.Combine(CurrentProjectFolder, CurrentMidiFileName); 
        }
    }
}