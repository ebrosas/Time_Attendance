using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Web;

namespace GARMCO.AMS.TAS.UI.Models
{
    public partial class TASDBEntities : DbContext
    {
        #region Constructors
        /// <summary>
        /// This is a partial class required to create the actual entity framework class
        /// by passing custom connection string. This is required for DB First model.
        /// </summary>
        /// <param name="IsAutoConString"></param>
        public TASDBEntities(bool IsAutoConString) : base(GetSqlConnection(), true)
        {
        }
        #endregion

        /// <summary>
        /// creates a entity framework compatible connectionstring by using the ADO.Net connectionstring 
        /// from the config file.
        /// </summary>
        /// <returns>Formatted connection string</returns>
        public static DbConnection GetSqlConnection()
        {
            try
            {
                // Initialize the EntityConnectionStringBuilder. 
                EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();

                // Get the GAP connectionstring
                var connectionSettings = ConfigurationManager.ConnectionStrings["DBConnection"];

                // Set the provider name. 
                entityBuilder.Provider = connectionSettings.ProviderName;

                // Set the specific connection string and add the missing attributes which are needed by EF
                entityBuilder.ProviderConnectionString = connectionSettings.ConnectionString + ";MultipleActiveResultSets=True;App=EntityFramework;";

                // Set the Metadata location. 
                entityBuilder.Metadata = "res://*/Models.TASDataModel.csdl|res://*/Models.TASDataModel.ssdl|res://*/Models.TASDataModel.msl";

                return new EntityConnection(entityBuilder.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }
    }
}