using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace SteamAccountToolkit.ViewModels
{
    public class UsersListViewModel : BindableBase
    {
        private IRegionManager _regionManager;
        public DelegateCommand<Classes.SteamUser> ViewUserCommand { get; private set; }
        public DelegateCommand GoToAddUserCommand { get; private set; }
        public DelegateCommand<Classes.SteamUser> SubmitInfoCommand { get; private set; }

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

            _regionManager.RequestNavigate("ContentRegion", "CaptchaSubmitPage", nav);
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
