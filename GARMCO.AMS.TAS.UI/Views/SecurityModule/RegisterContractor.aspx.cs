using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Models;
using GARMCO.AMS.TAS.UI.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;

namespace GARMCO.AMS.TAS.UI.Views.SecurityModule
{
    [System.Web.Script.Services.ScriptService]
    public partial class RegisterContractor : System.Web.UI.Page
    {
        #region Fields
        static ContractorRepository _repository;
        private const string CONST_SUCCESS = "SUCCESS";
        private const string CONST_FAILED = "FAILED";
        private const string CONST_CARD_EXIST = "CARDEXIST";
        #endregion

        #region Properties
        public string JSVersion { get { return Session[UIHelper.CONST_JS_VERSION].ToString(); } }

        public bool ShowLoadingPanel
        {
            get
            {
                return UIHelper.ConvertObjectToBolean(Session["ShowLoadingPanel"]);
            }
            set
            {
                Session["ShowLoadingPanel"] = value;
            }
        }

        static ContractorRepository Repository
        {
            get
            {
                if (_repository == null)
                    _repository = new ContractorRepository();
                return _repository;
            }
            set
            {
                _repository = value;
            }
        }
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                #region Initialize hidden fields
                this.hidCurrentUserEmpNo.Value = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERID]);
                this.hidCurrentUserID.Value = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USERNAME]);
                this.hidCurrentUserEmpName.Value = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_FULLNAME]);
                this.hidCostCenter.Value = UIHelper.ConvertObjectToString(Session[UIHelper.GARMCO_USER_COST_CENTER]);
                #endregion
            }
        }

        #region Private Methods
        private static void SaveLicense(List<LicenseEntity> licenseList, int empNo)
        {
            try
            {
                DatabaseSaveResult dbResult = Repository.InsertUpdateDeleteLicense(ContractorRepository.DataAccessType.Create, licenseList, empNo);
                if (dbResult != null)
                {
                    if (dbResult.HasError)
                    {
                        throw new Exception(dbResult.ErrorDesc);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void DeleteLicense(int empNo)
        {
            try
            {
                DatabaseSaveResult dbResult = Repository.InsertUpdateDeleteLicense(ContractorRepository.DataAccessType.Delete, null, empNo);
                if (dbResult != null)
                {
                    if (dbResult.HasError)
                    {
                        throw new Exception(dbResult.ErrorDesc);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void DeleteIDCard(int empNo)
        {
            try
            {
                DatabaseSaveResult dbResult = Repository.InsertUpdateDeleteIDCard(ContractorRepository.DataAccessType.Delete, null, empNo);
                if (dbResult != null)
                {
                    if (dbResult.HasError)
                    {
                        throw new Exception(dbResult.ErrorDesc);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void DeleteCardHistory(int empNo)
        {
            try
            {
                DatabaseSaveResult dbResult = Repository.InsertUpdateDeleteIDCardHistory(ContractorRepository.DataAccessType.Delete, null, empNo);
                if (dbResult != null)
                {
                    if (dbResult.HasError)
                    {
                        throw new Exception(dbResult.ErrorDesc);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static List<ContractorRegistryEntity> GetDuplicateContractor(string idNumber, DateTime? contractStartDate, DateTime? contractEndDate)
        {
            try
            {
                List<ContractorRegistryEntity> model = Repository.GetDuplicateContractor(idNumber, contractStartDate, contractEndDate);
                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Web Methods
        [WebMethod]
        public static string GetRegistrationLookup()
        {
            string jsonString = string.Empty;
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

            List<object> list = Repository.GetContractorRegistrationLookup();
            if (list != null)
            {
                jsonString = JsonConvert.SerializeObject(list);     // Convert to JSON using Newton Json.Net DLL
            }

            return jsonString;
        }

        /// <summary>
        /// Search contractor record from the database
        /// </summary>
        /// <param name="contractorNo"></param>
        /// <returns></returns>
        [WebMethod]
        public static string GetContractorDetails(int contractorNo)
        {
            try
            {
                string jsonString = null;
                ContractorRegistryEntity model = Repository.GetContractorDetails(contractorNo);
                if (model != null)
                {
                    jsonString = JsonConvert.SerializeObject(model);
                }

                return jsonString;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetBaseException().Message, ex.GetBaseException());
            }
        }

        /// <summary>
        /// This method fetches the maximum value of Contractor No. field from the database
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public static int GetMaxContractorNo()
        {
            int maxNum = Repository.GetMaxContractorNo();

            if (maxNum == 0)
                maxNum = UIHelper.ConvertObjectToInt(UIHelper.GetConfigurationValue("ContractorStartSequence"));

            return maxNum + 1;
        }

        /// <summary>
        /// This method adds new Contractor record in the database
        /// </summary>
        /// <param name="contractorData"></param>
        /// <returns></returns>
        [WebMethod]
        public static string AddNewContractor(ContractorRegistryEntity contractorData)
        {
            string result = CONST_SUCCESS;

            try
            {
                // Setup the date inputs
                contractorData.registrationDate = UIHelper.ConvertObjectToDate(contractorData.registrationDateStr);
                contractorData.contractStartDate = UIHelper.ConvertObjectToDate(contractorData.contractStartDateStr).Value;
                contractorData.contractEndDate = UIHelper.ConvertObjectToDate(contractorData.contractEndDateStr).Value;
                contractorData.createdDate = DateTime.Now;


                List<ContractorRegistryEntity> duplicateRecords = GetDuplicateContractor(contractorData.idNumber, contractorData.contractStartDate, contractorData.contractEndDate);
                if (duplicateRecords != null && duplicateRecords.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("A matching database records were found with the same ID Number and contract duration that you have specified. Please make sure that the contract duration does not overlaps with an existing record. Details of the duplicate records are as follow:");
                    sb.Append("<br>");
                    duplicateRecords.ForEach(a => sb.AppendLine(string.Format("Contractor No.: {0}, ID Number: <b>{1}</b>, Contractor Name: {2}, Contract Start Date: <b>{3}</b>, Contract End Date: <b>{4}</b>",
                        a.contractorNo, a.idNumber, a.contractorFullName, a.contractStartDate.Value.ToString("dd-MMM-yyyy"), a.contractEndDate.Value.ToString("dd-MMM-yyyy"))));

                    throw new Exception(sb.ToString());
                }
                else
                {
                    DatabaseSaveResult dbResult = Repository.InsertUpdateDeleteContractor(ContractorRepository.DataAccessType.Create, contractorData);
                    if (dbResult != null)
                    {
                        if (dbResult.HasError)
                            result = CONST_FAILED;
                        else
                        {
                            if (contractorData.licenseList != null)
                                SaveLicense(contractorData.licenseList, contractorData.contractorNo);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /// <summary>
        /// This method will update the Contractor record in the database.
        /// </summary>
        /// <param name="contractorData"></param>
        /// <returns></returns>
        [WebMethod]
        public static string UpdateContractor(ContractorRegistryEntity contractorData)
        {
            string result = CONST_SUCCESS;

            try
            {
                // Setup the date inputs
                contractorData.registrationDate = UIHelper.ConvertObjectToDate(contractorData.registrationDateStr);
                contractorData.contractStartDate = UIHelper.ConvertObjectToDate(contractorData.contractStartDateStr).Value;
                contractorData.contractEndDate = UIHelper.ConvertObjectToDate(contractorData.contractEndDateStr).Value;
                contractorData.lastUpdatedDate = DateTime.Now;

                DatabaseSaveResult dbResult = Repository.InsertUpdateDeleteContractor(ContractorRepository.DataAccessType.Update, contractorData);
                if (dbResult != null)
                {
                    if (dbResult.HasError)
                        result = CONST_FAILED;
                    else
                    {
                        if (contractorData.licenseList != null && contractorData.licenseList.Count > 0)
                            SaveLicense(contractorData.licenseList, contractorData.contractorNo);
                        else
                            DeleteLicense(contractorData.contractorNo);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /// <summary>
        /// This method will delete the Contractor record in the database.
        /// </summary>
        /// <param name="contractorData"></param>
        /// <returns></returns>
        [WebMethod]
        public static string DeleteContractor(ContractorRegistryEntity contractorData)
        {
            string result = string.Empty;

            try
            {
                // Setup the date inputs
                contractorData.registrationDate = UIHelper.ConvertObjectToDate(contractorData.registrationDateStr);
                contractorData.contractStartDate = UIHelper.ConvertObjectToDate(contractorData.contractStartDateStr).Value;
                contractorData.contractEndDate = UIHelper.ConvertObjectToDate(contractorData.contractEndDateStr).Value;
                contractorData.lastUpdatedDate = DateTime.Now;

                DatabaseSaveResult dbResult = Repository.InsertUpdateDeleteContractor(ContractorRepository.DataAccessType.Delete, contractorData);
                if (dbResult != null)
                {
                    if (dbResult.HasError)
                        result = CONST_FAILED;
                    else
                    {
                        // Delete associated licenses
                        DeleteLicense(contractorData.contractorNo);

                        // Delete associated ID card record
                        DeleteIDCard(contractorData.contractorNo);

                        // Delete associated card history records
                        DeleteCardHistory(contractorData.contractorNo);

                        // Set the return data
                        result = CONST_SUCCESS;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /// <summary>
        /// Get the Purchase Order information based on the supplied PO Number
        /// </summary>
        /// <param name="poNumber"></param>
        /// <returns></returns>
        [WebMethod]
        public static string GetPurchaseOrderDetails(double poNumber)
        {
            try
            {
                string jsonString = null;
                POEntity model = Repository.GetPurchaseOrderDetails(poNumber);
                if (model != null)
                {
                    jsonString = JsonConvert.SerializeObject(model);
                }

                return jsonString;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetBaseException().Message, ex.GetBaseException());
            }
        }


        /// <summary>
        /// Get purchase order list based on supplier number
        /// </summary>
        /// <param name="supplierNo"></param>
        /// <returns></returns>
        [WebMethod]
        public static string GetPurchaseOrderList(double supplierNo)
        {
            try
            {
                string jsonString = null;
                List<POEntity> model = Repository.GetPurchaseOrderList(supplierNo);
                if (model != null)
                {
                    jsonString = JsonConvert.SerializeObject(model);
                }

                return jsonString;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetBaseException().Message, ex.GetBaseException());
            }
        }
        #endregion
    }
}