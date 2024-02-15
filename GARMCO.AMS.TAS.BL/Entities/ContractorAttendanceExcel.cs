using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class ContractorAttendanceExcel
    {
        #region Properties
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public int CPRNo { get; set; }
        public string JobTitle { get; set; }
        public string EmployerName { get; set; }
        public string StatusDesc { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public double WorkHour { get; set; }
        public double NetHour { get; set; }
        public double OvertimeHour { get; set; }
        public DateTime? SwipeDate { get; set; }
        public DateTime? SwipeIn { get; set; }
        public DateTime? SwipeOut { get; set; }
        public string ReaderName { get; set; }
        #endregion
    }
}
