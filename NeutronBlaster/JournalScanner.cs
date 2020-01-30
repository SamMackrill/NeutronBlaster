using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace NeutronBlaster
{
    public class JournalScanner
    {
        public EventHandler<string> CurrentSystemChanged;
        public EventHandler<string> CommanderChanged;

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

        private string currentSystem;
        public string CurrentSystem
        {
            get => currentSystem;
            private set
            {
                currentSystem = value;
                CurrentSystemChanged?.Invoke(this, currentSystem);
            }
        }

        public DirectoryInfo JournalFolder { get; set; }

        private long position;
        public List<LogEvent> JumpHistory { get; private set; }
        private int lastJumpCount;


        private string currentLogFile;

        public JournalScanner(string journalFolderPath)
        {
            JournalFolder = new DirectoryInfo(journalFolderPath);
            if (!JournalFolder.Exists)
            {
                throw new Exception($"Journal folder not found! {JournalFolder}");
            }
        }

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

        public void ScanJournals()
        {
            JumpHistory = new List<LogEvent>();
            lastJumpCount = 0;
            var journalFiles = JournalFolder.GetFiles("Journal.*.log").OrderByDescending(f => f.Name);
            foreach (var logFile in journalFiles)
            {
                position = 0;
                if (SetLocationFromFile(logFile.FullName)) break;
            }

            // TODO go back far enough to find last system on route
        }

        public bool SetLocationFromFile(string filePath)
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
                            switch (logEvent.EventType)
                            {
                                case "Commander":
                                {
                                    Console.WriteLine($"Commander found: {logEvent.Name}");
                                    if (Commander == null)
                                    {
                                        Commander = logEvent.Name;
                                    }
                                    else if (logEvent.Name != Commander) return false;
                                    break;
                                }
                                case "FSDJump":
                                case "Location":
                                    JumpHistory.Add(logEvent);
                                    Console.WriteLine($"Jump added: {logEvent}");
                                    break;
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
