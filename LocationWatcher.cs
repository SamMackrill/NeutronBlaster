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

        public LocationWatcher(string journalFolderPath)
        {
            var journalFolder = new DirectoryInfo(journalFolderPath);
            if (!journalFolder.Exists)
            {
                throw new Exception($"Journal folder not found! {journalFolder}");
            }
            
            latestFile = journalFolder.GetFiles("Journal.*.log").OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            if (latestFile != null)
            {
                var journalLines = File.ReadAllLines(latestFile.FullName).ToList();
                journalLines.Reverse();
                var currentSystemEvent = journalLines.FirstOrDefault(l => l.Contains("\"event\":\"FSDJump\""))
                                      ?? journalLines.FirstOrDefault(l => l.Contains("\"event\":\"Location\""));
                var systemMatch = StarSystemMatch.Match(currentSystemEvent);
                if (systemMatch.Success)
                {
                    CurrentSystem = systemMatch.Groups[1].Value;
                }
                if (journalLines.Any(l => l.Contains("\"event\":\"Shutdown\"")))
                {
                    latestFile = null;
                }
            }
        }

        public void Watch()
        {

        }
    }
}