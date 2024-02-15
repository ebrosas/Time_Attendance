using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class VisitorSwipeEntity
    {
        #region Table Properties
        public long SwipeID { get; set; }
        public long LogID { get; set; }
        public DateTime? SwipeDate { get; set; }
        public string SwipeTypeCode { get; set; }
        public DateTime? SwipeIn { get; set; }
        public DateTime? SwipeOut { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int CreatedByEmpNo { get; set; }
        public string CreatedByUserID { get; set; }
        public string CreatedByEmpName { get; set; }
        public string CreatedByFullName { get; set; }
        public string CreatedByEmpEmail { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public int LastUpdateEmpNo { get; set; }
        public string LastUpdateUserID { get; set; }
        public string LastUpdateEmpName { get; set; }
        public string LastUpdateFullName { get; set; }
        public string LastUpdateEmpEmail { get; set; }
        #endregion

        #region Extended Properties
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string SwipeLocation { get; set; }
        public string SwipeType { get; set; }
        public string SwipeTypeDesc { get; set; }
        public int LocationCode { get; set; }
        public string LocationName { get; set; }
        public int ReaderNo { get; set; }
        public string ReaderName { get; set; }
        public string SwipeCode { get; set; }
        public DateTime? SwipeTime { get; set; }
        public int TotalRecords { get; set; }
        #endregion
    }
}
