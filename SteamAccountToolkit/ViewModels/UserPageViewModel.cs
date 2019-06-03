using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamAccountToolkit.ViewModels
{
    public class UserPageViewModel : BindableBase, INavigationAware
    {
        private Classes.SteamUser _user;
        public Classes.SteamUser User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public UserPageViewModel()
        {

        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            var user = navigationContext.Parameters["user"] as Classes.SteamUser;
            if (user != null)
                return User != null && User.User == user.User;
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var user = navigationContext.Parameters["user"] as Classes.SteamUser;
            if (user != null)
                User = user;
        }
    }
}
