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

        public string ProfileUrl => SteamId64 == string.Empty ? string.Empty : $"https://steamcommunity.com/profiles/{SteamId64}";

        public BitmapImage ProfileImage
        { 
            get
            {
                var img = new BitmapImage();
                if (SteamId64 == string.Empty)
                {
                    img.BeginInit();
                    img.UriSource = new Uri("pack://application:,,,/SteamAccountToolkit;component/Assets/user_default.jpg");
                    img.EndInit();

                    return img;
                }

                var bData = new WebClient().DownloadData(ProfileIconUrl);
                MemoryStream ms = new MemoryStream(bData);
                img.BeginInit();
                img.StreamSource = ms;
                img.EndInit();
                return img;
            }
        }

        public string SteamGuardCode => SteamGuard.GenerateSteamGuardCode();

        public string PersonaName {
            get
            {
                if (SteamId64 == string.Empty)
                    return User;
                HtmlDocument doc = new HtmlWeb().Load(new Uri(ProfileUrl));
                return doc.DocumentNode.Descendants().Where(n => n.HasClass("actual_persona_name")).First().GetDirectInnerText();
            }
        }

        public string ProfileIconUrl
        {
            get
            {
                if (SteamId64 == string.Empty)
                    return string.Empty;
                HtmlDocument doc = new HtmlWeb().Load(new Uri(ProfileUrl));
                return doc.DocumentNode.Descendants().Where(n => n.HasClass("playerAvatarAutoSizeInner")).First().FirstChild.GetAttributeValue("src", null);
            }
        }
    }
}
