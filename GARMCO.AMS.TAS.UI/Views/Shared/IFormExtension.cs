using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GARMCO.AMS.TAS.UI.Views.Shared
{
    public interface IFormExtension
    {
        void ClearForm();
        void AddControlsAttribute();
        void ProcessQueryString();
        void KillSessions();
    }
}
