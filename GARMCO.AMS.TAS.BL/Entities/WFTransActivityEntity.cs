using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class WFTransActivityEntity
    {
        #region Properties
        public long WorkflowTransactionID { get; set; }
        public long OTRequestNo { get; set; }
        public int TS_AutoID { get; set; }
        public string WFModuleCode { get; set; }
        public string ActivityCode { get; set; }
        public string NextActivityCode { get; set; }
        public string ActivityDesc1 { get; set; }
        public string ActivityDesc2 { get; set; }
        public string ActivityTypeCode { get; set; }
        public string ActivityTypeDesc { get; set; }
        public int SequenceNo { get; set; }
        public int? SequenceType { get; set; }
        public int? ApprovalType { get; set; }
        public int? ActionRole { get; set; }
        public string ActionMemberCode { get; set; }
        public string ParameterSourceTable { get; set; }
        public string ParameterName { get; set; }
        public string ParameterDataType { get; set; }
        public string ConditionCheckValue { get; set; }
        public string ConditionCheckDataType { get; set; }
        public string EmailSourceName { get; set; }
        public string EmailCCRecipient { get; set; }
        public string EmailCCRecipientType { get; set; }
        public bool? IsCurrent { get; set; }
        public bool? IsCompleted { get; set; }
        public bool? IsFinalAct { get; set; }
        public int? ActStatusID { get; set; }
        public DateTime? RequestSubmissionDate { get; set; }
        public string CreatedByUser { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedByUserEmpNo { get; set; }
        public string CreatedByUserEmpName { get; set; }
        public string LastUpdateUser { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public int? LastUpdateEmpNo { get; set; }
        public string LastUpdateEmpName { get; set; }
        #endregion

        #region Extended Properties
        public DateTime? CompletionDate { get; set; }
        public string StatusDesc { get; set; }
        #endregion
    }
}
