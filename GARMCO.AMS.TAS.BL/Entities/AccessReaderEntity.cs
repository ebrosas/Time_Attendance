using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.BL.Entities
{
    [Serializable]
    public class AccessReaderEntity
    {
        public int AutoID { get; set; }
        public int LocationCode { get; set; }
        public int ReaderNo { get; set; }
        public string LocationName { get; set; }
        public string LocationFullName { get; set; }
        public string ReaderName { get; set; }
        public string Direction { get; set; }
    }
}
