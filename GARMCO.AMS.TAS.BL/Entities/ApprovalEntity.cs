using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class ApprovalEntity
    {
        #region Properties
        public int AutoID { get; set; }
        public long OTRequestNo { get; set; }
        public int TS_AutoID { get; set; }
        public DateTime? RequestSubmissionDate { get; set; }
        public bool? AppApproved { get; set; }
        public string AppRemarks { get; set; }
        public int? AppActID { get; set; }
        public int? AppRoutineSeq { get; set; }
        public int AppCreatedBy { get; set; }
        public string AppCreatedName { get; set; }
        public string AppCreatedFullName { get; set; }
        public DateTime? AppCreatedDate { get; set; }
        public int? AppModifiedBy { get; set; }
        public string AppModifiedName { get; set; }
        public DateTime? AppModifiedDate { get; set; }
        public string ApprovalRole { get; set; }
        public string ApproverPosition { get; set; }
        #endregion
    }
}
