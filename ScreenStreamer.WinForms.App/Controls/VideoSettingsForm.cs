﻿using MediaToolkit;
using MediaToolkit.Core;
using MediaToolkit.UI;
using ScreenStreamer.Common;
using ScreenStreamer.WinForms.App;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestStreamer.Controls
{
    public partial class VideoSettingsForm : Form
    {


		private int MinWidth = 64;
		private int MinHeight = 64;

		private int MaxWidth = 4096;
		private int MaxHeight = 4096;

		public VideoSettingsForm()
        {
            InitializeComponent();

            //LoadEncoderProfilesItems();

            ////LoadTransportItems();
            //LoadEncoderItems();

            //LoadRateModeItems();

            //LoadCaptureTypes();

            syncContext = SynchronizationContext.Current;

        }

        public VideoStreamSettings VideoSettings { get; private set; }


        public void Setup(VideoStreamSettings settingsParams)
        {

            this.VideoSettings = settingsParams;

            LoadEncoderProfilesItems();

            LoadEncoderItems();

            LoadRateModeItems();

            LoadCaptureTypes();

            var screenCaptureDevice = VideoSettings.CaptureDevice as ScreenCaptureDevice;
            if (screenCaptureDevice != null)
            {
               // var captureDescr = screenCaptureParams.CaptureDescription;
                this.displayTextBox.Text = screenCaptureDevice.Name;

                this.CaptureRegion = screenCaptureDevice.CaptureRegion;
                this.captureMouseCheckBox.Checked = screenCaptureDevice.Properties.CaptureMouse;

                this.aspectRatioCheckBox.Checked = screenCaptureDevice.Properties.AspectRatio;

                cameraTableLayoutPanel.Visible = false;
                screenCaptureTableLayoutPanel.Visible = true;
                screenCaptureDetailsPanel.Visible = true;
                //WebCamGroup.Visible = false;
                //ScreenCaptureGroup.Visible = true;

                //adjustAspectRatioButton.Visible = true;

                showDebugInfoCheckBox.Checked = screenCaptureDevice.Properties.ShowDebugInfo;
                showCaptureBorderCheckBox.Checked = screenCaptureDevice.Properties.ShowDebugBorder;

                if (screenCaptureDevice.DisplayRegion.IsEmpty)
                {
                    displayTextBox.Visible = false;
                    labelDisplay.Visible = false;
                }

            }

            var uvcDevice = VideoSettings.CaptureDevice as UvcDevice;
            if (uvcDevice != null)
            {
                CaptureDeviceTextBox.Text = uvcDevice.Name;

                var profile = uvcDevice?.CurrentProfile;
                List<ComboBoxItem> profileItems = new List<ComboBoxItem>
                {
                    new ComboBoxItem
                    {
                        Name = profile?.ToString() ?? "",
                        Tag = profile,
                    },
                };
                CaptureDeviceProfilesComboBox.DataSource = profileItems;
                CaptureDeviceProfilesComboBox.DisplayMember = "Name";
                CaptureDeviceProfilesComboBox.ValueMember = "Tag";


                cameraTableLayoutPanel.Visible = true;
                screenCaptureTableLayoutPanel.Visible = false;
                screenCaptureDetailsPanel.Visible = false;
                //WebCamGroup.Visible = true;
                //ScreenCaptureGroup.Visible = false;

                //adjustAspectRatioButton.Visible = false;
            }

            

            this.captureRegionTextBox.Text =  CaptureRegion.ToString();

            var videoEncoderPars = VideoSettings.EncoderSettings;

            var resolution = videoEncoderPars.Resolution;

            if (resolution.Width <= destWidthNumeric.Maximum && resolution.Width >= destWidthNumeric.Minimum)
            {
                this.destWidthNumeric.Value = resolution.Width;
            }
            if (resolution.Height <= destHeightNumeric.Maximum && resolution.Height >= destHeightNumeric.Minimum)
            {
                this.destHeightNumeric.Value = resolution.Height;
            }

            bool useEncoderResoulutionFromSource = VideoSettings.StreamFlags.HasFlag(VideoStreamFlags.UseEncoderResoulutionFromSource);
            this.checkBoxResoulutionFromSource.Checked = VideoSettings.UseEncoderResoulutionFromSource;

            this.encoderComboBox.SelectedItem = videoEncoderPars.Encoder;
            this.encProfileComboBox.SelectedItem = videoEncoderPars.Profile;
            this.bitrateModeComboBox.SelectedItem = videoEncoderPars.BitrateMode;
            this.MaxBitrateNumeric.Value = videoEncoderPars.MaxBitrate;
            this.bitrateNumeric.Value = videoEncoderPars.Bitrate;
            this.fpsNumeric.Value = videoEncoderPars.FrameRate;
            this.latencyModeCheckBox.Checked = videoEncoderPars.LowLatency;

			//this.adjustResolutionCheckBox.Checked = adjustResolutionToSrcAspectRatio;

			this.destWidthNumeric.Maximum = MaxWidth;
			this.destWidthNumeric.Minimum = MinWidth;

			this.destHeightNumeric.Minimum = MinHeight;
			this.destHeightNumeric.Maximum = MaxHeight;


		}

        private void applyButton_Click(object sender, EventArgs e)
        {
            var screenCaptureParams = VideoSettings.CaptureDevice as ScreenCaptureDevice;
            if (screenCaptureParams != null)
            {
                screenCaptureParams.CaptureRegion = this.CaptureRegion;
                screenCaptureParams.Properties.CaptureMouse = this.captureMouseCheckBox.Checked;
                screenCaptureParams.Properties.AspectRatio = this.aspectRatioCheckBox.Checked;

                screenCaptureParams.Properties.ShowDebugInfo = showDebugInfoCheckBox.Checked;
                screenCaptureParams.Properties.ShowDebugBorder = showCaptureBorderCheckBox.Checked;
            }

            var deviceDescr = VideoSettings.CaptureDevice as UvcDevice;
            if (deviceDescr != null)
            {

            }

            VideoSettings.StreamFlags &= ~VideoStreamFlags.UseEncoderResoulutionFromSource;
            if (this.checkBoxResoulutionFromSource.Checked)
            {
                VideoSettings.StreamFlags |= VideoStreamFlags.UseEncoderResoulutionFromSource;
            }

            

            if (!VideoSettings.UseEncoderResoulutionFromSource)
            {
                int width = (int)this.destWidthNumeric.Value;
                int height = (int)this.destHeightNumeric.Value;

                if (width % 2 != 0)
                {
                    width--;
                }

                if (height % 2 != 0)
                {
                    height--;
                }

                VideoSettings.EncoderSettings.Width = width;
                VideoSettings.EncoderSettings.Height = height;
            }


            VideoSettings.EncoderSettings.Encoder = (VideoEncoderMode)this.encoderComboBox.SelectedItem;
            VideoSettings.EncoderSettings.Profile = (H264Profile)this.encProfileComboBox.SelectedItem;
            VideoSettings.EncoderSettings.BitrateMode = (BitrateControlMode)this.bitrateModeComboBox.SelectedItem;

            VideoSettings.EncoderSettings.MaxBitrate = (int)this.MaxBitrateNumeric.Value;
            VideoSettings.EncoderSettings.Bitrate = (int)this.bitrateNumeric.Value;

            VideoSettings.EncoderSettings.FrameRate = (int)this.fpsNumeric.Value;

            VideoSettings.EncoderSettings.LowLatency = this.latencyModeCheckBox.Checked;

			//adjustResolutionToSrcAspectRatio = this.adjustResolutionCheckBox.Checked;

			this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //private void LoadTransportItems()
        //{
        //    var items = new List<TransportMode>
        //    {
        //        TransportMode.Tcp,
        //        TransportMode.Udp,
        //    };
        //    transportComboBox.DataSource = items;
        //}

        private void LoadRateModeItems()
        {

            var items = new List<BitrateControlMode>
            {
               BitrateControlMode.CBR,
               BitrateControlMode.VBR,
               BitrateControlMode.Quality,

            };

            bitrateModeComboBox.DataSource = items;
        }

        private void LoadEncoderProfilesItems()
        {

            var items = new List<H264Profile>
            {
               H264Profile.Main,
               H264Profile.Base,
               H264Profile.High,

            };

            encProfileComboBox.DataSource = items;
        }

        private void LoadEncoderItems()
        {
            var items = new List<VideoEncoderMode>
            {
                VideoEncoderMode.H264,
                VideoEncoderMode.JPEG,
            };

            encoderComboBox.DataSource = items;

        }

        private void LoadCaptureTypes()
        {

            //List<VideoCaptureType> captureTypes = new List<VideoCaptureType>();
            //captureTypes.Add(VideoCaptureType.DXGIDeskDupl);
            //captureTypes.Add(VideoCaptureType.GDI);
            ////captureTypes.Add(CaptureType.GDIPlus);
            //captureTypes.Add(VideoCaptureType.Direct3D9);
            //captureTypes.Add(VideoCaptureType.Datapath);

            //captureTypesComboBox.DataSource = captureTypes;

            List<ComboBoxItem> _captureTypes = new List<ComboBoxItem>();


            var captureProps = Config.Data.ScreenCaptureProperties;
            var caputreDevice = VideoSettings.CaptureDevice;

            var resolution = caputreDevice.Resolution;
            var propsStr = resolution.Width + "x" + resolution.Height + ", " + captureProps.Fps + "fps" + ", " + (captureProps.UseHardware ? "GPU" : "CPU");

            // var propsStr = resolution.Width + "x" + resolution.Height + (captureProps.UseHardware ? "GPU" : "CPU") + ", " + captureProps.Fps + " Fps";
            _captureTypes.Add(new ComboBoxItem
            {
                Name = "Desktop Duplication API (" + propsStr + ")",
                Tag = VideoCaptureType.DXGIDeskDupl,
            });


            captureTypesComboBox.DataSource = _captureTypes;
            captureTypesComboBox.DisplayMember = "Name";
            captureTypesComboBox.ValueMember = "Tag";
        }

        private Rectangle CaptureRegion = Rectangle.Empty;

        private void snippingToolButton_Click(object sender, EventArgs e)
        {
            var captureDescr = VideoSettings.CaptureDevice as ScreenCaptureDevice;
            if (captureDescr == null)
            {
                return;
            }

            var selectedRegion = SnippingTool.Snip(captureDescr.DisplayRegion);
            if (!selectedRegion.IsEmpty)
            {
                this.CaptureRegion = selectedRegion;

                this.captureRegionTextBox.Text = CaptureRegion.ToString();
            }
            else
            {// error, cancelled...

            }

        }

        private void adjustAspectRatioButton_Click(object sender, EventArgs e)
        {
            var srcSize = new Size (this.CaptureRegion.Width, this.CaptureRegion.Height);
            var destSize= new Size((int)this.destWidthNumeric.Value, (int)this.destHeightNumeric.Value);


  
            var ratio = srcSize.Width / (double)srcSize.Height;
            int destWidth = destSize.Width;

		    int destHeight = (int)(destWidth / ratio);
			if(destHeight > MaxHeight)
			{
				destHeight = MaxHeight;
				destWidth = (int)(destHeight * ratio);
			}
			
			if(destHeight< MinHeight)
			{
				destHeight = MinHeight;
				destWidth = (int)(destHeight * ratio);
			}

            if (ratio < 1)
            {
                destHeight = destSize.Height;
                destWidth = (int)(destHeight * ratio);


				if (destWidth > MaxWidth)
				{
					destWidth = MaxWidth;
					destHeight = (int)(destWidth / ratio);
				}

				if (destWidth < MinWidth)
				{
					destWidth = MinWidth;
					destHeight = (int)(destWidth / ratio);
				}
			}
			

            this.destWidthNumeric.Value = destWidth;
            this.destHeightNumeric.Value = destHeight;
            
        }

        private void showDebugInfoCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBoxResoulutionFromSource_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxResoulutionFromSource.Checked)
            {
                panelEncoderResoulution.Enabled = false;
                //aspectRatioCheckBox.Enabled = false;
				adjustAspectRatioButton.Enabled = false;
			}
            else
            {
                panelEncoderResoulution.Enabled = true;
                //aspectRatioCheckBox.Enabled = true;
				adjustAspectRatioButton.Enabled = true;

			}
        }


        private void destWidthNumeric_ValueChanged(object sender, EventArgs e)
		{
			Debug.WriteLine("destWidthNumeric_ValueChanged(...)");
			return;

			if (resolutionEdited)
			{
				return;
			}

			

			var srcSize = new Size(this.CaptureRegion.Width, this.CaptureRegion.Height);
			var destSize = new Size((int)this.destWidthNumeric.Value, (int)this.destHeightNumeric.Value);


			if (adjustResolutionToSrcAspectRatio)
			{



				var ratio = srcSize.Width / (double)srcSize.Height;
				int destWidth = destSize.Width;
				int destHeight = (int)(destWidth / ratio);

				resolutionEdited = true;

				this.destHeightNumeric.Value = destHeight;

				resolutionEdited = false;
			}
			else
			{

			}


		}


		private bool resolutionEdited = false;
		private void destHeightNumeric_ValueChanged(object sender, EventArgs e)
		{

			Debug.WriteLine("destHeightNumeric_ValueChanged(...)");
			return;

			if (resolutionEdited)
			{
				return;
			}



			var srcSize = new Size(this.CaptureRegion.Width, this.CaptureRegion.Height);
			var destSize = new Size((int)this.destWidthNumeric.Value, (int)this.destHeightNumeric.Value);
			var ratio = srcSize.Width / (double)srcSize.Height;

			if (adjustResolutionToSrcAspectRatio)
			{

				var destHeight = destSize.Height;
				var destWidth = (int)(destHeight * ratio);

				if (destWidth > MaxWidth)
				{
					destWidth = MaxWidth;
					destHeight = (int)(destWidth / ratio);


					try
					{
						resolutionEdited = true;
						destWidthNumeric.Value = destWidth;
						destHeightNumeric.Value = destHeight;

					}
					finally
					{
						resolutionEdited = false;
					}

					return;
				}

				try
				{
					resolutionEdited = true;
					destWidthNumeric.Value = destWidth;

				}
				finally
				{
					resolutionEdited = false;
				}

				//this.destHeightNumeric.Value = destHeight;
			}
			else
			{

			}
		}

		private bool adjustResolutionToSrcAspectRatio = false;

        //private void adjustResolutionCheckBox_CheckedChanged(object sender, EventArgs e)
        //{
        //	adjustResolutionToSrcAspectRatio = adjustResolutionCheckBox.Checked;
        //}





        private PreviewForm previewForm = null;
        private IVideoSource videoSource = null;

        private SynchronizationContext syncContext = null;

        private void previewButton_Click(object sender, EventArgs e)
        {
            var captureDevice= this.VideoSettings.CaptureDevice;

            if (videoSource == null)
            {
                if (captureDevice.CaptureMode == CaptureMode.UvcDevice)
                {
                    videoSource = new VideoCaptureSource();
                    videoSource.Setup(captureDevice);
                }
                else if (captureDevice.CaptureMode == CaptureMode.Screen)
                {
                    videoSource = new ScreenSource();
                    videoSource.Setup(captureDevice);
                }

                videoSource.CaptureStarted += VideoSource_CaptureStarted;
                videoSource.CaptureStopped += VideoSource_CaptureStopped;
            }


            if (videoSource.State == CaptureState.Capturing)
            {
                videoSource.Stop();
            }
            else
            {
                videoSource.Start();
            }

            this.Cursor = Cursors.WaitCursor;
            this.Enabled = false;


        }

        private void VideoSource_CaptureStarted()
        {
            syncContext.Send(_ =>
            {
                OnVideoSourceStarted();

            }, null);

        }

        private void VideoSource_CaptureStopped(object obj)
        {

            syncContext.Send(_ =>
            {
                OnVideoSourceStopped();

            }, null);

        }



        private void OnVideoSourceStarted()
        {

            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Close();
                previewForm.FormClosed -= PreviewForm_FormClosed;
                previewForm = null;
            }


            if (previewForm == null || previewForm.IsDisposed)
            {
                previewForm = new PreviewForm
                {
                    StartPosition = FormStartPosition.CenterParent,
                    Icon = ScreenStreamer.WinForms.App.Properties.Resources.logo,
                    //ShowIcon = false,
                };

                previewForm.FormClosed += PreviewForm_FormClosed;

                previewForm.Setup(videoSource);

            }


            var caputreDevice = VideoSettings.CaptureDevice;

            previewForm.Text = caputreDevice.Name;

            previewForm.Visible = true;

            this.Cursor = Cursors.Default;
            this.Enabled = true;

        }

        private void OnVideoSourceStopped()
        {
            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Visible = false;
            }

            videoSource.Close(true);
            videoSource.CaptureStarted -= VideoSource_CaptureStarted;
            videoSource.CaptureStopped -= VideoSource_CaptureStopped;

            videoSource = null;

            this.Cursor = Cursors.Default;
            this.Enabled = true;
        }


        private void PreviewForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (videoSource != null)
            {
                videoSource.Stop();
            }
        }


        protected override void OnClosed(EventArgs e)
        {

            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Close();

                previewForm.FormClosed -= PreviewForm_FormClosed;
                previewForm = null;
            }

            base.OnClosed(e);
        }

    }
}
