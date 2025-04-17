using System.Diagnostics;
using System.IO.Ports;
using DyDrums.Controllers;
using DyDrums.Models;

namespace DyDrums.Services
{
    public class SerialManager
    {
        //Define o comando para solicitar todos os Pads para o Firmware
        public const byte CMD_SEND_ALL_PADS = 0x25;



        private SerialPort _serialPort;
        private MainForm _mainForm;
        private MidiController _midiController;

        public event Action<List<byte[]>>? SysexBatchReceived;
        public event Action SysExTransmissionComplete;

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
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
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
            const byte CMD_SEND_ALL_PADS = 0x25;

            if (!EnsurePortOpen())
            {
                MessageBox.Show($"A Porta Serial não está aberta.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Envia o comando mágico para o Arduino enviar tudo
            _serialPort.Write(new byte[] { CMD_SEND_ALL_PADS }, 0, 1);
            Debug.WriteLine("[Serial] Handshake enviado: CMD_SEND_ALL_PADS (0x25)");
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
            if (data == null || data.Length < 4)
                return;

            if (data[0] != 0xF0 || data[1] != 0x77 || data[^1] != 0xF7)
                return;

            // Mensagem de fim de transmissão
            if (data[2] == 0x7F)
            {
                Debug.WriteLine("✅ SysEx de fim da transmissão recebido.");
                SysExTransmissionComplete?.Invoke(); // novo evento que vamos usar no Controller
                return;
            }

            // Mensagem de parâmetro de pad
            if (data.Length >= 7 && data[2] == 0x02)
            {
                int padIndex = data[3];
                byte paramId = data[4];
                int value = data[5];

                Debug.WriteLine($"[SysEx] PadIndex: {padIndex} - ParamId: {paramId} - Valor: {value}");
                SysExParameterReceived?.Invoke(padIndex, paramId, value);
            }
        }


        //EM DESENVOLVIMENTO
        public void SendAllPadsToArduino(List<Pad> pads)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
            {
                Debug.WriteLine("[SendAllPads] Porta serial não está aberta.");
                return;
            }

            foreach (var pad in pads)
            {
                SendPadToArduino(pad);
                Thread.Sleep(10); // delayzinho entre pads (ajusta se precisar)
            }

            Debug.WriteLine("[SendAllPads] Envio completo.");
        }

        public void SendPadToArduino(Pad pad)
        {
            Dictionary<byte, int> parametros = new Dictionary<byte, int>
            {
                { 0x00, pad.Note },
                { 0x01, pad.Threshold },
                { 0x02, pad.ScanTime },
                { 0x03, pad.MaskTime },
                { 0x04, pad.Retrigger },
                { 0x05, pad.Curve },
                { 0x06, pad.Xtalk },
                { 0x07, pad.XtalkGroup },
                { 0x08, pad.CurveForm },
                { 0x0D, pad.Type },
                { 0x0E, pad.Channel },
                { 0x0F, pad.Gain },
            };

            foreach (var kvp in parametros)
            {
                byte[] sysex = BuildSysExWrite(pad.Id, kvp.Key, kvp.Value);
                _serialPort.Write(sysex, 0, sysex.Length);
                Thread.Sleep(5); // delay entre parâmetros
            }

            Debug.WriteLine($"[SendPad] PAD {pad.Id} enviado.");
        }

        private byte[] BuildSysExWrite(int padId, byte paramId, int value)
        {
            return new byte[]
            {
                0xF0,
                0x77,
                0x01, // código de escrita
                (byte)padId,
                paramId,
                (byte)value,
                0xF7
            };
        }


    }
}




