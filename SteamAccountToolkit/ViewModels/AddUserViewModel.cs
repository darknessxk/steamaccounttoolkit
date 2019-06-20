using System.Windows.Controls;
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

            AddUserCommand = new DelegateCommand<PasswordBox[]>(AddUser);
            CancelCommand = new DelegateCommand(Cancel);
        }

        public SteamUser User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public DelegateCommand<PasswordBox[]> AddUserCommand { get; }
        public DelegateCommand CancelCommand { get; }

        private void AddUser(PasswordBox[] data)
        {
            User.Password = data[0].Password;
            User.AuthKey = data[1].Password;

            Globals.Steam.AddNewUser(User);
            _regionManager.RequestNavigate("ContentRegion", "UsersList");
            User = new SteamUser();
        }

        private void Cancel()
        {
            _regionManager.RequestNavigate("ContentRegion", "UsersList");
        }
    }
}