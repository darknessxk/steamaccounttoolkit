using System.Threading;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SteamAccountToolkit.Classes;

namespace SteamAccountToolkit.ViewModels
{
    public class UserPageViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;

        private readonly int _intervalPerTick = 1000; //ms

        private string _steamGuard;

        private int _steamGuardUpdateInterval = 30;
        private int _threadTickCount;
        private SteamUser _user;

        public UserPageViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            DeleteUserCommand = new DelegateCommand(DeleteUser);
            CopySteamGuardCommand = new DelegateCommand(CopySteamGuard);
            LoginCommand = new DelegateCommand(Login);
            EditUserCommand = new DelegateCommand(EditUser);
            GoBackCommand = new DelegateCommand(GoBack);

            var steamGuardTh = new Thread(SteamGuardThread);
            steamGuardTh.Start();
        }

        public SteamUser User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public DelegateCommand DeleteUserCommand { get; }
        public DelegateCommand CopySteamGuardCommand { get; }
        public DelegateCommand LoginCommand { get; }
        public DelegateCommand EditUserCommand { get; }
        public DelegateCommand GoBackCommand { get; }

        public string SteamGuard
        {
            get => _steamGuard;
            set => SetProperty(ref _steamGuard, value);
        }

        public int ThreadTickCount
        {
            get => _threadTickCount;
            set => SetProperty(ref _threadTickCount, value);
        }

        public int SteamGuardUpdateInterval
        {
            get => _steamGuardUpdateInterval;
            set => SetProperty(ref _steamGuardUpdateInterval, value);
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

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters["user"] is SteamUser user)
                User = user;
        }

        private void SteamGuardThread()
        {
            while (Globals.IsAppRunning)
                if (User == null)
                {
                    Thread.Sleep(100);
                }
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
                    ThreadTickCount += 1;
                }
        }

        private void GoBack()
        {
            _regionManager.RequestNavigate("ContentRegion", "UsersList");
        }

        private void CopySteamGuard()
        {
            Clipboard.SetText(SteamGuard);
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
    }
}