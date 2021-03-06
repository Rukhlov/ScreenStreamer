﻿using Prism.Commands;
using ScreenStreamer.Wpf.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ScreenStreamer.Wpf.ViewModels.Dialogs
{
    public class MessageBoxViewModel : WindowViewModel
    {

        public string Title { get; set; }
        public string DialogText { get; set; }

        //public MessageBoxResult DialogResult { get; private set; }

        public string OkButtonText { get; set; } = LocalizationManager.GetString("MessageBoxButtonOKText");//"OK";
		public string CancelButtonText { get; set; } = LocalizationManager.GetString("MessageBoxButtonCancelText");//"Cancel";

		public bool IsCancelVisible => (messageBoxButton != MessageBoxButton.OK);

        public override string Caption => Title;

        public override double MinWidth => 300d;

        public override bool IsBottomVisible => false;

        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }


        private TrackableViewModel parentViewModel = null; // по идее не нужно...
        private MessageBoxButton messageBoxButton = MessageBoxButton.OK;

        public MessageBoxViewModel(TrackableViewModel parent = null) : this("", "", MessageBoxButton.OK, MessageBoxImage.None, parent)
        { }

        public MessageBoxViewModel(string message, TrackableViewModel parent = null) : this(message, "", MessageBoxButton.OK, MessageBoxImage.None, parent)
        { }

        public MessageBoxViewModel(string message, string title, MessageBoxButton button, TrackableViewModel parent = null) : this(message, title, button, MessageBoxImage.None, parent)
        { }

        public MessageBoxViewModel(string message, string title, MessageBoxImage image, TrackableViewModel parent = null) : this(message, title, MessageBoxButton.OK, image, parent)
        { }

        public MessageBoxViewModel(string message, string title, TrackableViewModel parent = null) : this(message, title, MessageBoxButton.OK, MessageBoxImage.None, parent)
        { }

        public MessageBoxViewModel(string message, string title, MessageBoxButton button, MessageBoxImage image, TrackableViewModel parent = null) : base(parent)
        {
            this.parentViewModel = parent;
            this.messageBoxButton = button;

            if (iconDict.ContainsKey(image))
            {
                base.captionImage = iconDict[image];
            }
            else
            {
                base.captionImage = iconDict[MessageBoxImage.Error];
            }

            if (messageBoxButton == MessageBoxButton.OK)
            {
				OkButtonText = LocalizationManager.GetString("MessageBoxButtonOKText");//"OK";
            }
            else if (messageBoxButton == MessageBoxButton.OKCancel)
            {
                OkButtonText = LocalizationManager.GetString("MessageBoxButtonOKText");//"OK";
				CancelButtonText = LocalizationManager.GetString("MessageBoxButtonCancelText");//"Cancel";
			}
            else if (messageBoxButton == MessageBoxButton.YesNo)
            {
                OkButtonText = LocalizationManager.GetString("MessageBoxButtonYesText");//"Yes";
				CancelButtonText = LocalizationManager.GetString("MessageBoxButtonNoText");//"No";
			}
            else
            {
                // not supported...
            }

            this.OkCommand = new DelegateCommand<Window>(OnExecuteOkCommand);

            this.DialogText = message;
            this.Title = title;

        }

        private void OnExecuteOkCommand(Window selfWindow)
        {

            selfWindow.DialogResult = true;
            selfWindow.Close();
        }


        private static Dictionary<MessageBoxImage, BitmapImage> iconDict = new Dictionary<MessageBoxImage, BitmapImage>
        {
            { MessageBoxImage.Information, new BitmapImage(new Uri("pack://application:,,,/ScreenStreamer.Wpf.App;Component/Icons/Info_32x32.png")) },
            { MessageBoxImage.Warning, new BitmapImage(new Uri("pack://application:,,,/ScreenStreamer.Wpf.App;Component/Icons/Warn_32x32.png")) },
            { MessageBoxImage.Error, new BitmapImage(new Uri("pack://application:,,,/ScreenStreamer.Wpf.App;Component/Icons/Error_32x32.png")) },
        };
    }
}
