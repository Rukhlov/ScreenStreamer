﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NLog;
using ScreenStreamer.Common;

using ScreenStreamer.Wpf.Helpers;
using ScreenStreamer.Wpf.Managers;


namespace ScreenStreamer.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static SystemManager SystemMan { get; } = new SystemManager();

        private NotifyIcon notifyIcon = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            logger.Debug("OnStartup(...) " + string.Join(" ", e.Args));

            var args = e.Args;

			var startupParams = Program.StartupParams;
			bool resetConfig = startupParams.ResetConfig;

			var appModel = ConfigManager.GetConfig(resetConfig);

			ServiceLocator.RegisterInstance(appModel);

            if (!appModel.Init())
            {// reset config...
				//...
			}

            // SystemMan.Initialize();


			var dialogService = new DialogService();
            ServiceLocator.RegisterInstance<Interfaces.IDialogService>(dialogService);

            var mainViewModel = new ViewModels.Dialogs.MainViewModel(appModel);
            Views.AppWindow mainWindow = new Views.AppWindow(mainViewModel);

            dialogService.Register(mainViewModel, mainWindow);

            //var interopHelper = new System.Windows.Interop.WindowInteropHelper(mainWindow);
            //interopHelper.EnsureHandle();

            notifyIcon = new NotifyIcon(mainViewModel);

            bool autoStartStream = startupParams.AutoStream;
            if (!autoStartStream)
            {
                mainViewModel.IsVisible = true;
                dialogService.Show(mainViewModel);
            }
            else
            {
                mainViewModel.IsVisible = false;
            }

            base.OnStartup(e);

            if (autoStartStream)
            {
                mainViewModel.StartAllCommand.Execute(null);
            }

            //
            //Console.WriteLine("=======================");
        }


		protected override void OnExit(ExitEventArgs e)
        {
            logger.Debug("OnExit(...) " + e.ApplicationExitCode);

           
            ConfigManager.Save();

            //SystemMan.Shutdown();
            notifyIcon?.Dispose();

            base.OnExit(e);
        }

    }
}
