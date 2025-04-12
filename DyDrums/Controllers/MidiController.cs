using DyDrums.Services;

namespace DyDrums.Controllers
{
    public class MidiController
    {
        private MidiManager _midiManager;
        private readonly MainForm _mainForm;
        public event Action<int>? HHCValueReceived;

        public MidiController(MainForm mainform, MidiManager midiManager)
        {
            _mainForm = mainform;
            _midiManager = new MidiManager();
            _midiManager.HHCValueReceived += (value) => HHCValueReceived?.Invoke(value);

        }

        public void GetMidiDevices()
        {
            var devices = _midiManager.GetMidiDevices();
            _mainForm.UpdateMidiDevicesComboBox(devices);
        }

        public void Connect(string deviceName)
        {
            _midiManager.Connect(deviceName);
        }

        public void Disconnect()
        {
            _midiManager.Disconnect();
        }

        public void SendMidiMessage(int note, int velocity, int duration = 10, int channel = 1)
        {
            _midiManager.PlayNoteSafe(note, velocity, duration, channel);
        }

        public void HandleHHCControlChange(int channel, int value)
        {
            _midiManager.SendControlChange(channel, 4, value);
            _midiManager.ProcessHHCValue(value); // dispara evento pra UI
        }
    }
}
