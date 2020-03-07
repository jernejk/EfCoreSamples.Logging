using System;

namespace EfCoreSamples.Logging.Persistence.Entities
{
    public class Tweet
    {
        public Guid Id { get; set; }
        public string Username {get;set;}
        public string Message { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
