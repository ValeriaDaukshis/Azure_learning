using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDb_learning
{
    public class Film
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public DateTime EnderDate { get; set; }
    }
}
