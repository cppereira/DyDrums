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
        public event Action<int, int, int>? MidiMessageReceived;
        public SerialManager() { }

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

                Debug.WriteLine($"[SerialManager] Porta {portName} conectada com sucesso.");
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
                        MessageBox.Show("Recebi 0xF0");
                        currentSysex.Clear();
                        currentSysex.Add((byte)b);
                    }
                    //else if (b == 0xF7)
                    //{
                    //    currentSysex.Add((byte)b);
                    //    fullSysexMessages.Add(currentSysex.ToArray());
                    //    currentSysex.Clear();

                    //    MainForm.Instance.BeginInvoke(new Action(() =>
                    //    {
                    //        padManager.ProcessSysex(fullSysexMessages, MainForm.Instance.PadsTable);
                    //        fullSysexMessages.Clear();
                    //    }));
                    //}
                    //else if (currentSysex.Count > 0)
                    //{
                    //    currentSysex.Add((byte)b);
                    //}
                    else
                    {
                        if (_serialPort.BytesToRead >= 2)
                        {
                            int channel = (b & 0x0F) + 1;
                            int data1 = _serialPort.ReadByte();
                            int data2 = _serialPort.ReadByte();

                            if (data1 == 4)
                            {
                                //MidiManager.Instance.SendControlChange(channel, 4, data2);


                                //MainForm.Instance?.Invoke(() =>
                                //{
                                //    if (MainForm.Instance.HHCVerticalProgressBar != null)
                                //    {
                                //        int max = MainForm.Instance.HHCVerticalProgressBar.Maximum;
                                //        int invertedValue = max - data2;
                                //        MainForm.Instance.HHCVerticalProgressBar.Value = Math.Max(MainForm.Instance.HHCVerticalProgressBar.Minimum, invertedValue);
                                //    }
                                //}); ;
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
                Debug.WriteLine($"[SerialManager] Erro na leitura: {ex.Message}");
            }
        }

        public string[] GetCOMPorts()
        {
            return SerialPort.GetPortNames();
        }
    }
}
