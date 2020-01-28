using System;
using System.Media;
using System.Threading;
using System.Windows;

namespace NeutronBlaster
{
    public class MainWindowViewModel : BaseViewModel
    {

        private readonly Router router;
        private readonly SoundPlayer player;
        private readonly string userProfilePath;

        public MainWindowViewModel()
        {
            player = new SoundPlayer {SoundLocation = @"Resources\Hitting_Metal.wav"};
            player.Load();
            userProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");
            router = new Router($@"{userProfilePath}\Downloads");
        }

        private string currentLocation;
        public string CurrentSystem
        {
            get => currentLocation ?? "Unknown";
            set
            {
                if (Equals(value, currentLocation)) return;
                currentLocation = value;
                OnPropertyChanged();
            }
        }


        private string lastLocationOnRoute;
        public string LastSystemOnRoute
        {
            get => lastLocationOnRoute ?? "Unknown";
            set
            {
                if (Equals(value, lastLocationOnRoute)) return;
                lastLocationOnRoute = value;
                OnPropertyChanged();
                var destination = router.NextDestination(lastLocationOnRoute);
                if (destination != null) TargetSystem = destination;
            }
        }

        private string targetSystem;
        public string TargetSystem
        {
            get => targetSystem ?? "Unknown";
            set
            {
                if (Equals(value, targetSystem)) return;
                targetSystem = value;
                OnPropertyChanged();
                UpdateTarget();
            }
        }

        private Thread UpdateTarget()
        {
            var thread = new Thread(() =>
            {
                Clipboard.SetText(targetSystem);
                player.Play();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return thread;
        }

        public void Begin()
        {
            try
            {
                var logLocation = $@"{userProfilePath}\Saved Games\Frontier Developments\Elite Dangerous";
                var watcher = new LocationWatcher(logLocation, router);
                watcher.CurrentSystemChanged += (sender, l) => CurrentSystem = l;
                watcher.LastSystemOnRouteChanged += (sender, l) => LastSystemOnRoute = l;
                watcher.StartWatching();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}