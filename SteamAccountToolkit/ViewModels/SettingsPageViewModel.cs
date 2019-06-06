using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace SteamAccountToolkit.ViewModels
{
    public class SettingsPageViewModel : BindableBase
    {
        public ObservableCollection<Swatch> PrimarySwatchesColors { get; private set; }
        public ObservableCollection<Swatch> AccentSwatchesColors { get; private set; }
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

        private SwatchesProvider _swatchesProvider;
        private PaletteHelper _paletteHelper;

        public DelegateCommand SetColorDarkModeCommand { get; private set; }
        public DelegateCommand SetColorLightModeCommand { get; private set; }

        public SettingsPageViewModel()
        {
            _paletteHelper = new PaletteHelper();
            _swatchesProvider = new SwatchesProvider();

            PrimarySwatchesColors = new ObservableCollection<Swatch>(_swatchesProvider.Swatches);
            foreach (var sw in _swatchesProvider.Swatches)
                if(!string.IsNullOrEmpty(sw.Name))
                    PrimarySwatchesColors.Add(sw);

            AccentSwatchesColors = new ObservableCollection<Swatch>();
            foreach (var sw in _swatchesProvider.Swatches)
                if (sw.IsAccented)
                    if (!string.IsNullOrEmpty(sw.Name))
                        AccentSwatchesColors.Add(sw);

            SetColorDarkModeCommand = new DelegateCommand(() => SetColorMode(true));
            SetColorLightModeCommand = new DelegateCommand(() => SetColorMode(false));
        }

        private void SetColorMode(bool? isDark)
        {
            if(isDark.HasValue)
                _paletteHelper.SetLightDark(isDark.Value);
        }
    }
}
