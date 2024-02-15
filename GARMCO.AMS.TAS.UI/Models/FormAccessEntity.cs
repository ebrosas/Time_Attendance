using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GARMCO.AMS.TAS.UI.Models
{
    public class FormAccessEntity : EmployeeEntity
    {
        #region Properties
        public string FormCode { get; set; }
        public string FormName { get; set; }
        public string FormFilename { get; set; }
        public bool FormPublic { get; set; }
        public string UserFrmCRUDP { get; set; }
        public string ApplicationName { get; set; }
        #endregion

        #region Parameters
        public byte mode { get; set; }
        public int userFrmFormAppID { get; set; }
        public string userFrmFormCode { get; set; }
        public string userFrmCostCenter { get; set; }
        public int userFrmEmpNo { get; set; }
        public string userFrmEmpName { get; set; }
        public string sort { get; set; }
        #endregion
    }
}