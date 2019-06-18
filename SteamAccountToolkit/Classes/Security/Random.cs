using System;
using System.Security.Cryptography;
using System.Threading;

namespace SteamAccountToolkit.Classes.Security
{
    public sealed class Random
    {
        private readonly RandomNumberGenerator _rng;
        private readonly Mutex _mutex;

        public Random()
        {
            _rng = new RNGCryptoServiceProvider();
            _mutex = new Mutex(true, $"RNGClass-{GetHashCode()}");
        }

        public void NextBytes(ref byte[] byteArr)
        {
            _mutex.WaitOne(TimeSpan.FromSeconds(10));
            _rng.GetBytes(byteArr);
            _mutex.ReleaseMutex();
        }
    }
}
