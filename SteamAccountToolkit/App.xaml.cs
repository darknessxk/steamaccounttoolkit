using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
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
            Globals.Options = Globals.SettingsManager.Load();

            var paletteHelper = new PaletteHelper();
            var swatchesProvider = new SwatchesProvider();
            paletteHelper.ReplacePrimaryColor(swatchesProvider.Swatches.First(x => x.Name == Globals.Options.ThemeColor.Value));
            paletteHelper.ReplaceAccentColor(swatchesProvider.Swatches.First(x => x.Name == Globals.Options.ThemeAccent.Value && x.IsAccented));
            paletteHelper.SetLightDark(Globals.Options.ThemeIsDark.Value);

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