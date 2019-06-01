using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Security;
using ProtoBuf;

namespace SteamAutoLogin
{
    public static class ProtoSerialize
    {
        public static byte[] Serialize<T>(T obj) where T : class
        {
            if (obj == null)
                return new byte[] { };
            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize<T>(stream, obj);
                return stream.ToArray();
            }
        }
        public static T Deserialize<T>(byte[] data) where T : class
        {
            if (data == null)
                return default(T);
            using (var stream = new MemoryStream(data))
            {
                return ProtoBuf.Serializer.Deserialize<T>(stream);
            }
        }
    }

    public class DataStorage
    {
        private Aes CryptoAlgo { get; }
        private HMACSHA1 HashAlgo { get; }

        private string FolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"KatsuhiroGG\SteamAutoLogin\");
        private string UsersPath => Path.Combine(FolderPath, "Users");

        private Encoding Encoder => Encoding.UTF8;

        private ICryptoTransform Decryptor;
        private ICryptoTransform Encryptor;

        public DataStorage()
        {
            CryptoAlgo = AesManaged.Create();
            HashAlgo = new HMACSHA1(Encoder.GetBytes(Properties.Settings.Default.RandomKey_DEV));

            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            CryptoAlgo.Key = HashAlgo.ComputeHash(Encoder.GetBytes(Properties.Settings.Default.UserEncryptionKey_DEV));
            CryptoAlgo.KeySize = 256;
            CryptoAlgo.IV = HashAlgo.ComputeHash(Encoder.GetBytes(Properties.Settings.Default.RandomKey_DEV));
            CryptoAlgo.Mode = CipherMode.CBC;

            Decryptor = CryptoAlgo.CreateDecryptor();
            Encryptor = CryptoAlgo.CreateEncryptor();
        }

        private byte[] RandomBytes(int count, int rounds = 4)
        {
            // for each byte a new rng crypto and seed
            byte[] b = new byte[count];
            
            for(var n = 0; n < count; n++)
            {
                using(RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    for(var r = 0; r < rounds; r ++)
                    {
                        byte[] temp = new byte[1];
                        rng.GetBytes(temp);
                        b[n] = temp[0];
                    }
                }
            }

            return b;
        }

        public void SaveToFile(LoginData steamUser, bool encrypted = false)
        {
            byte[] hashValue = HashAlgo.ComputeHash(Encoder.GetBytes(steamUser.User.ToString()));
            string fileName = $"{BitConverter.ToString(hashValue)}.user";
            using (FileStream fs = new FileStream(Path.Combine(UsersPath, fileName), FileMode.OpenOrCreate, FileAccess.Write))
            {
                if (fs.CanWrite)
                {
                    if(Properties.Settings.Default.Encrypt)
                    {
                        using (CryptoStream cs = new CryptoStream(fs, Encryptor, CryptoStreamMode.Write))
                        {
                            var b = ProtoSerialize.Serialize(steamUser);
                            cs.Write(b, 0, b.Length);
                        }
                    }
                    else
                    {
                        var b = ProtoSerialize.Serialize(steamUser);
                        fs.Write(b, 0, b.Length);
                    }
                }

            }
        }

        public IList<LoginData> LoadFileUsers()
        {
            List<LoginData> loginList = new List<LoginData>();
            Directory.GetFiles(UsersPath).ToList().ForEach(x =>
            {
                if(x.EndsWith(".user"))
                {
                    using(FileStream fs = new FileStream(x, FileMode.Open, FileAccess.Read))
                    {
                        if(fs.CanRead)
                        {
                            if (Properties.Settings.Default.Encrypt)
                            {
                                using (CryptoStream cs = new CryptoStream(fs, Decryptor, CryptoStreamMode.Read))
                                {
                                    var b = new byte[cs.Length];
                                    cs.Read(b, 0, b.Length);

                                    loginList.Add(ProtoSerialize.Deserialize<LoginData>(b));
                                }
                            }
                            else
                            {
                                var b = new byte[fs.Length];
                                fs.Read(b, 0, b.Length);

                                loginList.Add(ProtoSerialize.Deserialize<LoginData>(b));
                            }
                        }
                    }
                }
            });

            return loginList;
        }
    }
}
