using System;
using System.CodeDom;
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
            public class Option<T>
            {
                [DataMember] private T _value;

                [field: DataMember]
                public string Id { get; }

                [field: DataMember]
                public string Name { get; }

                public T Value
                {
                    get => _value;
                    set => _value = value;
                }

                public Option(string id, string name)
                {
                    Name = name;
                    Id = id;
                }

                public void Set(T value) => Value = value;
                public T Get() => Value;
            }

            [DataMember] public Option<string> ThemeAccent { get; set; }
            [DataMember] public Option<bool> ThemeIsDark { get; set; }
            [DataMember] public Option<string> ThemeColor { get; set; }
            [DataMember] public Option<bool> EncryptionEnabled { get; set; }
        }

        private readonly Storage _storage;
        private string SettingsPath => Path.Combine(Path.Combine(_storage.FolderPath, ""), "settings.sat");
        private static byte[] FileSignature => Globals.Encoder.GetBytes("Options");

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
                _storage.Save(SettingsPath, ms.ToArray(), FileSignature);
            }
        }

        public SettingsOptions Load()
        {
            if (!File.Exists(SettingsPath))
            {
                return new SettingsOptions
                {
                    EncryptionEnabled = new SettingsOptions.Option<bool>("EncryptionEnabled", "Encryption Enabled") { Value = false },
                    ThemeColor = new SettingsOptions.Option<string>("ThemeColor", "Theme Color") { Value = "deeppurple" },
                    ThemeAccent = new SettingsOptions.Option<string>("ThemeAccent", "Theme Accent") { Value = "deeppurple" },
                    ThemeIsDark = new SettingsOptions.Option<bool>("ThemeIsDark", "Theme Is Dark") { Value = false },
                };
            }

            var pack = _storage.Load(SettingsPath, FileSignature);
            if (pack.Data.Length <= 0) return null;

            using (var ms = new MemoryStream(pack.Data))
            {
                IFormatter f = new BinaryFormatter();

                var objData = f.Deserialize(ms);

                if (objData is SettingsOptions options)
                    return options;
            }

            return null;
        }
    }
}