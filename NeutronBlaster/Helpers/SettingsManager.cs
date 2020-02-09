using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace NeutronBlaster
{
    public class SettingsManager<T> where T : class, new()
    {
        private readonly string fileName;
        private readonly string userFilePath;

        public T Settings { get; private set; }
        public T DefaultSettings { get; private set; }

        public SettingsManager(string fileName)
        {
            this.fileName = fileName;
            userFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), fileName);
        }

        public async Task<T> Load()
        {
            var applicationFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (File.Exists(applicationFilePath))
            {
                DefaultSettings = await DeserializeAsyncFromFile(applicationFilePath);
            }

            if (File.Exists(userFilePath))
            {
                Settings = await DeserializeAsyncFromFile(userFilePath);
            }
            else
            {
                Settings = await DeserializeAsyncFromFile(applicationFilePath);
            }

            Settings ??= new T();
            return Settings;
        }

        private async Task<T> DeserializeAsyncFromFile(string filePath)
        {
            //return await JsonSerializer.DeserializeAsync<T>(File.ReadAllText(filePath));

            await using var sourceStream = new FileStream(
                path: filePath,
                mode: FileMode.Open,
                access: FileAccess.Read,
                share: FileShare.Read,
                bufferSize: 4096,
                useAsync: true
            );
            return await JsonSerializer.DeserializeAsync<T>(sourceStream);
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(Settings);
            File.WriteAllText(userFilePath, json);
        }

    }
}
