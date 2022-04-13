using System;

namespace CDN.ViewModels
{
    public class User
    {
        public int id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string uniq_id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string access_credentials { get; set; }
        public string username { get; set; }
        public string token { get; set; }
    }
}