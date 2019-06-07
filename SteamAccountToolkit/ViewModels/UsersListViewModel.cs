using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace SteamAccountToolkit.ViewModels
{
    public class UsersListViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        public DelegateCommand<Classes.SteamUser> ViewUserCommand { get; }
        public DelegateCommand GoToAddUserCommand { get; }
        public DelegateCommand<Classes.SteamUser> SubmitInfoCommand { get; }

        public UsersListViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            ViewUserCommand = new DelegateCommand<Classes.SteamUser>(ViewUser);
            GoToAddUserCommand = new DelegateCommand(GoToAddUser);
            SubmitInfoCommand = new DelegateCommand<Classes.SteamUser>(SubmitInfo);
        }

        private void SubmitInfo(Classes.SteamUser user)
        {
            var nav = new NavigationParameters
            {
                { "user", user }
            };

            switch (user.RequestType)
            {
                case 1:
                    _regionManager.RequestNavigate("ContentRegion", "EmailCodeSubmitPage", nav);
                    break;
                case 2:
                    _regionManager.RequestNavigate("ContentRegion", "CaptchaSubmitPage", nav);
                    break;
            }
        }

        private void GoToAddUser()
        {
            _regionManager.RequestNavigate("ContentRegion", "AddUser");
        }

        private void ViewUser(Classes.SteamUser user)
        {
            if (!user.IsInitialized)
                return;

            var nav = new NavigationParameters
            {
                { "user", user }
            };

            _regionManager.RequestNavigate("ContentRegion", "UserPage", nav);
        }
    }
}
