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
        public string User { get; set; }

        [DataMember]
        public string SteamId64 { get; set; } = string.Empty;

        [DataMember]
        public string Pass { get; set; }

        [DataMember]
        public string AuthKey { get; set; } = string.Empty;

        public SteamAuth.SteamGuardAccount SteamGuard => new SteamAuth.SteamGuardAccount { SharedSecret = AuthKey.ToString() };

        public string SteamGuardCode => SteamGuard.GenerateSteamGuardCode();

        public string ProfileUrl => SteamId64 == string.Empty ? string.Empty : $"https://steamcommunity.com/profiles/{SteamId64}";

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
                if (SteamId64 == string.Empty)
                    _personaName = User;
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
            if (SteamId64 == string.Empty)
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
            _profileDocument = new HtmlWeb().Load(new Uri(ProfileUrl));
        }
    }
}
