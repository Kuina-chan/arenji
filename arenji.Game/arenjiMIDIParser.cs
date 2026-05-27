using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace arenji.MIDIParser
{
    public class MIDIReader
    {
        private byte[] _rawData;
        public List<string> DebugLog { get; private set; } = new(); // For simple debug output

        public void LoadMidi(string filePath)
        {
            _pointer = 0; // Reset pointer for new file
            _rawData = File.ReadAllBytes(filePath);
            MIDIParserInternal();
        }

        private int _pointer = 0;

        private void MIDIParserInternal()
        {
            string headerSignature = Encoding.ASCII.GetString(_rawData, _pointer, 4);
            _pointer += 4;
            
            if (headerSignature != "MThd") throw new Exception("Not a valid MIDI file!");

            _pointer += 4; // Skip length

            short format = ReadBigEndianShort();
            short trackCount = ReadBigEndianShort();
            short division = ReadBigEndianShort();

            Console.WriteLine($"--- HEADER ---");
            Console.WriteLine($"Format: {format}, Tracks: {trackCount}, Division: {division}");

            for (int i = 0; i < trackCount; i++)
            {
                Console.WriteLine($"\n--- PARSING TRACK {i} ---");
                ParseTrack();
            }
        }

        private void ProcessEvent(byte status, int deltaTime)
        {
            // MIDI Status bytes are split: 
            // High nibble (bits 4-7) is the command
            // Low nibble (bits 0-3) is the channel
            byte command = (byte)(status & 0xF0);
            byte channel = (byte)(status & 0x0F);

            if (status == 0xFF) // Meta Event
            {
                byte type = _rawData[_pointer++];
                int length = ReadVLQ();
                _pointer += length; // Skip meta data for now
                Console.WriteLine($"[Delta: {deltaTime}] Meta Event Type: 0x{type:X2}, Length: {length}");
            }
            else if (command == 0x90 || command == 0x80) // Note On or Note Off
            {
                byte note = _rawData[_pointer++];
                byte velocity = _rawData[_pointer++];
                string type = (command == 0x90 && velocity > 0) ? "Note On" : "Note Off";
                Console.WriteLine($"[Delta: {deltaTime}] {type} | Ch: {channel} | Note: {note} | Vel: {velocity}");
            }
            else if (command == 0xB0 || command == 0xE0) // Control Change or Pitch Bend (3-byte messages)
            {
                _pointer += 2;
                Console.WriteLine($"[Delta: {deltaTime}] 3-Byte Control Message (0x{command:X2})");
            }
            else if (command == 0xC0 || command == 0xD0) // Program Change or Aftertouch (2-byte messages)
            {
                _pointer += 1;
                Console.WriteLine($"[Delta: {deltaTime}] 2-Byte Control Message (0x{command:X2})");
            }
        }

        // --- HELPERS ---

        private int ReadVLQ()
        {
            int value = 0;
            byte b;
            do
            {
                b = _rawData[_pointer++];
                value = (value << 7) | (b & 0x7F);
            } while ((b & 0x80) != 0);
            return value;
        }

        private short ReadBigEndianShort()
        {
            byte[] bytes = { _rawData[_pointer + 1], _rawData[_pointer] };
            _pointer += 2;
            return BitConverter.ToInt16(bytes, 0);
        }

        private int ReadBigEndianInt()
        {
            byte[] bytes = { _rawData[_pointer + 3], _rawData[_pointer + 2], _rawData[_pointer + 1], _rawData[_pointer] };
            _pointer += 4;
            return BitConverter.ToInt32(bytes, 0);
        }

        byte lastStatus = 0;

        private void ParseTrack()
        {
            //string trackSignature = Encoding.ASCII.GetString(_rawData, _pointer, 4);
            _pointer += 4;
            
            int trackLength = ReadBigEndianInt();
            int endOfTrack = _pointer + trackLength;

            while (_pointer < endOfTrack)
            {
                int deltaTime = ReadVLQ();
                byte status = _rawData[_pointer];

                if (status < 0x80) 
                {
                    status = lastStatus;
                }
                else 
                {
                    _pointer++;
                    lastStatus = status;
                }
                ProcessEvent(status, deltaTime);
            }
        }
    }
}