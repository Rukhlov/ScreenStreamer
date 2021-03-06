﻿using ScreenStreamer.Wpf.Helpers;
using ScreenStreamer.Wpf.ViewModels.Properties;
using System.Collections.ObjectModel;
using ScreenStreamer.Wpf.Models;

using System.Linq;
using ScreenStreamer.Wpf.ViewModels.Common;

namespace ScreenStreamer.Wpf.ViewModels.Dialogs
{
    public class VideoSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => LocalizationManager.GetString("VideoSettingsCaption");// Video Stream;

        public ObservableCollection<VideoSourceItem> Displays { get; set; } = new ObservableCollection<VideoSourceItem>();
        public ObservableCollection<ScreenCaptureItem> ScreenCaptures { get; set; } = new ObservableCollection<ScreenCaptureItem>();

        public System.Windows.Input.ICommand UpdateVideoSourcesCommand { get; }

        public VideoSettingsViewModel(PropertyVideoViewModel property, TrackableViewModel parent) : base(property, parent)
        {
            this.parent.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(IsChanged))
                {
                    RaisePropertyChanged(() => IsChanged);
                }
            };

            UpdateVideoSourcesCommand = new Prism.Commands.DelegateCommand(UpdateSources);

			//Displays.AddRange(ScreenHelper.GetDisplayItems());


			//ScreenCaptures.AddRange(ScreenHelper.AllSupportedCaptures);

			var propVideoViewModel = ((PropertyVideoViewModel)this.Property);
            
            var streamModel = propVideoViewModel.Parent.MediaStreamer;

            var videoModel = streamModel.PropertyVideo;

			var appModel = ServiceLocator.GetInstance<Wpf.Models.AppModel>();
			ScreenCaptures.AddRange(appModel.ScreenCaptures);

			var captType = videoModel.CaptType;
            propVideoViewModel.CaptureType = ScreenCaptures.FirstOrDefault(c => c.CaptType == captType) ?? ScreenCaptures.FirstOrDefault();

			Displays.AddRange(appModel.VideoSources);
			

			//Displays.AddRange(ScreenHelper.GetVideoSources());

            var deviceId = videoModel.DeviceId;      

            if (string.IsNullOrEmpty(deviceId))
            {// если девайс не задан то берем первый попавшийся
                propVideoViewModel.Display = Displays.FirstOrDefault();
            }
            else
            {// если девайс есть в конфиге, то используем его даже если его нет в списке 
                // чтобы пользователь поменял его вручную
                propVideoViewModel.Display = Displays.FirstOrDefault(d => d.DeviceId == deviceId);
            }

           // ((PropertyVideoViewModel)this.Property).Display = Displays.FirstOrDefault(d => d.DeviceId == deviceId) ?? Displays.FirstOrDefault();

        }

        public void UpdateSources()
        {
            Displays.Clear();

			var appModel = ServiceLocator.GetInstance<AppModel>();
			if (appModel != null)
			{
				appModel.UpdateVideoSources();
				Displays.AddRange(appModel.VideoSources);
			}

			//Displays.AddRange(ScreenHelper.GetVideoSources(true));

            var propVideoViewModel = ((PropertyVideoViewModel)this.Property);

            var streamModel = propVideoViewModel.Parent.MediaStreamer;

            var deviceId = streamModel.PropertyVideo.DeviceId;

            propVideoViewModel.Display = Displays.FirstOrDefault(d => d.DeviceId == deviceId) ?? Displays.FirstOrDefault();

        }

        public override bool IsClosableOnLostFocus
        {
            get
            {
                //return false;

                return base.IsClosableOnLostFocus;
            }
        }

        protected override bool CheckChanges()
        {
            return base.CheckChanges() || parent.IsChanged;
        }



        //public new bool IsClosableOnLostFocus
        //{
        //    get
        //    {
        //        return !IsChanged;
        //    }

        //}

        //=> this is PropertyWindowViewModel && !IsChanged;
    }
}
