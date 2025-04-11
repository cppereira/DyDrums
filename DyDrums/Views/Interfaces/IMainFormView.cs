using DyDrums.Models;

namespace DyDrums.Views.Interfaces
{
    public interface IMainFormView
    {
        public void UpdateCOMPortsComboBox(string[] ports);
        public void UpdateMidiDevicesComboBox(List<MidiDevice> devices);
    }
}
