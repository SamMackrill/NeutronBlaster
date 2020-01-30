﻿using System.IO;

namespace NeutronBlaster
{
    /// <summary>
    /// This class wraps FileSystemEventArgs and RenamedEventArgs objects and detection of duplicate events.
    /// </summary>
    public class DelayedEvent
    {
        public DelayedEvent(FileSystemEventArgs args)
        {
            Delayed = false;
            Args = args;
        }

        public FileSystemEventArgs Args { get; }

        /// <summary>
        /// Only delayed events that are unique will be fired.
        /// </summary>
        public bool Delayed { get; set; }

        public virtual bool IsDuplicate(object obj)
        {
            if (!(obj is DelayedEvent delayedEvent))
                return false; // this is not null so they are different
            var eO1 = Args;
            var reO1 = Args as RenamedEventArgs;
            var eO2 = delayedEvent.Args;
            var reO2 = delayedEvent.Args as RenamedEventArgs;
            // The events are equal only if they are of the same type (reO1 and reO2
            // are both null or NOT NULL) and have all properties equal.        
            // We also eliminate Changed events that follow recent Created events
            // because many apps create new files by creating an empty file and then
            // they update the file with the file content.
            return ((eO1 != null && eO2 != null && eO1.ChangeType == eO2.ChangeType
                && eO1.FullPath == eO2.FullPath && eO1.Name == eO2.Name) &&
                ((reO1 == null && reO2 == null) || (reO1 != null && reO2 != null &&
                reO1.OldFullPath == reO2.OldFullPath && reO1.OldName == reO2.OldName))) ||
                (eO1 != null && eO2 != null && eO1.ChangeType == WatcherChangeTypes.Created
                && eO2.ChangeType == WatcherChangeTypes.Changed
                && eO1.FullPath == eO2.FullPath && eO1.Name == eO2.Name);
        }
    }
}
