using DyDrums.Services;

namespace DyDrums.Controllers
{
    public class SerialController

    {
        private SerialManager _serialManager;

        private readonly MainForm _mainForm;
        public event Action<int, int, int> MidiMessageReceived;
        public event Action<int>? HHCVelocityReceived;

        public SerialController(MainForm mainform)
        {
            _mainForm = mainform;
            _serialManager = new SerialManager();
            _serialManager.MidiMessageReceived += (ch, d1, d2) =>
            {
                MidiMessageReceived?.Invoke(ch, d1, d2); // repassa
            };



            _serialManager.HHCVelocityReceived += (velocity) =>
            {
                HHCVelocityReceived?.Invoke(velocity); // Repassa pra View
            };
        }

        public void GetCOMPorts()
        {
            string[] ports = _serialManager.GetCOMPorts();
            _mainForm.UpdateCOMPortsComboBox(ports);
        }

        public void Connect(string portName, int baudRate = 115200)
        {
            _serialManager.Connect(portName, baudRate);
        }

        public void Disconnect()
        {
            _serialManager.Disconnect();
        }


    }
}
