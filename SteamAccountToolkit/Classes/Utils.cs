using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                if (a[i] != b[i])
                    return false;

            return true;
        }

        public static int PatternFinder(IReadOnlyList<byte> source, byte[] pattern, string mask = null)
        {
            if (mask == null)
            {
                mask = "";
                for (var i = 0; i < pattern.Length; i++)
                    mask = $"{mask}x";
            }

            foreach (var c in mask)
                if (c != '?' || c != 'x' || c != '*')
                    return -1;

            for (var index = 0; index < source.Count; index++)
            {
                var b = source[index];
                for (int sIdx = 0, pos = 0; sIdx < pattern.Length; sIdx++)
                {
                    if (b != pattern[sIdx] && mask[sIdx] != '?') continue;
                    pos++;
                    if (pos == mask.Length)
                        return index;
                }
            }

            return -1;
        }

        public static byte[] RandomBytes(int count, int rounds = 4)
        {
            var b = new byte[count];
            var rng = new RNGCryptoServiceProvider();
            for (var n = 0; n < count; n++)
            {
                for (var r = 0; r < rounds; r++)
                {
                    var temp = new byte[1];
                    rng.GetBytes(temp);
                    b[n] = temp[0];
                }
            }

            return b;
        }
    }
}