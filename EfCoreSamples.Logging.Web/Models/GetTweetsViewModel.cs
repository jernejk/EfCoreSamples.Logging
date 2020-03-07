using EfCoreSamples.Logging.Persistence.Entities;
using System.Collections.Generic;

namespace EfCoreSamples.Logging.Web.Models
{
    public class GetTweetsViewModel
    {
        public IEnumerable<Tweet> Tweets { get; set; }

        public string LogTypeName { get; set; }
        public string LogType { get; set; }
        public string ImagePath { get; internal set; }
    }
}
