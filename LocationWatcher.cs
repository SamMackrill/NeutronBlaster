using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NeutronBlaster
{
    public class LocationWatcher
    {
        public EventHandler<string> Changed;

        private string currentSystem;

        public string CurrentSystem
        {
            get => currentSystem;
            private set
            {
                currentSystem = value;
                Changed?.Invoke(this, CurrentSystem);
            }
        }

        private string currentLogFile;
        public string CurrentLogFile
        {
            get => currentLogFile;
            private set
            {
                if (value == currentLogFile) return;
                currentLogFile = value;
                position = 0;
            }
        }

        private readonly DirectoryInfo journalFolder;
        private FileSystemSafeWatcher watcher;
        private long position;

        public LocationWatcher(string journalFolderPath)
        {
            journalFolder = new DirectoryInfo(journalFolderPath);
            if (!journalFolder.Exists)
            {
                throw new Exception($"Journal folder not found! {journalFolder}");
            }
        }

        public void StartWatching()
        {
            void OnFileChanged(object sender, FileSystemEventArgs e)
            {
                try
                {
                    watcher.EnableRaisingEvents = false;
                    Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
                    SetLocationFromFile(e.FullPath);
                }

                finally
                {
                    watcher.EnableRaisingEvents = true;
                }
            }

            var logFile = journalFolder.GetFiles("Journal.*.log").OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            position = 0;
            SetLocationFromFile(logFile?.FullName);

            watcher = new FileSystemSafeWatcher
            {
                Path = journalFolder.FullName, 
                NotifyFilter = NotifyFilters.LastWrite, 
                Filter = "Journal.*.log"
            };

            watcher.Changed += OnFileChanged;

            watcher.EnableRaisingEvents = true;
        }

        private void SetLocationFromFile(string filePath)
        {
            if (filePath == null) return;

            CurrentLogFile = filePath;

            var journalEvents = new List<LogEvent>();
            using (var file = File.Open(CurrentLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (position >= file.Length)
                {
                    position = file.Length;
                    return;
                }

                file.Position = position;
                using (var reader = new StreamReader(file))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        journalEvents.Add(JsonSerializer.Deserialize<LogEvent>(line));
                    }
                    position = file.Position; 
                }
            }

            journalEvents = journalEvents.OrderByDescending(j => j.Date).ToList();
            var currentSystemEvent = journalEvents.FirstOrDefault(l => l.EventType == "FSDJump")
                                  ?? journalEvents.FirstOrDefault(l => l.EventType == "Location");
            if (currentSystemEvent == null) return;
            
            CurrentSystem = currentSystemEvent.StarSystem;
        }

    }
}