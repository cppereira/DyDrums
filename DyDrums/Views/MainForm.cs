using System.ComponentModel;
using System.Diagnostics;
using DyDrums.Controllers;
using DyDrums.Helpers;
using DyDrums.Models;
using DyDrums.Services;
using DyDrums.Views;
using DyDrums.Views.Interfaces;

namespace DyDrums
{
    public partial class MainForm : Form, IMainFormView
    {
        //Define o comando para solicitar todos os Pads para o Firmware
        public const byte CMD_GET_ALL_PADS = 0x25;
        //Define o comando para eniar todos os Pads para o Firmware
        public const byte CMD_SET_ALL_PADS = 0x26;

        private SerialController _serialController;
        private MidiController _midiController;
        private SerialManager _serialManager;
        private MidiManager _midiManager;
        private PadManager _padManager;
        private EEPROMManager _eepromManager;
        private EEPROMController _eepromController;
        private List<Pad>? allPads;
        private HHCVerticalProgressBar _hHCVerticalProgressBar;
        public MainForm()
        {
            InitializeComponent();
            InitializeManagersAndControllers();

            // HHC controller 
            _midiController.HHCValueReceived += OnHHCValueReceived;


            //Atualizar JSON e Arduino direto da Grid
            PadsGridView.CellEndEdit += PadsGridView_CellEndEdit;
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
            allPads = new List<Pad>(_padManager.Pads);
            UpdateGrid(allPads);
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

            //Envia o Pads para o Controller, para manter os Nomes dos pads carregados do JSON, pois o Arduino nao salva nomes....
            _serialController.InitializePadList(_padManager.Pads);
            SetupComboColumns();
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
            if (HHCVerticalProgressBar2 != null)
            {
                int max = HHCVerticalProgressBar2.Maximum;
                int invertedValue = max - data2;
                HHCVerticalProgressBar2.Value = Math.Max(HHCVerticalProgressBar2.Minimum, invertedValue);
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
                    //Thread.Sleep(5000);
                    //MessageBox.Show("Conectado!", "AVISO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    EEPROMWriteButton.Enabled = true;
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
                    EEPROMWriteButton.Enabled = false;
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
                int maxLines = 30;
                string timestamp = DateTime.Now.ToString("ss,fff");
                string formatted = string.Format("[{0}] => Canal: {1,-2} | Nota: {2,-3} | Velocity: {3,-3}", timestamp, channel, note, velocity);


                if (MidiMonitorRichText.GetLineFromCharIndex(MidiMonitorRichText.TextLength) > maxLines)
                {
                    int firstCharIndex = MidiMonitorRichText.GetFirstCharIndexFromLine(1); // ignora a primeira linha
                    MidiMonitorRichText.Select(0, firstCharIndex);
                    MidiMonitorRichText.Clear(); // remove sem mexer em formatação restante
                }

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

                // Garante que vai inserir no fim
                MidiMonitorRichText.SelectionStart = MidiMonitorRichText.TextLength;
                MidiMonitorRichText.SelectionLength = 0;

                // Já tem font e cor definidos
                MidiMonitorRichText.SelectedText = formatted + Environment.NewLine;
                MidiMonitorRichText.ScrollToCaret();


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
            byte command = CMD_GET_ALL_PADS;
            _serialController?.StartHandshake(command);
        }

        private void EEPROMWriteButton_Click(object sender, EventArgs e)
        {
            byte command = CMD_SET_ALL_PADS;
            if (_serialManager == null)
            {
                MessageBox.Show("SerialManager não está inicializado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_padManager == null || _padManager.Pads == null || !_padManager.Pads.Any())
            {
                MessageBox.Show("Nenhum dado de pad encontrado para enviar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Tem certeza que deseja enviar TODOS os dados para o Arduino?",
                "Confirmar envio",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Debug.WriteLine("[Botão] Enviando todos os pads para o Arduino...");
                _serialManager.SendHandshake(0x26, true); // ativa write mode, envia, finaliza
                _serialManager.SendAllPadsToArduino(allPads);
                _serialManager.SendHandshake(0x28); //envia mensagem de FIM para arduino...            
                MessageBox.Show("Todos os dados foram enviados para o Arduino com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ResetCellStyles();//Reseta as cores das celulas depois de enviar....
            }

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
            PadsGridView.DataSource = pads;
            // PadsGridView.Rows.Clear(); // limpa tudo
            PadsGridView.Refresh(); // força a UI a perceber a mudança

            // Clona a lista se ela vier de um binding anterior
            var padsClone = pads.Select(p => new Pad
            {
                Id = p.Id,
                Type = p.Type,
                PadName = p.PadName,
                Note = p.Note,
                Threshold = p.Threshold,
                ScanTime = p.ScanTime,
                MaskTime = p.MaskTime,
                Retrigger = p.Retrigger,
                Curve = p.Curve,
                CurveForm = p.CurveForm,
                Xtalk = p.Xtalk,
                XtalkGroup = p.XtalkGroup,
                Channel = p.Channel + 1,
                Gain = p.Gain
            }).ToList();

            PadsGridView.DataSource = padsClone;
        }



        private void PadsGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var pad = allPads[e.RowIndex];
            var columnName = PadsGridView.Columns[e.ColumnIndex].Name;
            var cell = PadsGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
            var newValue = cell.Value?.ToString()?.Trim();

            try
            {
                bool changed = false;

                switch (columnName)
                {
                    case "Type":
                        if (int.TryParse(newValue, out var type) && pad.Type != type)
                        {
                            pad.Type = type;
                            changed = true;
                        }
                        break;

                    case "PadName":
                        if (pad.PadName != newValue)
                        {
                            pad.PadName = newValue;
                            changed = true;
                        }
                        break;

                    case "Note":
                        if (int.TryParse(newValue, out var note) && pad.Note != note)
                        {
                            pad.Note = note;
                            changed = true;
                        }
                        break;
                    case "Threshold":
                        if (int.TryParse(newValue, out var threshold) && pad.Threshold != threshold)
                        {
                            pad.Threshold = threshold;
                            changed = true;
                        }
                        break;
                    case "ScanTime":
                        if (int.TryParse(newValue, out var scanTime) && pad.ScanTime != scanTime)
                        {
                            pad.ScanTime = scanTime;
                            changed = true;
                        }
                        break;
                    case "MaskTime":
                        if (int.TryParse(newValue, out var maskTime) && pad.MaskTime != maskTime)
                        {
                            pad.MaskTime = maskTime;
                            changed = true;
                        }
                        break;
                    case "Retrigger":
                        if (int.TryParse(newValue, out var retrigger) && pad.Retrigger != retrigger)
                        {
                            pad.Retrigger = retrigger;
                            changed = true;
                        }
                        break;
                    case "Curve":
                        if (int.TryParse(newValue, out var curve) && pad.Curve != curve)
                        {
                            pad.Curve = curve;
                            changed = true;
                        }
                        break;
                    case "CurveForm":
                        if (int.TryParse(newValue, out var curveForm) && pad.CurveForm != curveForm)
                        {
                            pad.CurveForm = curveForm;
                            changed = true;
                        }
                        break;
                    case "XTalk":
                        if (int.TryParse(newValue, out var xtalk) && pad.Xtalk != xtalk)
                        {
                            pad.Xtalk = xtalk;
                            changed = true;
                        }
                        break;
                    case "XTalkGroup":
                        if (int.TryParse(newValue, out var xtalkGroup) && pad.XtalkGroup != xtalkGroup)
                        {
                            pad.XtalkGroup = xtalkGroup;
                            changed = true;
                        }
                        break;
                    case "Channel":
                        if (int.TryParse(newValue, out var channel) && pad.Channel != channel)
                        {
                            pad.Channel = channel;
                            changed = true;
                        }
                        break;
                    case "Gain":
                        if (int.TryParse(newValue, out var gain) && pad.Gain != gain)
                        {
                            pad.Gain = gain;
                            changed = true;
                        }
                        break;
                }

                if (changed)
                {
                    //Envia apenas o pad alterado para o JSON
                    _serialController.SendPadsToJSON(allPads);
                    cell.Style.BackColor = Color.LightGreen;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao editar célula: " + ex.Message);
            }
        }

        private void COMPortsScanButton_Click(object sender, EventArgs e)
        {
            _serialController.GetCOMPorts();
        }

        private void MidiDevicesScanButton_Click(object sender, EventArgs e)
        {
            _midiController.GetMidiDevices();
        }

        private void PadsGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            var grid = sender as DataGridView;
            string columnName = grid.Columns[e.ColumnIndex].Name;
            string displayValue = e.FormattedValue.ToString();

            // Se a coluna for do tipo ComboBox, pegue o valor "por trás"
            if (grid.Columns[e.ColumnIndex] is DataGridViewComboBoxColumn comboColumn)
            {
                if (DataGridValidationRules.ComboBoxValues.TryGetValue(columnName, out var options))
                {
                    var match = options.FirstOrDefault(opt => opt.Text == displayValue);
                    if (match == null)
                    {
                        e.Cancel = true;
                        MessageBox.Show("Opção inválida.");
                        return;
                    }

                    // Agora sim valida com o valor numérico real
                    if (DataGridValidationRules.NumericLimits.TryGetValue(columnName, out var limits))
                    {
                        if (match.Value < limits.Min || match.Value > limits.Max)
                        {
                            e.Cancel = true;
                            MessageBox.Show($"Valor inválido. Esperado entre {limits.Min} e {limits.Max}.");
                        }
                    }

                    return; // evita validar de novo abaixo
                }
            }

            // Validação normal para campos que não são ComboBox
            if (DataGridValidationRules.NumericLimits.TryGetValue(columnName, out var numLimits))
            {
                if (!int.TryParse(displayValue, out int result) || result < numLimits.Min || result > numLimits.Max)
                {
                    e.Cancel = true;
                    MessageBox.Show($"Valor inválido. Esperado entre {numLimits.Min} e {numLimits.Max}.");
                }
            }
        }

        private void SetupComboColumns()
        {
            foreach (var comboInfo in DataGridValidationRules.ComboBoxValues)
            {
                string columnName = comboInfo.Key;
                var options = comboInfo.Value;

                if (!PadsGridView.Columns.Contains(columnName))
                    continue;

                var comboColumn = new DataGridViewComboBoxColumn
                {
                    Name = columnName,
                    HeaderText = columnName,
                    DataPropertyName = columnName,
                    DisplayMember = "Text",
                    ValueMember = "Value"
                };

                foreach (var opt in options)
                    comboColumn.Items.Add(opt);

                var oldColumn = PadsGridView.Columns[columnName];
                int index = oldColumn.Index;
                PadsGridView.Columns.Remove(oldColumn);
                PadsGridView.Columns.Insert(index, comboColumn);
            }
        }

        private void ResetCellStyles()
        {
            foreach (DataGridViewRow row in PadsGridView.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.BackColor = PadsGridView.DefaultCellStyle.BackColor;
                }
            }
        }
    }
}
