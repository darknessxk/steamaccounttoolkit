using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SteamAutoLogin
{
    [Serializable]
    public class LoginData
    {
        [DataMember]
        public string User { get; set; }

        [DataMember]
        public string SteamId64 { get; set; } = string.Empty;

        [DataMember]
        public string Pass { get; set; }

        [DataMember]
        public string SteamGuardPrivateKey { get; set; } = string.Empty;
    }
}
