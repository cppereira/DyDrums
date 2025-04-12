using DyDrums.Services;

namespace DyDrums.Controllers
{
    public class MidiController
    {
        private MidiManager _midiManager;
        private readonly MainForm _mainForm;

        public MidiController(MainForm mainform, MidiManager midiManager)
        {
            _mainForm = mainform;
            _midiManager = new MidiManager();
        }

        public void GetMidiDevices()
        {
            var devices = _midiManager.GetMidiDevices();
            _mainForm.UpdateMidiDevicesComboBox(devices);
        }
    }
}
