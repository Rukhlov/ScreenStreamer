﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using MediaToolkit.Core;
using Newtonsoft.Json;
using NLog;
using ScreenStreamer.Common;
using ScreenStreamer.Wpf.Common.Enums;
using ScreenStreamer.Wpf.Common.Helpers;

namespace ScreenStreamer.Wpf.Common.Models
{
    public class StreamMainModel
    {
        public static StreamMainModel Default => CreateDefault();

        private static StreamMainModel CreateDefault()
        {
            var defaultStream = new StreamModel() { Name = "Stream 1" };

            //defaultStream.Init();


            var @default = new StreamMainModel();
            @default.StreamList.Add(defaultStream);
            return @default;
        }

        public List<StreamModel> StreamList { get; set; } = new List<StreamModel>();
    }


    public class StreamModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public StreamModel()
        {

            mediaStreamer = new MediaStreamer();

            mediaStreamer.StateChanged += MediaStreamer_StateChanged;

        }

        private MediaStreamer mediaStreamer = null;
        private StreamSession currentSession = null;

        public AudioStreamSettings AudioSettings => currentSession.AudioSettings;
        public VideoStreamSettings VideoSettings => currentSession.VideoSettings;


        public event Action OnStreamStateChanged;

        public string Name { get; set; } = "";

        public bool IsStarted
        {
            get
            {
                bool isStarted = false;

                if (currentSession != null && mediaStreamer != null)
                {
                    isStarted = (mediaStreamer.State == MediaStreamerState.Streamming);
                }

                return isStarted;
            }
            //set;
        }

        public bool IsBusy
        {
            get
            {
                bool isBusy = false;
                if (currentSession != null && mediaStreamer != null)
                {
                    isBusy = (mediaStreamer.State == MediaStreamerState.Starting ||
                        mediaStreamer.State == MediaStreamerState.Stopping);
                }
                return isBusy;
            }
        }

        public AdvancedSettingsModel AdvancedSettingsModel { get; set; } = new AdvancedSettingsModel();
        public PropertyVideoModel PropertyVideo { get; set; } = new PropertyVideoModel();
        public PropertyQualityModel PropertyQuality { get; set; } = new PropertyQualityModel();
        public PropertyCursorModel PropertyCursor { get; set; } = new PropertyCursorModel();
        public PropertyAudioModel PropertyAudio { get; set; } = new PropertyAudioModel();
        public PropertyBorderModel PropertyBorder { get; set; } = new PropertyBorderModel();
        public PropertyNetworkModel PropertyNetwork { get; set; } = new PropertyNetworkModel();



        public void SwitchStreamingState()
        {
            logger.Debug("SwitchStreamingState()");

            if (!IsStarted)
            {
                StartStreaming();
            }
            else
            {
                StopStreaming();
            }
        }

        public void StartStreaming()
        {
            logger.Debug("StartStreaming()");

            if (mediaStreamer.State == MediaStreamerState.Shutdown)
            {
                currentSession = CreateSession();


                currentSession.Setup();

                mediaStreamer.Start(currentSession);
            }

        }

        public void StopStreaming()
        {
            logger.Debug("StopStreaming()");

            if (mediaStreamer != null)
            {
                mediaStreamer.Stop();
            }
        }

		private void MediaStreamer_StateChanged()
        {
            var state = mediaStreamer.State;

            logger.Debug("MediaStreamer_StateChanged() " + state);

            if (state == MediaStreamerState.Starting)
            {

			}
            else if (state == MediaStreamerState.Streamming)
            {

			}
            else if (state == MediaStreamerState.Stopping)
            {

			}
            else if (state == MediaStreamerState.Stopped)
            {


				mediaStreamer.Shutdown();
            }


            OnStreamStateChanged?.Invoke();

        }



        private StreamSession CreateSession()
        {

            var session = StreamSession.Default();

            var transport = TransportMode.Udp;
            if (PropertyNetwork.UnicastProtocol == ProtocolKind.TCP)
            {
                transport = TransportMode.Tcp;
            }


            session.StreamName = Name;
            session.NetworkIpAddress = PropertyNetwork.Network ?? "0.0.0.0";
            session.CommunicationPort = PropertyNetwork.Port;
            session.TransportMode = transport;

            session.IsMulticast = !PropertyNetwork.IsUnicast;


            var videoSettings = session.VideoSettings;
            var videoEncoderSettings = videoSettings.EncoderSettings;

            videoEncoderSettings.Encoder = AdvancedSettingsModel.VideoEncoder;
            videoEncoderSettings.Bitrate = AdvancedSettingsModel.Bitrate;
            videoEncoderSettings.MaxBitrate = AdvancedSettingsModel.MaxBitrate;

            videoEncoderSettings.FrameRate = AdvancedSettingsModel.Fps;
            videoEncoderSettings.Profile = AdvancedSettingsModel.H264Profile;
            videoEncoderSettings.LowLatency = AdvancedSettingsModel.LowLatency;

            int x = (int)PropertyVideo.Left;
            int y = (int)PropertyVideo.Top;
            int w = (int)PropertyVideo.ResolutionWidth;
            int h = (int)PropertyVideo.ResolutionHeight;

            var captureRegion = new Rectangle(x, y, w, h);

            var screenCaptureProperties = new ScreenCaptureProperties
            {
                CaptureMouse = PropertyCursor.IsCursorVisible,
                AspectRatio = PropertyVideo.AspectRatio,
                CaptureType = VideoCaptureType.DXGIDeskDupl,
                UseHardware = true,
                Fps = 30,
                ShowDebugInfo = false,
            };

            ScreenCaptureDevice captureDevice = new ScreenCaptureDevice
            {
                CaptureRegion = captureRegion,
                DisplayRegion = captureRegion,
                Name = PropertyVideo.Display,

                Resolution = captureRegion.Size,
                Properties = screenCaptureProperties,
                DeviceId = PropertyVideo.Display,


            };

            session.VideoSettings.CaptureDevice = captureDevice;

            return session;
        }

        public void Dispose()
        {
            logger.Debug("Dispose()");

            if (mediaStreamer != null)
            {
                if(mediaStreamer!=null && mediaStreamer.State != MediaStreamerState.Shutdown)
                {
                    //Stop and shutdown...
                    logger.Warn("mediaStreamer!=null && mediaStreamer.State != MediaStreamerState.Shutdown");
                }

                mediaStreamer.StateChanged -= MediaStreamer_StateChanged;

                mediaStreamer.Shutdown();

            }

        }

    }

    public class PropertyNetworkModel
    {
        public int Port { get; set; } = 808;
        public bool IsUnicast { get; set; } = true;
        public ProtocolKind UnicastProtocol { get; set; } = ProtocolKind.TCP;
        public string MulticastIp { get; set; } = "239.0.0.1";
        public string Network { get; set; } = "0.0.0.0";
    }

    public class PropertyBorderModel
    {
        public bool IsBorderVisible { get; set; }
    }

    public class _PropertyVideoModel
    {
        public string Display { get; set; } = ScreenHelper.ALL_DISPLAYS;
        public bool IsRegion { get; set; }

        public double Top { get; set; } = 0;
        public double Left { get; set; } = 0;
        public double ResolutionHeight { get; set; } = 1920;
        public double ResolutionWidth { get; set; } = 1080;
        public bool AspectRatio { get; set; } = true;
    }

    public class PropertyVideoModel
    {
        public string Display { get; set; } = ScreenHelper.ALL_DISPLAYS;
        public bool IsRegion { get; set; }

        public double Top { get; set; } = 0;
        public double Left { get; set; } = 0;
        public double ResolutionHeight { get; set; } = 1920;
        public double ResolutionWidth { get; set; } = 1080;
        public bool AspectRatio { get; set; } = true;
    }

    public class PropertyQualityModel
    {
        public QualityPreset Preset { get; set; } = QualityPreset.Standard;
    }

    public class PropertyCursorModel
    {
        public bool IsCursorVisible { get; set; } = true;
    }

    public class PropertyAudioModel
    {
        public bool IsMicrophoneEnabled { get; set; }
        public bool IsComputerSoundEnabled { get; set; } = true;
        public string DeviceId { get; set; }
    }


    public class AdvancedSettingsModel
    {
        public int Bitrate { get; set; } = 2500;
        public int Fps { get; set; } = 30;
        public bool LowLatency { get; set; } = true;
        public int MaxBitrate { get; set; } = 5000;
        public H264Profile H264Profile { get; set; } = H264Profile.Main;
        public VideoEncoderMode VideoEncoder { get; set; } = VideoEncoderMode.H264;
    }
}