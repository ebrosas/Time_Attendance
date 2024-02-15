using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class TASFormEntity
    {
        #region Properties
        public string FormCode { get; set; }
        public string FormName { get; set; }
        public int FormAppID { get; set; }
        public string FormFilename { get; set; }
        public bool FormPublic { get; set; }
        public int FormSeq { get; set; }
        #endregion
    }
}
