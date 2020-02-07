using System;
using System.IO;
using System.Linq;

namespace NeutronBlaster
{
    public class JournalWatcher
    {
        private readonly Router router;
        public EventHandler<string> CurrentSystemChanged;
        public EventHandler<string> LastSystemOnRouteChanged;
        public EventHandler<string> CommanderChanged;

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
                    LastSystemOnRoute = journalScanner.JumpHistory
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

        private string commander;
        public string Commander
        {
            get => commander;
            private set
            {
                commander = value;
                CommanderChanged?.Invoke(this, commander);
            }
        }


        private FileSystemWatcher watcher;
        private readonly JournalScanner journalScanner;

        public JournalWatcher(string journalFolderPath, Router router)
        {
            journalScanner = new JournalScanner(journalFolderPath);
            journalScanner.CurrentSystemChanged += (sender, l) => CurrentSystem = l;
            journalScanner.CommanderChanged += (sender, c) => Commander = c;
            this.router = router;
        }

        void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                watcher.EnableRaisingEvents = false;
                Console.WriteLine($"File: {e.FullPath} {e.ChangeType} {e.Name}");
                if (journalScanner.SetLocationFromFile(e.FullPath))
                {
                    CurrentSystem = journalScanner.CurrentSystem;
                }
            }

            finally
            {
                watcher.EnableRaisingEvents = true;
            }
        }

        public void StartWatching()
        {
            journalScanner.ScanJournals();

            watcher = new FileSystemWatcher
            {
                Path = journalScanner.JournalFolder.FullName,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                Filter = "Journal.*.log"
            };

            watcher.Changed += OnFileChanged;

            watcher.EnableRaisingEvents = true;
        }




    }
}