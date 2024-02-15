using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GARMCO.AMS.TAS.UI.Models
{
    [Serializable]
    public class EmployeeEntity
    {
        #region Properties
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
        public string Position { get; set; }
        public string CostCenter { get; set; }
        public string CostCenterName { get; set; }
        public string CustomCostCenter { get; set; }
        public int PayGrade { get; set; }
        public int? SupervisorNo { get; set; }
        public string SupervisorName { get; set; }
        public int? ManagerNo { get; set; }
        public string ManagerName { get; set; }
        public string CompanyName { get; set; }
        public string IDNumber { get; set; }
        public byte[] EmpPhoto { get; set; }
        public string Base64Photo { get; set; }
        public string ImageFileName { get; set; }
        public string ImageFileExt { get; set; }
        public string ImagePath { get; set; }
        public string ImageURL { get; set; }
        public string ImageURLBase64 { get; set; }
        public DateTime? UserActionDate { get; set; }
        public int? UserEmpNo { get; set; }
        public string UserID { get; set; }
        public int RegistryID { get; set; }
        public bool IsContractor { get; set; }
        public bool ExcludePhoto { get; set; }
        public string BloodGroup { get; set; }
        public string BloodGroupDesc { get; set; }
        public string CPRNo { get; set; }
        public string CardNo { get; set; }
        public List<LicenseEntity> LicenseList { get; set; }
        public List<CardHistoryEntity> CardHistoryList { get; set; }

        public string CostCenterFullName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(CostCenter))
                    return string.Format("{0} - {1}", CostCenter, CostCenterName);
                else
                    return CostCenterName;
            }
        }

        public string EmployeeFullName
        {
            get
            {
                if (EmpNo > 0)
                    return string.Format("{0} - {1}", EmpNo, EmpName);
                else
                    return EmpName;
            }
        }

        public string SupervisorFullName
        {
            get
            {
                if (SupervisorNo > 0)
                    return string.Format("{0} - {1}", SupervisorNo, SupervisorName);
                else
                    return SupervisorName;
            }
        }

        public string ManagerFullName
        {
            get
            {
                if (ManagerNo > 0)
                    return string.Format("{0} - {1}", ManagerNo, ManagerName);
                else
                    return ManagerName;
            }
        }        
        #endregion

        #region Public Methods
        public string GetEmployeeFullName()
        {
            if (EmpNo > 0)
                return string.Format("{0} - {1}", EmpNo, EmpName);
            else
                return EmpName;
        }

        public string GetSupervisorFullName()
        {
            if (SupervisorNo > 0)
                return string.Format("{0} - {1}", SupervisorNo, SupervisorName);
            else
                return SupervisorName;
        }

        public string GetManagerFullName()
        {
            if (ManagerNo > 0)
                return string.Format("{0} - {1}", ManagerNo, ManagerName);
            else
                return ManagerName;
        }
        #endregion
    }
}