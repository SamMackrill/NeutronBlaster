using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NeutronBlaster
{
    public class LocationWatcher
    {
        private readonly Router router;
        public EventHandler<string> CurrentSystemChanged;
        public EventHandler<string> LastSystemOnRouteChanged;

        private string currentSystem;
        public string CurrentSystem
        {
            get => currentSystem;
            private set
            {
                currentSystem = value;
                CurrentSystemChanged?.Invoke(this, CurrentSystem);
                if (router.HasSystem(currentSystem)) LastSystemOnRoute = currentSystem;
                if (LastSystemOnRoute == null)
                {
                    LastSystemOnRoute = JumpHistory
                                            .OrderByDescending(s => s.Date)
                                            .FirstOrDefault(s => router.HasSystem(s.StarSystem))?.StarSystem ??
                                        router.FirstSystem;
                }
            }
        }

        private string lastSystemOnRoute;
        public string LastSystemOnRoute
        {
            get => lastSystemOnRoute;
            private set
            {
                lastSystemOnRoute = value;
                LastSystemOnRouteChanged?.Invoke(this, LastSystemOnRoute);
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

        public LocationWatcher(string journalFolderPath, Router router)
        {
            this.router = router;
            journalFolder = new DirectoryInfo(journalFolderPath);
            if (!journalFolder.Exists)
            {
                throw new Exception($"Journal folder not found! {journalFolder}");
            }
        }

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

        public void StartWatching()
        {
            JumpHistory = new List<LogEvent>();
            var logFile = journalFolder.GetFiles("Journal.*.log").OrderByDescending(f => f.Name).FirstOrDefault();
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

        private List<LogEvent> JumpHistory { get; set; }
        private void SetLocationFromFile(string filePath)
        {
            if (filePath == null) return;

            CurrentLogFile = filePath;

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
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        LogEvent logEvent;
                        try
                        {
                           logEvent = JsonSerializer.Deserialize<LogEvent>(line, new JsonSerializerOptions{IgnoreNullValues=true});
                        }
                        catch (System.Exception ex)
                        {
                            continue;
                        }
                        if (logEvent.EventType == "FSDJump" || logEvent.EventType == "Location")
                        {
                            JumpHistory.Add(logEvent);
                        }
                    }
                    position = file.Position; 
                }
            }

            var currentSystemEvent = JumpHistory.OrderByDescending(j => j.Date).FirstOrDefault();
            if (currentSystemEvent == null) return;
            
            CurrentSystem = currentSystemEvent.StarSystem;
        }

    }
}