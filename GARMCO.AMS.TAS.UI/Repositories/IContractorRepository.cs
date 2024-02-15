using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using static GARMCO.AMS.TAS.UI.Repositories.ContractorRepository;

namespace GARMCO.AMS.TAS.UI.Repositories
{
    public interface IContractorRepository
    {
        List<object> GetContractorRegistrationLookup();
        Task<DatabaseSaveResult> SaveRegistration(DataAccessType dbAccessType, ContractorRegistryEntity contractorData);
        DatabaseSaveResult InsertUpdateDeleteContractor(DataAccessType dbAccessType, ContractorRegistryEntity contractorData);
        int GetMaxContractorNo();
        ContractorRegistryEntity GetContractorDetails(int contractorNo);
        DatabaseSaveResult SaveContractorLicense(DataAccessType dbAccessType, List<LicenseEntity> licenseList);
        DatabaseSaveResult InsertUpdateDeleteLicense(DataAccessType dbAccessType, List<LicenseEntity> licenseList, int empNo = 0);
        DatabaseSaveResult DeleteContractorLicense(int empNo);
        List<ContractorRegistryEntity> GetDuplicateContractor(string idNumber, DateTime? contractStartDate, DateTime? contractEndDate);
        List<ContractorRegistryEntity> SearchContractors(int? contractorNo, string idNumber, string contractorName, string companyName, string costCenter,
            string jobTitle, string supervisorName, DateTime? contractStartDate, DateTime? contractEndDate);
        EmployeeEntity SearchEmployee(int empNo);
        EmployeeEntity SearchIDCard(int empNo);
        DatabaseSaveResult InsertUpdateDeleteIDCard(DataAccessType dbAccessType, EmployeeEntity employeeData, int empNo = 0);
        bool CheckIfIDCardExist(int empNo);
        List<CardHistoryEntity> GetCardHistory(int empNo);
        DatabaseSaveResult InsertUpdateDeleteIDCardHistory(DataAccessType dbAccessType, List<CardHistoryEntity> cardList, int empNo = 0);
        FormAccessEntity GetUserFormAccess(FormAccessEntity userAcessParam);
        POEntity GetPurchaseOrderDetails(double poNumber);
        List<POEntity> GetPurchaseOrderList(double supplierNo);
    }
}