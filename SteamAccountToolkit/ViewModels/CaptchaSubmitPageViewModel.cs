using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SteamAccountToolkit.ViewModels
{
    public class CaptchaSubmitPageViewModel : BindableBase, INavigationAware
    {
        private string _captchaCode;
        public string CaptchaCode
        {
            get => _captchaCode;
            set => SetProperty(ref _captchaCode, value);
        }

        private BitmapImage _captchaImage;
        public BitmapImage CaptchaImage
        {
            get => _captchaImage;
            set => SetProperty(ref _captchaImage, value);
        }

        private Classes.SteamUser _user;
        public Classes.SteamUser User
        {
            get => _user;
            set
            {
                SetProperty(ref _user, value);
                var tmp = new BitmapImage();

                Task.Run(() =>
                {
                    return new WebClient().DownloadData($"https://steamcommunity.com/public/captcha.php?gid={value.AuthUser.CaptchaGID}");
                }).ContinueWith(t => {
                    Classes.Utils.InvokeDispatcherIfRequired(() =>
                    {
                        tmp.BeginInit();
                        tmp.StreamSource = new MemoryStream(t.Result);
                        tmp.EndInit();

                        CaptchaImage = tmp;
                    });
                });
            }
        }

        public DelegateCommand SubmitCaptchaCommand { get; private set; }
        private IRegionManager _regionManager;

        public CaptchaSubmitPageViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            SubmitCaptchaCommand = new DelegateCommand(SubmitCaptcha);
        }

        public void SubmitCaptcha()
        {
            User.AuthUser.CaptchaText = CaptchaCode;
            _regionManager.RequestNavigate("ContentRegion", "UsersList");
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var user = navigationContext.Parameters["user"] as Classes.SteamUser;
            if (user != null)
                User = user;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {

            var user = navigationContext.Parameters["user"] as Classes.SteamUser;
            if (user != null)
                return User != null && User.Username == user.Username;
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
