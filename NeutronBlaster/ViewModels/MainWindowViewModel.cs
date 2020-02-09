using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.CompilerServices;

using Squirrel;

namespace NeutronBlaster
{
    public class MainWindowViewModel : BaseViewModel
    {
        private Router router;
        private readonly SoundPlayer player;
        private const string ReleasePath = "https://neutron-blaster.s3.amazonaws.com";
        private readonly SettingsViewModel settings;

        public MainWindowViewModel(SettingsManager<Settings> settingsManager)
        {
            settings = new SettingsViewModel(this, settingsManager);
            settings.ClipboadSetSoundChanged += SetClipboardSound;
            settings.JournalLocationChanged += RefreshJournalLocation;
            settings.RouteLocationChanged += RefreshRouteLocation;

            player = new SoundPlayer { SoundLocation = settings.ClipboardSetSound };
            router = new Router(settings.RouteLocation);
        }

        private void RefreshJournalLocation(object sender, PropertyChangedEventArgs e)
        {
            Watch();
        }

        private void RefreshRouteLocation(object sender, PropertyChangedEventArgs e)
        {
            router = new Router(settings.RouteLocation);
            Watch();
        }

        private void SetClipboardSound(object sender, PropertyChangedEventArgs e)
        {
            if (player == null || !File.Exists(settings.ClipboardSetSound)) return;
            player.SoundLocation = settings.ClipboardSetSound;
            try
            {
                player.Play();
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Cannot play: {player.SoundLocation} because {exception.Message}");
            }
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
            if (string.IsNullOrWhiteSpace(text)) return null;

            var thread = new Thread(() =>
            {
                Clipboard.SetText(text);

                if (!File.Exists(player.SoundLocation)) return;
                try
                {
                    player.PlaySync();
                }
                catch (Exception)
                {
                    // swallow bad wav
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return thread;
        }

        private JournalWatcher watcher;

        public void Begin()
        {
            try
            {
                player.LoadAsync();
                Watch();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Begin() {ex.Message}");
            }
        }

        private void Watch()
        {
            CurrentSystem = "";
            LastSystemOnRoute = "";
            TargetSystem = "";
            watcher = new JournalWatcher(settings.JournalLocation, router);
            watcher.CurrentSystemChanged += (sender, l) => CurrentSystem = l;
            watcher.LastSystemOnRouteChanged += (sender, l) => LastSystemOnRoute = l;
            watcher.CommanderChanged += (sender, c) =>
            {
                commander = c;
                OnPropertyChanged(nameof(Title));
            };
            watcher.StartWatching();
        }

        public RelayCommand ShowSettingsCommand => new RelayCommand( () =>
        {
            var settingsView = new SettingsWindow { DataContext = settings };
            settingsView.ShowDialog();
        });

        private string version;
        public string Version
        {
            get => version;
            private set
            {
                if (version == value) return;
                version = value;
                OnPropertyChanged();
            }
        }

        public async Task CheckForUpdates(string[] args)
        {
            using (var updateManager = new UpdateManager(ReleasePath, App.ApplicationName))
            {
                var update = await updateManager.UpdateApp();
                Version = update?.Version.ToString() ?? "0.0.0 (dev)";
            }
        }

    }
}