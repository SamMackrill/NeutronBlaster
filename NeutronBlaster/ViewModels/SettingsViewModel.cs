using System.ComponentModel;
using System.Windows.Forms;

namespace NeutronBlaster
{
    public class SettingsViewModel: BaseViewModel
    {
        public event PropertyChangedEventHandler JournalFileLocationChanged;
        public event PropertyChangedEventHandler RouteLocationChanged;
        public event PropertyChangedEventHandler ClipboadSetSoundChanged;


        public string JournalFileLocation
        {
            get => App.Settings.JournalFileLocation;
            set
            {
                if (App.Settings.JournalFileLocation == value) return;
                App.Settings.JournalFileLocation = value;
                OnPropertyChanged();
                JournalFileLocationChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JournalFileLocation)));
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
            get => App.Settings.ClipboadSetSound;
            set
            {
                if (App.Settings.ClipboadSetSound == value) return;
                App.Settings.ClipboadSetSound = value;
                OnPropertyChanged();
                ClipboadSetSoundChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClipboardSetSound)));
            }
        }

        public RelayCommand ChooseJournalFileLocationCommand => new RelayCommand(() =>
        {
            using var browser = new FolderBrowserDialog
            {
                Description = "Select Location of Elite Dangerous Journal Files", 
                SelectedPath = JournalFileLocation
            };
            var result = browser.ShowDialog();

            if (result != DialogResult.OK || string.IsNullOrWhiteSpace(browser.SelectedPath)) return;

            JournalFileLocation = browser.SelectedPath;
        });

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
    }

}
