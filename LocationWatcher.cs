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
        private FileSystemWatcher watcher;
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
                Console.WriteLine($"File: {e.FullPath} {e.ChangeType} {e.Name}");
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
            lastJumpCount = 0;
            var journalFiles = journalFolder.GetFiles("Journal.*.log").OrderByDescending(f => f.Name);
            foreach(var logFile in journalFiles)
            {
                position = 0;
                if (SetLocationFromFile(logFile.FullName)) break;
            }

            // TODO go back far enough to find last system on route

            watcher = new FileSystemWatcher
            {
                Path = journalFolder.FullName,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                Filter = "Journal.*.log"
            };

            watcher.Changed += OnFileChanged;

            watcher.EnableRaisingEvents = true;
        }

        private List<LogEvent> JumpHistory { get; set; }
        private int lastJumpCount;
        private bool SetLocationFromFile(string filePath)
        {
            Console.WriteLine($"SetLocationFromFile: {filePath ?? "null"}");

            if (filePath == null) return false;

            CurrentLogFile = filePath;

            try
            {
                using (var file = File.Open(CurrentLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    Console.WriteLine($"File position: {position} of {file.Length}");
                    if (position >= file.Length)
                    {
                        position = file.Length;
                        return false;
                    }
                    // TODO check logile is for correct commander
                    file.Position = position;
                    using (var reader = new StreamReader(file))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            Console.WriteLine($"line: {line}");
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            LogEvent logEvent;
                            try
                            {
                                logEvent = JsonSerializer.Deserialize<LogEvent>(line, new JsonSerializerOptions { IgnoreNullValues = true });
                            }
                            catch (System.Exception ex)
                            {
                                Console.WriteLine($"JsonSerializer error: {ex.Message}");
                                continue;
                            }
                            if (logEvent.EventType == "FSDJump" || logEvent.EventType == "Location")
                            {
                                JumpHistory.Add(logEvent);
                                Console.WriteLine($"Jump added: {logEvent}");
                            }
                        }
                        position = file.Position;
                    }


                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error reading log file: {ex.Message}");
                return false;
            }

            Console.WriteLine($"Jumps Found: {JumpHistory.Count}");

            if (JumpHistory.Count == lastJumpCount)
            {
                Console.WriteLine($"No new jumps, skipping");
                return false;
            }

            var currentSystemEvent = JumpHistory.OrderByDescending(j => j.Date).FirstOrDefault();
            if (currentSystemEvent == null)
            {
                Console.WriteLine($"No jumps, skipping");
                return false;
            } 

            lastJumpCount = JumpHistory.Count;
            CurrentSystem = currentSystemEvent.StarSystem;
            return true;
        }

    }
}