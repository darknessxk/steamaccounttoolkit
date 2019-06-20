using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Prism.Mvvm;

namespace SteamAccountToolkit.Classes
{
    public class Settings
    {
        [Serializable]
        public class SettingsOptions
        {
            [Serializable]
            public class Option<T> : BindableBase
            {
                [DataMember] private T _data;

                [field: DataMember]
                public string Id { get; }

                [field: DataMember]
                public string Name { get; }

                public T Value
                {
                    get => _data;
                    set => SetProperty(ref _data, value);
                }

                public Option(string id, string name, T defaultValue = default(T))
                {
                    Name = name;
                    Id = id;
                    Value = defaultValue;
                }
            }

            [DataMember] public Option<string> ThemeAccent => new Option<string>("ThemeAccent", "Theme Accent", "deeppurple");
            [DataMember] public Option<bool> ThemeIsDark => new Option<bool>("ThemeIsDark", "Theme Is Dark");
            [DataMember] public Option<string> ThemeColor => new Option<string>("ThemeColor", "Theme Color", "deeppurple");
            [DataMember] public Option<bool> EncryptionEnabled => new Option<bool>("EncryptionEnabled", "Encryption Enabled");
        }

        private readonly Storage _storage;
        private string SettingsPath => Path.Combine(Path.Combine(_storage.FolderPath, ""), "settings.sat");

        public Settings(Storage storage)
        {
            _storage = storage;
        }

        public void Save(SettingsOptions opts)
        {
            IFormatter f = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                f.Serialize(ms, opts);
                _storage.Save(SettingsPath, new DataPack(_storage.FileHash)
                {
                    Data = ms.ToArray(),
                    Header = new DataPack.DataHeader(_storage.FileHash.ComputeHash(Globals.Encoder.GetBytes("Options")))
                });
            }
        }

        public SettingsOptions Load()
        {
            if(!File.Exists(SettingsPath))
                return new SettingsOptions();
            else
            {
                var pack = _storage.Load(SettingsPath, _storage.FileHash.ComputeHash(Globals.Encoder.GetBytes("Options")));
                if (pack.Data.Length <= 0) return null;

                using (var ms = new MemoryStream(pack.Data))
                {
                    IFormatter f = new BinaryFormatter();

                    var objData = f.Deserialize(ms);

                    if (objData is SettingsOptions options)
                        return options;
                }
            }

            return null;
        }
    }
}