using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;

namespace SteamAccountToolkit.Classes
{
    [Serializable]
    public class SteamUser
    {
        [DataMember]
        public SteamAuth.UserLogin User { get; set; }

        [DataMember]
        public string AuthKey { get; set; } = string.Empty;

        public SteamAuth.SteamGuardAccount SteamGuard { get; private set; }

        public string SteamGuardCode => SteamGuard.GenerateSteamGuardCode();

        public string ProfileUrl => User.SteamID == 0 ? string.Empty : $"https://steamcommunity.com/profiles/{User.SteamID}";

        private HtmlDocument _profileDocument;

        private BitmapImage _profileImage;
        public BitmapImage ProfileImage
        { 
            get
            {
                if (_profileImage == null)
                    UpdateImage();

                return _profileImage;
            }
        }

        private string _personaName;
        public string PersonaName {
            get
            {
                if (_profileDocument == null)
                    Initialize();
                if (User.SteamID == 0)
                    _personaName = User.Username;
                else
                    _personaName = _profileDocument.DocumentNode.Descendants().Where(n => n.HasClass("actual_persona_name")).First().GetDirectInnerText();
                return _personaName;
            }
        }

        private string _profileIconUrl;
        public string ProfileIconUrl
        {
            get
            {
                if (_profileDocument == null)
                    Initialize();
                if (string.IsNullOrEmpty(_profileIconUrl))
                    _profileIconUrl = _profileDocument.DocumentNode.Descendants().Where(n => n.HasClass("playerAvatarAutoSizeInner")).First().FirstChild.GetAttributeValue("src", null);
                return _profileIconUrl;
            }
        }

        public void UpdateImage()
        {
            _profileImage = new BitmapImage();
            if (User.SteamID == 0)
            {
                _profileImage.BeginInit();
                _profileImage.UriSource = new Uri("pack://application:,,,/SteamAccountToolkit;component/Assets/user_default.jpg");
                _profileImage.EndInit();
            }
            else
            {
                var bData = new WebClient().DownloadData(ProfileIconUrl);
                MemoryStream ms = new MemoryStream(bData);
                _profileImage.BeginInit();
                _profileImage.StreamSource = ms;
                _profileImage.EndInit();
            }
        }

        public void Initialize()
        {
            SteamGuard = new SteamAuth.SteamGuardAccount { SharedSecret = AuthKey.ToString() };
            GetUserInfos();
            _profileDocument = new HtmlWeb().Load(new Uri(ProfileUrl));
        }

        public void GetUserInfos()
        {
            if (User.DoLogin() == SteamAuth.LoginResult.Need2FA)
            {
                User.TwoFactorCode = SteamGuardCode;
                var res = User.DoLogin();
                if (res == SteamAuth.LoginResult.LoginOkay)
                {
                    User.SteamID = User.Session.SteamID;
                    SteamGuard.Session = User.Session;
                }
                else
                    throw new Exception($"Login Error: {res}");

                User.TwoFactorCode = "";
                System.Diagnostics.Debugger.Break();
            }
        }

        public SteamUser()
        {
            User = new SteamAuth.UserLogin("", "");
            Initialize();
        }
    }
}
