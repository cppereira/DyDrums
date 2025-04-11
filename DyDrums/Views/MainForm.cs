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
        private SerialManager _serialManager;
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
    }
}
