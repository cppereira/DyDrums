using DyDrums.Services;

namespace DyDrums.Controllers
{
    public class SerialController

    {
        private SerialManager _serialManager;

        private readonly MainForm _mainForm;
        public event Action<int, int, int> MidiMessageReceived;
        public event Action<int>? HHCVelocityReceived;

        public SerialController(MainForm mainform, SerialManager serialManager)
        {
            _mainForm = mainform;
            _serialManager = serialManager;
            _serialManager.MidiMessageReceived += (channel, data1, data2) =>
            {
                MidiMessageReceived?.Invoke(channel, data1, data2); // repassa
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
