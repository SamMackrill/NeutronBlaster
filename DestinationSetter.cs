using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;

namespace NeutronBlaster
{
    public class DestinationSetter
    {
        private List<string> route;
        private SoundPlayer player;
        public DestinationSetter(string routeFolderPath)
        {

            var routeFolder = new DirectoryInfo(routeFolderPath);
            if (!routeFolder.Exists)
            {
                throw new Exception($"Route folder not found! {routeFolder}");
            }

            var routeFile = routeFolder.GetFiles("neutron-*.csv").OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            if (routeFile == null)
            {
                throw new Exception($"No Route file found in {routeFolder}");
            }

            route = File.ReadAllLines(routeFile.FullName).ToList().Skip(1)
                .Select(l => l.Split(',').First().Trim('"')).ToList();

            player = new SoundPlayer();
            player.SoundLocation = "Resources\\Hitting_Metal.wav";
            player.Load();
        }

        public void SetClipWhenLocationChanges(LocationWatcher watcher)
        {
            if (watcher.CurrentSystem != null)
            {
                var destination = NextDestination(watcher.CurrentSystem);
                if (destination == null) return;
                SetClipboard(destination);
                TargetSystem = destination;
            }
        }

        private void SetClipboard(string text)
        {
            System.Windows.Clipboard.SetText(text);
            player.Play();
        }

        public string TargetSystem { get; private set; }

        private string NextDestination(string fromLocation)
        {
            return route.SkipWhile(s => s != fromLocation).ElementAtOrDefault(1);
        }
    }
}