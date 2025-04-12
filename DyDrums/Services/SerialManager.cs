using System.Diagnostics;
using System.IO.Ports;

namespace DyDrums.Services
{
    public class SerialManager
    {
        private SerialPort _serialPort;
        private MainForm _mainForm;
        private List<byte> currentSysex = new();
        private static List<byte[]> fullSysexMessages = new();
        public event Action<List<byte[]>>? SysexBatchReceived;

        //Events
        public event Action<int, int, int>? MidiMessageReceived;
        public event Action<int>? HHCVelocityReceived;


        public SerialManager()
        {

            Debug.WriteLine($"[SerialManager] Instanciado: {GetHashCode()}");
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

                    // Início de uma nova mensagem SysEx
                    if (b == 0xF0)
                    {
                        currentSysex.Clear();
                        currentSysex.Add((byte)b);
                    }
                    // Fim da mensagem SysEx
                    else if (b == 0xF7)
                    {
                        currentSysex.Add((byte)b);
                        fullSysexMessages.Add(currentSysex.ToArray());
                        currentSysex.Clear();

                        // Dispara o evento para processamento externo
                        SysexBatchReceived?.Invoke(fullSysexMessages.ToList());
                        fullSysexMessages.Clear();
                    }
                    // Continuando mensagem SysEx
                    else if (currentSysex.Count > 0)
                    {
                        currentSysex.Add((byte)b);
                    }
                    // Se não é SysEx, pode ser MIDI normal
                    else
                    {
                        // Verifica se tem dados suficientes pro pacote MIDI (3 bytes)
                        if (_serialPort.BytesToRead >= 2)
                        {
                            int channel = (b & 0x0F) + 1;
                            int data1 = _serialPort.ReadByte();
                            int data2 = _serialPort.ReadByte();

                            // Se for mensagem de HHC (nota 4 por convenção)
                            if (data1 == 4)
                            {
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
                Debug.WriteLine($"[SerialManager] Erro na leitura da serial: {ex.Message}");
            }
        }


        public string[] GetCOMPorts()
        {
            return SerialPort.GetPortNames();
        }


        public async Task HandshakeAsync()
        {
            const int totalPads = 15;

            if (_serialPort == null)
            {
                Debug.WriteLine("[HandshakeAsync] _serialPort está NULL.");
                return;
            }

            if (!EnsurePortOpen())
            {
                Debug.WriteLine("[HandshakeAsync] Porta não foi aberta.");
                return;
            }

            byte[] parameters = new byte[]
            {
                0x00, 0x01, 0x02, 0x03,
                0x04, 0x05, 0x06, 0x07,
                0x08, 0x0D, 0x0E, 0x0F
            };

            try
            {
                // só 1 pad por vez, e 13 mensagens com pausa de 20ms
                for (int pad = 0; pad < totalPads; pad++)
                {
                    foreach (byte param in parameters)
                    {
                        var msg = BuildSysExRequest((byte)pad, param);
                        if (_serialPort == null)
                        {
                            Debug.WriteLine("[SerialManager] ERRO: _serialPort está NULL antes do Write!");
                            return;
                        }
                        _serialPort.Write(msg, 0, msg.Length);
                        await Task.Delay(20); // tempo p/ Arduino respirar
                    }
                    await Task.Delay(100); // pausa entre pads
                }

                var endMessage = new byte[] { 0xF0, 0x77, 0x02, 0x7F, 0x7F, 0x7F, 0xF7 };
                _serialPort.Write(endMessage, 0, endMessage.Length);

                Debug.WriteLine("[HANDSHAKE] Handshake concluído.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HANDSHAKE] Erro: {ex.Message}");
            }
        }



        private byte[] BuildSysExRequest(byte pad, byte param)
        {
            return new byte[] { 0xF0, 0x77, 0x02, pad, param, 0x00, 0xF7 };
        }

        private bool EnsurePortOpen()
        {
            Debug.WriteLine($"[EnsurePortOpen] _serialPort is null? {_serialPort == null}");
            Debug.WriteLine($"[EnsurePortOpen] _serialPort.IsOpen? {_serialPort?.IsOpen}");

            if (_serialPort == null) return false;

            if (!_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Open();
                    Debug.WriteLine("[SerialManager] Porta aberta dinamicamente.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[SerialManager] Falha ao abrir: " + ex.Message);
                    return false;
                }
            }

            return true;
        }
    }
}




