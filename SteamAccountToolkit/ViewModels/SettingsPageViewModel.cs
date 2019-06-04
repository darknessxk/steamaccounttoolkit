using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SteamAccountToolkit.ViewModels
{
    public class SettingsPageViewModel : BindableBase
    {
        public ObservableCollection<Swatch> PrimarySwatchesColors { get; private set; }
        public ObservableCollection<Swatch> AccentSwatchesColors { get; private set; }
        public Swatch CurrentPrimary
        {
            get => paletteHelper.QueryPalette().PrimarySwatch;
            set => paletteHelper.ReplacePrimaryColor(value);
        }
        public Swatch CurrentAccent
        {
            get => paletteHelper.QueryPalette().AccentSwatch;
            set => paletteHelper.ReplaceAccentColor(value);
        }

        private SwatchesProvider swatchesProvider;
        private PaletteHelper paletteHelper;

        public DelegateCommand SetColorDarkModeCommand { get; private set; }
        public DelegateCommand SetColorLightModeCommand { get; private set; }

        public SettingsPageViewModel()
        {
            paletteHelper = new PaletteHelper();
            swatchesProvider = new SwatchesProvider();

            PrimarySwatchesColors = new ObservableCollection<Swatch>(swatchesProvider.Swatches);
            foreach (var sw in swatchesProvider.Swatches)
                if(!string.IsNullOrEmpty(sw.Name))
                    PrimarySwatchesColors.Add(sw);

            AccentSwatchesColors = new ObservableCollection<Swatch>();
            foreach (var sw in swatchesProvider.Swatches)
                if (sw.IsAccented)
                    if (!string.IsNullOrEmpty(sw.Name))
                        AccentSwatchesColors.Add(sw);

            SetColorDarkModeCommand = new DelegateCommand(() => SetColorMode(true));
            SetColorLightModeCommand = new DelegateCommand(() => SetColorMode(false));
        }

        private void SetColorMode(bool? IsDark)
        {
            if(IsDark.HasValue)
                paletteHelper.SetLightDark(IsDark.Value);
        }
    }
}
