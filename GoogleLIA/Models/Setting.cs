using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoogleLIA.Models
{
    [Table("GoogleSetting")]
    public class Setting
    {
        [Key]
        [Column(TypeName = "bigint")]
        public int Id { get; set; }
        public int Merchant_id { get; set; }
        public int Ads_id { get; set; }
        public string Base_url { get; set; }
        public string Shopping_cart {
            get
            {
                string s_cart = "";
                if (isMagento)
                    s_cart += "1,";
                else
                    s_cart += "0,";

                if (isBigcommerce)
                    s_cart += "1,";
                else
                    s_cart += "0,";

                if (isWoocommerce)
                    s_cart += "1,";
                else
                    s_cart += "0,";

                if (isShopify)
                    s_cart += "1";
                else
                    s_cart += "0";

                return s_cart;
            }
            set
            {
                string[] states = value.Replace(" ", "").Split(',');

                isMagento = states[0] == "1";
                isBigcommerce = states[1] == "1";
                isWoocommerce = states[2] == "1";
                isShopify = states[3] == "1";
            }
        }
        public string Status {
            get
            {
                return Enum.GetName(typeof(option), op);
            }
            set
            {
                op = (option)Enum.Parse(typeof(option), value);
            }
        }

        [NotMapped]
        public option op { get; set; }

        [NotMapped]
        public bool isMagento { get; set; }
        [NotMapped]
        public bool isBigcommerce { get; set; }
        [NotMapped]
        public bool isWoocommerce { get; set; }
        [NotMapped]
        public bool isShopify { get; set; }
    }
 
    public enum option
    {
        Enable,
        Disable
    }
}