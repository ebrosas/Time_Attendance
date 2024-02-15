using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class WorkflowEmailDeliveryEntity
    {
        #region Properties
        public int DeliveryID { get; set; }
        public long OTRequestNo { get; set; }
        public int TS_AutoID { get; set; }
        public int CurrentlyAssignedEmpNo { get; set; }
        public string CurrentlyAssignedEmpName { get; set; }
        public string CurrentlyAssignedEmpEmail { get; set; }
        public string ActivityCode { get; set; }
        public string ActionMemberCode { get; set; }
        public string EmailSourceName { get; set; }
        public string EmailCCRecipient { get; set; }
        public int EmailCCRecipientType { get; set; }
        public bool IsDelivered { get; set; }
        public int CreatedByEmpNo { get; set; }
        public string CreatedByEmpName { get; set; }
        public string CreatedByFullName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? LastUpdateEmpNo { get; set; }
        public string LastUpdateEmpName { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string EmpFullName { get; set; }
        public string Position { get; set; }
        public int PayGrade { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public string CostCenterFullName { get; set; }
        public bool IsProcessedByTimesheet { get; set; }
        public bool IsCorrected { get; set; }
        public int CorrectionType { get; set; }
        public string ShiftPatCode { get; set; }
        public string ShiftCode { get; set; }
        public string ActualShiftCode { get; set; }        
        public DateTime? DT { get; set; }
        public DateTime? OTStartTime { get; set; }
        public DateTime? OTEndTime { get; set; }
        public string OTType { get; set; }
        public string MealVoucherEligibility { get; set; }
        public string CorrectionCode { get; set; }
        public string CorrectionDesc { get; set; }
        public string OTApproved { get; set; }
        public string OTComment { get; set; }
        #endregion
    }
}
