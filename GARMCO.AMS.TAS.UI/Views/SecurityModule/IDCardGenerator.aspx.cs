using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Models;
using GARMCO.AMS.TAS.UI.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace GARMCO.AMS.TAS.UI.Views.SecurityModule
{
    [System.Web.Script.Services.ScriptService]
    public partial class IDCardGenerator : System.Web.UI.Page
    {
        #region Fields
        static ContractorRepository _repository;
        private const string CONST_SUCCESS = "SUCCESS";
        private const string CONST_FAILED = "FAILED";
        private const string CONST_CARD_EXIST = "CARDEXIST";
        #endregion

        #region Properties
        public string JSVersion { get { return Session[UIHelper.CONST_JS_VERSION].ToString(); } }

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

        private static void SaveCardHistory(List<CardHistoryEntity> cardList, int empNo)
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

        private static string ImageToBase64(string imagePath)
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

        private static Image Base64ToImage(string base64String)
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

        #region Web Methods
        /// <summary>
        /// Fetch data for the combobox controls
        /// </summary>
        /// <returns></returns>
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
        /// Search database for matching records based on the Employee No. parameter
        /// </summary>
        /// <param name="empNo"></param>
        /// <returns></returns>
        [WebMethod]
        public static string SearchEmployee(int empNo)
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
        public static string SearchIDCard(int empNo)
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
                        model.CustomCostCenter = HttpContext.Current.Server.HtmlDecode(model.CustomCostCenter);

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
        public static string AddIDCard(EmployeeEntity employeeData)
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
                    string imagePath = HttpContext.Current.Server.MapPath(employeeData.ImagePath);
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
                    employeeData.CustomCostCenter = HttpContext.Current.Server.HtmlEncode(employeeData.CustomCostCenter);
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
        public static string UpdateIDCard(EmployeeEntity employeeData)
        {
            string result = string.Empty;

            try
            {
                // Initialize other parameters
                employeeData.UserActionDate = DateTime.Now;

                #region Initialize the employee photo object                
                if (!string.IsNullOrWhiteSpace(employeeData.ImagePath))
                {
                    
                    string imagePath = HttpContext.Current.Server.MapPath(employeeData.ImagePath);
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
        /// Updates the ID card record in the database
        /// </summary>
        /// <param name="employeeData"></param>
        /// <returns></returns>
        [WebMethod]
        public static string UpdateIDCardBase64(EmployeeEntity employeeData)
        {
            string result = string.Empty;

            try
            {
                // Initialize other parameters
                employeeData.UserActionDate = DateTime.Now;

                #region Initialize the employee photo object (Byte Array image)                
                if (!string.IsNullOrWhiteSpace(employeeData.ImagePath))
                {
                    byte[] imageBytes = Convert.FromBase64String(employeeData.ImagePath);
                    employeeData.EmpPhoto = imageBytes;
                }
                #endregion

                #region Initialize the employee photo object (Base64 Image)                
                if (!string.IsNullOrWhiteSpace(employeeData.ImageURLBase64))
                {
                    employeeData.Base64Photo = employeeData.ImageURLBase64;
                    employeeData.ImageFileName = Path.GetFileName(employeeData.ImageFileName);
                    employeeData.ImageFileExt = Path.GetExtension(employeeData.ImageFileName);
                }
                else
                {
                    employeeData.EmpPhoto = null;
                    employeeData.Base64Photo = null;
                    employeeData.ImageFileName = null;
                    employeeData.ImageFileExt = null;
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
                        else
                            DeleteLicense(employeeData.EmpNo);

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
        /// Saves the new ID card entry to the database
        /// </summary>
        /// <param name="employeeData"></param>
        /// <returns></returns>
        [WebMethod]
        public static string InserIDCardBase64(EmployeeEntity employeeData)
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

                #region Initialize the employee photo object (Byte Array image)                
                if (!string.IsNullOrWhiteSpace(employeeData.ImagePath))
                {
                    byte[] imageBytes = Convert.FromBase64String(employeeData.ImagePath);
                    employeeData.EmpPhoto = imageBytes;
                }
                #endregion

                #region Initialize the employee photo object (Base64 string)                
                if (!string.IsNullOrWhiteSpace(employeeData.ImageURLBase64))
                {
                    employeeData.Base64Photo = employeeData.ImageURLBase64;
                    employeeData.ImageFileName = Path.GetFileName(employeeData.ImageFileName);
                    employeeData.ImageFileExt = Path.GetExtension(employeeData.ImageFileName);
                }
                else
                {
                    employeeData.EmpPhoto = null;
                    employeeData.Base64Photo = null;
                    employeeData.ImageFileName = null;
                    employeeData.ImageFileExt = null;
                }
                #endregion

                // Encode the custom cost center
                if (!string.IsNullOrEmpty(employeeData.CustomCostCenter))
                {
                    employeeData.CustomCostCenter = HttpContext.Current.Server.HtmlEncode(employeeData.CustomCostCenter);
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
        /// Deletes ID card record in the database
        /// </summary>
        /// <param name="empNo"></param>
        /// <returns></returns>
        [WebMethod]
        public static string DeleteIDCard(int empNo, bool isContractor)
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
        /// Create a copy of the employee image from the database into the employee folder
        /// </summary>
        /// <param name="employeeData"></param>
        /// <returns></returns>
        [WebMethod]
        public static string GeneratePhoto(EmployeeEntity employeeData)
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
                        string imageFileName = HttpContext.Current.Server.MapPath(string.Format("{0}/{1}", photoFolder, employeeData.ImageFileName));
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

        /// <summary>
        /// This method saves a copy of the image file into the EmployeePhoto folder
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="empNo"></param>
        /// <returns></returns>
        [WebMethod]
        public static string CopyPhoto(string imagePath, int empNo)
        {
            string result = string.Empty;

            try
            {
                // Retrieve the folder where the files will be saved
                string photoFolder = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["EmpPhotoFolder"]);
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

        /// <summary>
        /// Create a copy of the employee image from the database into the employee folder
        /// </summary>
        /// <param name="employeeData"></param>
        /// <returns></returns>
        [WebMethod]
        public static string CreatePhoto(string imagePath, int empNo)
        {
            string result = string.Empty;

            try
            {
                //string photoFolder = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["EmpPhotoFolder"]);
                string photoFolder = HttpContext.Current.Request.MapPath(ConfigurationManager.AppSettings["EmpPhotoFolder"]);
                string fileName = Path.GetFileName(imagePath);
                string ext = Path.GetExtension(imagePath);
                string destinationPath = string.Format(@"{0}\{1}{2}", photoFolder, empNo, ext);

                if (!string.IsNullOrEmpty(photoFolder) &&
                    !string.IsNullOrEmpty(imagePath))
                {
                    string base64String = string.Empty;

                    #region Initialize the employee photo object                
                    if (File.Exists(imagePath))
                    {
                        // Convert image into base 64 string
                        base64String = ImageToBase64(imagePath);
                    }
                    #endregion

                    if (!string.IsNullOrWhiteSpace(base64String))
                    {
                        #region Create a copy of the image
                        MemoryStream ms = null;
                        try
                        {
                            byte[] imageBytes = Convert.FromBase64String(base64String);
                            ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                            ms.Write(imageBytes, 0, imageBytes.Length);
                            Image image = Image.FromStream(ms, true);

                            image.Save(destinationPath);

                            result = CONST_SUCCESS;
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
                    }
                }

                return result;
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
        public static string ReplicatePhoto(string imagePath, int empNo)
        {
            string result = string.Empty;

            try
            {
                // Retrieve the folder where the files will be saved
                string destinationFolder = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["EmpPhotoFolder"]);
                string sourceFolder = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["IDPhotoVirtualFolder"]);

                string fileName = Path.GetFileName(imagePath);
                string ext = Path.GetExtension(imagePath);

                string destinationPath = string.Format(@"{0}\{1}{2}", destinationFolder, empNo, ext);
                string sourcePath = string.Format(@"{0}\{1}", sourceFolder, fileName);

                if (File.Exists(destinationPath))
                {
                    // Delete existing file
                    File.Delete(destinationPath);
                }

                // Create a copy of the image file
                File.Copy(sourcePath, destinationPath);

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
    }
}