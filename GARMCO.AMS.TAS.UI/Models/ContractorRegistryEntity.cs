using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GARMCO.AMS.TAS.UI.Models
{
    public class ContractorRegistryEntity
    {
        #region Properties
        public int registryID { get; set; }

        public int contractorNo { get; set; }
        public string registrationDateStr { get; set; }
        public DateTime? registrationDate { get; set; }
        public string idNumber { get; set; }
        public byte idType { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string companyName { get; set; }
        public int? companyID { get; set; }
        public string companyCRNo { get; set; }
        public double? purchaseOrderNo { get; set; }
        public string jobCode { get; set; }
        public string jobTitle { get; set; }
        public string mobileNo { get; set; }
        public string visitedCostCenter { get; set; }
        public string visitedCostCenterName { get; set; }
        public int supervisorEmpNo { get; set; }
        public string supervisorEmpName { get; set; }
        public string purposeOfVisit { get; set; }
        public string contractStartDateStr { get; set; }
        public DateTime? contractStartDate { get; set; }
        public string contractEndDateStr { get; set; }
        public DateTime? contractEndDate { get; set; }
        public string bloodGroup { get; set; }
        public string bloodGroupDesc { get; set; }
        public string remarks { get; set; }
        public int? workDurationHours { get; set; }
        public int? workDurationMins { get; set; }
        public string companyContactNo { get; set; }
        public DateTime? createdDate { get; set; }
        public int createdByEmpNo { get; set; }
        public string createdByEmpName { get; set; }
        public string createdByUser { get; set; }
        public DateTime? lastUpdatedDate { get; set; }
        public int? lastUpdatedByEmpNo { get; set; }
        public string lastUpdateByEmpName { get; set; }
        public string lastUpdatedByUser { get; set; }
        public List<LicenseEntity> licenseList { get; set; }
        public string contractorFullName { get; set; }
        public string idTypeDesc { get; set; }
        public string cardNo { get; set; }

        public string VisitedDepartment
        {
            get
            {
                return string.Format("{0} - {1}", visitedCostCenter, visitedCostCenterName);
            }
        }

        public string IDTypeDescription
        {
            get
            {
                return idType == 0 ? "CPR" : "Passport" ;
            }
        }

        public string ContractDuration
        {
            get
            {
                if (contractStartDate.HasValue && contractEndDate.HasValue)
                    return string.Format("{0} to {1}", contractStartDate.Value.ToString("dd-MMM-yyyy"), contractEndDate.Value.ToString("dd-MMM-yyyy"));
                else
                    return string.Empty;
            }
        }

        public string IDDetails
        {
            get
            {
                if (idType == 0)
                    return string.Format("{0} (CPR)", idNumber);
                else
                    return string.Format("{0} (Passport)", idNumber);
            }
        }

        public string SupervisorDetails
        {
            get
            {
                if (supervisorEmpNo > 0 && !string.IsNullOrWhiteSpace(supervisorEmpName))
                    return string.Format("{0} - {1}", supervisorEmpNo, supervisorEmpName);
                else
                    return supervisorEmpName;
            }
        }

        public string CompanyFullName
        {
            get
            {
                if (companyID > 0 && !string.IsNullOrWhiteSpace(companyName))
                    return string.Format("{0} (Code: {1})", companyName, companyID);
                else
                    return companyName;
            }
        }

        public string workDuration
        {
            get
            {
                if (workDurationHours > 0 && workDurationMins > 0)
                    return string.Format("{0:D2}:{1:D2}", workDurationHours, workDurationMins);
                else if (workDurationHours == 0 && workDurationMins > 0)
                    return string.Format("00:{0:D2})", workDurationMins);
                else
                    return string.Empty;
            }
        }
        #endregion
    }
}