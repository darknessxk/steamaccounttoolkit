using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
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

            Task.Run(() =>
            {
                Classes.Utils.InvokeDispatcherIfRequired(() =>
                {
                    Globals.DefaultImage = new BitmapImage();
                    Globals.DefaultImage.BeginInit();
                    Globals.DefaultImage.UriSource =
                        new Uri("pack://application:,,,/SteamAccountToolkit;component/Assets/user_default.jpg");
                    Globals.DefaultImage.EndInit();
                });
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Globals.IsAppRunning = false;
        }

        public override void Initialize()
        {
            base.Initialize();
            Globals.Log.Info("App Initializing");

            Globals.IsAppRunning = true;
            Globals.Settings = Globals.SettingsManager.Load();

            var paletteHelper = new PaletteHelper();
            var swatchesProvider = new SwatchesProvider();

            Task.Run(() =>
            {
                var primary = swatchesProvider.Swatches.First(x => x.Name == Globals.Settings.ThemeColor.Value);
                var accent =
                    swatchesProvider.Swatches.First(x => x.Name == Globals.Settings.ThemeAccent.Value && x.IsAccented);

                Classes.Utils.InvokeDispatcherIfRequired(() =>
                {
                    if (primary != null)
                        paletteHelper.ReplacePrimaryColor(primary);

                    if (accent != null)
                        paletteHelper.ReplaceAccentColor(accent);

                    paletteHelper.SetLightDark(Globals.Settings.ThemeIsDark.Value);
                });
                Globals.Log.Info("App initialization completed, Task Completed");
            });

            Globals.Log.Info("App partially initialized, Task Pending");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            Globals.Log.Info("Registering Views");
            containerRegistry.RegisterForNavigation<UsersList>("UsersList");
            containerRegistry.RegisterForNavigation<UserPage>("UserPage");
            containerRegistry.RegisterForNavigation<AddUser>("AddUser");
            containerRegistry.RegisterForNavigation<SettingsPage>("SettingsPage");
            containerRegistry.RegisterForNavigation<CaptchaSubmitPage>("CaptchaSubmitPage");
            containerRegistry.RegisterForNavigation<EmailCodeSubmitPage>("EmailCodeSubmitPage");
            containerRegistry.RegisterForNavigation<ConsolePage>("ConsoleView");
        }
    }
}