using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class ContractorAttendance
    {
        #region Properties
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public string CostCenterFullName { get; set; }
        public int CPRNo { get; set; }
        public string JobTitle { get; set; }
        public string EmployerName { get; set; }
        public int StatusID { get; set; }
        public string StatusDesc { get; set; }
        public int ContractorTypeID { get; set; }
        public string ContractorTypeDesc { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedByNo { get; set; }
        public string CreatedByName { get; set; }
        public string CreatedByFullName { get; set; }
        public DateTime? IDStartDate { get; set; }
        public DateTime? IDEndDate { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public Double RequiredWorkDuration { get; set; }
        public double NetMinutes { get; set; }
        public double NetHour { get; set; }
        public double OvertimeMinutes { get; set; }
        public double OvertimeHour { get; set; }
        public bool HasMultipleSwipe { get; set; }
        public DateTime? SwipeDate { get; set; }
        public DateTime? SwipeTime { get; set; }
        public string SwipeType { get; set; }
        public DateTime? SwipeIn { get; set; }
        public DateTime? SwipeOut { get; set; }
        public string LocationName { get; set; }
        public string ReaderName { get; set; }
        public int TotalRecords { get; set; }
        #endregion
    }
}
