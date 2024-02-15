using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.UI.Helpers;
using GARMCO.AMS.TAS.UI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dapper;
using System.Configuration;
using System.IO;

namespace GARMCO.AMS.TAS.UI.Repositories
{
    public class ContractorRepository : IContractorRepository
    {
        #region Fields
        private readonly TASDBEntities _db;
        private IDbConnection _dapperDB; 

        public enum DataAccessType
        {
            Retrieve,
            Create,            
            Update,
            Delete
        }
        #endregion

        #region Constructors
        /// <summary>
        /// This default constructor uses the TAS connection string to connect to database
        /// </summary>
        public ContractorRepository()
        {
            _db = new TASDBEntities(true);            
        }
        #endregion

        #region Interface Implementations
        public List<object> GetContractorRegistrationLookup()
        {
            List<object> result = null;

            try
            {
                string sql = "EXEC tas.Pr_GetContRegistrationLookup";
                List<CostCenterEntity> costCenterList = new List<CostCenterEntity>();
                List<UserDefinedCodes> licenseList = new List<UserDefinedCodes>();
                List<EmployeeDetail> employeeList = new List<EmployeeDetail>();
                List<UserDefinedCodes> bloodGroupList = new List<UserDefinedCodes>();
                List<GenericEntity> supplierList = new List<GenericEntity>();
                List<UserDefinedCodes> jobTitleList = new List<UserDefinedCodes>();
                DbCommand cmd;
                DbDataReader reader;

                // Build the command object
                cmd = _db.Database.Connection.CreateCommand();
                cmd.CommandText = sql;

                // Open database connection
                _db.Database.Connection.Open();

                // Create a DataReader  
                reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                #region Get the cost center list
                CostCenterEntity costCenterItem = null;
                while (reader.Read())
                {
                    costCenterItem = new CostCenterEntity
                    {
                        CompanyCode = UIHelper.ConvertObjectToString(reader["Company"]),
                        CostCenter = UIHelper.ConvertObjectToString(reader["CostCenter"]),
                        CostCenterName = UIHelper.ConvertObjectToString(reader["CostCenterName"]),
                        ParentCostCenter = UIHelper.ConvertObjectToString(reader["ParentBU"]),
                        SuperintendentEmpNo = UIHelper.ConvertObjectToInt(reader["Superintendent"]),
                        ManagerEmpNo = UIHelper.ConvertObjectToInt(reader["CostCenterManager"])
                    };

                    if (!string.IsNullOrWhiteSpace(costCenterItem.CostCenter))
                        costCenterItem.CostCenterFullName = string.Format("{0} - {1}", costCenterItem.CostCenter, costCenterItem.CostCenterName);
                    else
                        costCenterItem.CostCenterFullName = costCenterItem.CostCenterName;

                    costCenterList.Add(costCenterItem);
                }
                #endregion                                

                #region Get License Types
                // Advance to the next result set  
                reader.NextResult();

                while (reader.Read())
                {
                    licenseList.Add(new UserDefinedCodes
                    {
                        UDCCode = UIHelper.ConvertObjectToString(reader["LicenseCode"]),
                        UDCDesc1 = UIHelper.ConvertObjectToString(reader["LicenseDesc"])
                    });
                }
                #endregion

                #region Get employee list
                // Advance to the next result set  
                reader.NextResult();

                while (reader.Read())
                {
                    employeeList.Add(new EmployeeDetail
                    {
                        EmpNo = UIHelper.ConvertObjectToInt(reader["EmpNo"]),
                        EmpName = UIHelper.ConvertObjectToString(reader["EmpName"]),
                        CostCenter = UIHelper.ConvertObjectToString(reader["CostCenter"]),
                        PayGrade = UIHelper.ConvertObjectToInt(reader["PayGrade"]),
                        Position = UIHelper.ConvertObjectToString(reader["Position"])
                    });
                }
                #endregion

                #region Get Blood Groups
                // Advance to the next result set  
                reader.NextResult();

                while (reader.Read())
                {
                    bloodGroupList.Add(new UserDefinedCodes
                    {
                        UDCCode = UIHelper.ConvertObjectToString(reader["BloodGroupCode"]),
                        UDCDesc1 = UIHelper.ConvertObjectToString(reader["BloodGroupDesc"])
                    });
                }
                #endregion

                #region Get Supplier list
                // Advance to the next result set  
                reader.NextResult();

                while (reader.Read())
                {
                    supplierList.Add(new GenericEntity
                    {
                        SupplierCode = UIHelper.ConvertObjectToInt(reader["SupplierCode"]),
                        SupplierName = UIHelper.ConvertObjectToString(reader["SupplierName"])
                    });
                }
                #endregion

                #region Get Contractor Job Titles
                // Advance to the next result set  
                reader.NextResult();

                while (reader.Read())
                {
                    jobTitleList.Add(new UserDefinedCodes
                    {
                        UDCCode = UIHelper.ConvertObjectToString(reader["JobTitleCode"]),
                        UDCDesc1 = UIHelper.ConvertObjectToString(reader["JobTitleDesc"])
                    });
                }
                #endregion

                // Close reader and database connection
                reader.Close();

                result = new List<object>
                {
                    costCenterList,
                    licenseList,
                    employeeList,
                    bloodGroupList,
                    supplierList,
                    jobTitleList
                };

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<DatabaseSaveResult> SaveRegistration(DataAccessType dbAccessType, ContractorRegistryEntity contractorData)
        {
            DatabaseSaveResult dbResult = null;

            try
            {
                switch(dbAccessType)
                {
                    case DataAccessType.Create:
                        #region Register new contractor
                        _db.ContractorRegistries.Add(new ContractorRegistry()
                        {
                            ContractorNo = contractorData.contractorNo,
                            RegistrationDate = contractorData.registrationDate,
                            IDNumber = contractorData.idNumber,
                            IDType = contractorData.idType,
                            FirstName = contractorData.firstName,
                            LastName = contractorData.lastName,
                            CompanyName = contractorData.companyName,
                            CompanyID = contractorData.companyID,
                            CompanyCRNo = contractorData.companyCRNo,
                            PurchaseOrderNo = contractorData.purchaseOrderNo,
                            JobTitle = contractorData.jobTitle,
                            MobileNo = contractorData.mobileNo,
                            VisitedCostCenter = contractorData.visitedCostCenter,
                            SupervisorEmpNo = contractorData.supervisorEmpNo,
                            PurposeOfVisit = contractorData.purposeOfVisit,
                            ContractStartDate = contractorData.contractStartDate.Value,
                            ContractEndDate = contractorData.contractEndDate.Value,
                            Remarks = contractorData.remarks,
                            CreatedDate = contractorData.createdDate,
                            CreatedByEmpNo = contractorData.createdByEmpNo,
                            CreatedByUser = contractorData.createdByUser,
                        });

                        await _db.SaveChangesAsync();

                        dbResult = new DatabaseSaveResult()
                        {
                            RowsAffected = 1,
                            NewIdentityID = _db.ContractorRegistries.LastOrDefault().RegistryID,
                            HasError = false
                        };

                        break;
                        #endregion
                }                

                return dbResult;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                return new DatabaseSaveResult()
                {
                    HasError = true,
                    ErrorCode = ex.HResult.ToString(),
                    ErrorDesc = ex.Message.ToString()
                };  
            }
        }

        public DatabaseSaveResult InsertUpdateDeleteContractorOld(DataAccessType dbAccessType, ContractorRegistryEntity contractorData)
        {
            DatabaseSaveResult dbResult = null;

            try
            {
                switch (dbAccessType)
                {
                    case DataAccessType.Create:
                        #region Register new contractor
                        using (var context = new TASDBEntities(true))
                        {
                            ContractorRegistry newRecord = new ContractorRegistry()
                            {
                                ContractorNo = contractorData.contractorNo,
                                RegistrationDate = contractorData.registrationDate,
                                IDNumber = contractorData.idNumber,
                                IDType = contractorData.idType,
                                FirstName = contractorData.firstName,
                                LastName = contractorData.lastName,
                                CompanyName = contractorData.companyName,
                                CompanyID = contractorData.companyID,
                                CompanyCRNo = contractorData.companyCRNo,
                                PurchaseOrderNo = contractorData.purchaseOrderNo,
                                JobTitle = contractorData.jobTitle,
                                MobileNo = contractorData.mobileNo,
                                VisitedCostCenter = contractorData.visitedCostCenter,
                                SupervisorEmpNo = contractorData.supervisorEmpNo,
                                PurposeOfVisit = contractorData.purposeOfVisit,
                                ContractStartDate = contractorData.contractStartDate.Value,
                                ContractEndDate = contractorData.contractEndDate.Value,
                                Remarks = contractorData.remarks,
                                CreatedDate = contractorData.createdDate,
                                CreatedByEmpNo = contractorData.createdByEmpNo,
                                CreatedByUser = contractorData.createdByUser
                            };

                            bool saveFailed;
                            do
                            {
                                saveFailed = false;
                                try
                                {
                                    context.ContractorRegistries.Add(newRecord);
                                    context.SaveChanges();
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    saveFailed = true;

                                    // Update original values from the database
                                    var entry = ex.Entries.Single();
                                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                                }

                            } while (saveFailed);
                        }

                        

                        //_db.ContractorRegistries.Add(newRecord);
                        //_db.SaveChanges();

                        dbResult = new DatabaseSaveResult()
                        {
                            RowsAffected = 1,
                            NewIdentityID = _db.ContractorRegistries.LastOrDefault().RegistryID,
                            HasError = false
                        };

                        break;
                        #endregion
                }

                return dbResult;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                return new DatabaseSaveResult()
                {
                    HasError = true,
                    ErrorCode = ex.HResult.ToString(),
                    ErrorDesc = ex.Message.ToString()
                };
            }
        }

        public DatabaseSaveResult InsertUpdateDeleteContractor(DataAccessType dbAccessType, ContractorRegistryEntity contractorData)
        {
            DatabaseSaveResult dbResult = null;
            int rowsAffected = 0;
            int registryID = 0;
            ADONetParameter[] parameters = new ADONetParameter[27];

            try
            {
                switch(dbAccessType)
                {
                    case DataAccessType.Create:
                        #region Create new contractor

                        #region Initialize input parameters
                        
                        parameters[0] = new ADONetParameter("@actionType", SqlDbType.TinyInt, Convert.ToByte(dbAccessType));                        
                        parameters[1] = new ADONetParameter("@contractorNo", SqlDbType.Int, contractorData.contractorNo);
                        parameters[2] = new ADONetParameter("@registrationDate", SqlDbType.DateTime, contractorData.registrationDate);
                        parameters[3] = new ADONetParameter("@idNumber", SqlDbType.VarChar, 20, contractorData.idNumber);
                        parameters[4] = new ADONetParameter("@idType", SqlDbType.TinyInt, contractorData.idType);
                        parameters[5] = new ADONetParameter("@firstName", SqlDbType.VarChar, 30, contractorData.firstName);
                        parameters[6] = new ADONetParameter("@lastName", SqlDbType.VarChar, 30, contractorData.lastName);
                        parameters[7] = new ADONetParameter("@companyName", SqlDbType.VarChar, 50, contractorData.companyName);
                        parameters[8] = new ADONetParameter("@companyID", SqlDbType.Int, contractorData.companyID);
                        parameters[9] = new ADONetParameter("@companyCRNo", SqlDbType.VarChar, 20, contractorData.companyCRNo);
                        parameters[10] = new ADONetParameter("@purchaseOrderNo", SqlDbType.Float, contractorData.purchaseOrderNo);
                        parameters[11] = new ADONetParameter("@jobTitle", SqlDbType.VarChar, 10, contractorData.jobTitle);
                        parameters[12] = new ADONetParameter("@mobileNo", SqlDbType.VarChar, 20, contractorData.mobileNo);
                        parameters[13] = new ADONetParameter("@visitedCostCenter", SqlDbType.VarChar, 12, contractorData.visitedCostCenter);
                        parameters[14] = new ADONetParameter("@supervisorEmpNo", SqlDbType.Int, contractorData.supervisorEmpNo);
                        parameters[15] = new ADONetParameter("@supervisorEmpName", SqlDbType.VarChar, 100, contractorData.supervisorEmpName);
                        parameters[16] = new ADONetParameter("@purposeOfVisit", SqlDbType.VarChar, 300, contractorData.purposeOfVisit);
                        parameters[17] = new ADONetParameter("@contractStartDate", SqlDbType.DateTime, contractorData.contractStartDate);
                        parameters[18] = new ADONetParameter("@contractEndDate", SqlDbType.DateTime, contractorData.contractEndDate);
                        parameters[19] = new ADONetParameter("@bloodGroup", SqlDbType.VarChar, 10, contractorData.bloodGroup);
                        parameters[20] = new ADONetParameter("@remarks", SqlDbType.VarChar, 500, contractorData.remarks);
                        parameters[21] = new ADONetParameter("@userActionDate", SqlDbType.DateTime, contractorData.createdDate);
                        parameters[22] = new ADONetParameter("@userEmpNo", SqlDbType.Int, contractorData.createdByEmpNo);
                        parameters[23] = new ADONetParameter("@userID", SqlDbType.VarChar, 50, contractorData.createdByUser);
                        parameters[24] = new ADONetParameter("@workDurationHours", SqlDbType.Int, contractorData.workDurationHours);
                        parameters[25] = new ADONetParameter("@workDurationMins", SqlDbType.Int, contractorData.workDurationMins);
                        parameters[26] = new ADONetParameter("@companyContactNo", SqlDbType.VarChar, 30, contractorData.companyContactNo);
                        #endregion

                        #region Connect to database 
                        using (SqlConnection con = new SqlConnection(_db.Database.Connection.ConnectionString))
                        {
                            using (SqlCommand command = new SqlCommand())
                            {
                                command.CommandType = CommandType.StoredProcedure;
                                command.CommandText = "tas.ContractorRegistry_CRUD";
                                command.CommandTimeout = 300;
                                command.Connection = con;

                                CompileParameters(command, parameters);

                               // Initialize output parameters                                  
                                SqlParameter paramRegistryID = AddParameter(command, "@registryID", SqlDbType.Int, ParameterDirection.InputOutput, registryID);

                                // Establish DB connection
                                con.Open();
                                rowsAffected = command.ExecuteNonQuery();

                                // Fetch the value of the output parameters
                                registryID = UIHelper.ConvertObjectToInt(paramRegistryID.Value);
                            }

                            if (con.State == ConnectionState.Open)
                                con.Close();
                        }
                        #endregion

                        #region Set the return object
                        dbResult = new DatabaseSaveResult()
                        {
                            NewIdentityID = registryID,
                            RowsAffected = rowsAffected,
                            HasError = false
                        };
                        #endregion

                        break;
                        #endregion

                    case DataAccessType.Update:
                    case DataAccessType.Delete:
                        #region Update/Delete existing contractor

                        #region Initialize input parameters
                        parameters[0] = new ADONetParameter("@actionType", SqlDbType.TinyInt, Convert.ToByte(dbAccessType));
                        parameters[1] = new ADONetParameter("@contractorNo", SqlDbType.Int, contractorData.contractorNo);
                        parameters[2] = new ADONetParameter("@registrationDate", SqlDbType.DateTime, contractorData.registrationDate);
                        parameters[3] = new ADONetParameter("@idNumber", SqlDbType.VarChar, 20, contractorData.idNumber);
                        parameters[4] = new ADONetParameter("@idType", SqlDbType.TinyInt, contractorData.idType);
                        parameters[5] = new ADONetParameter("@firstName", SqlDbType.VarChar, 30, contractorData.firstName);
                        parameters[6] = new ADONetParameter("@lastName", SqlDbType.VarChar, 30, contractorData.lastName);
                        parameters[7] = new ADONetParameter("@companyName", SqlDbType.VarChar, 50, contractorData.companyName);
                        parameters[8] = new ADONetParameter("@companyID", SqlDbType.Int, contractorData.companyID);
                        parameters[9] = new ADONetParameter("@companyCRNo", SqlDbType.VarChar, 20, contractorData.companyCRNo);
                        parameters[10] = new ADONetParameter("@purchaseOrderNo", SqlDbType.Float, contractorData.purchaseOrderNo);
                        parameters[11] = new ADONetParameter("@jobTitle", SqlDbType.VarChar, 10, contractorData.jobTitle);
                        parameters[12] = new ADONetParameter("@mobileNo", SqlDbType.VarChar, 20, contractorData.mobileNo);
                        parameters[13] = new ADONetParameter("@visitedCostCenter", SqlDbType.VarChar, 12, contractorData.visitedCostCenter);
                        parameters[14] = new ADONetParameter("@supervisorEmpNo", SqlDbType.Int, contractorData.supervisorEmpNo);
                        parameters[15] = new ADONetParameter("@supervisorEmpName", SqlDbType.VarChar, 100, contractorData.supervisorEmpName);
                        parameters[16] = new ADONetParameter("@purposeOfVisit", SqlDbType.VarChar, 300, contractorData.purposeOfVisit);
                        parameters[17] = new ADONetParameter("@contractStartDate", SqlDbType.DateTime, contractorData.contractStartDate);
                        parameters[18] = new ADONetParameter("@contractEndDate", SqlDbType.DateTime, contractorData.contractEndDate);
                        parameters[19] = new ADONetParameter("@bloodGroup", SqlDbType.VarChar, 10, contractorData.bloodGroup);
                        parameters[20] = new ADONetParameter("@remarks", SqlDbType.VarChar, 500, contractorData.remarks);
                        parameters[21] = new ADONetParameter("@userActionDate", SqlDbType.DateTime, contractorData.createdDate);
                        parameters[22] = new ADONetParameter("@userEmpNo", SqlDbType.Int, contractorData.createdByEmpNo);
                        parameters[23] = new ADONetParameter("@userID", SqlDbType.VarChar, 50, contractorData.createdByUser);
                        parameters[24] = new ADONetParameter("@workDurationHours", SqlDbType.Int, contractorData.workDurationHours);
                        parameters[25] = new ADONetParameter("@workDurationMins", SqlDbType.Int, contractorData.workDurationMins);
                        parameters[26] = new ADONetParameter("@companyContactNo", SqlDbType.VarChar, 30, contractorData.companyContactNo);
                        #endregion

                        #region Connect to database 
                        using (SqlConnection con = new SqlConnection(_db.Database.Connection.ConnectionString))
                        {
                            using (SqlCommand command = new SqlCommand())
                            {
                                command.CommandType = CommandType.StoredProcedure;
                                command.CommandText = "tas.ContractorRegistry_CRUD";
                                command.CommandTimeout = 300;
                                command.Connection = con;

                                CompileParameters(command, parameters);

                                // Initialize output parameters                                  
                                SqlParameter paramRegistryID = AddParameter(command, "@registryID", SqlDbType.Int, ParameterDirection.InputOutput, contractorData.registryID);

                                // Establish DB connection
                                con.Open();
                                rowsAffected = command.ExecuteNonQuery();
                            }

                            if (con.State == ConnectionState.Open)
                                con.Close();
                        }
                        #endregion

                        #region Set the return object
                        dbResult = new DatabaseSaveResult()
                        {
                            RowsAffected = rowsAffected,
                            HasError = false
                        };
                        #endregion

                        break;
                        #endregion
                }

                return dbResult;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                return new DatabaseSaveResult()
                {
                    HasError = true,
                    ErrorCode = ex.HResult.ToString(),
                    ErrorDesc = ex.Message.ToString()
                };
            }
        }

        public int GetMaxContractorNo()
        {
            try
            {
                int maxContractorNo = (from a in _db.ContractorRegistries
                                   select a.ContractorNo).Max();

                return maxContractorNo;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the contractor details from the database
        /// </summary>
        /// <param name="contractorNo"></param>
        /// <returns></returns>
        public ContractorRegistryEntity GetContractorDetails(int contractorNo)
        {
            ContractorRegistryEntity contractorData = null;

            try
            {
                using (var db = new TASDBEntities(true))
                {
                    var model = db.Vw_Contractor.Where(o => o.ContractorNo == contractorNo).FirstOrDefault();
                    if (model != null)
                    {
                        contractorData = new ContractorRegistryEntity()
                        {
                            registryID = model.RegistryID,
                            contractorNo = model.ContractorNo,
                            registrationDate = model.RegistrationDate,
                            idNumber = model.IDNumber,
                            idType = model.IDType,
                            firstName = model.FirstName,
                            lastName = model.LastName,
                            contractorFullName = string.Format("{0} {1}", model.FirstName, model.LastName),
                            companyName = model.CompanyName,
                            companyID = model.CompanyID,
                            companyCRNo = model.CompanyCRNo,
                            purchaseOrderNo = model.PurchaseOrderNo,
                            jobCode = model.JobCode,
                            jobTitle = model.JobTitle,
                            mobileNo = model.MobileNo,
                            visitedCostCenter = model.VisitedCostCenter,
                            visitedCostCenterName = model.VisitedCostCenterName,
                            supervisorEmpNo = model.SupervisorEmpNo,
                            supervisorEmpName = model.SupervisorEmpName,
                            purposeOfVisit = model.PurposeOfVisit,
                            contractStartDate = model.ContractStartDate,
                            contractEndDate = model.ContractEndDate,
                            remarks = model.Remarks,
                            bloodGroup = model.BloodGroup,
                            bloodGroupDesc = model.BloodGroupDesc,
                            workDurationHours = model.WorkDurationHours,
                            workDurationMins = model.WorkDurationMins,
                            companyContactNo = model.CompanyContactNo,
                            createdDate = model.CreatedDate,
                            createdByEmpNo = model.CreatedByEmpNo,
                            createdByUser = model.CreatedByUser,
                            lastUpdatedDate = model.LastUpdatedDate,
                            lastUpdatedByEmpNo = model.LastUpdatedByEmpNo,
                            lastUpdatedByUser = model.LastUpdatedByUser,
                            cardNo = model.CardNo
                        };                                                

                        #region Get the license details
                        try
                        {
                            var licenseModel = db.LicenseRegistries.Where(o => o.EmpNo == contractorNo).ToList();
                            if (licenseModel.Count > 0)
                            {
                                List<LicenseEntity> licenseList = new List<LicenseEntity>();
                                foreach (var item in licenseModel)
                                {
                                    licenseList.Add(new LicenseEntity()
                                    {
                                        registryID = item.RegistryID,
                                        empNo = item.EmpNo,
                                        licenseNo = item.LicenseNo,
                                        licenseTypeCode = item.LicenseTypeCode,
                                        licenseTypeDesc = item.LicenseTypeDesc,
                                        issuingAuthority = item.IssuingAuthority,
                                        issuedDate = item.IssuedDate,
                                        expiryDate = item.ExpiryDate,
                                        remarks = item.Remarks,
                                        licenseGUID = item.LicenseGUID,
                                        createdDate = item.CreatedDate,
                                        createdByEmpNo = item.CreatedByEmpNo,
                                        createdByEmpName = item.CreatedByEmpName,
                                        createdByUser = item.CreatedByUser
                                    });
                                }

                                contractorData.licenseList = new List<LicenseEntity>();
                                contractorData.licenseList.AddRange(licenseList);
                            }
                        }
                        catch(Exception err)
                        {

                        }
                        #endregion
                    }
                }

                return contractorData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DatabaseSaveResult SaveContractorLicense(DataAccessType dbAccessType, List<LicenseEntity> licenseList)
        {
            DatabaseSaveResult dbResult = null;

            try
            {
                List<LicenseRegistry> itemsToAdd = new List<LicenseRegistry>();

                #region Delete existing records
                int empNo = licenseList.FirstOrDefault().empNo;
                var itemsToDelete = from a in _db.LicenseRegistries
                                    where a.EmpNo == empNo
                                    select a;
                if (itemsToDelete.ToList().Count > 0)
                {
                    _db.LicenseRegistries.RemoveRange(itemsToDelete);
                    _db.SaveChanges();
                }
                #endregion

                #region Add licenses
                foreach (LicenseEntity item in licenseList)
                {
                    itemsToAdd.Add(new LicenseRegistry()
                    {
                        EmpNo = item.empNo,
                        LicenseNo = item.licenseNo,
                        LicenseTypeCode = item.licenseTypeCode,
                        LicenseTypeDesc = item.licenseTypeDesc,
                        LicenseGUID = item.licenseGUID,
                        IssuingAuthority = item.issuingAuthority,
                        IssuedDate = item.issuedDate,
                        ExpiryDate = item.expiryDate,
                        Remarks = item.remarks,
                        CreatedDate = item.createdDate,
                        CreatedByEmpNo = item.createdByEmpNo,
                        CreatedByEmpName = item.createdByEmpName,
                        CreatedByUser = item.createdByUser
                    });
                }

                _db.LicenseRegistries.AddRange(itemsToAdd);
                _db.SaveChanges();
                #endregion

                dbResult = new DatabaseSaveResult()
                {
                    RowsAffected = itemsToAdd.Count,
                    HasError = false
                };

                return dbResult;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                return new DatabaseSaveResult()
                {
                    HasError = true,
                    ErrorCode = ex.HResult.ToString(),
                    ErrorDesc = ex.Message.ToString()
                };
            }
        }

        public DatabaseSaveResult InsertUpdateDeleteLicense(DataAccessType dbAccessType, List<LicenseEntity> licenseList, int empNo = 0)
        {
            DatabaseSaveResult dbResult = null;
            int rowsAffected = 0;

            try
            {
                using (_dapperDB = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString))
                {
                    switch(dbAccessType)
                    {
                        case DataAccessType.Create:
                            #region Insert operation

                            var parameters = new DynamicParameters();

                            #region Delete existing records
                            if (empNo > 0)
                            {
                                parameters = new DynamicParameters();
                                parameters.Add("@actionType", DataAccessType.Delete);
                                parameters.Add("@registryID", 0, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                                parameters.Add("@empNo", empNo);

                                int delRows = _dapperDB.Execute("tas.LicenseRegistry_CRUD", parameters, commandType: CommandType.StoredProcedure);
                            }
                            #endregion

                            #region Loop through each license to insert to database 
                            foreach (LicenseEntity item in licenseList)
                            {
                                // Create a dynamic object and pass a value to that object.
                                parameters = new DynamicParameters();
                                parameters.Add("@actionType", dbAccessType);
                                parameters.Add("@registryID", item.registryID, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                                parameters.Add("@empNo", item.empNo);
                                parameters.Add("@licenseNo", item.licenseNo);
                                parameters.Add("@licenseTypeCode", item.licenseTypeCode);
                                parameters.Add("@licenseTypeDesc", item.licenseTypeDesc);
                                parameters.Add("@issuingAuthority", item.issuingAuthority);
                                parameters.Add("@issuedDate", item.issuedDate);
                                parameters.Add("@expiryDate", item.expiryDate);
                                parameters.Add("@remarks", item.remarks);
                                parameters.Add("@licenseGUID", item.licenseGUID);
                                parameters.Add("@userActionDate", item.createdDate);
                                parameters.Add("@userEmpNo", item.createdByEmpNo);
                                parameters.Add("@userEmpName", item.createdByEmpName);
                                parameters.Add("@userID", item.createdByUser);

                                // Call a Stored Procedure using the db.execute method
                                _dapperDB.Execute("tas.LicenseRegistry_CRUD", parameters, commandType: CommandType.StoredProcedure);

                                //To get newly created ID back  
                                item.registryID = parameters.Get<Int32>("@registryID");

                                rowsAffected++;
                            }
                            #endregion

                            #region Return the DB call result
                            if (rowsAffected > 0)
                            {
                                dbResult = new DatabaseSaveResult()
                                {
                                    RowsAffected = rowsAffected,
                                    HasError = false
                                };
                            }
                            #endregion

                            break;
                            #endregion

                        case DataAccessType.Delete:
                            #region Delete operation
                            if (empNo > 0)
                            {
                                parameters = new DynamicParameters();
                                parameters.Add("@actionType", dbAccessType);
                                parameters.Add("@registryID", 0, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                                parameters.Add("@empNo", empNo);

                                rowsAffected = _dapperDB.Execute("tas.LicenseRegistry_CRUD", parameters, commandType: CommandType.StoredProcedure);
                                if (rowsAffected > 0)
                                {
                                    dbResult = new DatabaseSaveResult()
                                    {
                                        RowsAffected = rowsAffected,
                                        HasError = false
                                    };
                                }
                            }
                            break;
                            #endregion
                    }
                }
                
                
                return dbResult;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                return new DatabaseSaveResult()
                {
                    HasError = true,
                    ErrorCode = ex.HResult.ToString(),
                    ErrorDesc = ex.Message.ToString()
                };
            }
        }

        public DatabaseSaveResult DeleteContractorLicense(int empNo)
        {
            DatabaseSaveResult dbResult = null;

            try
            {
                using (var context = new TASDBEntities(true))
                {
                    List<LicenseRegistry> itemsToAdd = new List<LicenseRegistry>();

                    // Delete existing records
                    var itemsToDelete = from a in _db.LicenseRegistries
                                        where a.EmpNo == empNo
                                        select a;
                    int recordCount = itemsToDelete.ToList().Count;

                    if (recordCount > 0)
                    {
                        _db.LicenseRegistries.RemoveRange(itemsToDelete);
                        _db.SaveChanges();

                        dbResult = new DatabaseSaveResult()
                        {
                            RowsAffected = recordCount,
                            HasError = false
                        };
                    }
                }

                return dbResult;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                return new DatabaseSaveResult()
                {
                    HasError = true,
                    ErrorCode = ex.HResult.ToString(),
                    ErrorDesc = ex.Message.ToString()
                };
            }
        }

        public List<ContractorRegistryEntity> GetDuplicateContractor(string idNumber, DateTime? contractStartDate, DateTime? contractEndDate)
        {
            List<ContractorRegistryEntity> contractorList = null;

            try
            {
                using (_dapperDB = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@idNumber", idNumber);
                    parameters.Add("@contractStartDate", contractStartDate);
                    parameters.Add("@contractEndDate", contractEndDate);

                    var duplicateRecords = _dapperDB.Query("tas.Pr_CheckDuplicateContractor", parameters, commandType: CommandType.StoredProcedure).ToList();
                    if (duplicateRecords.Count > 0)
                    {
                        contractorList = new List<ContractorRegistryEntity>();

                        foreach (var item in duplicateRecords)
                        {
                            contractorList.Add(new ContractorRegistryEntity()
                            {
                                contractorNo = UIHelper.ConvertObjectToInt(item.ContractorNo),
                                idNumber = UIHelper.ConvertObjectToString(item.IDNumber),
                                contractorFullName = string.Format("{0} {1}", UIHelper.ConvertObjectToString(item.FirstName), UIHelper.ConvertObjectToString(item.LastName)),
                                contractStartDate = UIHelper.ConvertObjectToDate(item.ContractStartDate),
                                contractEndDate = UIHelper.ConvertObjectToDate(item.ContractEndDate)

                            });
                        }
                    }
                }

                return contractorList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This method is used to search for contractor records from the database
        /// </summary>
        /// <param name="contractorNo"></param>
        /// <param name="idNumber"></param>
        /// <param name="contractorName"></param>
        /// <param name="companyName"></param>
        /// <param name="costCenter"></param>
        /// <param name="jobTitle"></param>
        /// <param name="supervisorName"></param>
        /// <param name="contractStartDate"></param>
        /// <param name="contractEndDate"></param>
        /// <returns></returns>
        public List<ContractorRegistryEntity> SearchContractors(int? contractorNo, string idNumber, string contractorName, string companyName, string costCenter,
            string jobTitle, string supervisorName, DateTime? contractStartDate, DateTime? contractEndDate)
        {
            List<ContractorRegistryEntity> contractorList = null;

            try
            {
                using (_dapperDB = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@contractorNo", contractorNo, DbType.Int32);
                    parameters.Add("@idNumber", idNumber, DbType.String, ParameterDirection.Input, 20);
                    parameters.Add("@contractorName", contractorName, DbType.String, ParameterDirection.Input, 60);
                    parameters.Add("@companyName", companyName, DbType.String, ParameterDirection.Input, 50);
                    parameters.Add("@costCenter", costCenter, DbType.String, ParameterDirection.Input, 12);
                    parameters.Add("@jobTitle", jobTitle, DbType.String, ParameterDirection.Input, 10);
                    parameters.Add("@supervisorName", supervisorName, DbType.String, ParameterDirection.Input, 100);
                    parameters.Add("@contractStartDate", contractStartDate, DbType.DateTime);
                    parameters.Add("@contractEndDate", contractEndDate, DbType.DateTime);

                    var model = _dapperDB.Query("tas.Pr_SearchContractors", parameters, commandType: CommandType.StoredProcedure).ToList();
                    if (model.Count > 0)
                    {
                        contractorList = new List<ContractorRegistryEntity>();

                        foreach (var item in model)
                        {
                            contractorList.Add(new ContractorRegistryEntity()
                            {
                                registryID = item.RegistryID,
                                contractorNo = item.ContractorNo,
                                registrationDate = item.RegistrationDate,
                                idNumber = item.IDNumber,
                                idTypeDesc = item.IDType,
                                firstName = item.FirstName,
                                lastName = item.LastName,
                                companyName = item.CompanyName,
                                companyID = item.CompanyID,
                                companyCRNo = item.CompanyCRNo,
                                purchaseOrderNo = item.PurchaseOrderNo,
                                jobTitle = item.JobTitle,
                                mobileNo = item.MobileNo,
                                visitedCostCenter = item.VisitedCostCenter,
                                visitedCostCenterName = item.VisitedCostCenterName,
                                supervisorEmpNo = item.SupervisorEmpNo,
                                supervisorEmpName = item.SupervisorEmpName,
                                purposeOfVisit = item.PurposeOfVisit,
                                contractStartDate = item.ContractStartDate,
                                contractEndDate = item.ContractEndDate,
                                remarks = item.Remarks,
                                bloodGroup = item.BloodGroup,
                                workDurationHours = item.WorkDurationHours,
                                workDurationMins = item.WorkDurationMins,
                                companyContactNo = item.CompanyContactNo,
                                createdDate = item.CreatedDate,
                                createdByEmpNo = item.CreatedByEmpNo,
                                createdByEmpName = item.CreatedByEmpName,
                                createdByUser = item.CreatedByUser
                            });
                        }
                    }
                }

                return contractorList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public EmployeeEntity SearchEmployee(int empNo)
        {
            EmployeeEntity employeeData = null;

            try
            {
                using (_dapperDB = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@empNo", empNo, DbType.Int32);
                    var model = _dapperDB.QueryFirstOrDefault("tas.Pr_SearchEmployee", parameters, commandType: CommandType.StoredProcedure);

                    if (model != null)
                    {
                        int employeeNo = model.EmpNo;
                        employeeData = new EmployeeEntity()
                        {
                            EmpNo = model.EmpNo,
                            EmpName = model.EmpName,
                            Position = model.Position,
                            CostCenter = model.CostCenter,
                            CostCenterName = model.CostCenterName,
                            PayGrade = model.PayGrade,
                            SupervisorNo = model.SupervisorNo,
                            SupervisorName = model.SupervisorName,
                            ManagerNo = model.ManagerNo,
                            ManagerName = model.ManagerName,
                            CPRNo = model.CPRNo,
                            CustomCostCenter = model.CustomCostCenter,
                            BloodGroup = model.BloodGroup,
                            BloodGroupDesc = model.BloodGroupDesc
                        };

                        #region Get the license details
                        try
                        {
                            var licenseModel = _db.LicenseRegistries.Where(o => o.EmpNo == employeeNo).ToList();
                            if (licenseModel.Count > 0)
                            {
                                List<LicenseEntity> licenseList = new List<LicenseEntity>();
                                foreach (var lic in licenseModel)
                                {
                                    licenseList.Add(new LicenseEntity()
                                    {
                                        registryID = lic.RegistryID,
                                        empNo = lic.EmpNo,
                                        licenseNo = lic.LicenseNo,
                                        licenseTypeCode = lic.LicenseTypeCode,
                                        licenseTypeDesc = lic.LicenseTypeDesc,
                                        issuingAuthority = lic.IssuingAuthority,
                                        issuedDate = lic.IssuedDate,
                                        expiryDate = lic.ExpiryDate,
                                        remarks = lic.Remarks,
                                        licenseGUID = lic.LicenseGUID,
                                        createdDate = lic.CreatedDate,
                                        createdByEmpNo = lic.CreatedByEmpNo,
                                        createdByEmpName = lic.CreatedByEmpName,
                                        createdByUser = lic.CreatedByUser
                                    });
                                }

                                employeeData.LicenseList = new List<LicenseEntity>();
                                employeeData.LicenseList.AddRange(licenseList);
                            }
                        }
                        catch (Exception err)
                        {

                        }
                        #endregion
                    }
                }

                return employeeData;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public EmployeeEntity SearchIDCard(int empNo)
        {
            EmployeeEntity employeeData = null;

            try
            {
                using (_dapperDB = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@actionType", 0);
                    parameters.Add("@registryID", 0, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                    parameters.Add("@empNo", empNo, DbType.Int32);

                    var model = _dapperDB.QueryFirstOrDefault("tas.Pr_IDCardRegistry_CRUD", parameters, commandType: CommandType.StoredProcedure);
                    if (model != null)
                    {
                        int employeeNo = model.EmpNo;
                        employeeData = new EmployeeEntity()
                        {
                            RegistryID = model.RegistryID,
                            EmpNo = model.EmpNo,
                            EmpName = model.EmpName,
                            Position = model.Position,
                            IDNumber = model.IDNumber,
                            CompanyName = model.CompanyName,
                            CostCenter = model.CostCenter,
                            CostCenterName = model.CostCenterName,
                            CustomCostCenter = model.CustomCostCenter,
                            CPRNo = model.CPRNo,
                            BloodGroup = model.BloodGroup,
                            BloodGroupDesc = model.BloodGroupDesc,
                            SupervisorNo = model.SupervisorNo,
                            SupervisorName = model.SupervisorName,
                            ManagerNo = model.ManagerNo,
                            ManagerName = model.ManagerName                            
                        };

                        #region Get the image information (commented)
                        //if (model.EmpPhoto != null)
                        //{
                        //    employeeData.ImageFileName = model.ImageFileName;
                        //    employeeData.ImageFileExt = model.ImageFileExt;
                        //    employeeData.ImageURLBase64 = model.Base64Photo;
                        //    employeeData.ImageURL = "data:image;base64," + Convert.ToBase64String(model.EmpPhoto);
                        //    //employeeData.ImageURL = "data:image/jpeg;charset=utf-8;base64," + Convert.ToBase64String((byte[])model.EmpPhoto);
                        //    //employeeData.ImageURL = "data:image/png;base64," + Convert.ToBase64String((byte[])model.EmpPhoto);
                        //}
                        #endregion

                        #region Get the image information using base64 string
                        employeeData.ImageFileName = model.ImageFileName;
                        employeeData.ImageFileExt = model.ImageFileExt;
                        employeeData.ImageURLBase64 = model.Base64Photo;
                        #endregion

                        #region Get license information
                        try
                        {
                            var licenseModel = _db.LicenseRegistries.Where(o => o.EmpNo == employeeNo).ToList();
                            if (licenseModel.Count > 0)
                            {
                                List<LicenseEntity> licenseList = new List<LicenseEntity>();
                                foreach (var lic in licenseModel)
                                {
                                    licenseList.Add(new LicenseEntity()
                                    {
                                        registryID = lic.RegistryID,
                                        empNo = lic.EmpNo,
                                        licenseNo = lic.LicenseNo,
                                        licenseTypeCode = lic.LicenseTypeCode,
                                        licenseTypeDesc = lic.LicenseTypeDesc,
                                        issuingAuthority = lic.IssuingAuthority,
                                        issuedDate = lic.IssuedDate,
                                        expiryDate = lic.ExpiryDate,
                                        remarks = lic.Remarks,
                                        licenseGUID = lic.LicenseGUID,
                                        createdDate = lic.CreatedDate,
                                        createdByEmpNo = lic.CreatedByEmpNo,
                                        createdByEmpName = lic.CreatedByEmpName,
                                        createdByUser = lic.CreatedByUser
                                    });
                                }

                                employeeData.LicenseList = new List<LicenseEntity>();
                                employeeData.LicenseList.AddRange(licenseList);
                            }
                        }
                        catch (Exception err)
                        {

                        }
                        #endregion

                        #region Get card history information
                        try
                        {
                            var cardModel = _db.Vw_IDCardHistory.Where(o => o.EmpNo == employeeNo).ToList();
                            if (cardModel.Count > 0)
                            {
                                List<CardHistoryEntity> cardList = new List<CardHistoryEntity>();
                                foreach (var card in cardModel)
                                {
                                    cardList.Add(new CardHistoryEntity()
                                    {
                                        historyID = card.HistoryID,
                                        empNo = card.EmpNo,
                                        isContractor = card.IsContractor,
                                        cardRefNo = card.CardRefNo,
                                        remarks = card.Remarks,
                                        cardGUID = card.CardGUID,
                                        createdDate = card.CreatedDate,
                                        createdByEmpNo = card.CreatedByEmpNo,
                                        createdByEmpName = card.CreatedByEmpName,
                                        createdByUser = card.CreatedByUser,
                                        lastUpdatedDate = card.LastUpdatedDate,
                                        lastUpdatedByEmpNo = card.LastUpdatedByEmpNo,
                                        lastUpdatedByEmpName = card.LastUpdatedByEmpName,
                                        lastUpdatedByUser = card.LastUpdatedByUser
                                    });
                                }

                                employeeData.CardHistoryList = new List<CardHistoryEntity>();
                                employeeData.CardHistoryList.AddRange(cardList);
                            }
                        }
                        catch (Exception err)
                        {

                        }
                        #endregion
                    }
                }

                return employeeData;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DatabaseSaveResult InsertUpdateDeleteIDCard(DataAccessType dbAccessType, EmployeeEntity employeeData, int empNo = 0)
        {
            DatabaseSaveResult dbResult = null;
            int rowsAffected = 0;

            try
            {
                using (_dapperDB = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString))
                {
                    var parameters = new DynamicParameters();

                    switch (dbAccessType)
                    {
                        case DataAccessType.Create:
                            #region Insert operation

                            #region Call stored procedure to insert record to DB
                            
                            // Create a dynamic object and pass a value to that object.
                            parameters = new DynamicParameters();
                            parameters.Add("@actionType", dbAccessType);
                            parameters.Add("@registryID", employeeData.RegistryID, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                            parameters.Add("@empNo", employeeData.EmpNo);
                            parameters.Add("@empName", employeeData.EmpName);
                            parameters.Add("@position", employeeData.Position);
                            parameters.Add("@customCostCenter", employeeData.CustomCostCenter);
                            parameters.Add("@cprNo", employeeData.CPRNo);
                            parameters.Add("@bloodGroup", employeeData.BloodGroup);
                            parameters.Add("@isContractor", employeeData.IsContractor);                            
                            parameters.Add("@imageFileName", employeeData.ImageFileName);
                            parameters.Add("@imageFileExt", employeeData.ImageFileExt);                            
                            parameters.Add("@userActionDate", employeeData.UserActionDate);
                            parameters.Add("@userEmpNo", employeeData.UserEmpNo);
                            parameters.Add("@userID", employeeData.UserID);

                            if (employeeData.EmpPhoto != null)
                                parameters.Add("@empPhoto", employeeData.EmpPhoto);

                            if (!string.IsNullOrWhiteSpace(employeeData.Base64Photo))
                                parameters.Add("@base64Photo", employeeData.Base64Photo);

                            // Call a Stored Procedure using the db.execute method
                            rowsAffected = _dapperDB.Execute("tas.Pr_IDCardRegistry_CRUD", parameters, commandType: CommandType.StoredProcedure);

                            //To get newly created ID back  
                            employeeData.RegistryID = parameters.Get<Int32>("@registryID");
                            #endregion

                            #region Return the DB call result
                            if (rowsAffected > 0)
                            {
                                dbResult = new DatabaseSaveResult()
                                {
                                    RowsAffected = rowsAffected,
                                    NewIdentityID = parameters.Get<Int32>("@registryID"),
                                    HasError = false
                                };
                            }
                            #endregion

                            break;
                            #endregion

                        case DataAccessType.Update:
                            #region Update operation

                            #region Call stored procedure to update existing DB record
                            
                            // Create a dynamic object and pass a value to that object.
                            parameters = new DynamicParameters();
                            parameters.Add("@actionType", dbAccessType);
                            parameters.Add("@registryID", employeeData.RegistryID, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                            parameters.Add("@empNo", employeeData.EmpNo);
                            parameters.Add("@empName", employeeData.EmpName);
                            parameters.Add("@position", employeeData.Position);
                            parameters.Add("@customCostCenter", employeeData.CustomCostCenter);
                            parameters.Add("@cprNo", employeeData.CPRNo);
                            parameters.Add("@bloodGroup", employeeData.BloodGroup);
                            parameters.Add("@isContractor", employeeData.IsContractor);
                            parameters.Add("@imageFileName", employeeData.ImageFileName);
                            parameters.Add("@imageFileExt", employeeData.ImageFileExt);
                            parameters.Add("@userActionDate", employeeData.UserActionDate);
                            parameters.Add("@userEmpNo", employeeData.UserEmpNo);
                            parameters.Add("@userID", employeeData.UserID);
                            parameters.Add("@excludePhoto", employeeData.ExcludePhoto);

                            if (employeeData.EmpPhoto != null)
                                parameters.Add("@empPhoto", employeeData.EmpPhoto);

                            if (!string.IsNullOrWhiteSpace(employeeData.Base64Photo))
                                parameters.Add("@base64Photo", employeeData.Base64Photo);

                            // Call a Stored Procedure using the db.execute method
                            rowsAffected = _dapperDB.Execute("tas.Pr_IDCardRegistry_CRUD", parameters, commandType: CommandType.StoredProcedure);
                            #endregion

                            #region Return the DB call result
                            if (rowsAffected > 0)
                            {
                                dbResult = new DatabaseSaveResult()
                                {
                                    RowsAffected = rowsAffected,
                                    HasError = false
                                };
                            }
                            #endregion

                            break;
                            #endregion

                        case DataAccessType.Delete:
                            #region Delete operation
                            if (empNo > 0)
                            {
                                parameters = new DynamicParameters();
                                parameters.Add("@actionType", dbAccessType);
                                parameters.Add("@registryID", 0, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                                parameters.Add("@empNo", empNo);

                                rowsAffected = _dapperDB.Execute("tas.Pr_IDCardRegistry_CRUD", parameters, commandType: CommandType.StoredProcedure);
                                if (rowsAffected > 0)
                                {
                                    dbResult = new DatabaseSaveResult()
                                    {
                                        RowsAffected = rowsAffected,
                                        HasError = false
                                    };
                                }
                            }
                            break;
                            #endregion
                    }
                }


                return dbResult;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                return new DatabaseSaveResult()
                {
                    HasError = true,
                    ErrorCode = ex.HResult.ToString(),
                    ErrorDesc = ex.Message.ToString()
                };
            }
        }

        public bool CheckIfIDCardExist(int empNo)
        {
            var model = _db.IDCardRegistries.Where(o => o.EmpNo == empNo).FirstOrDefault();
            return model != null;
        }

        public List<CardHistoryEntity> GetCardHistory(int empNo)
        {
            List<CardHistoryEntity> historyList = null;

            try
            {
                using (_dapperDB = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@actionType", 0);
                    parameters.Add("@registryID", 0, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                    parameters.Add("@empNo", empNo, DbType.Int32);

                    var model = _dapperDB.Query("tas.Pr_IDCardHistory_CRUD", parameters, commandType: CommandType.StoredProcedure).ToList();
                    if (model.Count > 0)
                    {
                        historyList = new List<CardHistoryEntity>();

                        foreach (var item in model)
                        {
                            historyList.Add(new CardHistoryEntity()
                            {
                                historyID = item.HistoryID,
                                empNo = item.EmpNo,
                                isContractor = item.IsContractor,
                                remarks = item.Remarks,
                                createdDate = item.CreatedDate,
                                createdByEmpNo = item.CreatedByEmpNo,
                                createdByEmpName = item.CreatedByEmpName,
                                createdByUser = item.CreatedByUser,
                                lastUpdatedDate = item.LastUpdatedDate,
                                lastUpdatedByEmpNo = item.LastUpdatedByEmpNo,
                                lastUpdatedByEmpName = item.LastUpdatedByEmpName,
                                lastUpdatedByUser = item.LastUpdatedByUser
                            });
                        }
                    }
                }

                return historyList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DatabaseSaveResult InsertUpdateDeleteIDCardHistory(DataAccessType dbAccessType, List<CardHistoryEntity> cardList, int empNo = 0)
        {
            DatabaseSaveResult dbResult = null;
            int rowsAffected = 0;

            try
            {
                using (_dapperDB = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString))
                {
                    switch (dbAccessType)
                    {
                        case DataAccessType.Create:
                            #region Insert operation

                            var parameters = new DynamicParameters();

                            #region Delete existing records
                            if (empNo > 0)
                            {
                                parameters = new DynamicParameters();
                                parameters.Add("@actionType", DataAccessType.Delete);
                                parameters.Add("@historyID", 0, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                                parameters.Add("@empNo", empNo);

                                int delRows = _dapperDB.Execute("tas.Pr_IDCardHistory_CRUD", parameters, commandType: CommandType.StoredProcedure);
                            }
                            #endregion

                            #region Loop through each license to insert to database 
                            foreach (CardHistoryEntity item in cardList)
                            {
                                // Create a dynamic object and pass a value to that object.
                                parameters = new DynamicParameters();
                                parameters.Add("@actionType", dbAccessType);
                                parameters.Add("@historyID", item.historyID, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                                parameters.Add("@empNo", item.empNo);
                                parameters.Add("@isContractor", item.isContractor);
                                parameters.Add("@cardRefNo", item.cardRefNo);
                                parameters.Add("@remarks", item.remarks);
                                parameters.Add("@cardGUID", item.cardGUID);
                                parameters.Add("@userActionDate", item.createdDate);
                                parameters.Add("@userEmpNo", item.createdByEmpNo);
                                parameters.Add("@userID", item.createdByUser);

                                // Call a Stored Procedure using the db.execute method
                                _dapperDB.Execute("tas.Pr_IDCardHistory_CRUD", parameters, commandType: CommandType.StoredProcedure);

                                //To get newly created ID back  
                                item.historyID = parameters.Get<Int32>("@historyID");

                                rowsAffected++;
                            }
                            #endregion

                            #region Return the DB call result
                            if (rowsAffected > 0)
                            {
                                dbResult = new DatabaseSaveResult()
                                {
                                    RowsAffected = rowsAffected,
                                    HasError = false
                                };
                            }
                            #endregion

                            break;
                        #endregion

                        case DataAccessType.Delete:
                            #region Delete operation
                            if (empNo > 0)
                            {
                                parameters = new DynamicParameters();
                                parameters.Add("@actionType", dbAccessType);
                                parameters.Add("@historyID", 0, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                                parameters.Add("@empNo", empNo);

                                rowsAffected = _dapperDB.Execute("tas.Pr_IDCardHistory_CRUD", parameters, commandType: CommandType.StoredProcedure);
                                if (rowsAffected > 0)
                                {
                                    dbResult = new DatabaseSaveResult()
                                    {
                                        RowsAffected = rowsAffected,
                                        HasError = false
                                    };
                                }
                            }
                            break;
                            #endregion
                    }
                }


                return dbResult;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                return new DatabaseSaveResult()
                {
                    HasError = true,
                    ErrorCode = ex.HResult.ToString(),
                    ErrorDesc = ex.Message.ToString()
                };
            }
        }

        public EmployeeEntity GetEmployeeIDCardDetail(int empNo)
        {
            EmployeeEntity employeeData = null;

            try
            {
                using (_dapperDB = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@empNo", empNo, DbType.Int32);
                    var model = _dapperDB.QueryFirstOrDefault("tas.Pr_SearchEmployee", parameters, commandType: CommandType.StoredProcedure);

                    if (model != null)
                    {
                        int employeeNo = model.EmpNo;
                        employeeData = new EmployeeEntity()
                        {
                            EmpNo = model.EmpNo,
                            EmpName = model.EmpName,
                            Position = model.Position,
                            CostCenter = model.CostCenter,
                            CostCenterName = model.CostCenterName,
                            PayGrade = model.PayGrade,
                            SupervisorNo = model.SupervisorNo,
                            SupervisorName = model.SupervisorName,
                            ManagerNo = model.ManagerNo,
                            ManagerName = model.ManagerName,
                            BloodGroup = model.BloodGroup,
                            BloodGroupDesc = model.BloodGroupDesc,
                            CPRNo = model.CPRNo,
                            CardNo = model.CardNo
                        };

                        #region Get the license details
                        try
                        {
                            var licenseModel = _db.LicenseRegistries.Where(o => o.EmpNo == employeeNo).ToList();
                            if (licenseModel.Count > 0)
                            {
                                List<LicenseEntity> licenseList = new List<LicenseEntity>();
                                foreach (var lic in licenseModel)
                                {
                                    licenseList.Add(new LicenseEntity()
                                    {
                                        registryID = lic.RegistryID,
                                        empNo = lic.EmpNo,
                                        licenseNo = lic.LicenseNo,
                                        licenseTypeCode = lic.LicenseTypeCode,
                                        licenseTypeDesc = lic.LicenseTypeDesc,
                                        issuingAuthority = lic.IssuingAuthority,
                                        issuedDate = lic.IssuedDate,
                                        expiryDate = lic.ExpiryDate,
                                        remarks = lic.Remarks,
                                        licenseGUID = lic.LicenseGUID,
                                        createdDate = lic.CreatedDate,
                                        createdByEmpNo = lic.CreatedByEmpNo,
                                        createdByEmpName = lic.CreatedByEmpName,
                                        createdByUser = lic.CreatedByUser
                                    });
                                }

                                employeeData.LicenseList = new List<LicenseEntity>();
                                employeeData.LicenseList.AddRange(licenseList);
                            }
                        }
                        catch (Exception err)
                        {

                        }
                        #endregion
                    }
                }

                return employeeData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public FormAccessEntity GetUserFormAccess(FormAccessEntity userAcessParam)
        {
            FormAccessEntity userAccessData = null;

            try
            {
                using (_dapperDB = new SqlConnection(ConfigurationManager.ConnectionStrings["GARMCOCommon"].ConnectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@mode", userAcessParam.mode);
                    parameters.Add("@userFrmFormAppID", userAcessParam.userFrmFormAppID);
                    parameters.Add("@userFrmFormCode", userAcessParam.userFrmFormCode);
                    parameters.Add("@userFrmCostCenter", userAcessParam.userFrmCostCenter);
                    parameters.Add("@userFrmEmpNo", userAcessParam.userFrmEmpNo);
                    parameters.Add("@userFrmEmpName", userAcessParam.userFrmEmpName);
                    parameters.Add("@sort", userAcessParam.sort);

                    var model = _dapperDB.QueryFirstOrDefault("genuser.pr_GetUserFormAccess", parameters, commandType: CommandType.StoredProcedure);
                    if (model != null)
                    {
                        int employeeNo = model.EmpNo;
                        userAccessData = new FormAccessEntity()
                        {
                            EmpNo = model.EmpNo,
                            EmpName = model.EmpName,
                            CostCenter = model.CostCenter,
                            FormCode = model.FormCode,
                            FormName = model.FormName,
                            FormFilename = model.FormFilename,
                            FormPublic = model.FormPublic,
                            UserFrmCRUDP = model.UserFrmCRUDP,
                            ApplicationName = model.ApplicationName
                        };
                    }
                }

                return userAccessData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Get Purchase Order details based on supplier PO number
        /// </summary>
        /// <param name="poNumber"></param>
        /// <returns></returns>
        public POEntity GetPurchaseOrderDetails(double poNumber)
        {
            POEntity orderDetail = null;

            try
            {
                string sql = "SELECT * FROM tas.Vw_PurchaseOrder WHERE PONumber = @PONumber";
                using (_dapperDB = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString))
                {
                    orderDetail = _dapperDB.QuerySingleOrDefault<POEntity>(sql, new { PONumber = poNumber });
                }

                return orderDetail;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Get purchase order list based on supplier number
        /// </summary>
        /// <param name="supplierNo"></param>
        /// <returns></returns>
        public List<POEntity> GetPurchaseOrderList(double supplierNo)
        {
            List<POEntity> poList = null;

            try
            {
                string sql = "SELECT * FROM tas.Vw_PurchaseOrder WHERE SupplierNo = @SupplierNo ORDER BY PONumber DESC";
                using (_dapperDB = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString))
                {
                    poList = _dapperDB.Query<POEntity>(sql, new { SupplierNo = supplierNo }).ToList();
                }

                return poList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region ADO.NET Methods
        private SqlParameter AddParameter(SqlCommand command, string parameterName, SqlDbType dbType, ParameterDirection direction)
        {
            SqlParameter parameter = new SqlParameter(parameterName, dbType);
            parameter.Direction = direction;
            command.Parameters.Add(parameter);
            return parameter;
        }

        private SqlParameter AddParameter(SqlCommand command, string parameterName, SqlDbType dbType, ParameterDirection direction, object parameterValue)
        {
            SqlParameter parameter = new SqlParameter(parameterName, dbType);
            parameter.Direction = direction;
            parameter.Value = parameterValue;
            command.Parameters.Add(parameter);

            return parameter;
        }

        private SqlParameter AddParameter(SqlCommand command, string parameterName, SqlDbType dbType, ParameterDirection direction, object parameterValue, int parameterSize)
        {
            SqlParameter parameter = new SqlParameter(parameterName, dbType);
            parameter.Direction = direction;
            parameter.Size = parameterSize;

            if (parameterValue != null)
                parameter.Value = parameterValue;

            command.Parameters.Add(parameter);

            return parameter;
        }

        private DataSet RunSPReturnDataset(string spName, string connectionString, params ADONetParameter[] parameters)
        {
            try
            {
                SqlConnection connection = new SqlConnection()
                {
                    ConnectionString = connectionString
                };

                using (SqlCommand command = new SqlCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = spName;
                    command.CommandTimeout = 300;
                    command.Connection = connection;

                    CompileParameters(command, parameters);
                    //AddSQLCommand(command);

                    using (SqlDataAdapter adapter = new SqlDataAdapter())
                    {
                        adapter.SelectCommand = command;
                        adapter.SelectCommand.CommandTimeout = 300;
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);
                        return ds;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        private void CompileParameters(SqlCommand comm, ADONetParameter[] parameters)
        {
            try
            {
                foreach (ADONetParameter parameter in parameters)
                {
                    if (parameter.ParameterValue == null)
                        parameter.ParameterValue = DBNull.Value;

                    comm.Parameters.Add(parameter.Parameter);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}