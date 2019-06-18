using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SteamAccountToolkit.Classes;

namespace SteamAccountToolkit.ViewModels
{
    public class AddUserViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private SteamUser _user;

        public AddUserViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            _user = new SteamUser();

            AddUserCommand = new DelegateCommand(AddUser);
            CancelCommand = new DelegateCommand(Cancel);
        }

        public SteamUser User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public DelegateCommand AddUserCommand { get; }
        public DelegateCommand CancelCommand { get; }

        private void AddUser()
        {
            Globals.Steam.AddNewUser(User);
            _regionManager.RequestNavigate("ContentRegion", "UsersList");
        }

        private void Cancel()
        {
            _regionManager.RequestNavigate("ContentRegion", "UsersList");
        }
    }
}