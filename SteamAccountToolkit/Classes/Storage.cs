using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace SteamAccountToolkit.Classes
{
    public class Storage
    {
        public static byte[] CurrentPackageVersion => new byte[] { 0x1, 0x0, 0x0, 0x01 };
        public readonly static byte[] FileSignatureKey = new byte[] { 0x0f, 0x37, 0x0d, 0x7a, 0xb8, 0x92, 0x8f, 0x9a, 0x5c, 0x69, 0x2f, 0x9d, 0x98, 0x8d, 0xfa, 0x25, 0x22, 0xa1, 0x7e, 0x2b, 0x51, 0x6c, 0x0b, 0x18, 0x35, 0x6c, 0xfa, 0x5a, 0x26, 0x3c, 0x6a, 0x8b };

        private Aes CryptoAlgo { get; }
        public HMACSHA1 HashAlgo { get; }
        public HMACSHA1 FileHashAlgo { get; }

        public string FolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"KatsuhiroGG\SteamAccountToolkit\");

        private Encoding Encoder => Encoding.UTF8;

        public ICryptoTransform Decryptor { get; private set; }
        public ICryptoTransform Encryptor { get; private set; }

        private byte[] AesIV = {
            12, 24, 32, 64, 10, 20, 40, 50, // 8
            12, 24, 32, 64, 10, 20, 40, 50, // 16
        };

        public Storage()
        {
            CryptoAlgo = AesManaged.Create();
            HashAlgo = new HMACSHA1(Encoder.GetBytes(Properties.Settings.Default.RandomKey_DEV));
            FileHashAlgo = new HMACSHA1(FileSignatureKey);
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

        public void Save(string path, DataPack data)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                if (fs.CanWrite)
                {
                    IFormatter f = new BinaryFormatter();
                    f.Serialize(fs, data);
                }

            }
        }

        public DataPack Load(string path, byte[] signature)
        {
            DataPack pak = null;

            if(File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    if (fs.CanRead)
                    {
                        if (fs.Length < CurrentPackageVersion.Length)
                            return null;

                        IFormatter f = new BinaryFormatter();
                        var b = f.Deserialize(fs);
                        if (b is DataPack)
                            pak = b as DataPack;

                        if (pak.Header.Version != CurrentPackageVersion)
                            throw new Exception("Wrong version");

                        if (pak.Header.FileSignature != signature)
                            throw new Exception("Invalid file signature");

                        if (pak.IntegrityHash != FileHashAlgo.ComputeHash(pak.Data))
                            throw new Exception("Data is corrupted");
                    }
                }
            }
            
            return pak;
        }


        [Serializable]
        public class DataHeader
        {
            [DataMember]
            public byte[] Version => CurrentPackageVersion;

            [DataMember]
            public byte[] FileSignature { get; private set; }

            public DataHeader(byte[] signature)
            {
                FileSignature = signature;
            }
        }

        [Serializable]
        public class DataPack
        {
            [DataMember]
            public DataHeader Header { get; set; }

            [DataMember]
            public byte[] IntegrityHash { get; }

            [DataMember]
            public byte[] Data { get; set; }

            public DataPack(HMACSHA1 HashAlgo)
            {
                IntegrityHash = HashAlgo.ComputeHash(Data);
            }
        }
    }
}
