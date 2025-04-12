using System.Diagnostics;
using System.IO.Ports;
using DyDrums.Controllers;

namespace DyDrums.Services
{
    public class SerialManager
    {
        private List<byte> _currentSysex = new();
        private List<byte[]> _fullSysexMessages = new();
        private bool _isClosing = false; // se ainda não adicionou

        public bool IsConnected => _serialPort != null && _serialPort.IsOpen;


        private SerialPort _serialPort;
        private MainForm _mainForm;
        private MidiController _midiController;

        public event Action<List<byte[]>>? SysexBatchReceived;

        //Events
        public event Action<int, int, int>? MidiMessageReceived;
        public event Action<int>? HHCVelocityReceived;


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
                Debug.WriteLine("[Connect] Chamado para porta " + portName);

                if (_serialPort != null)
                {
                    Debug.WriteLine("[Connect] Fechando porta existente.");
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    if (_serialPort.IsOpen)
                        _serialPort.Close();
                    _serialPort.Dispose();
                    _serialPort = null;
                }

                _serialPort = new SerialPort(portName, baudRate);
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                Debug.WriteLine("[Connect] Porta aberta com sucesso.");
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"[SerialManager] Porta {portName} está em uso: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SerialManager] Erro ao abrir porta {portName}: {ex.Message}");
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

                    Debug.WriteLine("[SerialManager] Porta Serial desconectada.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SerialManager] Erro ao desconectar: {ex.Message}");
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
                        _currentSysex.Clear();
                        _currentSysex.Add((byte)b);
                        Debug.WriteLine("[Serial] <<< Início de SysEx (0xF0)");
                    }
                    else if (b == 0xF7)
                    {
                        _currentSysex.Add((byte)b);
                        _fullSysexMessages.Add(_currentSysex.ToArray());

                        Debug.WriteLine($"[Serial] <<< Fim de SysEx (0xF7) | Total: {_fullSysexMessages.Count}");
                        Debug.WriteLine($"[Serial] <<< SysEx Completo: {BitConverter.ToString(_currentSysex.ToArray())}");

                        _currentSysex.Clear();

                        // Evento para processar os dados
                        SysexBatchReceived?.Invoke(_fullSysexMessages.ToList());
                        _fullSysexMessages.Clear();
                    }
                    else if (_currentSysex.Count > 0)
                    {
                        _currentSysex.Add((byte)b);
                    }
                    else
                    {
                        // Mensagens MIDI simples
                        if (_serialPort.BytesToRead >= 2)
                        {
                            int channel = (b & 0x0F) + 1;
                            int data1 = _serialPort.ReadByte();
                            int data2 = _serialPort.ReadByte();

                            //Debug.WriteLine($"[Serial] <<< MIDI - Canal: {channel}, D1: {data1}, D2: {data2}");

                            if (data1 == 4)
                            {
                                if (_midiController == null)
                                {
                                    MessageBox.Show("MidiController está como NULL");
                                }
                                _midiController.HandleHHCControlChange(channel, data2);
                                HHCVelocityReceived?.Invoke(data2);
                            }

                            else
                            {
                                MidiMessageReceived?.Invoke(channel, data1, data2);
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Serial] ERRO: {ex.Message}");
            }
        }


        private bool IsEndMessage(byte[] message)
        {
            return message.Length >= 7 &&
                   message[0] == 0xF0 &&
                   message[1] == 0x77 &&
                   message[2] == 0x02 &&
                   message[3] == 0x7F &&  // Pino 127 = fim
                   message[4] == 0x7F &&
                   message[5] == 0x7F &&
                   message[6] == 0xF7;
        }

        public string[] GetCOMPorts()
        {
            return SerialPort.GetPortNames();
        }


        public void Handshake()
        {
            const int totalPads = 15;

            if (_serialPort == null || !_serialPort.IsOpen)
            {
                Debug.WriteLine("[Handshake] Porta serial não está aberta.");
                return;
            }

            byte[] parameters = new byte[]
            {
        0x00, 0x01, 0x02, 0x03, // Note, Threshold, ScanTime, MaskTime
        0x04, 0x05, 0x06, 0x07, // Retrigger, Curve, Xtalk, XtalkGroup
        0x08,                   // CurveForm
        0x0D, 0x0E, 0x0F        // Type, Channel, Gain
            };

            try
            {
                for (int pad = 0; pad < totalPads; pad++)
                {
                    foreach (byte param in parameters)
                    {
                        if (!_serialPort.IsOpen) return;

                        byte[] sysex = BuildSysExRequest((byte)pad, param);
                        Debug.WriteLine($"[Handshake] >> Enviando SysEx - Pad: {pad}, Param: {param:X2}, Bytes: {BitConverter.ToString(sysex)}");
                        _serialPort.Write(sysex, 0, sysex.Length);

                        // Remova o sleep se não for necessário.
                        Thread.Sleep(1);
                    }
                }

                byte[] endMessage = new byte[] { 0xF0, 0x77, 0x02, 0x7F, 0x7F, 0x7F, 0xF7 };
                _serialPort.Write(endMessage, 0, endMessage.Length);
                Debug.WriteLine("[Handshake] >> Mensagem de fim enviada.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Handshake] ERRO: " + ex.Message);
            }
        }


        private bool EnsurePortOpen()
        {
            if (_serialPort == null)
            {
                Debug.WriteLine("[EnsurePortOpen] _serialPort está null.");
                return false;
            }

            if (!_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Open();
                    Debug.WriteLine("[EnsurePortOpen] Porta serial aberta dinamicamente.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[EnsurePortOpen] Erro ao abrir a porta: {ex.Message}");
                    return false;
                }
            }

            return true;
        }





        private byte[] BuildSysExRequest(byte pad, byte param)
        {
            return new byte[] { 0xF0, 0x77, 0x02, pad, param, 0x00, 0xF7 };
        }
    }
}




