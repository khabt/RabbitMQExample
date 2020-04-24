using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQExample.Models
{
    public class Email
    {
        [JsonProperty("To")]
        public string To { get; set; }
        [JsonProperty("Subject")]
        public string Subject { get; set; }
        [JsonProperty("Body")]
        public string Body { get; set; }

    }
}
