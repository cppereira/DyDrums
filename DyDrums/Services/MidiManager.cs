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
        public event Action<int>? HHCValueReceived;



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

        public bool Connect(string deviceName)
        {
            for (int i = 0; i < MidiOut.NumberOfDevices; i++)
            {
                if (MidiOut.DeviceInfo(i).ProductName == deviceName)
                {
                    Disconnect(); // fecha se já houver conexão
                    midiOut = new MidiOut(i);
                    return true;
                }
            }
            return false;
        }

        public void Disconnect()
        {
            midiOut?.Dispose();
            midiOut = null;
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

        public void ProcessHHCValue(int rawValue)
        {
            HHCValueReceived?.Invoke(rawValue);
        }

        public void SendControlChange(int channel, int control, int value)
        {
            if (midiOut != null)
            {
                byte status = (byte)(0xB0 | channel - 1); // CC message
                int midiMessage = status | control << 8 | value << 16;
                midiOut.Send(midiMessage);
            }
        }
    }
}
