using System;

namespace CDN.Models
{
    public class Folder
    {
        public int? id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string uniq_id { get; set; }
        public string name { get; set; }
        public string access { get; set; }
        public string internal_folder_name{ get; set; }
    }
}
