using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPI.Model.EF
{
   public class ActionPlanDetail
    {
        public int ID { get; set; }
        public int ActionPlanID { get; set; }
        [Column("UserID")]
        public int UserID { get; set; }
        public bool Sent { get; set; }
        public bool Seen { get; set; }
        public DateTime CreateTime { get; set; }

        public ActionPlanDetail()
        {
            CreateTime = DateTime.Now;
        }


    }
}
