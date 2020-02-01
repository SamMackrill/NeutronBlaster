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
                var logLocation = $@"{userProfilePath}\Saved Games\Frontier Developments\Elite Dangerous";
                var watcher = new JournalWatcher(logLocation, router);
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

        public RelayCommand ShowSettingsCommand => new RelayCommand(() =>
        {
                // var settingsContext = new SettingsViewModel(Preferences);
                // var settings = new SettingsWindow
                // {
                //     DataContext = settingsContext
                // };
                // settingsContext.Window = settings;

                // var result = settings.ShowDialog();
                // if (result != true) return;

        });

    }
}