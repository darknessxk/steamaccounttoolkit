using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamAccountToolkit.ViewModels
{
    public class AppMainViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        private string _title = "Steam Account Toolkit";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public DelegateCommand<string> NavigateCommand { get; private set; }
        public DelegateCommand LoadedCommand { get; private set; }

        public AppMainViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            NavigateCommand = new DelegateCommand<string>(Navigate);
            LoadedCommand = new DelegateCommand(Loaded);
        }

        private void Loaded()
        {
            Globals.Steam.Initialize();
            Navigate("UsersList");
        }

        private void Navigate(string path)
        {
            if (path != null && path != string.Empty)
                _regionManager.RequestNavigate("ContentRegion", path);
        }
    }
}
