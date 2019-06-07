using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Prism.Regions;
using SteamAccountToolkit.Classes;

namespace SteamAccountToolkit.ViewModels
{
    public class EmailCodeSubmitPageViewModel : BindableBase, INavigationAware
    {
        private SteamUser _user;

        public SteamUser User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        private string _emailCode;
        public string EmailCode
        {
            get => _emailCode;
            set => SetProperty(ref _emailCode, value);
        }

        public DelegateCommand SubmitEmailCodeCommand { get; }
        private readonly IRegionManager _regionManager;

        public EmailCodeSubmitPageViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            SubmitEmailCodeCommand = new DelegateCommand(SubmitCode);
        }

        public void SubmitCode()
        {
            User.AuthUser.EmailCode = EmailCode;
            _regionManager.RequestNavigate("ContentRegion", "UsersList");
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters["user"] is SteamUser user)
                User = user;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters["user"] is SteamUser user)
                return User != null && User.Username == user.Username;
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
