using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoogleLIA.Models
{
    public class AdGroup
    {
        [Key]
        public int id { get; set; }
        public string Campaign_Name { get; set; }
        public string Ad_Group_Name { get; set; }
        public int Clicks { get; set; }
        public int Impressions { get; set; }
        public float CTR { get; set; }
        public float Avg_CPC { get; set; }
        public float Cost { get; set; }
        public int Conversion { get; set; }
        public float Cost_Conv { get; set; }
        public int Conv_Rate { get; set; }
        public int Bid { get; set; }
        public string Ad_Group_Audiences { get; set; }
    }
}