using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;

namespace NeutronBlaster
{
    [DebuggerDisplay("{" + nameof(DebugDisplay) + ",nq}")]
    public class LogEvent
    {
        [JsonPropertyName("timestamp")]
        public DateTime Date { get; set; }

        [JsonPropertyName("event")]
        public string EventType { get; set; }

        public string StarSystem { get; set; }


        public string DebugDisplay => ToString();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{EventType} {StarSystem} {Date:s}";
        }
    }
}
