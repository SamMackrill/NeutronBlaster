using System;
using System.IO;
using System.Text.Json;

namespace NeutronBlaster
{
    public class SettingsManager<T> where T : class, new()
    {
        private readonly string fileName;
        private readonly string userFilePath;

        public T Settings { get; private set; }

        public SettingsManager(string fileName)
        {
            this.fileName = fileName;
            userFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), fileName);
        }

        public T Load()
        {
            if (File.Exists(userFilePath))
            {
                Settings = JsonSerializer.Deserialize<T>(File.ReadAllText(userFilePath));
            }
            else
            {
                var applicationFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                if (File.Exists(applicationFilePath))
                {
                    Settings = JsonSerializer.Deserialize<T>(File.ReadAllText(applicationFilePath));
                }
            }

            Settings ??= new T();
            return Settings;
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(Settings);
            File.WriteAllText(userFilePath, json);
        }
    }
}
