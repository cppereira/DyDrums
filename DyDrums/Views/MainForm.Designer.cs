﻿namespace DyDrums
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            ConnectionGroupBox = new GroupBox();
            MidiDevicesScanButton = new Button();
            COMPortsScanButton = new Button();
            ConnectCheckBox = new CheckBox();
            MidiDevicesComboBoxLabel = new Label();
            COMPortLabel = new Label();
            MidiDevicesComboBox = new ComboBox();
            COMPortsComboBox = new ComboBox();
            PadsTableGroupBox = new GroupBox();
            PadsGridView = new DataGridView();
            ID = new DataGridViewTextBoxColumn();
            Type = new DataGridViewTextBoxColumn();
            PadName = new DataGridViewTextBoxColumn();
            Note = new DataGridViewTextBoxColumn();
            Threshold = new DataGridViewTextBoxColumn();
            ScanTime = new DataGridViewTextBoxColumn();
            MaskTime = new DataGridViewTextBoxColumn();
            Retrigger = new DataGridViewTextBoxColumn();
            Curve = new DataGridViewTextBoxColumn();
            CurveForm = new DataGridViewTextBoxColumn();
            XTalk = new DataGridViewTextBoxColumn();
            XTalkGroup = new DataGridViewTextBoxColumn();
            Channel = new DataGridViewTextBoxColumn();
            Gain = new DataGridViewTextBoxColumn();
            EEPROMReadButton = new Button();
            MonitorGroupBox = new GroupBox();
            HHCVerticalProgressBar = new DyDrums.Views.HHCVerticalProgressBar();
            HHCProgressBarLabel = new Label();
            MidiMonitorClearButton = new Button();
            MidiMonitorLabel = new Label();
            MidiMonitorRichText = new RichTextBox();
            EEPROMWriteButton = new Button();
            ConnectionGroupBox.SuspendLayout();
            PadsTableGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PadsGridView).BeginInit();
            MonitorGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // ConnectionGroupBox
            // 
            ConnectionGroupBox.Controls.Add(MidiDevicesScanButton);
            ConnectionGroupBox.Controls.Add(COMPortsScanButton);
            ConnectionGroupBox.Controls.Add(ConnectCheckBox);
            ConnectionGroupBox.Controls.Add(MidiDevicesComboBoxLabel);
            ConnectionGroupBox.Controls.Add(COMPortLabel);
            ConnectionGroupBox.Controls.Add(MidiDevicesComboBox);
            ConnectionGroupBox.Controls.Add(COMPortsComboBox);
            ConnectionGroupBox.Location = new Point(349, 12);
            ConnectionGroupBox.Name = "ConnectionGroupBox";
            ConnectionGroupBox.Size = new Size(1004, 84);
            ConnectionGroupBox.TabIndex = 0;
            ConnectionGroupBox.TabStop = false;
            ConnectionGroupBox.Text = "Conexão";
            // 
            // MidiDevicesScanButton
            // 
            MidiDevicesScanButton.Location = new Point(682, 33);
            MidiDevicesScanButton.Name = "MidiDevicesScanButton";
            MidiDevicesScanButton.Size = new Size(42, 23);
            MidiDevicesScanButton.TabIndex = 7;
            MidiDevicesScanButton.Text = "Scan";
            MidiDevicesScanButton.UseVisualStyleBackColor = true;
            MidiDevicesScanButton.Click += MidiDevicesScanButton_Click;
            // 
            // COMPortsScanButton
            // 
            COMPortsScanButton.Location = new Point(289, 33);
            COMPortsScanButton.Name = "COMPortsScanButton";
            COMPortsScanButton.Size = new Size(42, 23);
            COMPortsScanButton.TabIndex = 6;
            COMPortsScanButton.Text = "Scan";
            COMPortsScanButton.UseVisualStyleBackColor = true;
            COMPortsScanButton.Click += COMPortsScanButton_Click;
            // 
            // ConnectCheckBox
            // 
            ConnectCheckBox.Location = new Point(875, 22);
            ConnectCheckBox.Name = "ConnectCheckBox";
            ConnectCheckBox.Size = new Size(74, 19);
            ConnectCheckBox.TabIndex = 5;
            ConnectCheckBox.Text = "Conectar";
            ConnectCheckBox.UseVisualStyleBackColor = true;
            ConnectCheckBox.CheckedChanged += ConnectCheckBox_CheckedChanged;
            // 
            // MidiDevicesComboBoxLabel
            // 
            MidiDevicesComboBoxLabel.AutoSize = true;
            MidiDevicesComboBoxLabel.Location = new Point(385, 37);
            MidiDevicesComboBoxLabel.Name = "MidiDevicesComboBoxLabel";
            MidiDevicesComboBoxLabel.Size = new Size(101, 15);
            MidiDevicesComboBoxLabel.TabIndex = 4;
            MidiDevicesComboBoxLabel.Text = "Dispositivos MIDI:";
            // 
            // COMPortLabel
            // 
            COMPortLabel.AutoSize = true;
            COMPortLabel.Location = new Point(19, 37);
            COMPortLabel.Name = "COMPortLabel";
            COMPortLabel.Size = new Size(74, 15);
            COMPortLabel.TabIndex = 3;
            COMPortLabel.Text = "Portas COM:";
            // 
            // MidiDevicesComboBox
            // 
            MidiDevicesComboBox.FormattingEnabled = true;
            MidiDevicesComboBox.Location = new Point(492, 33);
            MidiDevicesComboBox.Name = "MidiDevicesComboBox";
            MidiDevicesComboBox.Size = new Size(184, 23);
            MidiDevicesComboBox.TabIndex = 2;
            // 
            // COMPortsComboBox
            // 
            COMPortsComboBox.FormattingEnabled = true;
            COMPortsComboBox.Location = new Point(99, 33);
            COMPortsComboBox.Name = "COMPortsComboBox";
            COMPortsComboBox.Size = new Size(184, 23);
            COMPortsComboBox.TabIndex = 1;
            // 
            // PadsTableGroupBox
            // 
            PadsTableGroupBox.Controls.Add(PadsGridView);
            PadsTableGroupBox.Location = new Point(349, 147);
            PadsTableGroupBox.Name = "PadsTableGroupBox";
            PadsTableGroupBox.Size = new Size(1004, 438);
            PadsTableGroupBox.TabIndex = 1;
            PadsTableGroupBox.TabStop = false;
            PadsTableGroupBox.Text = "Pads";
            // 
            // PadsGridView
            // 
            PadsGridView.AllowUserToAddRows = false;
            PadsGridView.AllowUserToDeleteRows = false;
            PadsGridView.AllowUserToResizeColumns = false;
            PadsGridView.AllowUserToResizeRows = false;
            PadsGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            PadsGridView.Columns.AddRange(new DataGridViewColumn[] { ID, Type, PadName, Note, Threshold, ScanTime, MaskTime, Retrigger, Curve, CurveForm, XTalk, XTalkGroup, Channel, Gain });
            PadsGridView.Enabled = false;
            PadsGridView.Location = new Point(10, 30);
            PadsGridView.Name = "PadsGridView";
            PadsGridView.Size = new Size(988, 400);
            PadsGridView.TabIndex = 0;
            PadsGridView.CellValidating += PadsGridView_CellValidating;
            // 
            // ID
            // 
            ID.DataPropertyName = "Id";
            ID.HeaderText = "ID";
            ID.Name = "ID";
            ID.Width = 30;
            // 
            // Type
            // 
            Type.DataPropertyName = "Type";
            Type.HeaderText = "Sensor";
            Type.Name = "Type";
            Type.Resizable = DataGridViewTriState.True;
            Type.Width = 60;
            // 
            // PadName
            // 
            PadName.DataPropertyName = "PadName";
            PadName.HeaderText = "Pad";
            PadName.Name = "PadName";
            PadName.Width = 80;
            // 
            // Note
            // 
            Note.DataPropertyName = "Note";
            Note.HeaderText = "Nota";
            Note.Name = "Note";
            Note.Width = 60;
            // 
            // Threshold
            // 
            Threshold.DataPropertyName = "Threshold";
            Threshold.HeaderText = "Sensi";
            Threshold.Name = "Threshold";
            Threshold.Width = 60;
            // 
            // ScanTime
            // 
            ScanTime.DataPropertyName = "ScanTime";
            ScanTime.HeaderText = "Leitura";
            ScanTime.Name = "ScanTime";
            ScanTime.Width = 70;
            // 
            // MaskTime
            // 
            MaskTime.DataPropertyName = "MaskTime";
            MaskTime.HeaderText = "Bloqueio";
            MaskTime.Name = "MaskTime";
            MaskTime.Width = 70;
            // 
            // Retrigger
            // 
            Retrigger.DataPropertyName = "Retrigger";
            Retrigger.HeaderText = "Repetição";
            Retrigger.Name = "Retrigger";
            Retrigger.Width = 70;
            // 
            // Curve
            // 
            Curve.DataPropertyName = "Curve";
            Curve.HeaderText = "Curva";
            Curve.Name = "Curve";
            Curve.Width = 75;
            // 
            // CurveForm
            // 
            CurveForm.DataPropertyName = "CurveForm";
            CurveForm.HeaderText = "Forma";
            CurveForm.Name = "CurveForm";
            CurveForm.Width = 60;
            // 
            // XTalk
            // 
            XTalk.DataPropertyName = "XTalk";
            XTalk.HeaderText = "Isolamento";
            XTalk.Name = "XTalk";
            XTalk.Width = 70;
            // 
            // XTalkGroup
            // 
            XTalkGroup.DataPropertyName = "XTalkGroup";
            XTalkGroup.HeaderText = "Grupo";
            XTalkGroup.Name = "XTalkGroup";
            XTalkGroup.Width = 80;
            // 
            // Channel
            // 
            Channel.DataPropertyName = "Channel";
            Channel.HeaderText = "Canal";
            Channel.Name = "Channel";
            Channel.Width = 60;
            // 
            // Gain
            // 
            Gain.DataPropertyName = "Gain";
            Gain.HeaderText = "Ganho";
            Gain.Name = "Gain";
            Gain.Width = 65;
            // 
            // EEPROMReadButton
            // 
            EEPROMReadButton.BackColor = SystemColors.ButtonFace;
            EEPROMReadButton.Enabled = false;
            EEPROMReadButton.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            EEPROMReadButton.Location = new Point(349, 104);
            EEPROMReadButton.Name = "EEPROMReadButton";
            EEPROMReadButton.Size = new Size(233, 39);
            EEPROMReadButton.TabIndex = 2;
            EEPROMReadButton.Text = "Ler tudo do Arduino";
            EEPROMReadButton.UseVisualStyleBackColor = false;
            EEPROMReadButton.Click += EEPROMReadButton_Click;
            // 
            // MonitorGroupBox
            // 
            MonitorGroupBox.Controls.Add(HHCVerticalProgressBar);
            MonitorGroupBox.Controls.Add(HHCProgressBarLabel);
            MonitorGroupBox.Controls.Add(MidiMonitorClearButton);
            MonitorGroupBox.Controls.Add(MidiMonitorLabel);
            MonitorGroupBox.Controls.Add(MidiMonitorRichText);
            MonitorGroupBox.Location = new Point(6, 12);
            MonitorGroupBox.Name = "MonitorGroupBox";
            MonitorGroupBox.Size = new Size(337, 573);
            MonitorGroupBox.TabIndex = 3;
            MonitorGroupBox.TabStop = false;
            MonitorGroupBox.Text = "Monitor";
            // 
            // HHCVerticalProgressBar
            // 
            HHCVerticalProgressBar.BarColor = Color.Aquamarine;
            HHCVerticalProgressBar.Location = new Point(310, 43);
            HHCVerticalProgressBar.Name = "HHCVerticalProgressBar";
            HHCVerticalProgressBar.Size = new Size(21, 477);
            HHCVerticalProgressBar.TabIndex = 8;
            // 
            // HHCProgressBarLabel
            // 
            HHCProgressBarLabel.AutoSize = true;
            HHCProgressBarLabel.Location = new Point(300, 19);
            HHCProgressBarLabel.Name = "HHCProgressBarLabel";
            HHCProgressBarLabel.Size = new Size(36, 15);
            HHCProgressBarLabel.TabIndex = 7;
            HHCProgressBarLabel.Text = "HHC:";
            // 
            // MidiMonitorClearButton
            // 
            MidiMonitorClearButton.Enabled = false;
            MidiMonitorClearButton.Location = new Point(68, 526);
            MidiMonitorClearButton.Name = "MidiMonitorClearButton";
            MidiMonitorClearButton.Size = new Size(179, 39);
            MidiMonitorClearButton.TabIndex = 4;
            MidiMonitorClearButton.Text = "Limpar";
            MidiMonitorClearButton.UseVisualStyleBackColor = true;
            MidiMonitorClearButton.Click += MidiMonitorClearButton_Click;
            // 
            // MidiMonitorLabel
            // 
            MidiMonitorLabel.AutoSize = true;
            MidiMonitorLabel.Location = new Point(85, 19);
            MidiMonitorLabel.Name = "MidiMonitorLabel";
            MidiMonitorLabel.Size = new Size(98, 15);
            MidiMonitorLabel.TabIndex = 6;
            MidiMonitorLabel.Text = "Mensagens MIDI:";
            // 
            // MidiMonitorRichText
            // 
            MidiMonitorRichText.Enabled = false;
            MidiMonitorRichText.Location = new Point(6, 43);
            MidiMonitorRichText.Name = "MidiMonitorRichText";
            MidiMonitorRichText.Size = new Size(298, 477);
            MidiMonitorRichText.TabIndex = 4;
            MidiMonitorRichText.Text = "";
            // 
            // EEPROMWriteButton
            // 
            EEPROMWriteButton.BackColor = SystemColors.ButtonFace;
            EEPROMWriteButton.Enabled = false;
            EEPROMWriteButton.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            EEPROMWriteButton.ForeColor = SystemColors.ActiveCaptionText;
            EEPROMWriteButton.Location = new Point(1120, 102);
            EEPROMWriteButton.Name = "EEPROMWriteButton";
            EEPROMWriteButton.Size = new Size(233, 39);
            EEPROMWriteButton.TabIndex = 4;
            EEPROMWriteButton.Text = "Enviar tudo para o Arduino";
            EEPROMWriteButton.UseVisualStyleBackColor = false;
            EEPROMWriteButton.Click += EEPROMWriteButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            ClientSize = new Size(1359, 591);
            Controls.Add(EEPROMWriteButton);
            Controls.Add(MonitorGroupBox);
            Controls.Add(EEPROMReadButton);
            Controls.Add(PadsTableGroupBox);
            Controls.Add(ConnectionGroupBox);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "DyDrums";
            Load += MainForm_Load;
            ConnectionGroupBox.ResumeLayout(false);
            ConnectionGroupBox.PerformLayout();
            PadsTableGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PadsGridView).EndInit();
            MonitorGroupBox.ResumeLayout(false);
            MonitorGroupBox.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox ConnectionGroupBox;
        private ComboBox MidiDevicesComboBox;
        private ComboBox COMPortsComboBox;
        private Label COMPortLabel;
        private Label MidiDevicesComboBoxLabel;
        private CheckBox ConnectCheckBox;
        private GroupBox PadsTableGroupBox;
        private DataGridView PadsGridView;
        private Button EEPROMReadButton;
        private GroupBox MonitorGroupBox;
        private Label MidiMonitorLabel;
        private RichTextBox MidiMonitorRichText;
        private Button MidiMonitorClearButton;
        private Label HHCProgressBarLabel;
        private Button EEPROMWriteButton;
        private Views.HHCVerticalProgressBar HHCVerticalProgressBar;
        private Views.HHCVerticalProgressBar HHCVerticalProgressBar2;
        private Button MidiDevicesScanButton;
        private Button COMPortsScanButton;
        private DataGridViewTextBoxColumn ID;
        private DataGridViewTextBoxColumn Type;
        private DataGridViewTextBoxColumn PadName;
        private DataGridViewTextBoxColumn Note;
        private DataGridViewTextBoxColumn Threshold;
        private DataGridViewTextBoxColumn ScanTime;
        private DataGridViewTextBoxColumn MaskTime;
        private DataGridViewTextBoxColumn Retrigger;
        private DataGridViewTextBoxColumn Curve;
        private DataGridViewTextBoxColumn CurveForm;
        private DataGridViewTextBoxColumn XTalk;
        private DataGridViewTextBoxColumn XTalkGroup;
        private DataGridViewTextBoxColumn Channel;
        private DataGridViewTextBoxColumn Gain;
    }
}
