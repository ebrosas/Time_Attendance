using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GARMCO.AMS.TAS.UI.Models
{
    public class CardHistoryEntity
    {
        #region Properties
        public int historyID { get; set; }
        public int empNo { get; set; }
        public bool isContractor { get; set; }
        public string cardRefNo { get; set; }
        public string remarks { get; set; }
        public DateTime? createdDate { get; set; }
        public int createdByEmpNo { get; set; }
        public string createdByEmpName { get; set; }
        public string createdByUser { get; set; }
        public DateTime? lastUpdatedDate { get; set; }
        public int? lastUpdatedByEmpNo { get; set; }
        public string lastUpdatedByEmpName { get; set; }
        public string lastUpdatedByUser { get; set; }
        public string cardGUID { get; set; }

        public string createdByFullName
        {
            get
            {
                if (createdByEmpNo > 0)
                    return string.Format("{0} - {1}", createdByEmpNo, createdByEmpName);
                else
                    return createdByEmpName;
            }
        }

        public string lastUpdatedByFullName
        {
            get
            {
                if (lastUpdatedByEmpNo > 0)
                    return string.Format("{0} - {1}", lastUpdatedByEmpNo, lastUpdatedByEmpName);
                else
                    return lastUpdatedByEmpName;
            }
        }
        #endregion
    }
}