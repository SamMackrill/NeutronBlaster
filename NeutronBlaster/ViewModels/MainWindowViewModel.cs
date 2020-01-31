using System;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeutronBlaster
{
    public class MainWindowViewModel : BaseViewModel
    {

        private readonly Router router;
        private readonly SoundPlayer player;
        private readonly string userProfilePath;
        private SettingsViewModel settings;

        public MainWindowViewModel()
        {
            settings = new SettingsViewModel();
            player = new SoundPlayer {SoundLocation = @"Resources\Hitting_Metal.wav"};
            player.Load();
            router = new Router(settings.RouteLocation);
        }


        private string commander;
        public string Title => $"Neutron Blaster{(commander == null ? "" : $": {commander}")}";

        private string currentLocation;
        public string CurrentSystem
        {
            get => currentLocation ?? "Unknown";
            set
            {
                if (Equals(value, currentLocation)) return;
                currentLocation = value;
                Console.WriteLine($"CurrentSystem: {value}");
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
                Console.WriteLine($"LastSystemOnRoute: {value}");
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
                Console.WriteLine($"TargetSystem: {value}");
                OnPropertyChanged();
                SetClipboard(targetSystem);
            }
        }

        public Thread SetClipboard(string text)
        {
            var thread = new Thread(() =>
            {
                Clipboard.SetText(text);
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
                var watcher = new JournalWatcher(settings.JournalFileLocation, router);
                watcher.CurrentSystemChanged += (sender, l) => CurrentSystem = l;
                watcher.LastSystemOnRouteChanged += (sender, l) => LastSystemOnRoute = l;
                watcher.CommanderChanged += (sender, c) =>
                {
                    commander = c;
                    OnPropertyChanged(nameof(Title));
                };
                watcher.StartWatching();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Begin() {ex.Message}");
            }
        }

        public RelayCommand ShowSettingsCommand => new RelayCommand( () =>
        {
            var settingsView = new SettingsWindow
            {
                DataContext = settings
            };

            var result = settingsView.ShowDialog();
            if (result != true) return;
        });

    }
}