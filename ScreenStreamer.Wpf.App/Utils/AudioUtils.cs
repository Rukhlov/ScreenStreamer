﻿using MediaToolkit.Utils;
//using NAudio.CoreAudioApi;

using System.Collections.Generic;
using System.Linq;

using ScreenStreamer.Wpf.Models;

namespace ScreenStreamer.Wpf.Helpers
{
    public static class AudioHelper
    {
        public static List<AudioSourceItem> GetAudioSources()
        {
            return AudioUtils.GetAudioCaptureDevices().Select(d => new AudioSourceItem(d)).ToList();
		}

        ////public static List<MMDevice> GetMultiMediaDevices()
        ////{
        ////    MMDeviceEnumerator names = new MMDeviceEnumerator();
        ////    return names.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
        ////                .Cast<MMDevice>()
        ////                .ToList();
        ////}

        //public static List<AudioDeviceViewModel> GetMultiMediaDeviceViewModels()
        //{

        //    var audioDevices = AudioTool.GetAudioCaptureDevices();

        //    return audioDevices.Select(d => new AudioDeviceViewModel(d)).ToList();

        //    //return GetMultiMediaDevices()
        //    //    .Select(mmd => new AudioDeviceViewModel { DeviceId = mmd.ID, DisplayName = mmd.FriendlyName })
        //    //    .ToList();
        //}
    }
}
