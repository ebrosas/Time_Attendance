using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class RelativeType
    {
        #region Properties
        public int SettingID { get; set; }
        public byte DegreeLevel { get; set; }
        public string RelativeTypeCode { get; set; }
        public string RelativeTypeName { get; set; }
        public byte SequenceNo { get; set; }
        #endregion
    }
}
