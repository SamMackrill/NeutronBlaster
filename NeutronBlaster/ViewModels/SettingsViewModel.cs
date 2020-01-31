
using System.IO;
using System.Windows.Forms;

using NeutronBlaster.Models;

namespace NeutronBlaster
{
    public class SettingsViewModel: BaseViewModel
    {

        public string JournalFileLocation
        {
            get => Settings.Default.JournalFileLocation;
            set
            {
                if (Settings.Default.JournalFileLocation == value) return;
                Settings.Default.JournalFileLocation = value;
                OnPropertyChanged();
            }
        }

        public string RouteLocation
        {
            get => Settings.Default.RouteLocation;
            set
            {
                if (Settings.Default.RouteLocation == value) return;
                Settings.Default.RouteLocation = value;
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
