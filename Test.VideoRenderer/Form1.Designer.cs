﻿namespace Test.VideoRenderer
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonPause = new System.Windows.Forms.Button();
            this.buttonSetup = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonAudioSetup = new System.Windows.Forms.Button();
            this.buttonAudioStart = new System.Windows.Forms.Button();
            this.buttonAudioStop = new System.Windows.Forms.Button();
            this.buttonCloseAudio = new System.Windows.Forms.Button();
            this.buttonProcessSample = new System.Windows.Forms.Button();
            this.checkBoxMute = new System.Windows.Forms.CheckBox();
            this.trackBarVolume = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(109, 92);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(101, 30);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(373, 92);
            this.buttonStop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(83, 30);
            this.buttonStop.TabIndex = 1;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonPause
            // 
            this.buttonPause.Location = new System.Drawing.Point(233, 92);
            this.buttonPause.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(101, 30);
            this.buttonPause.TabIndex = 2;
            this.buttonPause.Text = "Pause";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // buttonSetup
            // 
            this.buttonSetup.Location = new System.Drawing.Point(109, 25);
            this.buttonSetup.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonSetup.Name = "buttonSetup";
            this.buttonSetup.Size = new System.Drawing.Size(101, 30);
            this.buttonSetup.TabIndex = 3;
            this.buttonSetup.Text = "Setup";
            this.buttonSetup.UseVisualStyleBackColor = true;
            this.buttonSetup.Click += new System.EventHandler(this.buttonSetup_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(373, 198);
            this.buttonClose.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(101, 30);
            this.buttonClose.TabIndex = 4;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonAudioSetup
            // 
            this.buttonAudioSetup.Location = new System.Drawing.Point(29, 261);
            this.buttonAudioSetup.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonAudioSetup.Name = "buttonAudioSetup";
            this.buttonAudioSetup.Size = new System.Drawing.Size(117, 30);
            this.buttonAudioSetup.TabIndex = 5;
            this.buttonAudioSetup.Text = "SetupAudio";
            this.buttonAudioSetup.UseVisualStyleBackColor = true;
            this.buttonAudioSetup.Click += new System.EventHandler(this.buttonAudioSetup_Click);
            // 
            // buttonAudioStart
            // 
            this.buttonAudioStart.Location = new System.Drawing.Point(30, 308);
            this.buttonAudioStart.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonAudioStart.Name = "buttonAudioStart";
            this.buttonAudioStart.Size = new System.Drawing.Size(116, 30);
            this.buttonAudioStart.TabIndex = 6;
            this.buttonAudioStart.Text = "AudioStart";
            this.buttonAudioStart.UseVisualStyleBackColor = true;
            this.buttonAudioStart.Click += new System.EventHandler(this.buttonAudioStart_Click);
            // 
            // buttonAudioStop
            // 
            this.buttonAudioStop.Location = new System.Drawing.Point(325, 308);
            this.buttonAudioStop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonAudioStop.Name = "buttonAudioStop";
            this.buttonAudioStop.Size = new System.Drawing.Size(117, 30);
            this.buttonAudioStop.TabIndex = 7;
            this.buttonAudioStop.Text = "AudioStop";
            this.buttonAudioStop.UseVisualStyleBackColor = true;
            this.buttonAudioStop.Click += new System.EventHandler(this.buttonAudioStop_Click);
            // 
            // buttonCloseAudio
            // 
            this.buttonCloseAudio.Location = new System.Drawing.Point(325, 368);
            this.buttonCloseAudio.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonCloseAudio.Name = "buttonCloseAudio";
            this.buttonCloseAudio.Size = new System.Drawing.Size(117, 25);
            this.buttonCloseAudio.TabIndex = 8;
            this.buttonCloseAudio.Text = "CloseAudio";
            this.buttonCloseAudio.UseVisualStyleBackColor = true;
            this.buttonCloseAudio.Click += new System.EventHandler(this.buttonCloseAudio_Click);
            // 
            // buttonProcessSample
            // 
            this.buttonProcessSample.Location = new System.Drawing.Point(165, 308);
            this.buttonProcessSample.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonProcessSample.Name = "buttonProcessSample";
            this.buttonProcessSample.Size = new System.Drawing.Size(133, 30);
            this.buttonProcessSample.TabIndex = 9;
            this.buttonProcessSample.Text = "ProcessSample";
            this.buttonProcessSample.UseVisualStyleBackColor = true;
            this.buttonProcessSample.Click += new System.EventHandler(this.buttonProcessSample_Click);
            // 
            // checkBoxMute
            // 
            this.checkBoxMute.AutoSize = true;
            this.checkBoxMute.Location = new System.Drawing.Point(165, 383);
            this.checkBoxMute.Name = "checkBoxMute";
            this.checkBoxMute.Size = new System.Drawing.Size(61, 21);
            this.checkBoxMute.TabIndex = 10;
            this.checkBoxMute.Text = "Mute";
            this.checkBoxMute.UseVisualStyleBackColor = true;
            this.checkBoxMute.CheckedChanged += new System.EventHandler(this.checkBoxMute_CheckedChanged);
            // 
            // trackBarVolume
            // 
            this.trackBarVolume.AutoSize = false;
            this.trackBarVolume.Location = new System.Drawing.Point(165, 359);
            this.trackBarVolume.Maximum = 100;
            this.trackBarVolume.Name = "trackBarVolume";
            this.trackBarVolume.Size = new System.Drawing.Size(104, 18);
            this.trackBarVolume.TabIndex = 11;
            this.trackBarVolume.TickFrequency = 10;
            this.trackBarVolume.Scroll += new System.EventHandler(this.trackBarVolume_Scroll);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.trackBarVolume);
            this.Controls.Add(this.checkBoxMute);
            this.Controls.Add(this.buttonProcessSample);
            this.Controls.Add(this.buttonCloseAudio);
            this.Controls.Add(this.buttonAudioStop);
            this.Controls.Add(this.buttonAudioStart);
            this.Controls.Add(this.buttonAudioSetup);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonSetup);
            this.Controls.Add(this.buttonPause);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVolume)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.Button buttonSetup;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonAudioSetup;
        private System.Windows.Forms.Button buttonAudioStart;
        private System.Windows.Forms.Button buttonAudioStop;
        private System.Windows.Forms.Button buttonCloseAudio;
        private System.Windows.Forms.Button buttonProcessSample;
        private System.Windows.Forms.CheckBox checkBoxMute;
        private System.Windows.Forms.TrackBar trackBarVolume;
    }
}
