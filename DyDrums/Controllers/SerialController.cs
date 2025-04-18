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

            _serialManager.SysExParameterReceived += HandleSysExParameter;

            _serialManager.OnPadReceived += pad =>
            {
                _padList[pad.Id] = pad;
            };

            _serialManager.SendAllPadsToArduino(_padManager.Pads);

            _serialManager.SysExParameterReceived += HandleSysExParameter;
            _serialManager.SysExTransmissionComplete += HandleTransmissionComplete;
        }

        private void HandleSysExParameter(int padIndex, byte paramId, int value)
        {
            if (!_padParameterCache.ContainsKey(padIndex))
                _padParameterCache[padIndex] = new Dictionary<byte, int>();

            _padParameterCache[padIndex][paramId] = value;
        }

        private void HandleTransmissionComplete()
        {
            Debug.WriteLine("🔥 Transmissão finalizada. Montando pads...");

            foreach (var kv in _padParameterCache)
            {
                int padIndex = kv.Key;
                var paramDict = kv.Value;

                var pad = PadFactory.FromParameters(padIndex, paramDict);

                if (_padList.TryGetValue(pad.Id, out var _padListCache))
                {
                    // Aqui pegava do pad antigo
                    if (string.IsNullOrWhiteSpace(pad.PadName))
                    {
                        pad.PadName = _padListCache.PadName;
                    }
                }

                _padList[pad.Id] = pad;
            }

            _mainForm.BeginInvoke(() =>
            {
                _mainForm.UpdateGrid(_padList.Values.ToList());
            });

            _padManager.SaveAllPads(_padList.Values.ToList());

            Debug.WriteLine($"✨ {_padList.Count} pads atualizados e salvos.");
        }


        //Envia um pad para salvar na EEPROM
        public void SendPadToArduino(Pad pad)
        {
            _serialManager.SendPadToArduino(pad);
        }

        //Envia todos os Pads para salvar no JSON
        public void SendPadsToJSON(List<Pad> pads)
        {
            _padManager.SaveAllPads(pads);
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

        public void StartHandshake(byte command)
        {
            _serialManager.SendHandshake(command);
        }

        public void InitializePadList(List<Pad> pads)
        {
            _padList = pads.ToDictionary(p => p.Id);
        }
    }
}
