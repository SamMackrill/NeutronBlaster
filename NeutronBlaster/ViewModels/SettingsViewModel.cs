
using System.Windows.Forms;

namespace NeutronBlaster
{
    public class SettingsViewModel: BaseViewModel
    {

        public string JournalFileLocation
        {
            get => App.Settings.JournalFileLocation;
            set
            {
                if (App.Settings.JournalFileLocation == value) return;
                App.Settings.JournalFileLocation = value;
                OnPropertyChanged();
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
            }
        }

        public RelayCommand ChooseJournalFileLocationCommand => new RelayCommand(() =>
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select Location of Elite Dangerous Journal Files";
                fbd.SelectedPath = JournalFileLocation;
                var result = fbd.ShowDialog();

                if (result != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath)) return;

                JournalFileLocation = fbd.SelectedPath;
            }
        });

        public RelayCommand ChooseRouteLocationCommand => new RelayCommand(() =>
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select Location of Neutron Route CSV Files";
                fbd.SelectedPath = RouteLocation;
                var result = fbd.ShowDialog();

                if (result != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath)) return;

                RouteLocation = fbd.SelectedPath;
            }
        });
    }

}
