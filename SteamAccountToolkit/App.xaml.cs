using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Prism.Ioc;
using SteamAccountToolkit.Views;

namespace SteamAccountToolkit
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    internal partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<AppMain>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Globals.IsAppRunning = true;

            Globals.DefaultImage = new BitmapImage();
            Globals.DefaultImage.BeginInit();
            Globals.DefaultImage.UriSource =
                new Uri("pack://application:,,,/SteamAccountToolkit;component/Assets/user_default.jpg");
            Globals.DefaultImage.EndInit();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Globals.IsAppRunning = false;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<UsersList>("UsersList");
            containerRegistry.RegisterForNavigation<UserPage>("UserPage");
            containerRegistry.RegisterForNavigation<AddUser>("AddUser");
            containerRegistry.RegisterForNavigation<SettingsPage>("SettingsPage");
            containerRegistry.RegisterForNavigation<CaptchaSubmitPage>("CaptchaSubmitPage");
            containerRegistry.RegisterForNavigation<EmailCodeSubmitPage>("EmailCodeSubmitPage");
        }
    }
}