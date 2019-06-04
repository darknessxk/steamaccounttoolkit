using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SteamAccountToolkit.ViewModels
{
    public class UsersListViewModel : BindableBase
    {
        private IRegionManager _regionManager;
        public DelegateCommand<Classes.SteamUser> ViewUserCommand { get; private set; }
        public DelegateCommand GoToAddUserCommand { get; private set; }

        public UsersListViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            ViewUserCommand = new DelegateCommand<Classes.SteamUser>(ViewUser);
            GoToAddUserCommand = new DelegateCommand(GoToAddUser);
        }

        private void GoToAddUser()
        {
            _regionManager.RequestNavigate("ContentRegion", "AddUser");
        }

        private void ViewUser(Classes.SteamUser user)
        {
            NavigationParameters nav = new NavigationParameters();
            nav.Add("user", user);

            _regionManager.RequestNavigate("ContentRegion", "UserPage", nav);
        }
    }
}
