using DyDrums.Controllers;
using DyDrums.Models;
using NAudio.Midi;

namespace DyDrums.Services
{
    public class MidiManager
    {
        private SerialController _serialController;

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
    }
}
