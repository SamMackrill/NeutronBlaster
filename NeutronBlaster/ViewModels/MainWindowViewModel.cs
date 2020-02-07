using System;
using System.ComponentModel;
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
        private readonly Router router;
        private readonly SoundPlayer player;
        private const string ReleasePath = "https://neutron-blaster.s3.amazonaws.com";
        private readonly SettingsViewModel settings;

        public MainWindowViewModel(SettingsManager<Settings> settingsManager)
        {
            settings = new SettingsViewModel(settingsManager);
            settings.ClipboadSetSoundChanged += SetClipboardSound;
            player = new SoundPlayer { SoundLocation = settings.ClipboardSetSound };
            router = new Router(settings.RouteLocation);
        }

        private void SetClipboardSound(object sender, PropertyChangedEventArgs e)
        {
            if (player == null) return;
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
            var thread = new Thread(() =>
            {
                Clipboard.SetText(text);

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

        public void Begin()
        {
            try
            {
                player.LoadAsync();

                var watcher = new JournalWatcher(settings.JournalLocation, router);
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

        private string updateInformation;
        public string UpdateInformation
        {
            get => updateInformation;
            private set
            {
                if (updateInformation == value) return;
                updateInformation = value;
                OnPropertyChanged();
            }
        }

        private bool updateAvailable;
        public bool UpdateAvailable
        {
            get => updateAvailable;
            private set
            {
                if (updateAvailable == value) return;
                updateAvailable = value;
                OnPropertyChanged();
            }
        }

        public async Task CheckForUpdates(string[] args)
        {

            UpdateAvailable = false;

            // if (options.Contains("U"))
            // {
            //     UpdateInformation = "UpdatesDisabled!";
            //     return;
            // }

            try
            {

                UpdateInformation = "Checking...";

                string latestVersion;
                using (var updateManager = new UpdateManager(ReleasePath, App.ApplicationName))
                {
                    void OnDo(string caller, Action<Version> doAction, Version v = null)
                    {
                        try
                        {
                            doAction(v);
                        }
                        catch (Exception e)
                        {
                            UpdateInformation = $"Error in {caller}: {e.Message}";
                        }
                    }

                    void OnAppUninstall(Version v)
                    {
                        OnDo(GetCaller(), v0 =>
                        {
                            updateManager.RemoveShortcutForThisExe();
                        }, v);
                    }

                    void OnInitialInstall(Version v)
                    {
                        OnDo(GetCaller(), v0 =>
                        {
                            updateManager.CreateShortcutForThisExe();
                        }, v);
                    }

                    void OnAppUpdate(Version v)
                    {
                        OnDo(GetCaller(), v0 =>
                        {
                            updateManager.CreateShortcutForThisExe();
                        }, v);
                    }

                    void OnAppObsoleted(Version v) => OnDo(GetCaller(), v0 =>
                    {
                    }, v);

                    void OnFirstRun() => OnDo(GetCaller(), v0 =>
                    {
                    });

                    SquirrelAwareApp.HandleEvents(
                        onAppUninstall: OnAppUninstall,
                        onInitialInstall: OnInitialInstall,
                        onAppUpdate: OnAppUpdate,
                        onAppObsoleted: OnAppObsoleted,
                        onFirstRun: OnFirstRun
                    );

                    updates = await updateManager.CheckForUpdate();

                    Version = updates.CurrentlyInstalledVersion == null ? "development" : updates.CurrentlyInstalledVersion.Version.ToString();

                    if (!updates.ReleasesToApply.Any())
                    {
                        UpdateInformation = "You are running the latest version.";
                        return;
                    }

                    latestVersion = updates.ReleasesToApply.OrderBy(x => x.Version).LastOrDefault()?.Version.ToString() ?? "Unknown";
                    UpdateInformation = $"Version: {latestVersion} available. Downloading...";

                    await updateManager.DownloadReleases(updates.ReleasesToApply);
                }

                UpdateAvailable = true;
                UpdateInformation = $"Version: {latestVersion} ready";
            }
            catch (Exception e)
            {
                UpdateInformation = $"Error while updating: {e.Message}";
            }
        }

        private UpdateInfo updates;

        public void Dispose()
        {
            UpdateAvailable = false;
            updates = null;
        }

        private static string GetCaller([CallerMemberName] string caller = null)
        {
            return caller;
        }
    }
}