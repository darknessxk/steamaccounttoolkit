using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace SteamAccountToolkit.Classes
{
    public class Storage
    {
        public static byte[] CurrentPackageVersion => new byte[] { 0x1, 0x0, 0x0, 0x01 };
        public static readonly byte[] FileSignatureKey = new byte[] { 0x0f, 0x37, 0x0d, 0x7a, 0xb8, 0x92, 0x8f, 0x9a, 0x5c, 0x69, 0x2f, 0x9d, 0x98, 0x8d, 0xfa, 0x25, 0x22, 0xa1, 0x7e, 0x2b, 0x51, 0x6c, 0x0b, 0x18, 0x35, 0x6c, 0xfa, 0x5a, 0x26, 0x3c, 0x6a, 0x8b };
        public static readonly byte[] HashKey = new byte[] { 0x01, 0x2d, 0x5f, 0xb8, 0x95, 0xc2, 0xd7, 0x2e, 0xe0, 0x1d, 0xf4, 0x9a, 0x98, 0x41, 0xfd, 0xe4, 0x70, 0xfc, 0xeb, 0x5e, 0xd4, 0x95, 0x2e, 0xda, 0xaa, 0xd4, 0x56, 0x41, 0xd7, 0xed, 0xab, 0x3f };

        public HMACSHA1 HashAlgo { get; }
        public HMACSHA1 FileHashAlgo { get; }

        public string FolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"KatsuhiroGG\SteamAccountToolkit\");

        public Storage()
        {
            HashAlgo = new HMACSHA1(HashKey);
            FileHashAlgo = new HMACSHA1(FileSignatureKey);

            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
        }

        ~Storage()
        {
            _mutex.Dispose();
        }

        private readonly Mutex _mutex = new Mutex();

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
                if (fs.Length < CurrentPackageVersion.Length)
                    return null;

                IFormatter f = new BinaryFormatter();
                var b = f.Deserialize(fs);
                if (b is DataPack pack)
                    pak = pack;

                if (pak == null)
                    return null;

                if (!pak.Header.IsVersionValid(CurrentPackageVersion))
                    throw new Exception("Wrong version");

                if (!pak.Header.IsSignatureValid(signature))
                    throw new Exception("Invalid file signature");

                if (!pak.IsIntegrityHashValid(FileHashAlgo.ComputeHash(pak.Data)))
                    throw new Exception("Data is corrupted");
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

            public bool IsVersionValid(byte[] target) => Utils.CompareByteArrays(target, Version);
            public bool IsSignatureValid(byte[] target) => Utils.CompareByteArrays(target, FileSignature);
        }

        [Serializable]
        public class DataPack
        {
            [DataMember]
            public DataHeader Header { get; set; }

            [DataMember]
            public byte[] IntegrityHash { get; internal set; }

            public bool IsIntegrityHashValid(byte[] target) => Utils.CompareByteArrays(target, IntegrityHash);

            private byte[] _data;
            [DataMember]
            public byte[] Data
            {
                get => _data;
                set => UpdateData(value);
            }

            private void UpdateData(byte[] data)
            {
                _data = data;
                IntegrityHash = _hashingAlgo.ComputeHash(_data);
            }

            [NonSerialized]
            HMACSHA1 _hashingAlgo;

            public DataPack(HMACSHA1 hashAlgo)
            {
                _hashingAlgo = hashAlgo;
            }
        }
    }
}
