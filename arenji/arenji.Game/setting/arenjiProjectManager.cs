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
                $"WhiteNoteWidth={settingsPanel.WhiteNoteWidth.Value}",
                $"BlackNoteWidth={settingsPanel.BlackNoteWidth.Value}",
                $"NoteRoundness={settingsPanel.NoteRoundness.Value}",
                $"ColorMode={ArenjiColorManager.CurrentMode}",
                $"SolidColor={ArenjiColorManager.ToHex(ArenjiColorManager.SolidColor)}",
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

        public static string LoadProject(string iniPath, arenjiSettings settingsPanel)
        {
            CurrentProjectFolder = Path.GetDirectoryName(iniPath);
            string[] lines = File.ReadAllLines(iniPath);
            string currentSection = "";

            ArenjiColorManager.TrackColors.Clear();
            ArenjiColorManager.NoteColors.Clear();
            ArenjiColorManager.ChannelColors.Clear(); // Clear old channels!

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
                    else if (key == "WhiteNoteWidth") settingsPanel.WhiteNoteWidth.Value = float.Parse(value);
                    else if (key == "BlackNoteWidth") settingsPanel.BlackNoteWidth.Value = float.Parse(value);
                    else if (key == "NoteRoundness") settingsPanel.NoteRoundness.Value = float.Parse(value);
                    else if (key == "ColorMode") Enum.TryParse(value, out ArenjiColorManager.CurrentMode);
                    else if (key == "SolidColor") ArenjiColorManager.SolidColor = ArenjiColorManager.ParseString(value, Color4.Cyan);
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