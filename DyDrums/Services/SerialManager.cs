using System.IO.Ports;
using DyDrums.Controllers;

namespace DyDrums.Services
{
    public class SerialManager
    {
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
                //Debug.WriteLine("[Connect] Chamado para porta " + portName);

                if (_serialPort != null)
                {
                    //Debug.WriteLine("[Connect] Fechando porta existente.");
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    if (_serialPort.IsOpen)
                        _serialPort.Close();
                    _serialPort.Dispose();
                    _serialPort = null;
                }

                _serialPort = new SerialPort(portName, baudRate);
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                //Debug.WriteLine("[Connect] Porta aberta com sucesso.");
            }
            catch (UnauthorizedAccessException ex)
            {
                //Debug.WriteLine($"[SerialManager] Porta {portName} está em uso: {ex.Message}");
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"[SerialManager] Erro ao abrir porta {portName}: {ex.Message}");
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

                    //Debug.WriteLine("[SerialManager] Porta Serial desconectada.");
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine($"[SerialManager] Erro ao desconectar: {ex.Message}");
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
            catch (Exception ex)
            {
                //Debug.WriteLine($"[Serial] ERRO: {ex.Message}");
            }
        }


        public string[] GetCOMPorts()
        {
            return SerialPort.GetPortNames();
        }
    }
}




