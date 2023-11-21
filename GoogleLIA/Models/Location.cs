using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoogleLIA.Models
{
    [Table("LocationMapping")]
    public class Location
    {
        [Key]
        public int id { get; set; }
        public string criteria_id { get; set; }
        public string name { get; set; }
        public string canonical_name { get; set; }
        public string parent_id { get; set; }
        public string country_code { get; set; }
        public string target_type { get; set; }
        public string status { get; set; }
    }
}