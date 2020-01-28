using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NeutronBlaster
{
    public class LocationWatcher
    {
        public string CurrentSystem { get; }
        private readonly Regex StarSystemMatch = new Regex("\"StarSystem\":\"([^\"]+)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private FileInfo latestFile;
        private DirectoryInfo journalFolder;
        private FileSystemWatcher watcher;

        public LocationWatcher(string journalFolderPath)
        {
            journalFolder = new DirectoryInfo(journalFolderPath);
            if (!journalFolder.Exists)
            {
                throw new Exception($"Journal folder not found! {journalFolder}");
            }
        }

        public void Watch(Action<string> update)
        {

            void OnFileChanged(object sender, FileSystemEventArgs e)
            {
                Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
                SetLocationFromFile(e.FullPath, update);
            }

            latestFile = journalFolder.GetFiles("Journal.*.log").OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            SetLocationFromFile(latestFile.FullName, update);
            
            watcher = new FileSystemWatcher();

            watcher.Path = journalFolder.FullName;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "Journal.*.log";

            watcher.Changed += OnFileChanged;

            watcher.EnableRaisingEvents = true;
        }

        private void SetLocationFromFile(string filePath, Action<string> update)
        {
            if (filePath == null) return;
            
            var journalLines = File.ReadAllLines(filePath).ToList();
            journalLines.Reverse();
            var currentSystemEvent = journalLines.FirstOrDefault(l => l.Contains("\"event\":\"FSDJump\""))
                                    ?? journalLines.FirstOrDefault(l => l.Contains("\"event\":\"Location\""));
            var systemMatch = StarSystemMatch.Match(currentSystemEvent);
            if (systemMatch.Success)
            {
                update(systemMatch.Groups[1].Value);
            }  
        }

    }
}