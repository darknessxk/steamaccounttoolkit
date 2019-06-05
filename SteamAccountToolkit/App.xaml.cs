using Unity;
using Prism.Unity;
using System.Windows;
using Prism.Ioc;
using System;

namespace SteamAccountToolkit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    internal partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<Views.AppMain>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Globals.IsAppRunning = true;

            Globals.DefaultImage = new System.Windows.Media.Imaging.BitmapImage();
            Globals.DefaultImage.BeginInit();
            Globals.DefaultImage.UriSource = new Uri("pack://application:,,,/SteamAccountToolkit;component/Assets/user_default.jpg");
            Globals.DefaultImage.EndInit();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Globals.IsAppRunning = false;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Views.UsersList>("UsersList");
            containerRegistry.RegisterForNavigation<Views.UserPage>("UserPage");
            containerRegistry.RegisterForNavigation<Views.AddUser>("AddUser");
            containerRegistry.RegisterForNavigation<Views.SettingsPage>("SettingsPage");
        }
    }
}