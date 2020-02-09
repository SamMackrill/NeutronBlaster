using System;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace NeutronBlaster
{
    public class SettingsViewModel: BaseViewModel
    {
        public event PropertyChangedEventHandler JournalLocationChanged;
        public event PropertyChangedEventHandler RouteLocationChanged;
        public event PropertyChangedEventHandler ClipboadSetSoundChanged;

        private readonly SettingsManager<Settings> settingsManager;
        private readonly MainWindowViewModel main;
        private readonly string userProfilePath;

        public SettingsViewModel(MainWindowViewModel main, SettingsManager<Settings> settingsManager)
        {
            this.main = main;
            this.settingsManager = settingsManager;
            userProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");

            ValidateJournalLocation();
            ValidateRouteLocation();
        }

        public string Title => $"Neutron Blaster Settings V{main.Version}";

        private void ValidateJournalLocation()
        {
            if (!string.IsNullOrWhiteSpace(JournalLocation) && Directory.Exists(JournalLocation)) return;
            ResetJournalLocation();
        }

        private void ResetJournalLocation()
        {
            JournalLocation = $@"{userProfilePath}\Saved Games\Frontier Developments\Elite Dangerous";
        }

        private void ValidateRouteLocation()
        {
            if (!string.IsNullOrWhiteSpace(RouteLocation) && Directory.Exists(RouteLocation)) return;
            ResetRouteLocation();
        }

        private void ResetRouteLocation()
        {
            RouteLocation = $@"{userProfilePath}\Downloads";
        }


        public string JournalLocation
        {
            get => settingsManager.Settings.JournalLocation;
            set
            {
                if (settingsManager.Settings.JournalLocation == value) return;
                settingsManager.Settings.JournalLocation = value;
                OnPropertyChanged();
                JournalLocationChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JournalLocation)));
            }
        }

        public string RouteLocation
        {
            get => App.Settings.RouteLocation;
            set
            {
                if (App.Settings.RouteLocation == value) return;
                App.Settings.RouteLocation = value;
                OnPropertyChanged();
                RouteLocationChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RouteLocation)));
            }
        }

        public string ClipboardSetSound
        {
            get => settingsManager.Settings.ClipboardSetSound;
            set
            {
                if (settingsManager.Settings.ClipboardSetSound == value) return;
                settingsManager.Settings.ClipboardSetSound = value;
                OnPropertyChanged();
                ClipboadSetSoundChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClipboardSetSound)));
            }
        }

        public RelayCommand ChooseJournalLocationCommand => new RelayCommand(() =>
        {
            using var browser = new FolderBrowserDialog
            {
                Description = "Select Location of Elite Dangerous Journal Files", 
                SelectedPath = JournalLocation
            };
            var result = browser.ShowDialog();

            if (result != DialogResult.OK || string.IsNullOrWhiteSpace(browser.SelectedPath)) return;

            JournalLocation = browser.SelectedPath;
        });

        public RelayCommand ResetJournalLocationCommand => new RelayCommand(ResetJournalLocation);

        public RelayCommand ChooseRouteLocationCommand => new RelayCommand(() =>
        {
            using var browser = new FolderBrowserDialog
            {
                Description = "Select Location of Neutron Route CSV Files", 
                SelectedPath = RouteLocation
            };
            var result = browser.ShowDialog();

            if (result != DialogResult.OK || string.IsNullOrWhiteSpace(browser.SelectedPath)) return;

            RouteLocation = browser.SelectedPath;
        });

        public RelayCommand ResetRouteLocationCommand => new RelayCommand(ResetRouteLocation);

        public RelayCommand ChooseSoundFileCommand => new RelayCommand(() =>
        {
            using var browser = new OpenFileDialog
            {
                Title = "Select Sound to play when clipboard is set",
                FileName = ClipboardSetSound,
                Filter = "WAV files (*.wav)|*.wav",
                DefaultExt = ".wav",
                CheckFileExists = true
            };
            var result = browser.ShowDialog();

            if (result != DialogResult.OK || string.IsNullOrWhiteSpace(browser.FileName)) return;

            ClipboardSetSound = browser.FileName;
        });

        public RelayCommand ResetClipboardSoundCommand => new RelayCommand(() =>
        {
            ClipboardSetSound = settingsManager.DefaultSettings.ClipboardSetSound;
        });

        public RelayCommand PlayClipboardSoundCommand => new RelayCommand(() =>
        {
            var player = new SoundPlayer { SoundLocation = ClipboardSetSound };
            try
            {
                player.Play();
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Cannot play: {player.SoundLocation} because {exception.Message}");
            }
        });

    }

}
