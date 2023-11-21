using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoogleLIA.Models
{
    [Table("Countrylist")]
    public class Countrylist
    {
        [Key]
        public int id { get; set; }
        public string country_name { get; set; }
        public string country_code { get; set; }
    }
}