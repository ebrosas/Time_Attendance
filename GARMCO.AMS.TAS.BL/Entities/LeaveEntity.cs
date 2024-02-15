using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class LeaveEntity
    {
        #region Properties      
        public int LeaveEmpNo { get; set; }
        public DateTime? LeaveOpeningDate { get; set; }
        public DateTime? LeaveEndingDate { get; set; }
        public DateTime? LeaveEmpServiceDate { get; set; }
        public string LeaveEntitlement { get; set; }
        public string LeaveTakenAsOfDate { get; set; }
        public string LeaveTakenCurrentYear { get; set; }
        public string LeaveCurrentBal { get; set; }
        public DateTime? HolidayDate { get; set; }
        public string HolidayName { get; set; }
        public string HolidayType { get; set; }
        public string DOW { get; set; }
        #endregion

        #region Extended Properties
        public int AutoID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string LeaveCode { get; set; }
        public string LeaveDesc { get; set; }
        public string LeaveFullName { get; set; }
        public int TotalRecords { get; set; }
        #endregion
    }
}
