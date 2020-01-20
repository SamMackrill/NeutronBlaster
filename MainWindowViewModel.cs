using System;

namespace NeutronBlaster
{
    public class MainWindowViewModel : BaseViewModel
    {

        private string currentLocation;
        public string CurrentLocation
        {
            get => currentLocation ?? "Unknown";
            set
            {
                if (Equals(value, currentLocation)) return;
                currentLocation = value;
                OnPropertyChanged();
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
            }
        }

        public void Begin()
        {
            try
            {
                var destinationSetter = new DestinationSetter(@"C:\Users\Sam\Downloads");
                var watcher = new LocationWatcher(@"C:\Users\Sam\Saved Games\Frontier Developments\Elite Dangerous");
                destinationSetter.SetClipWhenLocationChanges(watcher);
                CurrentLocation = watcher.CurrentSystem;
                TargetSystem = destinationSetter.TargetSystem;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}