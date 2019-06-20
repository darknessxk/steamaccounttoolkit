using System.Collections.ObjectModel;
using System.Linq;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;

namespace SteamAccountToolkit.ViewModels
{
    public class SettingsPageViewModel : BindableBase
    {
        private readonly PaletteHelper _paletteHelper;
        private readonly SwatchesProvider _swatchesProvider;
        private Swatch _currentPrimary;
        private Swatch _currentAccent;
        public DelegateCommand SetColorDarkModeCommand { get; }
        public DelegateCommand SetColorLightModeCommand { get; }
        public DelegateCommand SaveSettingsCommand { get; }

        public SettingsPageViewModel()
        {
            _paletteHelper = new PaletteHelper();
            _swatchesProvider = new SwatchesProvider();

            PrimarySwatchesColors = new ObservableCollection<Swatch>(_swatchesProvider.Swatches);
            foreach (var sw in _swatchesProvider.Swatches)
                if (!string.IsNullOrEmpty(sw.Name))
                    PrimarySwatchesColors.Add(sw);

            AccentSwatchesColors = new ObservableCollection<Swatch>();
            foreach (var sw in _swatchesProvider.Swatches)
                if (sw.IsAccented)
                    if (!string.IsNullOrEmpty(sw.Name))
                        AccentSwatchesColors.Add(sw);

            _currentPrimary = _swatchesProvider.Swatches.First(x => x.Name == Globals.Settings.ThemeColor.Value);
            _currentAccent = _swatchesProvider.Swatches.First(x => x.Name == Globals.Settings.ThemeAccent.Value && x.IsAccented);
            SetColorMode(Globals.Settings.ThemeIsDark.Value);

            SetColorDarkModeCommand = new DelegateCommand(() => SetColorMode(true));
            SetColorLightModeCommand = new DelegateCommand(() => SetColorMode(false));
            SaveSettingsCommand = new DelegateCommand(() => Globals.SettingsManager.Save(Globals.Settings));
        }

        public ObservableCollection<Swatch> PrimarySwatchesColors { get; }
        public ObservableCollection<Swatch> AccentSwatchesColors { get; }

        public Swatch CurrentPrimary
        {
            get => _currentPrimary;
            set
            {
                SetProperty(ref _currentPrimary, value);
                _paletteHelper.ReplacePrimaryColor(value);
                Globals.Settings.ThemeColor.Set(value.Name);
            }
        }

        public Swatch CurrentAccent
        {
            get => _currentAccent;
            set
            {
                SetProperty(ref _currentAccent, value);
                _paletteHelper.ReplaceAccentColor(value);
                Globals.Settings.ThemeAccent.Set(value.Name);
            }
        }

        private void SetColorMode(bool? isDark)
        {
            if (!isDark.HasValue) return;
            _paletteHelper.SetLightDark(isDark.Value);
            Globals.Settings.ThemeIsDark.Set(isDark.Value);
        }
    }
}