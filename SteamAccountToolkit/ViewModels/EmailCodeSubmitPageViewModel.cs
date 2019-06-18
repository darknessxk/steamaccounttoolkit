using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SteamAccountToolkit.Classes;

namespace SteamAccountToolkit.ViewModels
{
    public class EmailCodeSubmitPageViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;

        private string _emailCode;
        private SteamUser _user;

        public EmailCodeSubmitPageViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            SubmitEmailCodeCommand = new DelegateCommand(SubmitCode);
        }

        public SteamUser User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public string EmailCode
        {
            get => _emailCode;
            set => SetProperty(ref _emailCode, value);
        }

        public DelegateCommand SubmitEmailCodeCommand { get; }

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

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void SubmitCode()
        {
            User.AuthUser.EmailCode = EmailCode;
            _regionManager.RequestNavigate("ContentRegion", "UsersList");
        }
    }
}