﻿namespace MediaToolkit.UI
{
    partial class DeckLinkInputControl
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.debugPanel = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.controlPanel = new System.Windows.Forms.Panel();
            this.comboBoxDisplayModes = new System.Windows.Forms.ComboBox();
            this.comboBoxDevices = new System.Windows.Forms.ComboBox();
            this.switchCaptureStateButton = new System.Windows.Forms.Button();
            this.findServiceButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.showDetailsButton = new System.Windows.Forms.Button();
            this.statusLabel2 = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.debugPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.controlPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // debugPanel
            // 
            this.debugPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.debugPanel.AutoSize = true;
            this.debugPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.debugPanel.BackColor = System.Drawing.Color.PeachPuff;
            this.debugPanel.Controls.Add(this.flowLayoutPanel1);
            this.debugPanel.Location = new System.Drawing.Point(3, 425);
            this.debugPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.debugPanel.Name = "debugPanel";
            this.debugPanel.Size = new System.Drawing.Size(441, 112);
            this.debugPanel.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.controlPanel);
            this.flowLayoutPanel1.Controls.Add(this.panel2);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(5, 5);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(433, 105);
            this.flowLayoutPanel1.TabIndex = 38;
            // 
            // controlPanel
            // 
            this.controlPanel.AutoSize = true;
            this.controlPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.controlPanel.Controls.Add(this.comboBoxDisplayModes);
            this.controlPanel.Controls.Add(this.comboBoxDevices);
            this.controlPanel.Controls.Add(this.switchCaptureStateButton);
            this.controlPanel.Controls.Add(this.findServiceButton);
            this.controlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlPanel.Location = new System.Drawing.Point(3, 2);
            this.controlPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new System.Drawing.Size(427, 70);
            this.controlPanel.TabIndex = 35;
            // 
            // comboBoxDisplayModes
            // 
            this.comboBoxDisplayModes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDisplayModes.FormattingEnabled = true;
            this.comboBoxDisplayModes.Location = new System.Drawing.Point(5, 42);
            this.comboBoxDisplayModes.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxDisplayModes.Name = "comboBoxDisplayModes";
            this.comboBoxDisplayModes.Size = new System.Drawing.Size(308, 24);
            this.comboBoxDisplayModes.TabIndex = 32;
            this.comboBoxDisplayModes.SelectedValueChanged += new System.EventHandler(this.comboBoxDisplayIds_SelectedValueChanged);
            // 
            // comboBoxDevices
            // 
            this.comboBoxDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDevices.FormattingEnabled = true;
            this.comboBoxDevices.Location = new System.Drawing.Point(5, 7);
            this.comboBoxDevices.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxDevices.Name = "comboBoxDevices";
            this.comboBoxDevices.Size = new System.Drawing.Size(309, 24);
            this.comboBoxDevices.TabIndex = 31;
            this.comboBoxDevices.SelectedValueChanged += new System.EventHandler(this.comboBoxDevices_SelectedValueChanged);
            // 
            // switchCaptureStateButton
            // 
            this.switchCaptureStateButton.Location = new System.Drawing.Point(321, 41);
            this.switchCaptureStateButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.switchCaptureStateButton.Name = "switchCaptureStateButton";
            this.switchCaptureStateButton.Size = new System.Drawing.Size(100, 27);
            this.switchCaptureStateButton.TabIndex = 28;
            this.switchCaptureStateButton.Text = "_Start";
            this.switchCaptureStateButton.UseVisualStyleBackColor = true;
            this.switchCaptureStateButton.Click += new System.EventHandler(this.switchCaptureStateButton_Click);
            // 
            // findServiceButton
            // 
            this.findServiceButton.Location = new System.Drawing.Point(323, 7);
            this.findServiceButton.Margin = new System.Windows.Forms.Padding(4);
            this.findServiceButton.Name = "findServiceButton";
            this.findServiceButton.Size = new System.Drawing.Size(100, 27);
            this.findServiceButton.TabIndex = 30;
            this.findServiceButton.Text = "_Find";
            this.findServiceButton.UseVisualStyleBackColor = true;
            this.findServiceButton.Click += new System.EventHandler(this.findServiceButton_Click);
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.Controls.Add(this.showDetailsButton);
            this.panel2.Controls.Add(this.statusLabel2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 76);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel2.MinimumSize = new System.Drawing.Size(29, 27);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(427, 27);
            this.panel2.TabIndex = 38;
            // 
            // showDetailsButton
            // 
            this.showDetailsButton.AutoSize = true;
            this.showDetailsButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.showDetailsButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.showDetailsButton.Location = new System.Drawing.Point(393, 0);
            this.showDetailsButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.showDetailsButton.Name = "showDetailsButton";
            this.showDetailsButton.Size = new System.Drawing.Size(34, 27);
            this.showDetailsButton.TabIndex = 36;
            this.showDetailsButton.Text = ">>";
            this.showDetailsButton.UseVisualStyleBackColor = true;
            this.showDetailsButton.Click += new System.EventHandler(this.showDetailsButton_Click);
            // 
            // statusLabel2
            // 
            this.statusLabel2.AutoEllipsis = true;
            this.statusLabel2.AutoSize = true;
            this.statusLabel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.statusLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.statusLabel2.Location = new System.Drawing.Point(0, 0);
            this.statusLabel2.Name = "statusLabel2";
            this.statusLabel2.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.statusLabel2.Size = new System.Drawing.Size(103, 21);
            this.statusLabel2.TabIndex = 37;
            this.statusLabel2.Text = "__STATUS___";
            this.statusLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.BackColor = System.Drawing.Color.White;
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 7.8F);
            this.statusLabel.Location = new System.Drawing.Point(0, 0);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(18, 19);
            this.statusLabel.TabIndex = 2;
            this.statusLabel.Text = "...";
            // 
            // DeckLinkInputControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.debugPanel);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "DeckLinkInputControl";
            this.Size = new System.Drawing.Size(749, 539);
            this.debugPanel.ResumeLayout(false);
            this.debugPanel.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.controlPanel.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel debugPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel controlPanel;
        private System.Windows.Forms.ComboBox comboBoxDevices;
        private System.Windows.Forms.Button switchCaptureStateButton;
        private System.Windows.Forms.Button findServiceButton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label statusLabel2;
        private System.Windows.Forms.Button showDetailsButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.ComboBox comboBoxDisplayModes;
    }
}
