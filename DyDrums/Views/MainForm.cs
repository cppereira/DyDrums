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

        public MainForm()
        {
            InitializeComponent();
            InitializeManagersAndControllers();

            // HHC controller 
            _midiController.HHCValueReceived += OnHHCValueReceived;


            //Atualizar JSON e Arduino direto da Grid
            PadsGridView.CellValueChanged += PadsGridView_CellValueChanged;
            PadsGridView.CellEndEdit += (s, e) =>
            {
                if (PadsGridView.IsCurrentCellDirty)
                    PadsGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

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

        public void LoadInitialData()
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

        private void PopulateGrid()
        {
            var bindingList = new BindingList<Pad>(_padManager.Pads);
            var source = new BindingSource(bindingList, null);
            PadsGridView.AutoGenerateColumns = false;
            PadsGridView.DataSource = source;

            // Se quiser esconder ou renomear colunas:            
            if (PadsGridView.Columns.Contains("Id"))
            {
                PadsGridView.Columns["Id"].Visible = false;
            }
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
                    PadsGridView.Enabled = true;
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
                    _midiManager.Disconnect();
                }
            }
            catch
            {
                MessageBox.Show($"{e}: ", "Erro de conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            _serialController?.StartHandshake();
        }

        private void MidiMonitorClearButton_Click(object sender, EventArgs e)
        {
            MidiMonitorRichText.Clear();
        }

        public void UpdateGrid(List<Pad> pads)
        {
            if (PadsGridView.InvokeRequired)
            {
                PadsGridView.Invoke(new Action(() => UpdateGrid(pads)));
                return;
            }

            // Solução definitiva para erro de DataSource mal resolvido
            PadsGridView.DataSource = null;
            PadsGridView.Rows.Clear(); // limpa tudo
            PadsGridView.Refresh(); // força a UI a perceber a mudança

            // Clona a lista se ela vier de um binding anterior
            var padsClone = pads.Select(p => new Pad
            {
                Id = p.Id,
                Type = p.Type,
                Name = p.Name,
                Note = p.Note,
                Threshold = p.Threshold,
                ScanTime = p.ScanTime,
                MaskTime = p.MaskTime,
                Retrigger = p.Retrigger,
                Curve = p.Curve,
                CurveForm = p.CurveForm,
                Xtalk = p.Xtalk,
                XtalkGroup = p.XtalkGroup,
                Channel = p.Channel,
                Gain = p.Gain
            }).ToList();

            PadsGridView.DataSource = padsClone;
        }

        private void PadsGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            try
            {
                // Atualiza a lista de Pads com base na nova célula editada
                var pads = new List<Pad>();

                foreach (DataGridViewRow row in PadsGridView.Rows)
                {
                    if (row.IsNewRow) continue;

                    var pad = new Pad
                    {
                        //Id = Convert.ToInt32(row.Cells["Id"].Value),
                        Type = Convert.ToInt32(row.Cells["Type"].Value),
                        Name = row.Cells["PadName"].Value?.ToString(),
                        Note = Convert.ToInt32(row.Cells["Note"].Value),
                        Threshold = Convert.ToInt32(row.Cells["Threshold"].Value),
                        ScanTime = Convert.ToInt32(row.Cells["ScanTime"].Value),
                        MaskTime = Convert.ToInt32(row.Cells["MaskTime"].Value),
                        Retrigger = Convert.ToInt32(row.Cells["Retrigger"].Value),
                        Curve = Convert.ToInt32(row.Cells["Curve"].Value),
                        CurveForm = Convert.ToInt32(row.Cells["CurveForm"].Value),
                        Xtalk = Convert.ToInt32(row.Cells["Xtalk"].Value),
                        XtalkGroup = Convert.ToInt32(row.Cells["XtalkGroup"].Value),
                        Channel = Convert.ToInt32(row.Cells["Channel"].Value),
                        Gain = Convert.ToInt32(row.Cells["Gain"].Value),
                    };

                    pads.Add(pad);
                }

                // Salva no JSON via PadManager
                _padManager.SavePads(pads);
                MessageBox.Show("Alterações salvas com sucesso.", "Atualização da Grid", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "ERRO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //MessageBox.Show($"[Grid] Erro ao salvar alterações: {ex.Message}");
            }
        }

    }
}
