using Newtonsoft.Json;

namespace CovidSupport.Api.Models
{
    public class HighlightFilter
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }

        public string FilterAlias { get; set; }
    }
}
