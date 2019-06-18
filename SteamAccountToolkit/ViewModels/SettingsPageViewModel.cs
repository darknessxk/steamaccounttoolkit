using System.Collections.ObjectModel;
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

            SetColorDarkModeCommand = new DelegateCommand(() => SetColorMode(true));
            SetColorLightModeCommand = new DelegateCommand(() => SetColorMode(false));
        }

        public ObservableCollection<Swatch> PrimarySwatchesColors { get; }
        public ObservableCollection<Swatch> AccentSwatchesColors { get; }

        public Swatch CurrentPrimary
        {
            get => _paletteHelper.QueryPalette().PrimarySwatch;
            set => _paletteHelper.ReplacePrimaryColor(value);
        }

        public Swatch CurrentAccent
        {
            get => _paletteHelper.QueryPalette().AccentSwatch;
            set => _paletteHelper.ReplaceAccentColor(value);
        }

        public DelegateCommand SetColorDarkModeCommand { get; }
        public DelegateCommand SetColorLightModeCommand { get; }

        private void SetColorMode(bool? isDark)
        {
            if (isDark.HasValue)
                _paletteHelper.SetLightDark(isDark.Value);
        }
    }
}