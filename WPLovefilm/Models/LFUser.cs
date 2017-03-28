using System;

namespace WPLovefilm.Models
{
    public class LFUser
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string QueuesUrl { get; set; }
        public string AtHomeUrl { get; set; }
        public string RentedUrl { get; set; }
        public string RatingsUrl { get; set; }
    }
}
