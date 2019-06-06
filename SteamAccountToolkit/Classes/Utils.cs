using System;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;

namespace SteamAccountToolkit.Classes
{
    public static class Utils
    {
        public static void InvokeDispatcherIfRequired(Action callback)
        {
            var d = Application.Current.Dispatcher;
            
            if (d.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                d.Invoke(callback);
            else
                callback.Invoke();
        }

        public static bool CompareByteArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }

        public static byte[] RandomBytes(int count, int rounds = 4)
        {
            var b = new byte[count];

            for (var n = 0; n < count; n++)
            {
                using (var rng = new RNGCryptoServiceProvider())
                {
                    for (var r = 0; r < rounds; r++)
                    {
                        var temp = new byte[1];
                        rng.GetBytes(temp);
                        b[n] = temp[0];
                    }
                }
            }
            return b;
        }
    }
}