using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NeutronBlaster
{
    public class LogEvent
    {
        [JsonPropertyName("timestamp")]
        public DateTime Date { get; set; }

        [JsonPropertyName("event")]
        public string EventType { get; set; }

        public string StarSystem { get; set; }


        
    }
}
