using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class UserFormAccessEntity
    {
        #region Properties
        public int ApplicationID { get; set; }
        public string ApplicationCode { get; set; }
        public string ApplicationName { get; set; }
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string Position { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public int FormAppID { get; set; }
        public string FormCode { get; set; }
        public string FormName { get; set; }
        public string UserFrmFormCode { get; set; }
        public string UserFrmCRUDP { get; set; }
        public bool HasViewAccess { get; set; }
        public bool HasCreateAccess { get; set; }
        public bool HasUpdateAccess { get; set; }
        public bool HasDeleteAccess { get; set; }
        public bool HasPrintAccess { get; set; }
        public int? CreatedByEmpNo { get; set; }
        public string CreatedByEmpName { get; set; }
        public string CreatedByFullName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? LastUpdatedByEmpNo { get; set; }
        public string LastUpdatedByEmpName { get; set; }
        public string LastUpdatedByFullName { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public bool IsDirty { get; set; }
        public bool FormPublic { get; set; }
        public bool ViewAccessEnable { get; set; }
        public bool CreateAccessEnable { get; set; }
        public bool UpdateAccessEnable { get; set; }
        public bool DeleteAccessEnable { get; set; }
        public bool PrintAccessEnable { get; set; }
        #endregion
    }
}
