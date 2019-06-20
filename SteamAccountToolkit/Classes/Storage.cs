using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading;

namespace SteamAccountToolkit.Classes
{
    public class Storage
    {
        public static readonly byte[] FileSignatureKey =
        {
            0x0f, 0x37, 0x0d, 0x7a, 0xb8, 0x92, 0x8f, 0x9a, 0x5c, 0x69, 0x2f, 0x9d, 0x98, 0x8d, 0xfa, 0x25, 0x22, 0xa1,
            0x7e, 0x2b, 0x51, 0x6c, 0x0b, 0x18, 0x35, 0x6c, 0xfa, 0x5a, 0x26, 0x3c, 0x6a, 0x8b
        };

        public static readonly byte[] HashKey =
        {
            0x01, 0x2d, 0x5f, 0xb8, 0x95, 0xc2, 0xd7, 0x2e, 0xe0, 0x1d, 0xf4, 0x9a, 0x98, 0x41, 0xfd, 0xe4, 0x70, 0xfc,
            0xeb, 0x5e, 0xd4, 0x95, 0x2e, 0xda, 0xaa, 0xd4, 0x56, 0x41, 0xd7, 0xed, 0xab, 0x3f
        };

        private readonly Mutex _mutex = new Mutex();

        public HashAlgorithm Hash { get; }
        public HashAlgorithm FileHash { get; }

        public Storage()
        {
            Hash = new HMACSHA1(HashKey);
            FileHash = new HMACSHA1(FileSignatureKey);

            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
        }

        public string FolderPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"KatsuhiroGG\SteamAccountToolkit\");

        ~Storage()
        {
            Hash.Dispose();
            FileHash.Dispose();
            _mutex.Dispose();
        }

        public void Save(string path, DataPack data)
        {
            _mutex.WaitOne(10000);
            using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                if (!fs.CanWrite) return;
                IFormatter f = new BinaryFormatter();
                f.Serialize(fs, data);
            }

            _mutex.ReleaseMutex();
        }

        public DataPack Load(string path, byte[] signature)
        {
            DataPack pak = null;

            if (!File.Exists(path)) return null;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (!fs.CanRead) return null;
                if (fs.Length < Globals.CurrentDataPackVersion.Length)
                    return null;

                IFormatter f = new BinaryFormatter();
                var b = f.Deserialize(fs);
                if (b is DataPack pack)
                    pak = pack;

                if (pak == null)
                    return null;

                if (!pak.Header.IsVersionValid(Globals.CurrentDataPackVersion))
                    throw new Exception("Wrong version");

                if (!pak.Header.IsSignatureValid(signature))
                    throw new Exception("Invalid file signature");

                if (!pak.IsIntegrityHashValid(FileHash.ComputeHash(pak.Data)))
                    throw new Exception("Data is corrupted");
            }

            return pak;
        }


        
    }
}