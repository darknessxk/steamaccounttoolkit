using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SteamAccountToolkit.Classes;

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

        private SteamUser _user;
        public SteamUser User
        {
            get => _user;
            set
            {
                SetProperty(ref _user, value);
                var tmp = new BitmapImage();

                Task.Run(() => new WebClient().DownloadData($"https://steamcommunity.com/public/captcha.php?gid={value.AuthUser.CaptchaGID}")).ContinueWith(t => {
                    Utils.InvokeDispatcherIfRequired(() =>
                    {
                        tmp.BeginInit();
                        tmp.StreamSource = new MemoryStream(t.Result);
                        tmp.EndInit();

                        CaptchaImage = tmp;
                    });
                });
            }
        }

        public DelegateCommand SubmitCaptchaCommand { get; }
        private readonly IRegionManager _regionManager;

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
