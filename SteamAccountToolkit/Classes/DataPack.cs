using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace SteamAccountToolkit.Classes
{
    [Serializable]
    public class DataPack
    {
        private static readonly byte[] HashKey =
        {
            0x01, 0x2d, 0x5f, 0xb8, 0x95, 0xc2, 0xd7, 0x2e, 0xe0, 0x1d, 0xf4, 0x9a, 0x98, 0x41, 0xfd, 0xe4, 0x70, 0xfc,
            0xeb, 0x5e, 0xd4, 0x95, 0x2e, 0xda, 0xaa, 0xd4, 0x56, 0x41, 0xd7, 0xed, 0xab, 0x3f
        };

        [Serializable]
        public class DataHeader
        {
            [field: NonSerialized]
            public HashAlgorithm Hash { get; private set; }

            [DataMember] public byte[] Version => Globals.CurrentDataPackVersion;

            [DataMember] public byte[] FileSignature { get; private set; }

            private void InitializeHash()
            {
                Hash = new HMACSHA1(HashKey);
            }

            public DataHeader(byte[] signature)
            {
                InitializeHash();
                FileSignature = Hash.ComputeHash(signature);
            }

            public bool IsVersionValid(byte[] target)
            {
                return Utils.CompareByteArrays(target, Version);
            }

            public bool IsSignatureValid(byte[] target)
            {
                if(Hash == null)
                    InitializeHash();
                return Utils.CompareByteArrays(Hash.ComputeHash(target), FileSignature);
            }
        }

        private byte[] _data;


        [DataMember] public DataHeader Header { get; set; }

        [DataMember] public byte[] IntegrityHash { get; internal set; }

        [DataMember]
        public byte[] Data
        {
            get => _data;
            set => UpdateData(value);
        }

        [field: NonSerialized]
        public HashAlgorithm Hash { get; private set; }

        public DataPack()
        {
            if (Hash == null)
                InitializeHash();
            if (Hash == null)
                throw new Exception("Hash Mechanism isn't ready");
        }


        private void InitializeHash()
        {
            Hash = new HMACSHA1(HashKey);
        }

        public bool IsIntegrityHashValid(byte[] target)
        {
            if (Hash == null)
                InitializeHash();
            return Utils.CompareByteArrays(Hash.ComputeHash(target), IntegrityHash);
        }

        private void UpdateData(byte[] data)
        {
            _data = data;
            IntegrityHash = Hash.ComputeHash(_data);
        }
    }
}