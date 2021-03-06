﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaToolkit.UI;
using NLog;
using MediaToolkit.Core;
using System.Windows.Threading;

namespace TestClient.Controls
{
    public partial class SimpleReceiverControl : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public SimpleReceiverControl()
        {
            InitializeComponent();

            LoadTransportItems();
        }


        public string ClientId { get; private set; }
        public string ServerId { get; private set; }

        private RemoteDesktopClient remoteClient = null;

        private VideoForm testForm = null;
        private D3DImageRenderer imageProvider = null;

        private void playButton_Click(object sender, EventArgs e)
        {
            logger.Debug("playButton_Click(...)");

            var address = addressTextBox.Text;
            if (string.IsNullOrEmpty(address))
            {
                return;
            }

            var port = (int)portNumeric.Value;

            try
            {
                remoteClient = new RemoteDesktopClient();

                remoteClient.UpdateBuffer += RemoteClient_UpdateBuffer;

                var w = (int)srcWidthNumeric.Value;
                var h = (int)srcHeightNumeric.Value;

                var inputPars = new VideoEncoderSettings
                {
                    //Width = (int)srcWidthNumeric.Value,
                    //Height = (int)srcHeightNumeric.Value,

                    //Width = 2560,
                    //Height = 1440,

                    //Width = 640,//2560,
                    //Height = 480,//1440,

                    Resolution = new Size(w, h),
                    FrameRate = 30,
                };

                var _w = (int)destWidthNumeric.Value;
                var _h = (int)destHeightNumeric.Value;
                var outputPars = new VideoEncoderSettings
                {
                    //Width = 640,//2560,
                    //Height = 480,//1440,
                    //Width = 2560,
                    //Height = 1440,

                    //Width = (int)destWidthNumeric.Value,
                    //Height = (int)destHeightNumeric.Value,
                    Resolution = new Size(_w, _h),
                    FrameRate = 30,
                };
                var transport = GetTransportMode();

                var networkPars = new NetworkSettings
                {
                    LocalAddr = address,
                    LocalPort = port,
                    TransportMode = transport,
                };

                remoteClient.Play(inputPars, outputPars, networkPars);

                string title = (@"rtp://" + address + ":" + port);

                ShowVideoForm(title);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                CleanUp();
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            logger.Debug("stopButton_Click(...)");

            try
            {
                remoteClient.UpdateBuffer -= RemoteClient_UpdateBuffer;

                remoteClient?.Stop();

                CloseVideoForm();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void ShowVideoForm(string title)
        {
            if (testForm == null || testForm.IsDisposed)
            {
                testForm = new VideoForm
                {
                    StartPosition = FormStartPosition.CenterParent,
                    Width = 1280,
                    Height = 720,

                    Text = title,
                };

                imageProvider?.Close(true);

                imageProvider = new D3DImageRenderer();
                var reciver = remoteClient.VideoReceiver;

                imageProvider.Setup(reciver.sharedTexture);
                imageProvider.Start();

                var video = testForm.userControl11;
                video.DataContext = imageProvider;

                testForm.FormClosed += TestForm_FormClosed;
            }


            testForm.Visible = true;
        }

        private void TestForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            remoteClient?.Disconnect();

        }


        private void CloseVideoForm()
        {
            if (testForm != null && !testForm.IsDisposed)
            {
                testForm.UnlinkInputManager();

                testForm.Close();
                testForm.FormClosed -= TestForm_FormClosed;
                testForm = null;
            }
        }

        private void RemoteClient_UpdateBuffer()
        {

            imageProvider?.Update();
        }


        private void CleanUp()
        {
            //...

        }


        private TransportMode GetTransportMode()
        {
            TransportMode transport = TransportMode.Unknown;
            var item = transportComboBox.SelectedItem;
            if (item != null)
            {
                transport = (TransportMode)item;
            }
            return transport;
        }

        private void LoadTransportItems()
        {

            var items = new List<TransportMode>
            {
                TransportMode.Udp,
                TransportMode.Tcp,

            };
            transportComboBox.DataSource = items;
        }

        private StatisticForm statisticForm = new StatisticForm();

        private void button8_Click(object sender, EventArgs e)
        {
            statisticForm.Visible = !statisticForm.Visible;
        }

    }
}
