using System;
using System.Media;

namespace NeutronBlaster
{
    public class MainWindowViewModel : BaseViewModel
    {

        private DestinationSetter destinationSetter;
        private SoundPlayer player;

        public MainWindowViewModel()
        {
            player = new SoundPlayer();
            player.SoundLocation = "Resources\\Hitting_Metal.wav";
            player.Load();

            destinationSetter =  new DestinationSetter(@"C:\Users\Sam\Downloads");
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
                var watcher = new LocationWatcher(@"C:\Users\Sam\Saved Games\Frontier Developments\Elite Dangerous");
                watcher.Watch((Action<string>)(l => CurrentLocation = l));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}