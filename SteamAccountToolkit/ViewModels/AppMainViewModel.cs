using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SteamAccountToolkit.Classes;

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

        public DelegateCommand<string> NavigateCommand { get; }
        public DelegateCommand LoadedCommand { get; }

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
            if (!string.IsNullOrEmpty(path))
                _regionManager.RequestNavigate("ContentRegion", path);
        }
    }
}
