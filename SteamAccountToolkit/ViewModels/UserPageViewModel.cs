using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Threading;

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

        public DelegateCommand DeleteUserCommand { get; private set; }
        public DelegateCommand CopySteamGuardCommand { get; private set; }
        public DelegateCommand LoginCommand { get; private set; }
        public DelegateCommand EditUserCommand { get; private set; }
        public DelegateCommand GoBackCommand { get; private set; }

        private string _steamGuard;
        public string SteamGuard
        {
            get => _steamGuard;
            set => SetProperty(ref _steamGuard, value);
        }

        private Thread _steamGuardTh;

        private IRegionManager _regionManager;

        public UserPageViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            DeleteUserCommand = new DelegateCommand(DeleteUser);
            CopySteamGuardCommand = new DelegateCommand(CopySteamGuard);
            LoginCommand = new DelegateCommand(Login);
            EditUserCommand = new DelegateCommand(EditUser);
            GoBackCommand = new DelegateCommand(GoBack);

            _steamGuardTh = new Thread(SteamGuardThread);
            _steamGuardTh.Start();
        }

        private int _intervalPerTick = 1000; //ms
        private int _threadTickCount;
        public int ThreadTickCount
        {
            get => _threadTickCount;
            set => SetProperty(ref _threadTickCount, value);
        }

        private int _steamGuardUpdateInterval = 30;
        public int SteamGuardUpdateInterval
        {
            get => _steamGuardUpdateInterval;
            set => SetProperty(ref _steamGuardUpdateInterval, value);
        }
        
        private void SteamGuardThread()
        {
            while(Globals.IsAppRunning)
            {
                if (User == null)
                    Thread.Sleep(100);
                else
                {
                    if (string.IsNullOrEmpty(SteamGuard))
                        SteamGuard = User.SteamGuard.GenerateSteamGuardCode();

                    if (SteamGuardUpdateInterval < ThreadTickCount)
                    {
                        SteamGuard = SteamGuard = User.SteamGuard.GenerateSteamGuardCode();
                        ThreadTickCount = 0;
                    }

                    Thread.Sleep(_intervalPerTick);
                    ThreadTickCount = ThreadTickCount + 1;
                }
            }
        }

        private void GoBack()
        {
            _regionManager.RequestNavigate("ContentRegion", "UsersList");
        }

        private void CopySteamGuard()
        {
            System.Windows.Clipboard.SetText(SteamGuard);
        }

        private void EditUser()
        {
            // W I P
        }

        private void Login()
        {
            Globals.Steam.DoLogin(User);
        }

        private void DeleteUser()
        {
            Globals.Steam.DeleteUser(User);
            _regionManager.RequestNavigate("ContentRegion", "UsersList");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            var user = navigationContext.Parameters["user"] as Classes.SteamUser;
            if (user != null)
                return User != null && User.Username == user.Username;
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
