using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Adviewmodel
    {
        public int pro_id { get; set; }
        public string pro_name { get; set; }
        public string pro_img { get; set; }
        public string pro_descrip { get; set; }
        public int pro_price { get; set; }
        public Nullable<int> pro_fk_cat { get; set; }
        public Nullable<int> pro_fk_usr { get; set; }
        public int cat_id { get; set; }
        public string cat_name { get; set; }
        public string u_name { get; set; }
        public string u_img { get; set; }
        public string u_contact { get; set; }
    }
}