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
    public partial class TASDBContext : DbContext
    {
        #region Constructors
        /// <summary>
        /// This default constructor uses the TAS connection string to connect to database
        /// </summary>
        public TASDBContext() : base(GetSQLConnectionString())
        {
            
        }

        /// <summary>
        /// This constructor overload passes the customize connection string to the DbContext class
        /// </summary>
        /// <param name="connectionString"></param>
        public TASDBContext(string connectionString) : base(connectionString)
        {
            
        }
        #endregion

        #region Private Methods
        private static DbConnection GetSqlConnection()
        {
            // Initialize the EntityConnectionStringBuilder. 
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();

            // Get the TAS connectionstring
            var connectionSettings = ConfigurationManager.ConnectionStrings["GAPConnectionString"];

            // Set the provider name. 
            entityBuilder.Provider = connectionSettings.ProviderName;

            // Set the specific connection string and add the missing attributes which are needed by EF
            entityBuilder.ProviderConnectionString = connectionSettings.ConnectionString + "MultipleActiveResultSets=True;App=EntityFramework;";

            // Set the Metadata location. 
            entityBuilder.Metadata = "res://*/DataModels.ECR.ECRDataModel.csdl|res://*/DataModels.ECR.ECRDataModel.ssdl|res://*/DataModels.ECR.ECRDataModel.msl";

            return new EntityConnection(entityBuilder.ToString());
        }

        private static string GetSQLConnectionString()
        {
            // Get the TAS connectionstring
            string connectionSettings = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString.Trim();
            return connectionSettings;
        }
        #endregion

        #region Data Models
        
        #endregion
    }
}