using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;
using Prism.Mvvm;
using SteamAuth;

namespace SteamAccountToolkit.Classes
{
    public class SteamUser : BindableBase
    {
        private bool _initialized;

        private DateTime _lastSaveTime = DateTime.Now;

        private string _personaName;

        private HtmlDocument _profileDocument;

        private string _profileIconUrl;

        private BitmapImage _profileImage = new BitmapImage();

        private byte[] _profileImageCache;

        private bool _requestingData;

        private BitmapImage _userImage;

        public SteamGuardAccount SteamGuard;

        public SteamUser(SerializableSteamUser u)
        {
            User = u;
        }

        public SteamUser()
        {
        }

        public SerializableSteamUser User { get; } = new SerializableSteamUser();

        public string Username
        {
            get => User.Username;
            set => SetProperty(ref User.Username, value);
        }

        public string Password
        {
            get => User.Password;
            set => SetProperty(ref User.Password, value);
        }

        public string AuthKey
        {
            get => User.AuthKey;
            set
            {
                if (SteamGuard != null)
                    SteamGuard.SharedSecret = value;
                SetProperty(ref User.AuthKey, value);
            }
        }

        public string SteamId
        {
            get => User.SteamId;
            set => SetProperty(ref User.SteamId, value);
        }

        public string ProfileUrl => string.IsNullOrEmpty(SteamId)
            ? string.Empty
            : $"https://steamcommunity.com/profiles/{SteamId}";

        public BitmapImage ProfileImage
        {
            get => _profileImage;
            private set => SetProperty(ref _profileImage, value);
        }

        public string PersonaName
        {
            get => _personaName;
            private set => SetProperty(ref _personaName, value);
        }

        public bool IsInitialized
        {
            get => _initialized;
            set => SetProperty(ref _initialized, value);
        }

        public string ProfileIconUrl
        {
            get => _profileIconUrl;
            private set => SetProperty(ref _profileIconUrl, value);
        }

        public bool RequestingLoginData
        {
            get => _requestingData;
            set => SetProperty(ref _requestingData, value);
        }

        public LoginResult LoginResult { get; private set; }

        public UserLogin AuthUser { get; private set; }

        public int RequestType { get; private set; }

        private void Save()
        {
            if (DateTime.Now <= _lastSaveTime.AddSeconds(1)) return;
            _lastSaveTime = DateTime.Now;
            Globals.Steam.SaveUser(this);
        }

        public void UpdateImage()
        {
            if (!string.IsNullOrEmpty(SteamId) && _profileImageCache != null)
            {
                if (_userImage == null)
                {
                    _userImage = new BitmapImage {CacheOption = BitmapCacheOption.OnDemand};
                    _userImage.BeginInit();
                    _userImage.StreamSource = new MemoryStream(_profileImageCache);
                    _userImage.EndInit();
                }

                ProfileImage = _userImage;
            }
            else
            {
                ProfileImage = Globals.DefaultImage;
            }
        }

        public void Instantiate()
        {
            if (!string.IsNullOrEmpty(ProfileUrl))
                _profileDocument = new HtmlWeb().Load(new Uri(ProfileUrl));

            if (string.IsNullOrEmpty(ProfileIconUrl))
                ProfileIconUrl = _profileDocument.DocumentNode.Descendants()
                    .First(n => n.HasClass("playerAvatarAutoSizeInner")).FirstChild.GetAttributeValue("src", null);

            if (!string.IsNullOrEmpty(ProfileIconUrl))
                _profileImageCache = new WebClient().DownloadData(ProfileIconUrl);

            PersonaName = string.IsNullOrEmpty(SteamId)
                ? Username
                : _profileDocument.DocumentNode.Descendants().First(n => n.HasClass("actual_persona_name"))
                    .GetDirectInnerText();

            Utils.InvokeDispatcherIfRequired(UpdateImage);

            IsInitialized = true;
        }

        public void Initialize()
        {
            SteamGuard = new SteamGuardAccount {SharedSecret = AuthKey};

            if (string.IsNullOrEmpty(SteamId))
            {
                AuthUser = new UserLogin(Username, Password);
                PersonaName = "Loading User";
                Task.Run(() =>
                {
                    LoginResult res;
                    while ((res = AuthUser.DoLogin()) != LoginResult.LoginOkay)
                    {
                        LoginResult = res;
                        switch (res)
                        {
                            case LoginResult.GeneralFailure:
                            case LoginResult.BadRSA:
                            case LoginResult.BadCredentials:
                            case LoginResult.TooManyFailedLogins:
                                Console.WriteLine($@"Error {res}");
                                RequestingLoginData = false;
                                return false;
                            case LoginResult.NeedEmail:
                                PersonaName = "Email code required";
                                RequestType = 1;
                                RequestingLoginData = true;
                                while (string.IsNullOrEmpty(AuthUser.EmailCode))
                                    Thread.Sleep(1000);
                                break;
                            case LoginResult.NeedCaptcha:
                                PersonaName = "Captcha required";
                                RequestType = 2;
                                RequestingLoginData = true;
                                while (string.IsNullOrEmpty(AuthUser.CaptchaText))
                                    Thread.Sleep(1000);
                                break;
                            case LoginResult.Need2FA:
                                AuthUser.TwoFactorCode = SteamGuard.GenerateSteamGuardCode();
                                break;
                            case LoginResult.LoginOkay:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    if (res == LoginResult.LoginOkay)
                    {
                        User.SteamId = AuthUser.Session.SteamID.ToString();
                        SteamId = User.SteamId;
                        SteamGuard.Session = AuthUser.Session;
                        Save();
                    }

                    RequestingLoginData = false;
                    return true;
                }).ContinueWith(t =>
                {
                    if (t.Result)
                        Instantiate();
                    else
                        PersonaName = "Error";
                });
            }
            else
            {
                Task.Run(Instantiate);
            }
        }

        [Serializable]
        public class SerializableSteamUser
        {
            [DataMember(EmitDefaultValue = false, IsRequired = true, Name = "AuthKey", Order = 2)]
            public string AuthKey;

            [DataMember(EmitDefaultValue = false, IsRequired = true, Name = "Password", Order = 1)]
            public string Password;

            [DataMember(EmitDefaultValue = true, IsRequired = false, Name = "SteamID", Order = 3)]
            public string SteamId;

            [DataMember(EmitDefaultValue = false, IsRequired = true, Name = "Username", Order = 0)]
            public string Username;
        }
    }
}