using System.Diagnostics;
using DyDrums.Models;
using DyDrums.Services;

namespace DyDrums.Controllers
{
    public class SerialController

    {
        private SerialManager _serialManager;
        private PadManager _padManager;
        private readonly MainForm _mainForm;
        public event Action<int, int, int> MidiMessageReceived;
        public event Action<int>? HHCVelocityReceived;

        private Dictionary<int, Dictionary<byte, int>> _padParameterCache = new();
        private Dictionary<int, Pad> _padList = new();


        public SerialController(MainForm mainform, SerialManager serialManager)
        {
            _mainForm = mainform;
            _serialManager = serialManager;
            _padManager = new PadManager(_serialManager);
            _serialManager.MidiMessageReceived += (channel, data1, data2) =>
            {
                MidiMessageReceived?.Invoke(channel, data1, data2); // repassa
            };

            _serialManager.HHCVelocityReceived += (velocity) =>
            {
                HHCVelocityReceived?.Invoke(velocity); // Repassa pra View
            };

            _serialManager.OnPadReceived += pad =>
            {
                _padList[pad.Id] = pad;
                //_mainForm.UpdateGrid(_padList.Values.Cast<Pad>().ToList());
            };

            _serialManager.SysExParameterReceived += HandleSysExParameter;

        }

        private void HandleSysExParameter(int padIndex, byte paramId, int value)
        {
            // Armazena o parâmetro
            if (!_padParameterCache.ContainsKey(padIndex))
                _padParameterCache[padIndex] = new Dictionary<byte, int>();

            _padParameterCache[padIndex][paramId] = value;

            // Quando tivermos todos os 13 parâmetros
            if (_padParameterCache[padIndex].Count == 13)
            {
                var pad = PadFactory.FromParameters(padIndex, _padParameterCache[padIndex]);
                _padList[pad.Id] = pad;

                Debug.WriteLine($"[Pad COMPLETO] PAD {padIndex} adicionado ao _padList.");

                // Se já coletamos os 15 pads
                if (_padList.Count == 15)
                {
                    Debug.WriteLine("[PadManager] Todos os pads lidos. Atualizando Grid e salvando JSON.");

                    //_mainForm.UpdateGrid(_padList.Values.ToList());
                    _padManager.SavePads(_padList.Values.ToList());
                    //_padManager.LoadPads();
                }
            }

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

        public void StartHandshake()
        {
            _serialManager.SendHandshake();
        }
    }
}
