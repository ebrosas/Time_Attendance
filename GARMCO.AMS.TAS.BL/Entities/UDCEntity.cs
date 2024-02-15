using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class UDCEntity
    {
        #region Properties
        public string UDCKey { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Description2 { get; set; }
        public string FieldRef { get; set; }
        #endregion
    }
}
