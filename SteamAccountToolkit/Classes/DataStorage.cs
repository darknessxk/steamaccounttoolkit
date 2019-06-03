using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace SteamAccountToolkit
{
    public class DataStorage
    {
        private Aes CryptoAlgo { get; }
        private HMACSHA1 HashAlgo { get; }

        private string FolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"KatsuhiroGG\SteamAccountToolkit\");
        private string UsersPath => Path.Combine(FolderPath, "Users");

        private Encoding Encoder => Encoding.UTF8;

        private ICryptoTransform Decryptor;
        private ICryptoTransform Encryptor;

        private string FileExtension => ".saluser";

        private byte[] AesIV = {
            12, 24, 32, 64, 10, 20, 40, 50, // 8
            12, 24, 32, 64, 10, 20, 40, 50, // 16
        };

        public DataStorage()
        {
            CryptoAlgo = AesManaged.Create();
            HashAlgo = new HMACSHA1(Encoder.GetBytes(Properties.Settings.Default.RandomKey_DEV));
            SHA256 keyHash = SHA256.Create();

            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            if (!Directory.Exists(UsersPath))
                Directory.CreateDirectory(UsersPath);

            var Hash = keyHash.ComputeHash(Encoder.GetBytes(Properties.Settings.Default.UserEncryptionKey_DEV));

            CryptoAlgo.Key = Hash;
            CryptoAlgo.KeySize = 256;
            CryptoAlgo.IV = AesIV;
            CryptoAlgo.Mode = CipherMode.CBC;

            Decryptor = CryptoAlgo.CreateDecryptor(Hash, AesIV);
            Encryptor = CryptoAlgo.CreateEncryptor(Hash, AesIV);
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

        public void SaveUser(SteamUser user)
        {
            byte[] hashValue = HashAlgo.ComputeHash(Encoder.GetBytes(user.User.ToString()));
            string fileName = $"{BitConverter.ToString(hashValue)}{FileExtension}".Replace("-", string.Empty);

            DeleteUser(user); // in case of a possible updating action lol

            using (FileStream fs = new FileStream(Path.Combine(UsersPath, fileName), FileMode.OpenOrCreate, FileAccess.Write))
            {
                if (fs.CanWrite)
                {
                    IFormatter formatter = new BinaryFormatter();
                    if (Properties.Settings.Default.Encrypt)
                    {
                        using (CryptoStream cs = new CryptoStream(fs, Encryptor, CryptoStreamMode.Write))
                        {
                            formatter.Serialize(cs, user);
                        }
                    }
                    else
                    {
                        formatter.Serialize(fs, user);
                    }
                }

            }
        }

        public void DeleteUser(SteamUser user)
        {
            byte[] hashValue = HashAlgo.ComputeHash(Encoder.GetBytes(user.User.ToString()));
            string fileName = $"{BitConverter.ToString(hashValue)}{FileExtension}".Replace("-", string.Empty);

            if (File.Exists(Path.Combine(UsersPath, fileName)))
                File.Delete(Path.Combine(UsersPath, fileName));
        }

        public IList<SteamUser> LoadUserList()
        {
            List<SteamUser> loginList = new List<SteamUser>();
            Directory.GetFiles(UsersPath).ToList().ForEach(x =>
            {
                if(x.EndsWith(FileExtension))
                {
                    using(FileStream fs = new FileStream(x, FileMode.Open, FileAccess.Read))
                    {
                        if(fs.CanRead)
                        {
                            IFormatter formatter = new BinaryFormatter();
                            if (Properties.Settings.Default.Encrypt)
                            {
                                using (CryptoStream cs = new CryptoStream(fs, Decryptor, CryptoStreamMode.Read))
                                {
                                    loginList.Add(formatter.Deserialize(fs) as SteamUser);
                                }
                            }
                            else
                            {
                                loginList.Add(formatter.Deserialize(fs) as SteamUser);
                            }
                        }
                    }
                }
            });

            return loginList;
        }
    }
}
