using GARMCO.AMS.TAS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using Newtonsoft.Json;
using GARMCO.AMS.TAS.UI.Repositories;
using GARMCO.AMS.TAS.UI.Models;
using GARMCO.AMS.TAS.BL.Entities;
using System.Globalization;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Drawing;

namespace GARMCO.AMS.TAS.UI.WebService
{
    /// <summary>
    /// Summary description for ContractorWS
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class ContractorWS : System.Web.Services.WebService
    {
        #region Fields
        ContractorRepository _repository;
        private const string CONST_SUCCESS = "SUCCESS";
        private const string CONST_FAILED = "FAILED";
        private const string CONST_CARD_EXIST = "CARDEXIST";
        #endregion

        #region Properties
        ContractorRepository Repository
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

        #region Private Methods
        private void SaveLicense(List<LicenseEntity> licenseList, int empNo)
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

        private void DeleteLicense(int empNo)
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

        private List<ContractorRegistryEntity> GetDuplicateContractor(string idNumber, DateTime? contractStartDate, DateTime? contractEndDate)
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

        private void SaveCardHistory(List<CardHistoryEntity> cardList, int empNo)
        {
            try
            {
                DatabaseSaveResult dbResult = Repository.InsertUpdateDeleteIDCardHistory(ContractorRepository.DataAccessType.Create, cardList, empNo);
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

        private void DeleteCardHistory(int empNo)
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
        
        private string ImageToBase64(string imagePath)
        {
            string base64String = null;

            try
            {
                using (Image image = Image.FromFile(imagePath))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.Save(ms, image.RawFormat);
                        byte[] imageBytes = ms.ToArray();
                        base64String = Convert.ToBase64String(imageBytes);
                        ms.Flush();
                    }
                }

                return base64String;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Image Base64ToImage(string base64String)
        {
            Image image = null;

            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);

                using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                {
                    ms.Write(imageBytes, 0, imageBytes.Length);
                    image = Image.FromStream(ms, true);
                    ms.Flush();
                }
                return image;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Public Methods
        [WebMethod]
        public async Task<string> GetRegistrationLookupAsync()
        {
            string jsonString = string.Empty;
            DALProxy bll = new DALProxy();
            List<object> list = await bll.GetContractorRegistrationLookupAsync();

            if (list != null)
                jsonString = (new JavaScriptSerializer()).Serialize(list);

            return jsonString;
        }

        [WebMethod]
        public string GetRegistrationLookup()
        {
            string jsonString = string.Empty;
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

            // Call DBContext using customize connection string
            //string connectionStr = System.Configuration.ConfigurationManager.ConnectionStrings["GARMCOCommon"].ConnectionString.Trim(); 
            //ContractorRepository test = new ContractorRepository(connectionStr);
            //List<object> list = test.GetContractorRegistrationLookup();

            List<object> list = Repository.GetContractorRegistrationLookup();
            if (list != null)
            {
                //jsonString = jsSerializer.Serialize(list);        // Convert to JSON using JavaScriptSerializer
                jsonString = JsonConvert.SerializeObject(list);     // Convert to JSON using Newton Json.Net DLL
            }

            return jsonString;
        }

        /// <summary>
        /// This method fetches the maximum value of Contractor No. field from the database
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public int GetMaxContractorNo()
        {
            int maxNum = Repository.GetMaxContractorNo();

            if (maxNum == 0)
                maxNum = UIHelper.ConvertObjectToInt(UIHelper.GetConfigurationValue("ContractorStartSequence"));

            return maxNum + 1;
        }

        [WebMethod]
        public string GetContractorDetails(int contractorNo)
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
        /// This method adds new Contractor record in the database
        /// </summary>
        /// <param name="contractorData"></param>
        /// <returns></returns>
        [WebMethod]
        public string AddNewContractor(ContractorRegistryEntity contractorData)
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
        public string UpdateContractor(ContractorRegistryEntity contractorData)
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
                        if (contractorData.licenseList != null)
                            SaveLicense(contractorData.licenseList, contractorData.contractorNo);
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
        public string DeleteContractor(ContractorRegistryEntity contractorData)
        {
            string result = CONST_SUCCESS;

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
                        DeleteLicense(contractorData.contractorNo);
                }

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /// <summary>
        /// This method is used to search for contractor records based on specific filter criteria
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string SearchContractor(GenericEntity filterData)
        {
            try
            {
                string jsonString = string.Empty;
                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

                // Format datetime parameters
                DateTime? startDate = UIHelper.ConvertObjectToDate(filterData.contractStartDateStr);
                DateTime? endDate = UIHelper.ConvertObjectToDate(filterData.contractEndDateStr);

                var model = Repository.SearchContractors(filterData.contractorNo, filterData.idNumber, filterData.contractorName, filterData.companyName, filterData.costCenter,
                    filterData.jobTitle, filterData.supervisorName, startDate, endDate);
                if (model != null)
                {
                    jsonString = JsonConvert.SerializeObject(model);     // Convert to JSON using Newton Json.Net DLL
                }

                return jsonString;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetBaseException().Message, ex.GetBaseException());
            }            
        }

        /// <summary>
        /// Search database for matching records based on the Employee No. parameter
        /// </summary>
        /// <param name="empNo"></param>
        /// <returns></returns>
        [WebMethod]
        public string SearchEmployee(int empNo)
        {
            try
            {
                string jsonString = null;
                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

                var model = Repository.SearchEmployee(empNo);
                if (model != null)
                {
                    jsonString = JsonConvert.SerializeObject(model);     // Convert to JSON using Newton Json.Net DLL
                }

                return jsonString;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetBaseException().Message, ex.GetBaseException());
            }
        }

        /// <summary>
        /// Search for existing ID card record based on Employee No.
        /// </summary>
        /// <param name="empNo"></param>
        /// <returns></returns>
        [WebMethod]
        public string SearchIDCard(int empNo)
        {
            try
            {
                string jsonString = null;
                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

                var model = Repository.SearchIDCard(empNo);
                if (model != null)
                {
                    #region Create a copy of the image into the EmployeePhoto folder
                    //string photoFolder = ConfigurationManager.AppSettings["EmpPhotoFolder"];
                    //if (!string.IsNullOrEmpty(model.ImageURLBase64) && 
                    //    !string.IsNullOrEmpty(photoFolder) &&
                    //    !string.IsNullOrEmpty(model.ImageFileName))
                    //{
                    //    Image image = Base64ToImage(model.ImageURLBase64);
                    //    image.Save(Server.MapPath(string.Format("{0}/{1}", photoFolder, model.ImageFileName)));
                    //}
                    #endregion

                    // Decode the custom cost center
                    if (!string.IsNullOrEmpty(model.CustomCostCenter))
                        model.CustomCostCenter = Server.HtmlDecode(model.CustomCostCenter);

                    jsonString = JsonConvert.SerializeObject(model);     // Convert to JSON using Newton Json.Net DLL
                }

                return jsonString;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetBaseException().Message, ex.GetBaseException());
            }
        }

        /// <summary>
        /// Saves the new ID card entry to the database
        /// </summary>
        /// <param name="employeeData"></param>
        /// <returns></returns>
        [WebMethod]
        public string AddIDCard(EmployeeEntity employeeData)
        {
            string result = null;

            try
            {
                #region Check if ID card already exists
                bool isCardExist = Repository.CheckIfIDCardExist(employeeData.EmpNo);
                if (isCardExist)
                {
                    result = CONST_CARD_EXIST;
                    return result;
                }
                #endregion

                // Initialize other parameters
                employeeData.UserActionDate = DateTime.Now;

                #region Initialize the employee photo object                
                if (!string.IsNullOrWhiteSpace(employeeData.ImagePath))
                {
                    string imagePath = Server.MapPath(employeeData.ImagePath);
                    if (File.Exists(imagePath))
                    {
                        // Convert image into byte array
                        using (FileStream fs = new FileStream(imagePath, FileMode.Open))
                        {
                            using (BinaryReader br = new BinaryReader(fs))
                            {
                                byte[] bytes = br.ReadBytes((Int32)fs.Length);
                                employeeData.EmpPhoto = bytes;
                            }
                        }

                        // Convert image into base 64 string
                        string base64String = ImageToBase64(imagePath);
                        employeeData.Base64Photo = base64String;

                        employeeData.ImageFileName = Path.GetFileName(imagePath);
                        employeeData.ImageFileExt = Path.GetExtension(imagePath);
                    }
                }
                #endregion

                // Encode the custom cost center
                if (!string.IsNullOrEmpty(employeeData.CustomCostCenter))
                {
                    employeeData.CustomCostCenter = Server.HtmlEncode(employeeData.CustomCostCenter);
                }

                DatabaseSaveResult dbResult = Repository.InsertUpdateDeleteIDCard(ContractorRepository.DataAccessType.Create, employeeData);
                if (dbResult != null)
                {
                    if (dbResult.HasError)
                    {
                        if (!string.IsNullOrWhiteSpace(dbResult.ErrorDesc))
                            result = dbResult.ErrorDesc;
                        else
                            result = CONST_FAILED;
                    }
                    else
                    {
                        result = string.Format("{0}|{1}", CONST_SUCCESS, dbResult.NewIdentityID);

                        if (employeeData.LicenseList != null && employeeData.LicenseList.Count > 0)
                            SaveLicense(employeeData.LicenseList, employeeData.EmpNo);

                        if (employeeData.CardHistoryList != null && employeeData.CardHistoryList.Count > 0)
                            SaveCardHistory(employeeData.CardHistoryList, employeeData.EmpNo);
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
        /// Updates the ID card record in the database
        /// </summary>
        /// <param name="employeeData"></param>
        /// <returns></returns>
        [WebMethod]
        public string UpdateIDCard(EmployeeEntity employeeData)
        {
            string result = string.Empty;

            try
            {
                // Initialize other parameters
                employeeData.UserActionDate = DateTime.Now;

                #region Initialize the employee photo object                
                if (!string.IsNullOrWhiteSpace(employeeData.ImagePath))
                {
                    string imagePath = Server.MapPath(employeeData.ImagePath);
                    if (File.Exists(imagePath))
                    {
                        // Convert image into byte array
                        using (FileStream fs = new FileStream(imagePath, FileMode.Open))
                        {
                            using (BinaryReader br = new BinaryReader(fs))
                            {
                                byte[] bytes = br.ReadBytes((Int32)fs.Length);
                                employeeData.EmpPhoto = bytes;
                            }
                        }

                        // Convert image into base 64 string
                        string base64String = ImageToBase64(imagePath);
                        employeeData.Base64Photo = base64String;

                        employeeData.ImageFileName = Path.GetFileName(imagePath);
                        employeeData.ImageFileExt = Path.GetExtension(imagePath);
                    }
                }
                else
                {
                    if (!employeeData.ExcludePhoto)
                    {
                        employeeData.EmpPhoto = null;
                        employeeData.Base64Photo = null;
                        employeeData.ImageFileName = null;
                        employeeData.ImageFileExt = null;
                    }
                }
                #endregion

                DatabaseSaveResult dbResult = Repository.InsertUpdateDeleteIDCard(ContractorRepository.DataAccessType.Update, employeeData);
                if (dbResult != null)
                {
                    if (dbResult.HasError)
                        result = CONST_FAILED;
                    else
                    {
                        if (employeeData.LicenseList != null && employeeData.LicenseList.Count > 0)
                            SaveLicense(employeeData.LicenseList, employeeData.EmpNo);

                        if (employeeData.CardHistoryList != null && employeeData.CardHistoryList.Count > 0)
                            SaveCardHistory(employeeData.CardHistoryList, employeeData.EmpNo);
                        else
                            DeleteCardHistory(employeeData.EmpNo);

                        // Set the return value
                        if (!string.IsNullOrWhiteSpace(employeeData.ImageFileName))
                            result = CONST_SUCCESS + "|" + employeeData.ImageFileName;
                        else
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
        /// Deletes ID card record in the database
        /// </summary>
        /// <param name="empNo"></param>
        /// <returns></returns>
        [WebMethod]
        public string DeleteIDCard(int empNo, bool isContractor)
        {
            string result = string.Empty;

            try
            {
                DatabaseSaveResult dbResult = Repository.InsertUpdateDeleteIDCard(ContractorRepository.DataAccessType.Delete, null, empNo);
                if (dbResult != null)
                {
                    if (dbResult.HasError)
                        result = CONST_FAILED;
                    else
                    {
                        // Delete associated license records
                        if (!isContractor)
                            DeleteLicense(empNo);

                        // Delete associated card history records
                        DeleteCardHistory(empNo);

                        if (dbResult.RowsAffected > 0)
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
        /// Get the ID card history records based on employee number
        /// </summary>
        /// <param name="empNo"></param>
        /// <returns></returns>
        [WebMethod]
        public string GetCardHistory(int empNo)
        {
            try
            {
                string jsonString = string.Empty;
                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

                var model = Repository.GetCardHistory(empNo);
                if (model != null)
                {
                    jsonString = JsonConvert.SerializeObject(model);     // Convert to JSON using Newton Json.Net DLL
                }

                return jsonString;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetBaseException().Message, ex.GetBaseException());
            }
        }

        /// <summary>
        /// This method saves a copy of the image file into the EmployeePhoto folder
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="empNo"></param>
        /// <returns></returns>
        [WebMethod]
        public string CopyPhoto(string imagePath, int empNo)
        {
            string result = string.Empty;

            try
            {
                // Retrieve the folder where the files will be saved
                string photoFolder = Server.MapPath(ConfigurationManager.AppSettings["EmpPhotoFolder"]);
                string fileName = Path.GetFileName(imagePath);
                string ext = Path.GetExtension(imagePath);
                string destinationPath = string.Format(@"{0}\{1}{2}", photoFolder, empNo, ext);

                if (File.Exists(destinationPath))
                {
                    // Delete existing file
                    File.Delete(destinationPath);
                }

                // Create a copy of the image file
                File.Copy(imagePath, destinationPath);

                // Set the return value
                result = CONST_SUCCESS;
                
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }
        #endregion

        /// <summary>
        /// Create a copy of the employee image from the database into the employee folder
        /// </summary>
        /// <param name="employeeData"></param>
        /// <returns></returns>
        [WebMethod]
        public string GeneratePhoto(EmployeeEntity employeeData)
        {
            string result = string.Empty;
            
            try
            {
                if (employeeData == null)
                {
                    result = "Unable to read the photo!";
                }
                else
                {
                    string photoFolder = ConfigurationManager.AppSettings["EmpPhotoFolder"];
                    if (!string.IsNullOrEmpty(employeeData.ImageURLBase64) &&
                        !string.IsNullOrEmpty(photoFolder) &&
                        !string.IsNullOrEmpty(employeeData.ImageFileName))
                    {
                        string imageFileName = Server.MapPath(string.Format("{0}/{1}", photoFolder, employeeData.ImageFileName));
                        if (!File.Exists(imageFileName))
                        {
                            #region Create a copy of the image
                            MemoryStream ms = null;
                            try
                            {
                                byte[] imageBytes = Convert.FromBase64String(employeeData.ImageURLBase64);
                                ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                                ms.Write(imageBytes, 0, imageBytes.Length);
                                Image image = Image.FromStream(ms, true);

                                image.Save(imageFileName);
                            }
                            catch (ArgumentException arg)
                            {
                                throw arg;
                            }
                            catch (Exception err)
                            {
                                throw err;
                            }
                            finally
                            {
                                ms.Close();
                                ms.Flush();
                            }
                            #endregion

                            //using (Image image = Base64ToImage(employeeData.ImageURLBase64))
                            //{
                            //    image.Save(imageFileName);
                            //}
                        }                        
                    }

                    result = CONST_SUCCESS;
                }
               
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetBaseException().Message, ex.GetBaseException());
            }
        }

        [WebMethod]
        public string GetUserFormAccess(FormAccessEntity userAcessParam)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            string result = CONST_FAILED;

            try
            {
                var model = Repository.GetUserFormAccess(userAcessParam);
                if (model != null)
                {
                    result = JsonConvert.SerializeObject(model);     
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetBaseException().Message, ex.GetBaseException());
            }
        }
    }
}
