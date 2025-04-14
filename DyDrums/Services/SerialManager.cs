using System.IO.Ports;
using DyDrums.Controllers;
using DyDrums.Models;

namespace DyDrums.Services
{
    public class SerialManager
    {
        private SerialPort _serialPort;
        private MainForm _mainForm;
        private MidiController _midiController;

        public event Action<List<byte[]>>? SysexBatchReceived;

        //Lists
        private List<byte> _sysexBuffer = new();

        //Events
        public event Action<int, int, int>? MidiMessageReceived;
        public event Action<int>? HHCVelocityReceived;
        public event Action<int, byte, int> SysExParameterReceived;

        private readonly Dictionary<int, Dictionary<byte, byte>> _padParameterCache = new();
        public event Action<Pad> OnPadReceived;

        public SerialManager(MidiController midiController)
        {
            if (midiController == null) MessageBox.Show("MidiController está como NULL");
            _midiController = midiController;
        }

        public void SetMidiController(MidiController controller)
        {
            _midiController = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public void Connect(string portName, int baudRate = 115200)
        {

            try
            {
                if (_serialPort != null)
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    if (_serialPort.IsOpen)
                        _serialPort.Close();
                    _serialPort.Dispose();
                    _serialPort = null;
                }
                _serialPort = new SerialPort(portName, baudRate);
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"A porta {portName} está em uso: {ex.Message}", "Erro de conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir a porta {portName}: {ex.Message}", "Erro de conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Disconnect()
        {
            if (_serialPort != null)
            {
                try
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;

                    if (_serialPort.IsOpen)
                        _serialPort.Close();

                    _serialPort.Dispose();
                    _serialPort = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao desconectar: {ex.Message}", "Erro de conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort == null || !_serialPort.IsOpen) return;

            try
            {
                while (_serialPort.BytesToRead > 0)
                {
                    int b = _serialPort.ReadByte();

                    if (b == 0xF0)
                    {
                        _sysexBuffer.Clear();
                        _sysexBuffer.Add((byte)b);

                        while (_serialPort.BytesToRead > 0)
                        {
                            byte nextByte = (byte)_serialPort.ReadByte();
                            _sysexBuffer.Add(nextByte);

                            if (nextByte == 0xF7) // fim da SysEx
                            {
                                ParseSysExMessage(_sysexBuffer.ToArray());
                                break;
                            }
                        }
                    }
                    else if ((_serialPort.BytesToRead >= 2) && ((b & 0xF0) == 0x90 || (b & 0xF0) == 0xB0)) // MIDI Messages
                    {
                        int channel = (b & 0x0F) + 1;
                        int data1 = _serialPort.ReadByte();
                        int data2 = _serialPort.ReadByte();

                        if (data1 == 4) // HHC control
                        {
                            _midiController?.HandleHHCControlChange(channel, data2);
                            HHCVelocityReceived?.Invoke(data2);
                        }
                        else
                        {
                            MidiMessageReceived?.Invoke(channel, data1, data2);
                        }
                    }
                    // else ignore other bytes (realtime messages, noise, etc)
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro na Porta Serial", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string[] GetCOMPorts()
        {
            return SerialPort.GetPortNames();
        }

        public void SendHandshake()
        {
            const int totalPads = 15;
            byte[] parameters = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x0D, 0x0E, 0x0F };

            if (!EnsurePortOpen())
            {
                MessageBox.Show($"A Porta Serial não está aberta.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            for (int pad = 0; pad < totalPads; pad++)
            {
                foreach (byte param in parameters)
                {
                    byte[] sysex = BuildSysExRequest((byte)pad, param);
                    _serialPort.Write(sysex, 0, sysex.Length);
                    Thread.Sleep(5); // Em todos os testes, este valor foi o mais performático. (0 = tartaruga, 50 = erro)
                }
            }

            byte[] endMessage = new byte[] { 0xF0, 0x77, 0x02, 0x7F, 0x7F, 0x7F, 0xF7 };
            _serialPort.Write(endMessage, 0, endMessage.Length);
        }

        private byte[] BuildSysExRequest(byte pad, byte param)
        {
            return new byte[] { 0xF0, 0x77, 0x02, pad, param, 0x00, 0xF7 };
        }

        private bool EnsurePortOpen()
        {
            if (_serialPort == null) return false;

            if (!_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Open();
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }

        private void ParseSysExMessage(byte[] data)
        {
            int padIndex = data[3];
            byte paramId = data[4];
            int value = data[5];
            if (padIndex < 0 || padIndex > 14)
            {
                return;
            }
            if (data == null || data.Length < 7)
            {
                return;
            }

            // Formato esperado: F0 77 02 <PAD_INDEX> <PARAM_ID> <VALUE> F7
            if (data[0] != 0xF0 || data[1] != 0x77 || data[2] != 0x02 || data[^1] != 0xF7)
            {
                return;
            }

            // Aciona o evento para notificar o SerialController
            SysExParameterReceived?.Invoke(padIndex, paramId, value);
        }
    }
}




