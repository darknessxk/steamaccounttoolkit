using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Threading;

namespace SteamAccountToolkit.Classes
{
    public class Storage
    {

        private readonly Mutex _mutex = new Mutex();

        public Storage()
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
        }

        public string FolderPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"KatsuhiroGG\SteamAccountToolkit\");

        ~Storage()
        {
            _mutex.Dispose();
        }

        public void Save(string path, byte[] data, byte[] signature)
        {
            Save(path, new DataPack
            {
                Data = data,
                Header = new DataPack.DataHeader(signature)
            });
            
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

                if (!pak.IsIntegrityHashValid(pak.Data))
                    throw new Exception("Data is corrupted");
            }

            return pak;
        }


        
    }
}