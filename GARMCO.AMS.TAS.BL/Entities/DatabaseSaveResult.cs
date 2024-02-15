using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class DatabaseSaveResult
    {
        #region Properties
        public int RowsAffected { get; set; }
        public int NewIdentityID { get; set; }
        public bool HasError { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        #endregion

        #region Extended Properties Used in "OT & Meal Voucher Approval Form"
        public int TimesheetRowsAffected { get; set; }
        public int TimesheetExtraRowsAffected { get; set; }
        public int OvertimeRowsAffected { get; set; }
        public int OvertimeRequestRowsAffected { get; set; }
        public long OTRequestNo { get; set; }
        public DateTime? OTStartTime { get; set; }
        public DateTime? OTEndTime { get; set; }
        public string OTType { get; set; }
        public DateTime? RequestSubmissionDate { get; set; }
        #endregion

        #region Workflow Properties
        public bool IsWorkflowCompleted { get; set; }
        public int? CurrentlyAssignedEmpNo { get; set; }
        public string CurrentlyAssignedEmpName { get; set; }
        public string CurrentlyAssignedEmpEmail { get; set; }
        public string EmailSourceName { get; set; }
        public string EmailCCRecipient { get; set; }
        public string EmailCCRecipientType { get; set; }
        #endregion
    }
}
