using DyDrums.Controllers;
using DyDrums.Models;
using NAudio.Midi;

namespace DyDrums.Services
{
    public class MidiManager
    {
        private SerialController _serialController;
        private MidiOut midiOut;
        public event Action<int, int, int>? MidiMessageReceived;


        public List<MidiDevice> GetMidiDevices()
        {
            var devices = new List<MidiDevice>();
            for (int i = 0; i < MidiOut.NumberOfDevices; i++)
            {
                devices.Add(new MidiDevice
                {
                    Id = i,
                    Name = MidiOut.DeviceInfo(i).ProductName
                });
            }
            return devices;
        }

        public void SendNoteOn(int note, int velocity, int channel = 0)
        {
            if (midiOut == null) return;
            int midiChannel = channel & 0x0F;
            int message = 0x90 | midiChannel | note << 8 | velocity << 16;
            midiOut.Send(message);
        }

        public void SendNoteOff(int note, int velocity = 0, int channel = 0)
        {
            if (midiOut == null) return;
            int midiChannel = channel & 0x0F;
            int message = 0x80 | midiChannel | note << 8 | velocity << 16;
            midiOut.Send(message);
        }
        public async void PlayNoteSafe(int note, int velocity, int durationMs = 10, int channel = 0)
        {
            SendNoteOn((byte)note, (byte)velocity, (byte)channel);
            await Task.Delay(durationMs);
            SendNoteOff((byte)note, (byte)channel);
        }
    }
}
