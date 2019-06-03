using System;
using System.Linq;
using System.Runtime.Serialization;
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

        public string SteamGuardCode => SteamGuard.GenerateSteamGuardCode();

        public string PersonaName {
            get
            {
                if (SteamId64 == string.Empty)
                    return string.Empty;
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
