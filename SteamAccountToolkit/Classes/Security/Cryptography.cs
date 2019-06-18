using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SteamAccountToolkit.Classes.Security
{
    public class Cryptography
    {
        private TripleDESCryptoServiceProvider _cipher;
        private HMACSHA256 _hash;
        private static Encoding Encoder => Encoding.UTF8;

        private static void GetIvFromByteArray(IReadOnlyList<byte> target, int ivSize, out byte[] output, out byte[] ivKey)
        {
            ivKey = new byte[ivSize];
            var ivStart = target.Count - ivSize;
            for (var idx = 0; idx < ivSize; idx++)
                ivKey[idx] = target[ivStart + idx];

            output = new byte[ivStart];
            for (var idx = 0; idx < ivStart; idx++)
                output[idx] = target[idx];
        }

        private static void InsertIvIntoByteArray(byte[] target, byte[] ivKey, out byte[] output)
        {
            output = new byte[target.Length + ivKey.Length];
            target.CopyTo(output, 0);
            ivKey.CopyTo(output, target.Length);
        }

        private void Initialize()
        {
            _cipher = new TripleDESCryptoServiceProvider
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 256
            };
        }

        private void SetKey(byte[] key)
        {
            _hash = new HMACSHA256();
            _cipher.Key = _hash.ComputeHash(key);
            _hash.Clear();
        }

        public void Encrypt(byte[] input, byte[] key, out byte[] output)
        {
            Initialize();
            SetKey(key);
            _cipher.GenerateIV();
            var iv = _cipher.IV;

            var ctx = _cipher.CreateEncryptor();

            var encrypted = ctx.TransformFinalBlock(input, 0, input.Length);
            InsertIvIntoByteArray(encrypted, iv, out output);

            _cipher.Clear();
        }

        public void Encrypt(byte[] input, string key, out byte[] output) =>
            Encrypt(input, Encoder.GetBytes(key), out output);

        public void Encrypt(string input, byte[] key, out byte[] output) =>
            Encrypt(Encoder.GetBytes(input), key, out output);

        public void Encrypt(string input, string key, out byte[] output) =>
            Encrypt(Encoder.GetBytes(input), Encoder.GetBytes(key), out output);

        public void Decrypt(byte[] input, byte[] key, out byte[] output)
        {
            Initialize();
            SetKey(key);
            _cipher.GenerateIV();

            GetIvFromByteArray(input, _cipher.IV.Length, out var encrypted, out var ivKey);

            _cipher.IV = ivKey;
            var ctx = _cipher.CreateDecryptor();

            output = ctx.TransformFinalBlock(encrypted, 0, encrypted.Length);

            _cipher.Clear();
        }

        public void Decrypt(byte[] input, string key, out byte[] output) =>
            Decrypt(input, Encoder.GetBytes(key), out output);

        public void Decrypt(string input, byte[] key, out byte[] output) =>
            Decrypt(Encoder.GetBytes(input), key, out output);

        public void Decrypt(string input, string key, out byte[] output) =>
            Decrypt(Encoder.GetBytes(input), Encoder.GetBytes(key), out output);
    }
}