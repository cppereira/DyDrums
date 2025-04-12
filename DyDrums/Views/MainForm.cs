using System.Diagnostics;
using DyDrums.Controllers;
using DyDrums.Models;
using DyDrums.Services;
using DyDrums.Views.Interfaces;

namespace DyDrums
{
    public partial class MainForm : Form, IMainFormView
    {
        private SerialController _serialController;
        private DyDrums.Controllers.MidiController _midiController;
        private SerialManager _serialManager = new SerialManager();
        private MidiManager _midiManager = new MidiManager();

        public MainForm()

        {
            InitializeComponent();
            _serialController = new SerialController(this); // <- passa a si mesmo para o Controller, sem instanciar...
            _midiController = new MidiController(this); // <- passa a si mesmo para o Controller, sem instanciar...
            _serialManager = new SerialManager();
        }

        private void MainForm_Load(object? sender, EventArgs? e)
        {
            //Transforma o CheckBox em Botao
            ConnectCheckBox.Appearance = Appearance.Button;
            ConnectCheckBox.TextAlign = ContentAlignment.MiddleCenter;
            ConnectCheckBox.FlatStyle = FlatStyle.Standard;
            ConnectCheckBox.Size = new Size(100, 40);

            _serialController.MidiMessageReceived += OnMidiMessageReceived;
            _serialController.HHCVelocityReceived += UpdateHHCBar;

            Debug.WriteLine("Inscrito no evento MIDI!");
            _serialController.GetCOMPorts();
            _midiController.GetMidiDevices();
        }

        public void UpdateCOMPortsComboBox(string[] ports)
        {
            COMPortsComboBox.Items.Clear();
            COMPortsComboBox.Items.AddRange(ports);

            // Selecionar a última porta COM disponível (normalmente onde já está o Arduino)
            if (ports.Length > 0)
            {
                COMPortsComboBox.SelectedItem = ports[^1];
            }
        }

        public void UpdateMidiDevicesComboBox(List<MidiDevice> devices)
        {
            MidiDevicesComboBox.Items.Clear();
            MidiDevicesComboBox.Items.AddRange(devices.ToArray());

            // Selecionar o último dispositivo MIDI disponível
            if (MidiDevicesComboBox.Items.Count > 0)
            {
                MidiDevicesComboBox.SelectedIndex = MidiDevicesComboBox.Items.Count - 1;
            }
        }

        private void ConnectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (ConnectCheckBox.Checked)
                {
                    ConnectCheckBox.Text = "Desconectar";
                    // Obter porta selecionada
                    string selectedPort = COMPortsComboBox.SelectedItem?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(selectedPort))
                        _serialController.Connect(selectedPort);
                    else
                        throw new Exception("Nenhuma porta COM selecionada.");

                    MidiMonitorRichText.Enabled = true;
                }
                else
                {
                    ConnectCheckBox.Text = "Conectar";
                    _serialController.Disconnect();
                    //_midiManager.Disconnect();
                }
            }
            catch
            {

            }
        }

        private void OnMidiMessageReceived(int channel, int note, int velocity)
        {
            this.Invoke(() =>
            {
                string timestamp = DateTime.Now.ToString("ss,fff");

                MidiMonitorRichText.SelectionFont = new Font(MidiMonitorRichText.Font, FontStyle.Bold);
                switch (note)
                {
                    case 36: MidiMonitorRichText.SelectionColor = Color.Green; break;
                    case 38: MidiMonitorRichText.SelectionColor = Color.Red; break;
                    case 98: MidiMonitorRichText.SelectionColor = Color.Brown; break;
                    case 47: MidiMonitorRichText.SelectionColor = Color.Brown; break;
                    case 45: MidiMonitorRichText.SelectionColor = Color.Brown; break;
                    case 43: MidiMonitorRichText.SelectionColor = Color.Brown; break;
                    case 41: MidiMonitorRichText.SelectionColor = Color.Brown; break;
                    case 49: MidiMonitorRichText.SelectionColor = Color.Blue; break;
                    case 55: MidiMonitorRichText.SelectionColor = Color.Blue; break;
                    case 57: MidiMonitorRichText.SelectionColor = Color.Blue; break;
                    case 51: MidiMonitorRichText.SelectionColor = Color.Blue; break;
                    case 52: MidiMonitorRichText.SelectionColor = Color.Blue; break;
                    case 53: MidiMonitorRichText.SelectionColor = Color.Blue; break;
                    case 7: MidiMonitorRichText.SelectionColor = Color.Blue; break;
                    case 24: MidiMonitorRichText.SelectionColor = Color.Blue; break;
                    default: MidiMonitorRichText.SelectionColor = Color.Black; break;
                }

                string formatted = string.Format("[{0}] => Canal: {1,-2} | Nota: {2,-3} | Velocity: {3,-3}", timestamp, channel, note, velocity);
                MidiMonitorRichText.AppendText(formatted + Environment.NewLine);
                MidiMonitorRichText.SelectionStart = MidiMonitorRichText.Text.Length;
                MidiMonitorRichText.ScrollToCaret();
                Application.DoEvents();

                // Envia para o MIDI
                _midiManager.SendNoteOn(note, velocity, 0);
                Task.Run(async () =>
                {
                    await Task.Delay(5);
                    _midiManager.PlayNoteSafe(note, velocity, 20, channel);
                });

                // Atualiza barra de Hi-Hat (nota 4 por padrão)
                if (note == 4)
                {
                    HHCVerticalProgressBar.BarColor = Color.DeepSkyBlue;
                    HHCVerticalProgressBar.Value = 84;
                    HHCVerticalProgressBar.Value = Math.Min(velocity, HHCVerticalProgressBar.Maximum);
                }
            });
        }

        private void UpdateHHCBar(int value)
        {
            BeginInvoke(() =>
            {
                int max = HHCVerticalProgressBar.Maximum;
                int invertedValue = max - value;
                HHCVerticalProgressBar.Value = Math.Max(HHCVerticalProgressBar.Minimum, invertedValue);
            });
        }
    }
}
