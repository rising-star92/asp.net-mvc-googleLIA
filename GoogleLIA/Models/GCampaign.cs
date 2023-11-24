using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoogleLIA.Models
{
    [Table("GoogleCampaign")]
    public class GCampaign
    {
        [Key]
        public int id { get; set; }
        public string campaign_id { get; set; }
        public string campaign_name { get; set; }
        public string campaign_type { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public double budget { get; set; }
        public string location { get; set; }
        public double ctr { get; set; }
        public double cpc { get; set; }
        public int clicks { get; set; }
        public int impressions { get; set; }
        public double cost { get; set; }
        public int conversion { get; set; }
        public string status { get; set; }

        [NotMapped]
        public string country { get; set; }
    }
}