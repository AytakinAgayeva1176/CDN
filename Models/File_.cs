using System;

namespace CDN.Models
{
    public class File_
    {
        public int? id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string uniq_id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string size { get; set; }
        public string path { get; set; }
        public string related_folder { get; set; }
        public string external_link { get; set; }
    }
}
