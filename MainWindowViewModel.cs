using System;
using System.Media;

namespace NeutronBlaster
{
    public class MainWindowViewModel : BaseViewModel
    {

        private readonly DestinationSetter destinationSetter;
        private readonly SoundPlayer player;
        private readonly string userProfilePath;

        public MainWindowViewModel()
        {
            player = new SoundPlayer {SoundLocation = @"Resources\Hitting_Metal.wav"};
            player.Load();
            userProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");
            destinationSetter = new DestinationSetter($@"{userProfilePath}\Downloads");
        }

        private string currentLocation;
        public string CurrentLocation
        {
            get => currentLocation ?? "Unknown";
            set
            {
                if (Equals(value, currentLocation)) return;
                currentLocation = value;
                OnPropertyChanged();
                var destination = destinationSetter.NextDestination(currentLocation);
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
                System.Windows.Clipboard.SetText(targetSystem);
                player.Play();
            }
        }

        public void Begin()
        {
            try
            {
                var watcher = new LocationWatcher($@"{userProfilePath}\Saved Games\Frontier Developments\Elite Dangerous");
                watcher.Changed += OnLocationChanged;
                watcher.StartWatching();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [STAThread]
        private void OnLocationChanged(object sender, string l) => CurrentLocation = l;
    }
}