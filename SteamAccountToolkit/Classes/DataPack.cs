using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace SteamAccountToolkit.Classes
{
    [Serializable]
    public class DataPack
    {
        [Serializable]
        public class DataHeader
        {
            public DataHeader(byte[] signature)
            {
                FileSignature = signature;
            }

            [DataMember] public byte[] Version => Globals.CurrentDataPackVersion;

            [DataMember] public byte[] FileSignature { get; private set; }

            public bool IsVersionValid(byte[] target)
            {
                return Utils.CompareByteArrays(target, Version);
            }

            public bool IsSignatureValid(byte[] target)
            {
                return Utils.CompareByteArrays(target, FileSignature);
            }
        }

        private byte[] _data;

        [NonSerialized] private readonly HashAlgorithm _hashingAlgo;

        public DataPack(HashAlgorithm hashAlgo)
        {
            _hashingAlgo = hashAlgo;
        }

        [DataMember] public DataHeader Header { get; set; }

        [DataMember] public byte[] IntegrityHash { get; internal set; }

        [DataMember]
        public byte[] Data
        {
            get => _data;
            set => UpdateData(value);
        }

        public bool IsIntegrityHashValid(byte[] target)
        {
            return Utils.CompareByteArrays(target, IntegrityHash);
        }

        private void UpdateData(byte[] data)
        {
            _data = data;
            IntegrityHash = _hashingAlgo.ComputeHash(_data);
        }
    }
}