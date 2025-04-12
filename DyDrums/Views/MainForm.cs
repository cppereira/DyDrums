using System.ComponentModel;
using DyDrums.Controllers;
using DyDrums.Models;
using DyDrums.Services;
using DyDrums.Views.Interfaces;

namespace DyDrums
{
    public partial class MainForm : Form, IMainFormView
    {

        private SerialController _serialController;
        private MidiController _midiController;
        private SerialManager _serialManager;
        private MidiManager _midiManager;
        private PadManager _padManager;
        private EEPROMManager _eepromManager;
        private EEPROMController _eepromController;

        public void SetMidiController(MidiController controller)
        {
            _midiController = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public MainForm()

        {
            InitializeComponent();
            InitializeManagersAndControllers();

            _padManager.PadsUpdated += (pads) =>
            {
                Invoke(() => RefreshPadGrid(pads));
            };

            _midiController.HHCValueReceived += OnHHCValueReceived;

        }

        private void MainForm_Load(object? sender, EventArgs? e)
        {
            SetupConnectCheckBox();
            RegisterSerialEvents();
            LoadInitialData();
        }

        private void InitializeManagersAndControllers()
        {

            _midiManager = new MidiManager();
            _midiController = new MidiController(this, _midiManager);
            _serialManager = new SerialManager(_midiController);

            _serialManager.SetMidiController(_midiController);

            _eepromManager = new EEPROMManager(_serialManager);
            _padManager = new PadManager(_serialManager);

            _serialController = new SerialController(this, _serialManager);
            _eepromController = new EEPROMController(_eepromManager, _padManager, this);


        }

        private void RefreshPadGrid(List<Pad> pads)
        {
            PadsGridView.DataSource = new BindingList<Pad>(pads);
        }

        private void SetupConnectCheckBox()
        {
            ConnectCheckBox.Appearance = Appearance.Button;
            ConnectCheckBox.TextAlign = ContentAlignment.MiddleCenter;
            ConnectCheckBox.FlatStyle = FlatStyle.Standard;
            ConnectCheckBox.Size = new Size(100, 40);
        }

        private void RegisterSerialEvents()
        {
            _serialController.MidiMessageReceived += OnMidiMessageReceived;
            _serialController.HHCVelocityReceived += UpdateHHCBar;
        }

        private void LoadInitialData()
        {
            _serialController.GetCOMPorts();
            _midiController.GetMidiDevices();

            _padManager.LoadPads();
            PopulateGrid();
            PadsGridView.DataSource = new BindingList<Pad>(_padManager.Pads);
        }



        private void OnHHCValueReceived(int data2)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => UpdateHHCProgressBar(data2));
            }
            else
            {
                UpdateHHCProgressBar(data2);
            }
        }

        private void UpdateHHCProgressBar(int data2)
        {
            if (HHCVerticalProgressBar != null)
            {
                int max = HHCVerticalProgressBar.Maximum;
                int invertedValue = max - data2;
                HHCVerticalProgressBar.Value = Math.Max(HHCVerticalProgressBar.Minimum, invertedValue);
            }
        }











        public void RefreshPadGrid()
        {
            PadsGridView.DataSource = new BindingList<Pad>(_padManager.Pads);
        }

        private void PopulateGrid()
        {
            var bindingList = new BindingList<Pad>(_padManager.Pads);
            var source = new BindingSource(bindingList, null);
            PadsGridView.AutoGenerateColumns = false;
            PadsGridView.DataSource = source;

            // Se quiser esconder ou renomear colunas:            
            if (PadsGridView.Columns.Contains("Pin"))
            {
                PadsGridView.Columns["Pin"].Visible = false;
            }

            //PadsGridView.Columns["CurveForm"].HeaderText = "Curve Form";
            //PadsGridView.Columns["XtalkGroup"].HeaderText = "Xtalk Group";
        }

        private void PadsGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            _padManager.SavePads(); // salva automático quando editar algo
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
                    MidiMonitorRichText.Enabled = true;
                    MidiMonitorClearButton.Enabled = true;
                    EEPROMReadButton.Enabled = true;
                    ConnectCheckBox.Text = "Desconectar";
                    PadsGridView.Enabled = true;
                    // Obter porta selecionada
                    string selectedPort = COMPortsComboBox.SelectedItem?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(selectedPort))
                        _serialController.Connect(selectedPort);
                    else
                        throw new Exception("Nenhuma porta COM selecionada.");

                    // Obter dispositivo MIDI selecionado
                    string selectedMidiDevice = MidiDevicesComboBox.SelectedItem?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(selectedMidiDevice))
                        _midiController.Connect(selectedMidiDevice);
                    else
                        throw new Exception("Nenhum dispositivo MIDI selecionado.");
                    MidiMonitorRichText.Enabled = true;
                }
                else
                {
                    PadsGridView.Enabled = false;
                    MidiMonitorRichText.Enabled = false;
                    MidiMonitorClearButton.Enabled = false;
                    EEPROMReadButton.Enabled = false;
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
                //_midiManager.SendNoteOn(note, velocity, 0);
                Task.Run(async () =>
                {
                    //await Task.Delay(5);
                    _midiController.SendMidiMessage(note, velocity, 20, channel);
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

        private void EEPROMReadButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Botão não faz nada ainda...");
        }

        private void MidiMonitorClearButton_Click(object sender, EventArgs e)
        {
            MidiMonitorRichText.Clear();
        }
    }
}
