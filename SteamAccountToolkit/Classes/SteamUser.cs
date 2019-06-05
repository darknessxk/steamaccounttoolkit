using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;
using Prism.Mvvm;

namespace SteamAccountToolkit.Classes
{
    public class SteamUser : BindableBase
    {
        [Serializable]
        public class SerializableSteamUser
        {
            [DataMember(EmitDefaultValue = false, IsRequired = true, Name = "Username", Order = 0)]
            public string Username;

            [DataMember(EmitDefaultValue = false, IsRequired = true, Name = "Password", Order = 1)]
            public string Password;

            [DataMember(EmitDefaultValue = false, IsRequired = true, Name = "AuthKey", Order = 2)]
            public string AuthKey;

            [DataMember(EmitDefaultValue = true, IsRequired = false, Name = "SteamID", Order = 3)]
            public string SteamID;
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

        public string SteamID
        {
            get => User.SteamID;
            set => SetProperty(ref User.SteamID, value);
        }

        public SteamAuth.SteamGuardAccount SteamGuard;

        public string SteamGuardCode => SteamGuard.GenerateSteamGuardCode();

        public string ProfileUrl => string.IsNullOrEmpty(SteamID) ? string.Empty : $"https://steamcommunity.com/profiles/{SteamID}";

        private HtmlDocument _profileDocument;

        private BitmapImage _profileImage = new BitmapImage();
        public BitmapImage ProfileImage
        {
            get => _profileImage;
            private set => SetProperty(ref _profileImage, value);
        }

        private string _personaName;
        public string PersonaName {
            get => _personaName;
            private set => SetProperty(ref _personaName, value);
        }

        private bool _initialized = false;
        public bool IsInitialized
        {
            get => _initialized;
            set => SetProperty(ref _initialized, value);
        }
        
        private string _profileIconUrl;
        public string ProfileIconUrl
        {
            get => _profileIconUrl;
            private set => SetProperty(ref _profileIconUrl, value);
        }

        private byte[] ProfileImageCache = null;

        public SteamUser(SerializableSteamUser u)
        {
            User = u;
            Initialize();
        }

        public SteamUser()
        {

        }

        private BitmapImage _userImage;
        public void UpdateImage()
        {
            if (!string.IsNullOrEmpty(SteamID) && ProfileImageCache != null)
            {
                if (_userImage == null)
                {
                    _userImage = new BitmapImage();
                    _userImage.CacheOption = BitmapCacheOption.OnDemand;
                    _userImage.BeginInit();
                    MemoryStream ms = new MemoryStream(ProfileImageCache);
                    _userImage.StreamSource = ms;
                    _userImage.EndInit();
                }

                ProfileImage = _userImage;
            }
            else
                ProfileImage = Globals.DefaultImage;


        }

        private bool _requestingData = false;
        public bool RequestingLoginData
        {
            get => _requestingData;
            set => SetProperty(ref _requestingData, value);
        }

        public SteamAuth.LoginResult LoginResult { get; private set; }

        public void Initialize()
        {
            SteamGuard = new SteamAuth.SteamGuardAccount { SharedSecret = AuthKey.ToString() };
            
            if(string.IsNullOrEmpty(SteamID))
            {
                var User = new SteamAuth.UserLogin(Username, Password);
                PersonaName = "Loading User";
                Task.Run(() =>
                {
                    SteamAuth.LoginResult res;
                    while ((res = User.DoLogin()) != SteamAuth.LoginResult.LoginOkay )
                    {
                        LoginResult = res;
                        switch (res)
                        {
                            case SteamAuth.LoginResult.GeneralFailure:
                            case SteamAuth.LoginResult.BadRSA:
                            case SteamAuth.LoginResult.BadCredentials:
                            case SteamAuth.LoginResult.TooManyFailedLogins:
                                Console.WriteLine($"Error {res}");
                                return;
                            case SteamAuth.LoginResult.NeedEmail:
                                PersonaName = "Email code required";
                                RequestingLoginData = true;
                                while (string.IsNullOrEmpty(User.EmailCode))
                                    System.Threading.Thread.Sleep(1000);
                                break;
                            case SteamAuth.LoginResult.NeedCaptcha:
                                PersonaName = "Captcha required";
                                RequestingLoginData = true;
                                while (string.IsNullOrEmpty(User.CaptchaText))
                                    System.Threading.Thread.Sleep(1000);
                                break;
                            case SteamAuth.LoginResult.Need2FA:
                                User.TwoFactorCode = SteamGuardCode;
                                break;
                        }
                    }
                    if (res == SteamAuth.LoginResult.LoginOkay)
                    {
                        User.SteamID = User.Session.SteamID;
                        SteamID = User.SteamID.ToString();
                        SteamGuard.Session = User.Session;
                    }
                }).ContinueWith(t => {
                    if(!string.IsNullOrEmpty(ProfileUrl))
                        _profileDocument = new HtmlWeb().Load(new Uri(ProfileUrl));

                    if (!string.IsNullOrEmpty(ProfileIconUrl))
                        ProfileIconUrl = _profileDocument.DocumentNode.Descendants().Where(n => n.HasClass("playerAvatarAutoSizeInner")).First().FirstChild.GetAttributeValue("src", null);

                    if(!string.IsNullOrEmpty(ProfileIconUrl))
                        ProfileImageCache = new WebClient().DownloadData(ProfileIconUrl);

                    if (string.IsNullOrEmpty(SteamID))
                        PersonaName = Username;
                    else
                        PersonaName = _profileDocument.DocumentNode.Descendants().Where(n => n.HasClass("actual_persona_name")).First().GetDirectInnerText();

                    Utils.InvokeDispatcherIfRequired(UpdateImage);

                    IsInitialized = true;
                });
            }
        }
    }
}
