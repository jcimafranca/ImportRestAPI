using System.Collections.Generic;

namespace ImportRestAPI.Models
{
    public class SummaryOutput
    {
        public string results { get; set; }
        public IList<string> message { get; set; }
        public DataOutput data { get; set; }
    }
}