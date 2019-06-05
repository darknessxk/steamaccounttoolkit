using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamAccountToolkit.ViewModels
{
    public class AddUserViewModel : BindableBase
    {
        private Classes.SteamUser _user;
        public Classes.SteamUser User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public DelegateCommand AddUserCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        private IRegionManager _regionManager;
        public AddUserViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            _user = new Classes.SteamUser();

            AddUserCommand = new DelegateCommand(AddUser);
            CancelCommand = new DelegateCommand(Cancel);
        }

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
