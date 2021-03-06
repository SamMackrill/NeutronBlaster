using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NeutronBlaster
{
    public class Router
    {
        private readonly List<string> route;
        public Router(string routeFolderPath)
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
        }

        public string FirstSystem => route.FirstOrDefault();

        public string NextDestination(string fromLocation)
        {
            return route.SkipWhile(s => s != fromLocation).ElementAtOrDefault(1);
        }

        public bool HasSystem(string system)
        {
            return route.Any(r => r == system);
        }
    }
}