using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class VisitorPassEntity
    {
        #region Properties
        public long LogID { get; set; }
        public string VisitorName { get; set; }
        public string IDNumber { get; set; }
        public int VisitorCardNo { get; set; }
        public int VisitEmpNo { get; set; }
        public DateTime? VisitDate { get; set; }
        public DateTime? VisitTimeIn { get; set; }
        public DateTime? VisitTimeOut { get; set; }
        public string Remarks { get; set; }
        public bool? IsBlock { get; set; }
        public string IsBlockDesc { get; set; }
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
        public string VisitEmpName { get; set; }
        public string VisitEmpFullName { get; set; }
        public string VisitEmpPosition { get; set; }
        public string VisitEmpExtension { get; set; }
        public string VisitEmpCostCenter { get; set; }
        public string VisitEmpCostCenterName { get; set; }
        public string VisitEmpFullCostCenter { get; set; }
        public int VisitEmpSupervisorNo { get; set; }
        public string VisitEmpSupervisorName { get; set; }
        public string VisitEmpSupervisorFullName { get; set; }
        public int VisitEmpManagerNo { get; set; }
        public string VisitEmpManagerName { get; set; }
        public string VisitEmpManagerFullName { get; set; }
        public List<VisitorSwipeEntity> VisitorSwipeList { get; set; }
        public int TotalRecords { get; set; }
        #endregion
    }
}
