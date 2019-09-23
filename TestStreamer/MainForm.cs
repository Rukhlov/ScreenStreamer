﻿using MediaToolkit;
using MediaToolkit.Common;
using MediaToolkit.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestStreamer.Controls;

namespace TestStreamer
{
    public partial class MainForm : Form
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MainForm()
        {
            InitializeComponent();

            this.Visible = false;

            //this.notifyIcon.Visible = true;
            this.notifyIcon.BalloonTipTitle = "TEST_TITLE";
            this.notifyIcon.BalloonTipText = "TEST_TEXT";

            this.notifyIcon.MouseClick += NotifyIcon_MouseClick;


            var screens = Screen.AllScreens.Select(s=>new ScreenItem {Screen = s, Title = s.DeviceName }).ToList();
            screenItems = new BindingList<ScreenItem>(screens);

            screensComboBox.DisplayMember = "Title";
            screensComboBox.DataSource = screenItems;

            UpdateControls();



        }

        private BindingList<ScreenItem> screenItems = new BindingList<ScreenItem>();

        class ScreenItem
        {
            public string Title { get; set; }
            public Screen Screen { get; set; }

        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Visible = !this.Visible;
            }
        }

        private void exitButton_Click(object sender, EventArgs e)
        {

            closing = true;
            this.Close();

            //this.notifyIcon.BalloonTipTitle = "TEST_TITLE";
            //this.notifyIcon.BalloonTipText = "TEST_TEXT";
            //this.notifyIcon.ShowBalloonTip(3000);
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            if (!closing)
            { 
                e.Cancel = true;
                this.Visible = false;
            }
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
           
            base.OnClosed(e);
        }

        private bool closing = false;
        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            closing = true;
            this.Close();
        }

        private void screensUpdateButton_Click(object sender, EventArgs e)
        {
            var screens = Screen.AllScreens.Select(s => new ScreenItem { Screen = s, Title = s.DeviceName }).ToList();

            screenItems = new BindingList<ScreenItem>(screens);

            screensComboBox.DisplayMember = "Title";
            screensComboBox.DataSource = screenItems;
        }

        private bool isStreaming = false;

        private VideoMulticastStreamer videoStreamer = null;
        private ScreenSource screenSource = null;
        private DesktopManager desktopMan = null;

        private void startButton_Click(object sender, EventArgs e)
        {
            int fps = (int)fpsNumeric.Value;

            bool showMouse = showMouseCheckBox.Checked;
            bool enableInputSimulator = inputSimulatorCheckBox.Checked;

            bool aspectRatio = false;

            var srcRect = currentScreen.Bounds;

            var destSize = new Size(srcRect.Width, srcRect.Height);
            if (aspectRatio)
            {
                var ratio = srcRect.Width / (double)srcRect.Height;
                int destWidth = destSize.Width;
                int destHeight = (int)(destWidth / ratio);
                if (ratio < 1)
                {
                    destHeight = destSize.Height;
                    destWidth = (int)(destHeight * ratio);
                }

                destSize = new Size(destWidth, destHeight);
            }


            screenSource = new ScreenSource();
            ScreenCaptureParams captureParams = new ScreenCaptureParams
            {
                SrcRect = srcRect,
                DestSize = destSize,
                CaptureType = CaptureType.DXGIDeskDupl,
                //CaptureType = CaptureType.Direct3D,
                //CaptureType = CaptureType.GDI,
                Fps = fps,
                CaptureMouse = showMouse,
            };

            screenSource.Setup(captureParams);

            var cmdOptions = new CommandLineOptions();
            cmdOptions.IpAddr = addressTextBox.Text;
            cmdOptions.Port = (int)portNumeric.Value;

            NetworkStreamingParams networkParams = new NetworkStreamingParams
            {
                Address = cmdOptions.IpAddr,
                Port = cmdOptions.Port,
            };

            VideoEncodingParams encodingParams = new VideoEncodingParams
            {
                Width = destSize.Width, // options.Width,
                Height = destSize.Height, // options.Height,
                FrameRate = cmdOptions.FrameRate,
                EncoderName = "libx264", // "h264_nvenc", //
            };

            videoStreamer = new VideoMulticastStreamer(screenSource);
            videoStreamer.Setup(encodingParams, networkParams);



            statisticForm.Location = currentScreen.Bounds.Location;
            statisticForm.Start();

            var captureTask = screenSource.Start();
            var streamerTask = videoStreamer.Start();

            //previewForm = new PreviewForm();
            //previewForm.Setup(screenSource);

            if (enableInputSimulator)
            {
                desktopMan = new DesktopManager();
                var inputSimulatorTask = desktopMan.Start();
            }

            isStreaming = true;
            UpdateControls();

        }


        private void stopButton_Click(object sender, EventArgs e)
        {
            if (screenSource != null)
            {
                screenSource.Close();
            }

            if (videoStreamer != null)
            {
                videoStreamer.Close();

            }

            if (statisticForm != null)
            {
                statisticForm.Stop();
                statisticForm.Visible = false;
            }

            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Close();
                previewForm = null;
            }

            if (desktopMan != null)
            {
                desktopMan.Stop();
                desktopMan = null;
            }

            isStreaming = false;

            UpdateControls();



        }

        private void UpdateControls()
        {
            this.settingPanel.Enabled = !isStreaming;

            this.startButton.Enabled = !isStreaming;
            this.previewButton.Enabled = isStreaming;

            this.stopButton.Enabled = isStreaming;
        }

        private Screen currentScreen = null;
        private void screensComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            var selectedItem = screensComboBox.SelectedItem;
            if (selectedItem != null)
            {
                var screenItem = selectedItem as ScreenItem;
                if (screenItem != null)
                {
                    var screen = screenItem.Screen;
                    if(screen != null)
                    {
                        currentScreen = screen;
                    }
                }
            }
        }

        private StatisticForm statisticForm = new StatisticForm();
        private PreviewForm previewForm = new PreviewForm();
        private void previewButton_Click(object sender, EventArgs e)
        {
            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Visible = !previewForm.Visible;
            }
            else
            {

                previewForm = new PreviewForm();
                previewForm.Setup(screenSource);
                previewForm.Visible = true;
            }
        }
    }
}
