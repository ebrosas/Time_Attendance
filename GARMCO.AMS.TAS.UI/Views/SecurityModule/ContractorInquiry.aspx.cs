using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Models;
using GARMCO.AMS.TAS.UI.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GARMCO.AMS.TAS.UI.Views.SecurityModule
{
    [System.Web.Script.Services.ScriptService]
    public partial class ContractorInquiry : System.Web.UI.Page
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
        /// This method is used to search for contractor records based on specific filter criteria
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public static string SearchContractor(GenericEntity filterData)
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
        #endregion
    }
}