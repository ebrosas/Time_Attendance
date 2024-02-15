using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GARMCO.AMS.TAS.BL.Entities;
using GARMCO.AMS.TAS.DAL.DataModels;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data;
using System.IO;
using System.Globalization;
using System.Transactions;
using System.Web.Hosting;
using System.Data.Common;

namespace GARMCO.AMS.TAS.BL.Helpers
{
    public class DataService
    {
        #region Enumeration
        public enum FormAccessIndex
        {
            Create, Retrieve, Update, Delete, Print
        }
        #endregion

        #region Members
        private string _conString;
        private TASEntities TASContext;
        private GenPurposeEntities GenPurposeContext;
        #endregion

        #region Constructors
        public DataService(string connectionString)
        {
            TASContext = new TASEntities();
            TASContext.Database.Connection.ConnectionString = connectionString;
        }

        public DataService(string connectionStringTAS, string connectStringCommonAdmin)
        {
            TASContext = new TASEntities();
            TASContext.Database.Connection.ConnectionString = connectionStringTAS;

            GenPurposeContext = new GenPurposeEntities();
            GenPurposeContext.Database.Connection.ConnectionString = connectStringCommonAdmin;
        }
        #endregion

        #region Properties
        public string ConnectionString
        {
            get { return _conString; }
            set { _conString = value; }
        }
        #endregion

        #region ADO.NET Methods
        private static DataSet RunSPReturnDataset(string spName, string connectionString, params ADONetParameter[] parameters)
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

        private static void CompileParameters(SqlCommand comm, ADONetParameter[] parameters)
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

        #region Private Methods
        private static List<FireTeamMember> BuildFireTeamCollection(DataTable dt, string imageRootPath, int actionType)
        {
            if (dt == null || dt.Rows.Count == 0)
                return null;

            try
            {
                List<FireTeamMember> result = new List<FireTeamMember>();

                foreach (DataRow dvRow in dt.Rows)
                {
                    #region Check for duplicate entries
                    if (result.Count > 0)
                    {
                        if (result.Where(a => a.EmpNo == BLHelper.ConvertObjectToInt(dvRow["EmpNo"])).FirstOrDefault() != null)
                            continue;
                    }
                    #endregion

                    #region Set the value of the common fields
                    FireTeamMember fireTeamItem = new FireTeamMember()
                    {
                        SwipeDate = BLHelper.ConvertObjectToDate(dvRow["SwipeDate"]),
                        SwipeTime = BLHelper.ConvertObjectToDate(dvRow["SwipeTime"]),
                        EmpNo = BLHelper.ConvertObjectToInt(dvRow["EmpNo"]),
                        EmpName = BLHelper.ConvertObjectToString(dvRow["EmpName"]),
                        EmpFullName = BLHelper.ConvertObjectToInt(dvRow["EmpNo"]) > 0
                            ? string.Format("{0} - {1}", BLHelper.ConvertObjectToInt(dvRow["EmpNo"]), BLHelper.ConvertObjectToString(dvRow["EmpName"]))
                            : BLHelper.ConvertObjectToString(dvRow["EmpName"]),
                        Position = BLHelper.ConvertObjectToString(dvRow["Position"]),
                        Extension = BLHelper.ConvertObjectToString(dvRow["Extension"]),
                        MobileNo = BLHelper.ConvertObjectToString(dvRow["MobileNo"]),
                        GradeCode = BLHelper.ConvertObjectToInt(dvRow["GradeCode"]),
                        PayStatus = BLHelper.ConvertObjectToString(dvRow["PayStatus"]),
                        CostCenter = BLHelper.ConvertObjectToString(dvRow["CostCenter"]),
                        CostCenterName = BLHelper.ConvertObjectToString(dvRow["CostCenterName"]),
                        CostCenterFullName = string.Format("{0} - {1}",
                            BLHelper.ConvertObjectToString(dvRow["CostCenter"]),
                            BLHelper.ConvertObjectToString(dvRow["CostCenterName"])),

                        SupervisorEmpNo = BLHelper.ConvertObjectToInt(dvRow["SupervisorEmpNo"]),
                        SupervisorEmpName = BLHelper.ConvertObjectToString(dvRow["SupervisorEmpName"]),
                        SupervisorFullName = BLHelper.ConvertObjectToInt(dvRow["SupervisorEmpNo"]) > 0
                            ? string.Format("{0} - {1}", BLHelper.ConvertObjectToInt(dvRow["SupervisorEmpNo"]), BLHelper.ConvertObjectToString(dvRow["SupervisorEmpName"]))
                            : BLHelper.ConvertObjectToString(dvRow["SupervisorEmpName"]),

                        SuperintendentEmpNo = BLHelper.ConvertObjectToInt(dvRow["SuperintendentEmpNo"]),
                        SuperintendentEmpName = BLHelper.ConvertObjectToString(dvRow["SuperintendentEmpName"]),
                        SuperintendentFullName = BLHelper.ConvertObjectToInt(dvRow["SuperintendentEmpNo"]) > 0
                            ? string.Format("{0} - {1}", BLHelper.ConvertObjectToInt(dvRow["SuperintendentEmpNo"]), BLHelper.ConvertObjectToString(dvRow["SuperintendentEmpName"]))
                            : BLHelper.ConvertObjectToString(dvRow["SuperintendentEmpName"]),

                        ShiftPatCode = BLHelper.ConvertObjectToString(dvRow["ShiftPatCode"]),
                        ShiftCode = BLHelper.ConvertObjectToString(dvRow["ShiftCode"]),
                        ShiftPointer = BLHelper.ConvertObjectToInt(dvRow["ShiftPointer"]),
                        SwipeLocation = BLHelper.ConvertObjectToString(dvRow["SwipeLocation"]),
                        SwipeType = BLHelper.ConvertObjectToString(dvRow["SwipeType"]),
                        Notes = BLHelper.ConvertObjectToString(dvRow["Notes"])
                    };

                    fireTeamItem.SwipeSummary = !string.IsNullOrEmpty(fireTeamItem.SwipeLocation)
                        ? string.Format("{0} ({1})", fireTeamItem.SwipeLocation, fireTeamItem.SwipeType)
                        : string.Empty;

                    #region Get the employee photo
                    try
                    {
                        string imageFullPath = string.Format(@"{0}\{1}.bmp", imageRootPath, fireTeamItem.EmpNo);
                        if (File.Exists(imageFullPath))
                        {
                            fireTeamItem.EmpImagePath = imageFullPath;
                            fireTeamItem.PhotoTooltip = fireTeamItem.Notes;
                        }
                        else
                        {
                            if (fireTeamItem.EmpNo > 10000000)
                            {
                                imageFullPath = string.Format(@"{0}\{1}.bmp", imageRootPath, fireTeamItem.EmpNo - 10000000);
                                if (File.Exists(imageFullPath))
                                {
                                    fireTeamItem.EmpImagePath = imageFullPath;
                                    fireTeamItem.PhotoTooltip = fireTeamItem.Notes;
                                }
                                else
                                {
                                    fireTeamItem.EmpImagePath = BLHelper.CONST_NO_EMPLOYEE_PHOTO;
                                    fireTeamItem.PhotoTooltip = BLHelper.CONST_NO_PHOTO_MESSAGE;
                                }
                            }
                            else
                            {
                                fireTeamItem.EmpImagePath = BLHelper.CONST_NO_EMPLOYEE_PHOTO;
                                fireTeamItem.PhotoTooltip = BLHelper.CONST_NO_PHOTO_MESSAGE;
                            }
                        }

                        // For testing purpose
                        //fireTeamItem.EmpImagePath = @"\\EmpPhoto\Images\1149.bmp";
                    }
                    catch (Exception)
                    {
                        fireTeamItem.EmpImagePath = BLHelper.CONST_NO_EMPLOYEE_PHOTO;
                        fireTeamItem.PhotoTooltip = BLHelper.CONST_NO_PHOTO_MESSAGE;
                    }
                    #endregion

                    #region Set the icon if employee is present or absent
                    //if (actionType == 1)
                    //{
                    //    fireTeamItem.EmpAttendanceFlag = BLHelper.CONST_EMP_PRESENT_ICON;
                    //    fireTeamItem.EmpAttendanceNotes = BLHelper.CONST_EMP_PRESENT_NOTES;
                    //}
                    //else
                    //{
                    if (fireTeamItem.SwipeTime.HasValue &&
                        fireTeamItem.SwipeType.ToUpper() != "OUT")
                    {
                        fireTeamItem.EmpAttendanceFlag = BLHelper.CONST_EMP_PRESENT_ICON;
                        fireTeamItem.EmpAttendanceNotes = BLHelper.CONST_EMP_PRESENT_NOTES;
                        fireTeamItem.IsPresent = true;
                    }
                    else
                    {
                        fireTeamItem.EmpAttendanceFlag = BLHelper.CONST_EMP_ABSENT_ICON;
                        fireTeamItem.EmpAttendanceNotes = BLHelper.CONST_EMP_ABSENT_NOTES;
                        fireTeamItem.IsPresent = false;
                    }
                    //}
                    #endregion

                    #endregion

                    // Add item to the collection
                    result.Add(fireTeamItem);
                }

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static List<FireTeamMember> BuildFireTeamFireWatchCollection(DataTable dt, string imageRootPath)
        {
            if (dt == null || dt.Rows.Count == 0)
                return null;

            try
            {
                List<FireTeamMember> result = new List<FireTeamMember>();

                foreach (DataRow dvRow in dt.Rows)
                {
                    #region Check for duplicate entries
                    if (result.Count > 0)
                    {
                        if (result.Where(a => a.EmpNo == BLHelper.ConvertObjectToInt(dvRow["EmpNo"])).FirstOrDefault() != null)
                            continue;
                    }
                    #endregion

                    #region Set the value of the common fields
                    FireTeamMember fireTeamItem = new FireTeamMember()
                    {
                        SwipeDate = BLHelper.ConvertObjectToDate(dvRow["SwipeDate"]),
                        SwipeTime = BLHelper.ConvertObjectToDate(dvRow["SwipeTime"]),
                        EmpNo = BLHelper.ConvertObjectToInt(dvRow["EmpNo"]),
                        EmpName = BLHelper.ConvertObjectToString(dvRow["EmpName"]),
                        EmpFullName = BLHelper.ConvertObjectToInt(dvRow["EmpNo"]) > 0
                            ? string.Format("{0} - {1}", BLHelper.ConvertObjectToInt(dvRow["EmpNo"]), BLHelper.ConvertObjectToString(dvRow["EmpName"]))
                            : BLHelper.ConvertObjectToString(dvRow["EmpName"]),
                        Position = BLHelper.ConvertObjectToString(dvRow["Position"]),
                        Extension = BLHelper.ConvertObjectToString(dvRow["Extension"]),
                        MobileNo = BLHelper.ConvertObjectToString(dvRow["MobileNo"]),
                        GradeCode = BLHelper.ConvertObjectToInt(dvRow["GradeCode"]),
                        //PayStatus = BLHelper.ConvertObjectToString(dvRow["PayStatus"]),
                        CostCenter = BLHelper.ConvertObjectToString(dvRow["CostCenter"]),
                        CostCenterName = BLHelper.ConvertObjectToString(dvRow["CostCenterName"]),
                        CostCenterFullName = string.Format("{0} - {1}",
                            BLHelper.ConvertObjectToString(dvRow["CostCenter"]),
                            BLHelper.ConvertObjectToString(dvRow["CostCenterName"])),

                        SupervisorEmpNo = BLHelper.ConvertObjectToInt(dvRow["SupervisorEmpNo"]),
                        SupervisorEmpName = BLHelper.ConvertObjectToString(dvRow["SupervisorEmpName"]),
                        SupervisorFullName = BLHelper.ConvertObjectToInt(dvRow["SupervisorEmpNo"]) > 0
                            ? string.Format("{0} - {1}", BLHelper.ConvertObjectToInt(dvRow["SupervisorEmpNo"]), BLHelper.ConvertObjectToString(dvRow["SupervisorEmpName"]))
                            : BLHelper.ConvertObjectToString(dvRow["SupervisorEmpName"]),
                       
                        ShiftPatCode = BLHelper.ConvertObjectToString(dvRow["ShiftPatCode"]),
                        ShiftCode = BLHelper.ConvertObjectToString(dvRow["ShiftCode"]),
                        ShiftPointer = BLHelper.ConvertObjectToInt(dvRow["ShiftPointer"]),
                        SwipeLocation = BLHelper.ConvertObjectToString(dvRow["SwipeLocation"]),
                        SwipeType = BLHelper.ConvertObjectToString(dvRow["SwipeType"]),
                        Notes = BLHelper.ConvertObjectToString(dvRow["Notes"]),
                        TotalRecords = dt.Columns.Contains("TotalRecords") ? BLHelper.ConvertObjectToInt(dvRow["TotalRecords"]) : 0,
                        GroupType = dt.Columns.Contains("GroupType") ? BLHelper.ConvertObjectToString(dvRow["GroupType"]) : string.Empty
                    };

                    fireTeamItem.SwipeSummary = !string.IsNullOrEmpty(fireTeamItem.SwipeLocation)
                        ? string.Format("{0} ({1})", fireTeamItem.SwipeLocation, fireTeamItem.SwipeType)
                        : string.Empty;

                    #region Get the employee photo
                    try
                    {
                        bool isPhotoFound = false;
                        //string imageFullPath_BMP = string.Format(@"{0}\{1}.bmp", imageRootPath, fireTeamItem.EmpNo);
                        //string imageFullPath_JPG = string.Format(@"{0}\{1}.jpg", imageRootPath, fireTeamItem.EmpNo);
                        string imageFullPath_BMP = string.Format(@"~/{0}/{1}.bmp", imageRootPath, fireTeamItem.EmpNo);
                        string imageFullPath_JPG = string.Format(@"~/{0}/{1}.jpg", imageRootPath, fireTeamItem.EmpNo);

                        #region Begin searching for bitmap photo                                
                        if (File.Exists(HostingEnvironment.MapPath(imageFullPath_BMP)))
                        {
                            fireTeamItem.EmpImagePath = imageFullPath_BMP;
                            isPhotoFound = true;
                        }
                        else
                        {
                            if (fireTeamItem.EmpNo > 10000000)
                            {
                                //imageFullPath_BMP = string.Format(@"{0}\{1}.bmp", imageRootPath, fireTeamItem.EmpNo - 10000000);
                                imageFullPath_BMP = string.Format(@"~/{0}/{1}.bmp", imageRootPath, fireTeamItem.EmpNo - 10000000);

                                if (File.Exists(HostingEnvironment.MapPath(imageFullPath_BMP)))
                                {
                                    fireTeamItem.EmpImagePath = imageFullPath_BMP;
                                    isPhotoFound = true;
                                }
                                else
                                {
                                    fireTeamItem.EmpImagePath = BLHelper.CONST_NO_EMPLOYEE_PHOTO;
                                    fireTeamItem.PhotoTooltip = BLHelper.CONST_NO_PHOTO_MESSAGE;
                                }
                            }
                            else
                            {
                                fireTeamItem.EmpImagePath = BLHelper.CONST_NO_EMPLOYEE_PHOTO;
                                fireTeamItem.PhotoTooltip = BLHelper.CONST_NO_PHOTO_MESSAGE;
                            }
                        }
                        #endregion

                        if (!isPhotoFound)
                        {
                            #region Search for JPEG photo
                            if (File.Exists(HostingEnvironment.MapPath(imageFullPath_JPG)))
                            {
                                fireTeamItem.EmpImagePath = imageFullPath_JPG;
                                isPhotoFound = true;
                            }
                            else
                            {
                                if (fireTeamItem.EmpNo > 10000000)
                                {
                                    //imageFullPath_JPG = string.Format(@"{0}\{1}.jpg", imageRootPath, fireTeamItem.EmpNo - 10000000);
                                    imageFullPath_JPG = string.Format(@"~/{0}/{1}.jpg", imageRootPath, fireTeamItem.EmpNo - 10000000);

                                    if (File.Exists(HostingEnvironment.MapPath(imageFullPath_JPG)))
                                    {
                                        fireTeamItem.EmpImagePath = imageFullPath_JPG;
                                        isPhotoFound = true;
                                    }
                                    else
                                    {
                                        fireTeamItem.EmpImagePath = BLHelper.CONST_NO_EMPLOYEE_PHOTO;
                                        fireTeamItem.PhotoTooltip = BLHelper.CONST_NO_PHOTO_MESSAGE;
                                    }
                                }
                                else
                                {
                                    fireTeamItem.EmpImagePath = BLHelper.CONST_NO_EMPLOYEE_PHOTO;
                                    fireTeamItem.PhotoTooltip = BLHelper.CONST_NO_PHOTO_MESSAGE;
                                }
                            }
                            #endregion
                        }
                    }
                    catch (Exception)
                    {
                        fireTeamItem.EmpImagePath = BLHelper.CONST_NO_EMPLOYEE_PHOTO;
                        fireTeamItem.PhotoTooltip = BLHelper.CONST_NO_PHOTO_MESSAGE;
                    }
                    #endregion

                    #region Set the icon if employee is present or absent
                    if (fireTeamItem.SwipeTime.HasValue &&
                        fireTeamItem.SwipeType.ToUpper() != "OUT")
                    {
                        fireTeamItem.EmpAttendanceFlag = BLHelper.CONST_EMP_PRESENT_ICON;
                        fireTeamItem.EmpAttendanceNotes = BLHelper.CONST_EMP_PRESENT_NOTES;
                        fireTeamItem.IsPresent = true;
                    }
                    else
                    {
                        fireTeamItem.EmpAttendanceFlag = BLHelper.CONST_EMP_ABSENT_ICON;
                        fireTeamItem.EmpAttendanceNotes = BLHelper.CONST_EMP_ABSENT_NOTES;
                        fireTeamItem.IsPresent = false;
                    }
                    #endregion

                    #endregion

                    // Add item to the collection
                    result.Add(fireTeamItem);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static bool CheckFormAccess(string access, FormAccessIndex formAccess)
        {
            bool hasAccess = false;

            int formAccessIndex = Convert.ToInt32(formAccess);
            if (access.Length > formAccessIndex && access.Substring(formAccessIndex, 1) == "1")
                hasAccess = true;

            return hasAccess;
        }
        #endregion

        #region Public Methods
        public EmployeeDetail GetEmployeeDetail(int empNo, ref string error, ref string innerError)
        {
            EmployeeDetail result = null;

            try
            {
                var rawData = TASContext.GetEmployeeInfo(empNo).FirstOrDefault();
                if (rawData != null)
                {
                    result = new EmployeeDetail()
                    {
                        EmpNo = BLHelper.ConvertObjectToInt(rawData.EmpNo),
                        EmpName = BLHelper.ConvertObjectToString(rawData.EmpName),
                        CostCenter = BLHelper.ConvertObjectToString(rawData.BusinessUnit),
                        CostCenterName = BLHelper.ConvertObjectToString(rawData.BusinessUnitName),
                        SupervisorEmpNo = BLHelper.ConvertObjectToInt(rawData.SupervisorNo),
                        SupervisorEmpName = BLHelper.ConvertObjectToString(rawData.SupervisorName),
                        SupervisorFullName = rawData.SupervisorNo.HasValue ? string.Format("({0}) {1}", rawData.SupervisorNo, rawData.SupervisorName) : rawData.SupervisorName,
                        ManagerEmpNo = BLHelper.ConvertObjectToInt(rawData.ManagerNo),
                        ManagerEmpName = BLHelper.ConvertObjectToString(rawData.ManagerName),
                        ManagerFullName = rawData.ManagerNo.HasValue ? string.Format("({0}) {1}", rawData.ManagerNo, rawData.ManagerName) : rawData.ManagerName,
                        Position = BLHelper.ConvertObjectToString(rawData.Position),
                        PhoneExtension = BLHelper.ConvertObjectToString(rawData.Extension),
                        WorkingCostCenter = BLHelper.ConvertObjectToString(rawData.WorkingCostCenter),
                        WorkingCostCenterName = BLHelper.ConvertObjectToString(rawData.WorkingCostCenterName),
                        PayGrade = rawData.GradeCode,
                        EmployeeStatus = BLHelper.ConvertObjectToString(rawData.EmpStatus)
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<AllowedCostCenter> GetPermittedCostCenter(int empNo, ref string error, ref string innerError)
        {
            List<AllowedCostCenter> result = new List<AllowedCostCenter>();

            try
            {
                var rawData = TASContext.GetPermittedCostCenter(empNo);
                if (rawData != null)
                {
                    foreach (var item in rawData)
                    {
                        result.Add(
                            new AllowedCostCenter()
                            {
                                PermitID = item.PermitID,
                                PermitEmpNo = item.PermitEmpNo,
                                PermitAppID = item.PermitAppID,
                                PermitCostCenter = item.PermitCostCenter
                            });
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<AllowedCostCenter> GetPermittedCostCenterByApplication(string appCode, int empNo, ref string error, ref string innerError)
        {
            try
            {
                List<AllowedCostCenter> result = new List<AllowedCostCenter>();

                var rawData = TASContext.GetPermittedCostCenterV2(empNo, appCode);
                if (rawData != null)
                {
                    foreach (var item in rawData)
                    {
                        result.Add(
                            new AllowedCostCenter()
                            {
                                PermitEmpNo = BLHelper.ConvertObjectToInt(item.PermitEmpNo),
                                PermitCostCenter = item.PermitCostCenter
                            });
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<VisitorPassEntity> GetVisitorPassLog(long? logID, string visitorName, string idNumber, int visitorCardNo, int? visitEmpNo, string visitCostCenter,
            DateTime? startDate, DateTime? endDate, byte? blockOption, int? createdByEmpNo, ref string error, ref string innerError)
        {
            try
            {
                List<VisitorPassEntity> result = null;
                var rawData = TASContext.GetVisitorPassLog(logID, visitorName, idNumber, visitorCardNo, visitEmpNo, visitCostCenter, startDate, endDate, blockOption, createdByEmpNo);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<VisitorPassEntity>();

                    foreach (var item in rawData)
                    {
                        VisitorPassEntity visitorItem = new VisitorPassEntity()
                        {
                            LogID = BLHelper.ConvertObjectToLong(item.LogID),
                            VisitorName = BLHelper.ConvertObjectToString(item.VisitorName),
                            IDNumber = BLHelper.ConvertObjectToString(item.IDNumber),
                            VisitorCardNo = item.VisitorCardNo,
                            VisitEmpNo = BLHelper.ConvertObjectToInt(item.VisitEmpNo),
                            VisitEmpName = BLHelper.ConvertObjectToString(item.VisitEmpName),
                            VisitEmpPosition = BLHelper.ConvertObjectToString(item.VisitEmpPosition),
                            VisitEmpExtension = BLHelper.ConvertObjectToString(item.VisitEmpExtension),
                            VisitEmpCostCenter = BLHelper.ConvertObjectToString(item.VisitEmpCostCenter),
                            VisitEmpCostCenterName = BLHelper.ConvertObjectToString(item.VisitEmpCostCenterName),
                            VisitEmpSupervisorNo = BLHelper.ConvertObjectToInt(item.VisitEmpSupervisorNo),
                            VisitEmpSupervisorName = BLHelper.ConvertObjectToString(item.VisitEmpSupervisorName),
                            VisitEmpManagerNo = BLHelper.ConvertObjectToInt(item.VisitEmpManagerNo),
                            VisitEmpManagerName = BLHelper.ConvertObjectToString(item.VisitEmpManagerName),
                            VisitDate = BLHelper.ConvertObjectToDate(item.VisitDate),
                            VisitTimeIn = BLHelper.ConvertObjectToDate(item.VisitTimeIn),
                            VisitTimeOut = BLHelper.ConvertObjectToDate(item.VisitTimeOut),
                            Remarks = BLHelper.ConvertObjectToString(item.Remarks),
                            IsBlock = BLHelper.ConvertObjectToBolean(item.IsBlock),
                            CreatedDate = BLHelper.ConvertObjectToDate(item.CreatedDate),
                            CreatedByEmpNo = BLHelper.ConvertObjectToInt(item.CreatedByEmpNo),
                            CreatedByEmpName = BLHelper.ConvertObjectToString(item.CreatedByEmpName),
                            CreatedByUserID = BLHelper.ConvertObjectToString(item.CreatedByUserID),
                            CreatedByEmpEmail = BLHelper.ConvertObjectToString(item.CreatedByEmpEmail),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(item.LastUpdateTime),
                            LastUpdateEmpNo = BLHelper.ConvertObjectToInt(item.LastUpdateEmpNo),
                            LastUpdateUserID = BLHelper.ConvertObjectToString(item.LastUpdateUserID),
                            LastUpdateEmpName = BLHelper.ConvertObjectToString(item.LastUpdateEmpName),
                            LastUpdateEmpEmail = BLHelper.ConvertObjectToString(item.LastUpdateEmpEmail)
                        };

                        #region Set value to extended properties
                        if (!string.IsNullOrEmpty(visitorItem.VisitEmpCostCenter))
                        {
                            visitorItem.VisitEmpFullCostCenter = string.Format("{0} - {1}",
                                visitorItem.VisitEmpCostCenter,
                                visitorItem.VisitEmpCostCenterName);
                        }

                        if (visitorItem.VisitEmpNo > 0)
                        {
                            visitorItem.VisitEmpFullName = string.Format("({0}) {1}",
                                visitorItem.VisitEmpNo,
                                visitorItem.VisitEmpName);
                        }

                        if (visitorItem.VisitEmpSupervisorNo > 0)
                        {
                            visitorItem.VisitEmpSupervisorFullName = string.Format("({0}) {1}",
                                visitorItem.VisitEmpSupervisorNo,
                                visitorItem.VisitEmpSupervisorName);
                        }

                        if (visitorItem.VisitEmpManagerNo > 0)
                        {
                            visitorItem.VisitEmpManagerFullName = string.Format("({0}) {1}",
                                visitorItem.VisitEmpManagerNo,
                                visitorItem.VisitEmpManagerName);
                        }

                        if (visitorItem.CreatedByEmpNo > 0)
                        {
                            visitorItem.CreatedByFullName = string.Format("({0}) {1}",
                                visitorItem.CreatedByEmpNo,
                                visitorItem.CreatedByEmpName);
                        }

                        if (visitorItem.CreatedByEmpNo > 0)
                        {
                            visitorItem.CreatedByFullName = string.Format("({0}) {1}",
                                visitorItem.CreatedByEmpNo,
                                visitorItem.CreatedByEmpName);
                        }

                        // Set value to extended properties
                        if (visitorItem.LastUpdateEmpNo > 0)
                        {
                            visitorItem.LastUpdateFullName = string.Format("({0}) {1}",
                                visitorItem.LastUpdateEmpNo,
                                visitorItem.LastUpdateEmpName);
                        }

                        visitorItem.IsBlockDesc = visitorItem.IsBlock == true ? "Yes" : "No";
                        #endregion

                        // Add item to the collection
                        result.Add(visitorItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<VisitorPassEntity> GetVisitorPassLogV2(long? logID, string visitorName, string idNumber, int visitorCardNo, int? visitEmpNo, string visitCostCenter,
            DateTime? startDate, DateTime? endDate, byte? blockOption, int? userEmpNo, int? createdByOtherEmpNo, byte? createdByTypeID,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<VisitorPassEntity> result = null;

            try
            {
                #region Initialize parameters
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[14];

                parameters[0] = new ADONetParameter("@logID", SqlDbType.BigInt, logID);
                parameters[1] = new ADONetParameter("@visitorName", SqlDbType.VarChar, 100, visitorName);
                parameters[2] = new ADONetParameter("@idNumber", SqlDbType.VarChar, 50, idNumber);
                parameters[3] = new ADONetParameter("@visitorCardNo", SqlDbType.Int, visitorCardNo);
                parameters[4] = new ADONetParameter("@visitEmpNo", SqlDbType.Int, visitEmpNo);
                parameters[5] = new ADONetParameter("@visitCostCenter", SqlDbType.VarChar, 12, visitCostCenter);
                parameters[6] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[7] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[8] = new ADONetParameter("@blockOption", SqlDbType.TinyInt, blockOption);
                parameters[9] = new ADONetParameter("@userEmpNo", SqlDbType.Int, userEmpNo);
                parameters[10] = new ADONetParameter("@createdByOtherEmpNo", SqlDbType.Int, createdByOtherEmpNo);
                parameters[11] = new ADONetParameter("@createdByTypeID", SqlDbType.TinyInt, createdByTypeID);
                parameters[12] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[13] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);
                #endregion

                DataSet ds = RunSPReturnDataset("tas.Pr_Retrieve_VisitorPassLog_V2", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<VisitorPassEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        #region Populate items to collection
                        VisitorPassEntity visitorItem = new VisitorPassEntity()
                        {
                            LogID = BLHelper.ConvertObjectToLong(row["LogID"]),
                            VisitorName = BLHelper.ConvertObjectToString(row["VisitorName"]),
                            IDNumber = BLHelper.ConvertObjectToString(row["IDNumber"]),
                            VisitorCardNo = BLHelper.ConvertObjectToInt(row["VisitorCardNo"]),
                            VisitEmpNo = BLHelper.ConvertObjectToInt(row["VisitEmpNo"]),
                            VisitEmpName = BLHelper.ConvertObjectToString(row["VisitEmpName"]),
                            VisitEmpPosition = BLHelper.ConvertObjectToString(row["VisitEmpPosition"]),
                            VisitEmpExtension = BLHelper.ConvertObjectToString(row["VisitEmpExtension"]),
                            VisitEmpCostCenter = BLHelper.ConvertObjectToString(row["VisitEmpCostCenter"]),
                            VisitEmpCostCenterName = BLHelper.ConvertObjectToString(row["VisitEmpCostCenterName"]),
                            VisitEmpSupervisorNo = BLHelper.ConvertObjectToInt(row["VisitEmpSupervisorNo"]),
                            VisitEmpSupervisorName = BLHelper.ConvertObjectToString(row["VisitEmpSupervisorName"]),
                            VisitEmpManagerNo = BLHelper.ConvertObjectToInt(row["VisitEmpManagerNo"]),
                            VisitEmpManagerName = BLHelper.ConvertObjectToString(row["VisitEmpManagerName"]),
                            VisitDate = BLHelper.ConvertObjectToDate(row["VisitDate"]),
                            VisitTimeIn = BLHelper.ConvertObjectToDate(row["VisitTimeIn"]),
                            VisitTimeOut = BLHelper.ConvertObjectToDate(row["VisitTimeOut"]),
                            Remarks = BLHelper.ConvertObjectToString(row["Remarks"]),
                            IsBlock = BLHelper.ConvertObjectToBolean(row["IsBlock"]),
                            CreatedDate = BLHelper.ConvertObjectToDate(row["CreatedDate"]),
                            CreatedByEmpNo = BLHelper.ConvertObjectToInt(row["CreatedByEmpNo"]),
                            CreatedByEmpName = BLHelper.ConvertObjectToString(row["CreatedByEmpName"]),
                            CreatedByUserID = BLHelper.ConvertObjectToString(row["CreatedByUserID"]),
                            CreatedByEmpEmail = BLHelper.ConvertObjectToString(row["CreatedByEmpEmail"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            LastUpdateEmpNo = BLHelper.ConvertObjectToInt(row["LastUpdateEmpNo"]),
                            LastUpdateUserID = BLHelper.ConvertObjectToString(row["LastUpdateUserID"]),
                            LastUpdateEmpName = BLHelper.ConvertObjectToString(row["LastUpdateEmpName"]),
                            LastUpdateEmpEmail = BLHelper.ConvertObjectToString(row["LastUpdateEmpEmail"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };
                        #endregion

                        #region Set value to extended properties
                        if (!string.IsNullOrEmpty(visitorItem.VisitEmpCostCenter))
                        {
                            visitorItem.VisitEmpFullCostCenter = string.Format("{0} - {1}",
                                visitorItem.VisitEmpCostCenter,
                                visitorItem.VisitEmpCostCenterName);
                        }

                        if (visitorItem.VisitEmpNo > 0)
                        {
                            visitorItem.VisitEmpFullName = string.Format("({0}) {1}",
                                visitorItem.VisitEmpNo,
                                visitorItem.VisitEmpName);
                        }

                        if (visitorItem.VisitEmpSupervisorNo > 0)
                        {
                            visitorItem.VisitEmpSupervisorFullName = string.Format("({0}) {1}",
                                visitorItem.VisitEmpSupervisorNo,
                                visitorItem.VisitEmpSupervisorName);
                        }

                        if (visitorItem.VisitEmpManagerNo > 0)
                        {
                            visitorItem.VisitEmpManagerFullName = string.Format("({0}) {1}",
                                visitorItem.VisitEmpManagerNo,
                                visitorItem.VisitEmpManagerName);
                        }

                        if (visitorItem.CreatedByEmpNo > 0)
                        {
                            visitorItem.CreatedByFullName = string.Format("({0}) {1}",
                                visitorItem.CreatedByEmpNo,
                                visitorItem.CreatedByEmpName);
                        }

                        if (visitorItem.CreatedByEmpNo > 0)
                        {
                            visitorItem.CreatedByFullName = string.Format("({0}) {1}",
                                visitorItem.CreatedByEmpNo,
                                visitorItem.CreatedByEmpName);
                        }

                        // Set value to extended properties
                        if (visitorItem.LastUpdateEmpNo > 0)
                        {
                            visitorItem.LastUpdateFullName = string.Format("({0}) {1}",
                                visitorItem.LastUpdateEmpNo,
                                visitorItem.LastUpdateEmpName);
                        }

                        visitorItem.IsBlockDesc = visitorItem.IsBlock == true ? "Yes" : "No";
                        #endregion

                        // Add item to collection
                        result.Add(visitorItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void InsertUpdateDeleteVisitorPassLog(int saveTypeID, VisitorPassEntity visitorPassData, ref string error, ref string innerError)
        {
            try
            {
                BLHelper.SaveType saveType = (BLHelper.SaveType)Enum.Parse(typeof(BLHelper.SaveType), saveTypeID.ToString());

                switch (saveType)
                {
                    case BLHelper.SaveType.Insert:
                        #region Perform Insert Operation

                        #region Check for duplicate records
                        List<VisitorPassEntity> duplicateRecordList = new List<VisitorPassEntity>();
                        VisitorPassLog duplicateRecord = null;
                        StringBuilder sbEmployee = new StringBuilder();

                        duplicateRecord = TASContext.VisitorPassLogs
                            .Where(a => a.IDNumber.Trim() == visitorPassData.IDNumber.Trim() &&
                                a.VisitorCardNo == visitorPassData.VisitorCardNo &&
                                a.VisitDate == visitorPassData.VisitDate)
                            .FirstOrDefault();

                        if (duplicateRecord != null)
                        {
                            duplicateRecordList.Add(visitorPassData);

                            if (sbEmployee.Length == 0)
                            {
                                sbEmployee.Append(string.Format("({0}) on {1}",
                                    visitorPassData.VisitorName,
                                    Convert.ToDateTime(visitorPassData.VisitDate).ToString("dd-MMM-yyyy")));
                            }
                            else
                            {
                                sbEmployee.Append(string.Format(", ({0}) on {1}",
                                    visitorPassData.VisitorName,
                                    Convert.ToDateTime(visitorPassData.VisitDate).ToString("dd-MMM-yyyy")));
                            }
                        }

                        if (duplicateRecordList.Count > 0)
                        {
                            error = string.Format("Similar record already exist for the following employees: {0}", sbEmployee.ToString());
                            throw new Exception(string.Format("Similar record already exist for the following employees: {0}", sbEmployee.ToString()));
                        }
                        #endregion

                        #region No duplicate record, proceed in saving to database
                        // Initialize collection
                        VisitorPassLog recordToInsert = new VisitorPassLog()
                        {
                            VisitorName = visitorPassData.VisitorName,
                            IDNumber = visitorPassData.IDNumber,
                            VisitorCardNo = visitorPassData.VisitorCardNo,
                            VisitEmpNo = visitorPassData.VisitEmpNo,
                            VisitDate = Convert.ToDateTime(visitorPassData.VisitDate),
                            VisitTimeIn = visitorPassData.VisitTimeIn,
                            VisitTimeOut = visitorPassData.VisitTimeOut,
                            Remarks = visitorPassData.Remarks,
                            IsBlock = visitorPassData.IsBlock,
                            CreatedDate = visitorPassData.CreatedDate,
                            CreatedByEmpNo = visitorPassData.CreatedByEmpNo,
                            CreatedByEmpName = visitorPassData.CreatedByEmpName,
                            CreatedByEmpEmail = visitorPassData.CreatedByEmpEmail,
                            CreatedByUserID = visitorPassData.CreatedByUserID
                        };

                        // Commit changes in the database
                        TASContext.VisitorPassLogs.Add(recordToInsert);
                        TASContext.SaveChanges();

                        // Get the value of the identity column
                        long newLogID = recordToInsert.LogID;
                        #endregion

                        #region Insert swipe records
                        if (visitorPassData.VisitorSwipeList != null &&
                            visitorPassData.VisitorSwipeList.Count > 0 &&
                            newLogID > 0)
                        {
                            // Initialize swipe collection
                            List<VisitorSwipeLog> swipeRecordToInsertList = new List<VisitorSwipeLog>();

                            foreach (VisitorSwipeEntity item in visitorPassData.VisitorSwipeList)
                            {
                                swipeRecordToInsertList.Add(new VisitorSwipeLog()
                                {
                                    LogID = newLogID,
                                    SwipeDate = Convert.ToDateTime(item.SwipeDate),
                                    SwipeTime = Convert.ToDateTime(item.SwipeTime),
                                    SwipeTypeCode = item.SwipeTypeCode,
                                    CreatedDate = item.CreatedDate,
                                    CreatedByEmpNo = item.CreatedByEmpNo,
                                    CreatedByEmpName = item.CreatedByEmpName,
                                    CreatedByEmpEmail = item.CreatedByEmpEmail,
                                    CreatedByUserID = item.CreatedByUserID
                                });
                            }

                            if (swipeRecordToInsertList.Count > 0)
                            {
                                // Commit changes in the database
                                TASContext.VisitorSwipeLogs.AddRange(swipeRecordToInsertList);
                                TASContext.SaveChanges();
                            }
                        }
                        #endregion

                        break;
                        #endregion

                    case BLHelper.SaveType.Update:
                        #region Perform Update Operation
                        VisitorPassLog recordToUpdate = TASContext.VisitorPassLogs
                                .Where(a => a.LogID == visitorPassData.LogID)
                            .FirstOrDefault();
                        if (recordToUpdate != null)
                        {
                            recordToUpdate.VisitorName = visitorPassData.VisitorName;
                            recordToUpdate.VisitEmpNo = visitorPassData.VisitEmpNo;
                            recordToUpdate.VisitTimeIn = visitorPassData.VisitTimeIn;
                            recordToUpdate.VisitTimeOut = visitorPassData.VisitTimeOut;
                            recordToUpdate.Remarks = visitorPassData.Remarks;
                            recordToUpdate.IsBlock = visitorPassData.IsBlock;
                            recordToUpdate.LastUpdateTime = visitorPassData.LastUpdateTime;
                            recordToUpdate.LastUpdateEmpNo = visitorPassData.LastUpdateEmpNo;
                            recordToUpdate.LastUpdateEmpName = visitorPassData.LastUpdateEmpName;
                            recordToUpdate.LastUpdateEmpEmail = visitorPassData.LastUpdateEmpEmail;
                            recordToUpdate.LastUpdateUserID = visitorPassData.LastUpdateUserID;

                            // Save to database
                            TASContext.SaveChanges();

                            #region Delete existing swipe records
                            List<VisitorSwipeLog> swipeRecordToDeleteList = TASContext.VisitorSwipeLogs
                                .Where(a => a.LogID == recordToUpdate.LogID)
                                .ToList();
                            if (swipeRecordToDeleteList != null &&
                                swipeRecordToDeleteList.Count > 0)
                            {
                                TASContext.VisitorSwipeLogs.RemoveRange(swipeRecordToDeleteList);
                                TASContext.SaveChanges();
                            }
                            #endregion

                            #region Insert swipe records
                            if (visitorPassData.VisitorSwipeList != null &&
                                visitorPassData.VisitorSwipeList.Count > 0)
                            {
                                // Initialize swipe collection                                
                                List<VisitorSwipeEntity> manualSwipeList = visitorPassData.VisitorSwipeList
                                    .Where(a => BLHelper.ConvertObjectToString(a.SwipeCode) == BLHelper.SwipeCode.MANUAL.ToString())
                                    .ToList();
                                if (manualSwipeList != null &&
                                    manualSwipeList.Count > 0)
                                {
                                    List<VisitorSwipeLog> swipeRecordToInsertList = new List<VisitorSwipeLog>();
                                    foreach (VisitorSwipeEntity item in manualSwipeList)
                                    {
                                        swipeRecordToInsertList.Add(new VisitorSwipeLog()
                                        {
                                            LogID = recordToUpdate.LogID,
                                            SwipeDate = Convert.ToDateTime(item.SwipeDate),
                                            SwipeTime = Convert.ToDateTime(item.SwipeTime),
                                            SwipeTypeCode = item.SwipeTypeCode,
                                            CreatedDate = item.CreatedDate,
                                            CreatedByEmpNo = item.CreatedByEmpNo,
                                            CreatedByEmpName = item.CreatedByEmpName,
                                            CreatedByEmpEmail = item.CreatedByEmpEmail,
                                            CreatedByUserID = item.CreatedByUserID
                                        });
                                    }

                                    if (swipeRecordToInsertList.Count > 0)
                                    {

                                        // Commit changes in the database
                                        TASContext.VisitorSwipeLogs.AddRange(swipeRecordToInsertList);
                                        TASContext.SaveChanges();
                                    }
                                }
                            }
                            #endregion
                        }

                        break;
                        #endregion

                    case BLHelper.SaveType.Delete:
                        #region Perform Delete Operation
                        VisitorPassLog recordToDelete = TASContext.VisitorPassLogs
                            .Where(a => a.LogID == visitorPassData.LogID)
                            .FirstOrDefault();
                        if (recordToDelete != null)
                        {
                            TASContext.VisitorPassLogs.Remove(recordToDelete);
                            TASContext.SaveChanges();
                        }

                        #region Delete related swipe records
                        List<VisitorSwipeLog> recordToDeleteList = TASContext.VisitorSwipeLogs
                            .Where(a => a.LogID == visitorPassData.LogID)
                            .ToList();
                        if (recordToDeleteList != null)
                        {
                            TASContext.VisitorSwipeLogs.RemoveRange(recordToDeleteList);
                            TASContext.SaveChanges();
                        }
                        #endregion

                        break;
                        #endregion
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public void DeleteVisitorPassMultipleRecord(List<VisitorPassEntity> visitorRecordList, ref string error, ref string innerError)
        {
            try
            {
                #region Build the collection for deletion
                List<VisitorPassLog> recordToDeleteList = new List<VisitorPassLog>();

                foreach (VisitorPassEntity item in visitorRecordList)
                {
                    VisitorPassLog itemToDelete = TASContext.VisitorPassLogs
                        .Where(a => a.LogID == item.LogID)
                        .FirstOrDefault();
                    if (itemToDelete != null)
                    {
                        TASContext.VisitorPassLogs.Remove(itemToDelete);
                        TASContext.SaveChanges();

                        #region Delete related swipe records
                        List<VisitorSwipeLog> swipeRecordToDeleteList = TASContext.VisitorSwipeLogs
                            .Where(a => a.LogID == itemToDelete.LogID)
                            .ToList();
                        if (recordToDeleteList != null)
                        {
                            TASContext.VisitorSwipeLogs.RemoveRange(swipeRecordToDeleteList);
                            TASContext.SaveChanges();
                        }
                        #endregion
                    }
                }
                #endregion
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public List<EmployeeAttendanceEntity> GetSwipeHistory(DateTime? startDate, DateTime? endDate, int? empNo, string costCenter, string locationName, 
            string readerName, ref string error, ref string innerError)
        {
            try
            {
                List<EmployeeAttendanceEntity> result = null;
                var rawData = TASContext.GetEmployeeSwipeHistory(startDate, endDate, empNo, costCenter, locationName, readerName);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (var item in rawData)
                    {
                        EmployeeAttendanceEntity attendanceItem = new EmployeeAttendanceEntity()
                        {
                            SwipeDate = BLHelper.ConvertObjectToDate(item.SwipeDate),
                            EmpNo = BLHelper.ConvertObjectToInt(item.EmpNo),
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                            Position = BLHelper.ConvertObjectToString(item.Position),
                            SwipeLocation = BLHelper.ConvertObjectToString(item.SwipeLocation),
                            SwipeType = BLHelper.ConvertObjectToString(item.SwipeType),
                            SwipeTime = BLHelper.ConvertObjectToDate(item.SwipeTime),
                            ShiftPatCode = BLHelper.ConvertObjectToString(item.ShiftPatCode),
                            ShiftCode = BLHelper.ConvertObjectToString(item.ShiftCode),
                            ShiftPointer = BLHelper.ConvertObjectToInt(item.ShiftPointer),
                            CostCenter = BLHelper.ConvertObjectToString(item.CostCenter),
                            CostCenterName = BLHelper.ConvertObjectToString(item.CostCenterName),
                            CostCenterFullName = string.Format("{0} - {1}",
                                BLHelper.ConvertObjectToString(item.CostCenter),
                                BLHelper.ConvertObjectToString(item.CostCenterName))
                        };

                        // Add item to the collection
                        result.Add(attendanceItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeDetail> GetEmployeeInfoFromJDE(int empNo, string costCenter, bool? isActiveOnly, ref string error, ref string innerError)
        {
            List<EmployeeDetail> employeeList = null;

            try
            {
                var rawData = TASContext.GetEmployeeInfoJDE(empNo, costCenter, isActiveOnly);
                if (rawData != null)
                {
                    // Initialize collection
                    employeeList = new List<EmployeeDetail>();

                    foreach (var item in rawData)
                    {
                        employeeList.Add(new EmployeeDetail()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(item.EmpNo),
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                            Position = BLHelper.ConvertObjectToString(item.Position),
                            CostCenter = BLHelper.ConvertObjectToString(item.CostCenter),
                            CostCenterName = BLHelper.ConvertObjectToString(item.CostCenterName),
                            CostCenterFullName = string.Format("{0} - {1}",
                                BLHelper.ConvertObjectToString(item.CostCenter),
                                BLHelper.ConvertObjectToString(item.CostCenterName)),
                            ActualCostCenter = BLHelper.ConvertObjectToString(item.ActualCostCenter),
                            PayGrade = BLHelper.ConvertObjectToInt(item.GradeCode),
                            DateJoined = BLHelper.ConvertObjectToDate(item.DateJoined),
                            YearsOfService = BLHelper.ConvertObjectToDouble(item.YearsOfService)
                        });
                    }
                    
                }

                return employeeList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<AccessReaderEntity> GetAccessReaders(int loadType, int locationCode, int readerNo, ref string error, ref string innerError)
        {
            List<AccessReaderEntity> result = null;

            try
            {
                var rawData = TASContext.GetAccessReaders(loadType, locationCode, readerNo);
                if (rawData != null)
                {
                    result = new List<AccessReaderEntity>();

                    int counter = 1;
                    foreach (var item in rawData)
                    {
                        result.Add(new AccessReaderEntity()
                        {
                            AutoID = counter,
                            LocationName = BLHelper.ConvertObjectToString(item.LocationName),
                            ReaderName = BLHelper.ConvertObjectToString(item.ReaderName),
                            LocationFullName = string.Format("{0} - {1}",
                                BLHelper.ConvertObjectToString(item.LocationName),
                                BLHelper.ConvertObjectToString(item.ReaderName))
                        });

                        counter++;
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetAbsencesHistory(int? empNo, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError)
        {
            try
            {
                List<EmployeeAttendanceEntity> result = null;
                var rawData = TASContext.GetAbsencesHistory(empNo, startDate, endDate);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (var item in rawData)
                    {
                        EmployeeAttendanceEntity attendanceItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(item.AutoID),
                            EmpNo = BLHelper.ConvertObjectToInt(item.EmpNo),
                            DT = BLHelper.ConvertObjectToDate(item.DT),
                            RemarkCode = BLHelper.ConvertObjectToString(item.RemarkCode),
                            AttendanceRemarks = BLHelper.ConvertObjectToString(item.Remarks)
                        };

                        // Add item to the collection
                        result.Add(attendanceItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetAbsencesHistoryv2(int empNo, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[5];
                parameters[0] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[1] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[2] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[3] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[4] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetAbsencesHistory_V2", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            RemarkCode = BLHelper.ConvertObjectToString(row["RemarkCode"]),
                            AttendanceRemarks = BLHelper.ConvertObjectToString(row["Remarks"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetLeaveHistory(int? empNo, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError)
        {
            try
            {
                List<EmployeeAttendanceEntity> result = null;
                var rawData = TASContext.GetLeaveHistory(empNo, startDate, endDate);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (var item in rawData)
                    {
                        EmployeeAttendanceEntity attendanceItem = new EmployeeAttendanceEntity()
                        {
                            LeaveNo = item.LeaveNo,
                            LeaveStartDate = item.LeaveStartDate,
                            LeaveEndDate = item.LeaveEndDate,
                            EmpNo = BLHelper.ConvertObjectToInt(item.EmpNo),
                            LeaveType = BLHelper.ConvertObjectToString(item.LeaveType),
                            LeaveTypeDesc = BLHelper.ConvertObjectToString(item.LeaveTypeDesc),
                            LeaveDuration = item.LeaveDuration
                        };

                        // Add item to the collection
                        result.Add(attendanceItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetLeaveHistoryV2(int empNo, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[5];
                parameters[0] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[1] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[2] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[3] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[4] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetLeaveHistory_V2", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            LeaveNo = BLHelper.ConvertObjectToInt(row["LeaveNo"]),
                            LeaveStartDate = BLHelper.ConvertObjectToDate(row["LeaveStartDate"]),
                            LeaveEndDate = BLHelper.ConvertObjectToDate(row["LeaveEndDate"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            LeaveType = BLHelper.ConvertObjectToString(row["LeaveType"]),
                            LeaveTypeDesc = BLHelper.ConvertObjectToString(row["LeaveTypeDesc"]),
                            LeaveDuration = BLHelper.ConvertObjectToDouble(row["LeaveDuration"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetAttendanceHistory(int? empNo, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError)
        {
            try
            {
                List<EmployeeAttendanceEntity> result = null;
                var rawData = TASContext.GetAttendanceHistory(empNo, startDate, endDate);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (var item in rawData)
                    {
                        EmployeeAttendanceEntity attendanceItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = item.AutoID,
                            EmpNo = item.EmpNo,
                            CostCenter = BLHelper.ConvertObjectToString(item.BusinessUnit),
                            CorrectionCode = BLHelper.ConvertObjectToString(item.CorrectionCode),
                            DT = item.DT,
                            dtIN = item.dtIN,
                            dtOUT = item.dtOUT,
                            ShavedTimeIn = item.Shaved_IN,
                            ShavedTimeOut = item.Shaved_OUT,
                            ShiftPatCode = BLHelper.ConvertObjectToString(item.ShiftPatCode),
                            ShiftCode = BLHelper.ConvertObjectToString(item.ShiftCode),
                            ActualShiftCode = BLHelper.ConvertObjectToString(item.Actual_ShiftCode),
                            OTType = BLHelper.ConvertObjectToString(item.OTtype),
                            OTStartTime = item.OTstartTime,
                            OTEndTime = item.OTendTime,
                            NoPayHours = item.NoPayHours,
                            AbsenceReasonCode = BLHelper.ConvertObjectToString(item.AbsenceReasonCode),
                            LeaveType = BLHelper.ConvertObjectToString(item.LeaveType),
                            DILEntitlement = BLHelper.ConvertObjectToString(item.DIL_Entitlement),
                            RemarkCode = BLHelper.ConvertObjectToString(item.RemarkCode),
                            LastUpdateUser = BLHelper.ConvertObjectToString(item.LastUpdateUser),
                            LastUpdateTime = item.LastUpdateTime
                        };

                        // Add item to the collection
                        result.Add(attendanceItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }                

        public List<EmployeeAttendanceEntity> GetAttendanceHistoryV2(int empNo, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[5];
                parameters[0] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[1] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[2] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[3] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[4] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetAttendanceHistory_V2", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            CorrectionCode = BLHelper.ConvertObjectToString(row["CorrectionCode"]),
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIN"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOUT"]),
                            DurationHourString = BLHelper.ConvertMinuteToHourString(row["Duration_Worked_Cumulative"]),
                            ShavedTimeIn = BLHelper.ConvertObjectToDate(row["Shaved_IN"]),
                            ShavedTimeOut = BLHelper.ConvertObjectToDate(row["Shaved_OUT"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                            ActualShiftCode = BLHelper.ConvertObjectToString(row["Actual_ShiftCode"]),
                            OTType = BLHelper.ConvertObjectToString(row["OTtype"]),
                            OTStartTime = BLHelper.ConvertObjectToDate(row["OTstartTime"]),
                            OTEndTime = BLHelper.ConvertObjectToDate(row["OTendTime"]),
                            NoPayHours = BLHelper.ConvertObjectToInt(row["NoPayHours"]),
                            AbsenceReasonCode = BLHelper.ConvertObjectToString(row["AbsenceReasonCode"]),
                            LeaveType = BLHelper.ConvertObjectToString(row["LeaveType"]),
                            DILEntitlement = BLHelper.ConvertObjectToString(row["DIL_Entitlement"]),
                            RemarkCode = BLHelper.ConvertObjectToString(row["RemarkCode"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            IsLastRow = BLHelper.ConvertObjectToBolean(row["IsLastRow"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<LeaveEntity> GetLeaveDetails(int? empNo, ref string error, ref string innerError)
        {
            List<LeaveEntity> result = null;

            try
            {
                var rawData = TASContext.GetLeaveInformation(empNo);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<LeaveEntity>();

                    foreach (var item in rawData)
                    {
                        LeaveEntity attendanceItem = new LeaveEntity()
                        {
                            LeaveEmpNo = BLHelper.ConvertObjectToInt(item.LeaveEmpNo),
                            LeaveOpeningDate = item.LeaveOpeningDate,
                            LeaveEndingDate = item.LeaveEndingDate,
                            LeaveEmpServiceDate = item.LeaveEmpServiceDate,
                            LeaveEntitlement = BLHelper.ConvertObjectToString(item.LeaveEntitlement),
                            LeaveTakenAsOfDate = BLHelper.ConvertObjectToString(item.LeaveTakenAsOfDate),
                            LeaveTakenCurrentYear = BLHelper.ConvertObjectToString(item.LeaveTakenCurrentYear),
                            LeaveCurrentBal = BLHelper.ConvertObjectToString(item.LeaveCurrentBal)
                        };

                        // Add item to the collection
                        result.Add(attendanceItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<DILEntity> GetDILEntitlements(byte loadType, int empNo, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError)
        {
            List<DILEntity> result = null;

            try
            {
                var rawData = TASContext.GetDILEntitlements(loadType, empNo, startDate, endDate);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<DILEntity>();

                    foreach (var item in rawData)
                    {
                        DILEntity attendanceItem = new DILEntity()
                        {
                            AutoID = item.AutoID,
                            EmpNo = item.EmpNo,
                            EntitlementDate = item.EntitlementDate,
                            DILCode = BLHelper.ConvertObjectToString(item.DILcode),
                            Remarks = BLHelper.ConvertObjectToString(item.Remark)
                        };

                        // Add item to the collection
                        result.Add(attendanceItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<ShiftPatternEntity> GetShiftPatternInfo(int? empNo, ref string error, ref string innerError)
        {
            try
            {
                List<ShiftPatternEntity> result = null;
                var rawData = TASContext.GetShiftPatternInfo(empNo);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<ShiftPatternEntity>();

                    foreach (var item in rawData)
                    {
                        ShiftPatternEntity attendanceItem = new ShiftPatternEntity()
                        {
                            AutoID = item.AutoID,
                            EmpNo = item.EmpNo,
                            ShiftPatCode = BLHelper.ConvertObjectToString(item.ShiftPatCode),
                            ShiftCode = BLHelper.ConvertObjectToString(item.ShiftCode),
                            ShiftCodeArray = BLHelper.ConvertObjectToString(item.ShiftCodeArray),
                            ShiftPointer = BLHelper.ConvertObjectToInt(item.ShiftPointer),
                            WorkingCostCenterFullName = BLHelper.ConvertObjectToString(item.WorkingBusinessUnit) != string.Empty
                                ? string.Format("{0} - {1}", BLHelper.ConvertObjectToString(item.WorkingBusinessUnit), BLHelper.ConvertObjectToString(item.WorkingBusinessUnitName))
                                : string.Empty,
                            LastUpdateUser = BLHelper.ConvertObjectToString(item.LastUpdateUser),
                            LastUpdateTime = item.LastUpdateTime
                        };

                        // Add item to the collection
                        result.Add(attendanceItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<LeaveEntity> GetGARMCOCalendar(int? year, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError)
        {
            try
            {
                List<LeaveEntity> result = null;
                var rawData = TASContext.GetGARMCOCalendar(year, startDate, endDate);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<LeaveEntity>();

                    foreach (var item in rawData)
                    {
                        LeaveEntity attendanceItem = new LeaveEntity()
                        {
                            HolidayDate = item.HolidayDate,
                            HolidayType = BLHelper.ConvertObjectToString(item.HolidayType),
                            HolidayName = BLHelper.ConvertObjectToString(item.HolidayName),
                            DOW = BLHelper.ConvertObjectToString(item.DOW)
                        };

                        // Add item to the collection
                        result.Add(attendanceItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<CostCenterEntity> GetCostCenterManagerInfo(string costCenter, string companyCode, ref string error, ref string innerError)
        {
            try
            {
                List<CostCenterEntity> result = null;
                var rawData = TASContext.GetCostCenterManager(costCenter, companyCode);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<CostCenterEntity>();

                    foreach (var item in rawData)
                    {
                        CostCenterEntity collectionItem = new CostCenterEntity()
                        {
                            CompanyCode = BLHelper.ConvertObjectToString(item.CompanyCode),
                            CompanyName = BLHelper.ConvertObjectToString(item.CompanyName),
                            CostCenter = BLHelper.ConvertObjectToString(item.CostCenter),
                            CostCenterName = BLHelper.ConvertObjectToString(item.CostCenterName),
                            SuperintendentEmpNo = BLHelper.ConvertObjectToInt(item.SuperintendentEmpNo),
                            SuperintendentEmpName = BLHelper.ConvertObjectToString(item.SuperintendentEmpName),
                            ManagerEmpNo = BLHelper.ConvertObjectToInt(item.ManagerEmpNo),
                            ManagerEmpName = BLHelper.ConvertObjectToString(item.ManagerEmpName),
                        };

                        if (collectionItem.SuperintendentEmpNo > 0 &&
                            !string.IsNullOrEmpty(collectionItem.SuperintendentEmpName))
                        {
                            collectionItem.SuperintendentFullName = string.Format("{0} - {1}",
                                collectionItem.SuperintendentEmpNo,
                                collectionItem.SuperintendentEmpName);
                        }

                        if (collectionItem.ManagerEmpNo > 0 &&
                            !string.IsNullOrEmpty(collectionItem.ManagerEmpName))
                        {
                            collectionItem.ManagerFullName = string.Format("{0} - {1}",
                                collectionItem.ManagerEmpNo,
                                collectionItem.ManagerEmpName);
                        }

                        // Add item to the collection
                        result.Add(collectionItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<DependentEntity> GetDependentInfo(int empNo, ref string error, ref string innerError)
        {
            try
            {
                List<DependentEntity> result = null;
                var rawData = TASContext.GetDependentInfo(empNo);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<DependentEntity>();

                    foreach (var item in rawData)
                    {
                        DependentEntity collectionItem = new DependentEntity()
                        {
                            EmpNo = empNo,
                            DependentNo = item.DependentNo,
                            DependentName = BLHelper.ConvertObjectToString(item.DependentName),
                            Relationship = BLHelper.ConvertObjectToString(item.Relationship),
                            RelationshipID = BLHelper.ConvertObjectToString(item.RelationshipID),
                            DOB = item.DOB,
                            CPRNo = BLHelper.ConvertObjectToString(item.CPRNo),
                            CPRExpDate = item.CPRExpDate,
                            ResPermitExpDate = item.ResPermitExpDate,
                            Sex = BLHelper.ConvertObjectToString(item.Sex)
                        };

                        // Add item to the collection
                        result.Add(collectionItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<ShiftPatternEntity> GetShiftPatternChanges(int? autoID, byte? loadType, int? empNo, string changeType, 
            DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<ShiftPatternEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[8];
                parameters[0] = new ADONetParameter("@autoID", SqlDbType.Int, autoID);
                parameters[1] = new ADONetParameter("@loadType", SqlDbType.TinyInt, loadType);
                parameters[2] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[3] = new ADONetParameter("@changeType", SqlDbType.VarChar, 10, changeType);
                parameters[4] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[5] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[6] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[7] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetShiftPatternChanges", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<ShiftPatternEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ShiftPatternEntity newItem = new ShiftPatternEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            EffectiveDate = BLHelper.ConvertObjectToDate(row["EffectiveDate"]),
                            EndingDate = BLHelper.ConvertObjectToDate(row["EndingDate"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftPointer = BLHelper.ConvertObjectToInt(row["ShiftPointer"]),
                            ChangeType = BLHelper.ConvertObjectToString(row["ChangeType"]),
                            ChangeTypeDesc = BLHelper.ConvertObjectToString(row["ChangeTypeDesc"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["BusinessUnitName"]),
                            SupervisorNo = BLHelper.ConvertObjectToInt(row["SupervisorEmpNo"]),
                            SupervisorName = BLHelper.ConvertObjectToString(row["SupervisorEmpName"])
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                            !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }

                        if (newItem.EmpNo > 0)
                        {
                            newItem.EmpFullName = string.Format("{0} - {1}",
                                newItem.EmpNo,
                                newItem.EmpName);
                        }
                        else
                            newItem.EmpFullName = newItem.EmpName;

                        if (newItem.SupervisorNo > 0)
                        {
                            newItem.SupervisorFullName = string.Format("{0} - {1}",
                                newItem.SupervisorNo,
                                newItem.SupervisorName);
                        }
                        else
                            newItem.SupervisorFullName = newItem.SupervisorName;

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void InsertUpdateDeleteShiftPattern(int saveTypeNum, List<ShiftPatternEntity> shiftPatternList, ref string error, ref string innerError)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                BLHelper.SaveType saveType = (BLHelper.SaveType)Enum.Parse(typeof(BLHelper.SaveType), saveTypeNum.ToString());

                switch(saveType)
                {
                    case BLHelper.SaveType.Insert:
                        #region Perform Insert Operation

                        #region Check for duplicate records
                        List<ShiftPatternEntity> duplicateRecordList = new List<ShiftPatternEntity>();
                        Tran_ShiftPatternChanges duplicateRecord = null;

                        foreach (ShiftPatternEntity item in shiftPatternList)
                        {
                            duplicateRecord = TASContext.Tran_ShiftPatternChanges
                                .Where(a => a.EmpNo == item.EmpNo &&
                                    a.EffectiveDate == item.EffectiveDate &&
                                    a.ShiftPatCode == item.ShiftPatCode &&
                                    a.ShiftPointer == item.ShiftPointer &&
                                    a.ChangeType == item.ChangeType)
                                .FirstOrDefault();
                            if (duplicateRecord != null)
                            {
                                duplicateRecordList.Add(item);

                                if (sb.Length == 0)
                                {
                                    sb.Append(string.Format("Emp. No.: {0}; Shift Pat. Code: {1}; Shift Pointer: {2}; Effective Date: {3}",
                                        item.EmpNo,
                                        item.ShiftPatCode,
                                        item.ShiftPointer,
                                        item.EffectiveDate));
                                }
                                else
                                {
                                    sb.Append(string.Format(", Emp. No.: {0}; Shift Pat. Code: {1}; Shift Pointer: {2}; Effective Date: {3}",
                                       item.EmpNo,
                                        item.ShiftPatCode,
                                        item.ShiftPointer,
                                        item.EffectiveDate));
                                }
                            }
                        }

                        if (duplicateRecordList.Count > 0)
                        {
                            error = string.Format("The following records already exist: {0}", sb.ToString());
                            throw new Exception(string.Format("The following records already exist: {0}", sb.ToString()));
                        }
                        #endregion

                        #region No duplicates, proceed in saving to database
                        List<Tran_ShiftPatternChanges> recordToInsertList = new List<Tran_ShiftPatternChanges>();
                        foreach (ShiftPatternEntity item in shiftPatternList)
                        {
                            recordToInsertList.Add(new Tran_ShiftPatternChanges()
                            {
                                EmpNo = item.EmpNo,
                                EffectiveDate = BLHelper.ConvertObjectToRealDate(item.EffectiveDate),
                                EndingDate = item.EndingDate,
                                ShiftPatCode = item.ShiftPatCode,
                                ShiftPointer = item.ShiftPointer,
                                ChangeType = item.ChangeType,
                                LastUpdateUser = item.LastUpdateUser,
                                LastUpdateTime = item.LastUpdateTime
                            });
                        }

                        // Save to database
                        if (recordToInsertList.Count > 0)
                        {
                            TASContext.Tran_ShiftPatternChanges.AddRange(recordToInsertList);
                            TASContext.SaveChanges();
                        }
                        #endregion

                        break;
                        #endregion

                    case BLHelper.SaveType.Update:
                        #region Perform Update Operation
                        foreach (ShiftPatternEntity item in shiftPatternList)
                        {
                            Tran_ShiftPatternChanges recordToUpdate = TASContext.Tran_ShiftPatternChanges
                                .Where(a => a.AutoID == item.AutoID)
                                .FirstOrDefault();
                            if (recordToUpdate != null)
                            {
                                recordToUpdate.EffectiveDate = BLHelper.ConvertObjectToRealDate(item.EffectiveDate);
                                recordToUpdate.EndingDate = item.EndingDate;
                                recordToUpdate.ShiftPatCode = item.ShiftPatCode;
                                recordToUpdate.ShiftPointer = item.ShiftPointer;
                                recordToUpdate.ChangeType = item.ChangeType;
                                recordToUpdate.LastUpdateUser = item.LastUpdateUser;
                                recordToUpdate.LastUpdateTime = item.LastUpdateTime;

                                // Save to database
                                TASContext.SaveChanges();
                            }
                        }

                        break;
                    #endregion

                    case BLHelper.SaveType.Delete:
                        #region Perform Delete Operation
                        List<Tran_ShiftPatternChanges> recordToDeleteList = new List<Tran_ShiftPatternChanges>();

                        foreach (ShiftPatternEntity item in shiftPatternList)
                        {
                            Tran_ShiftPatternChanges recordToDelete = TASContext.Tran_ShiftPatternChanges
                                .Where(a => a.AutoID == item.AutoID)
                                .FirstOrDefault();
                            if (recordToDelete != null)
                            {
                                // Add to collection
                                recordToDeleteList.Add(recordToDelete);
                            }
                        }

                        // Save to database
                        if (recordToDeleteList.Count > 0)
                        {
                            TASContext.Tran_ShiftPatternChanges.RemoveRange(recordToDeleteList);
                            TASContext.SaveChanges();
                        }

                        break;
                        #endregion
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public List<ShiftPatternEntity> GetShiftPatternCodes(string shiftPatCode, ref string error, ref string innerError)
        {
            try
            {
                List<ShiftPatternEntity> result = null;
                var rawData = TASContext.GetShiftPatternCodesV2(shiftPatCode);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<ShiftPatternEntity>();

                    foreach (var item in rawData)
                    {
                        ShiftPatternEntity collectionItem = new ShiftPatternEntity()
                        {
                            ShiftPatCode = BLHelper.ConvertObjectToString(item.Code),
                            ShiftPatDesc = BLHelper.ConvertObjectToString(item.Description),
                            RestrictionType = BLHelper.ConvertObjectToByte(item.RestrictionType),
                            RestrictedEmpNoArray = BLHelper.ConvertObjectToString(item.RestrictedEmpNoArray),
                            RestrictedCostCenterArray = BLHelper.ConvertObjectToString(item.RestrictedCostCenterArray),
                            RestrictionMessage = BLHelper.ConvertObjectToString(item.RestrictionMessage)
                        };

                        // Add item to the collection
                        result.Add(collectionItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<ShiftPatternEntity> GetShiftPointerCodes(string shiftPatCode, ref string error, ref string innerError)
        {
            try
            {
                List<ShiftPatternEntity> result = null;
                var rawData = TASContext.GetShiftPointer(shiftPatCode);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<ShiftPatternEntity>();

                    foreach (var item in rawData)
                    {
                        ShiftPatternEntity collectionItem = new ShiftPatternEntity()
                        {
                            ShiftPointer = BLHelper.ConvertObjectToInt(item.ShiftPointer),
                            ShiftPointerCode = BLHelper.ConvertObjectToString(item.PointerCode)
                        };

                        // Add item to the collection
                        result.Add(collectionItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeDetail> GetFireTeamMember( ref string error, ref string innerError)
        {
            try
            {
                List<EmployeeDetail> result = null;
                var rawData = TASContext.GetFireTeamMember();
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<EmployeeDetail>();

                    foreach (var item in rawData)
                    {
                        EmployeeDetail collectionItem = new EmployeeDetail()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(item.EmpNo),
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName)
                        };

                        // Add item to the collection
                        result.Add(collectionItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeDetail> GetWorkingCostCenter(int autoID, int empNo, string costCenter, string specialJobCatg, ref string error, ref string innerError)
        {
            try
            {
                List<EmployeeDetail> result = null;
                var rawData = TASContext.GetWorkingCostCenter(autoID, empNo, costCenter, specialJobCatg);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<EmployeeDetail>();

                    foreach (var item in rawData)
                    {
                        EmployeeDetail collectionItem = new EmployeeDetail()
                        {
                            AutoID = item.AutoID,
                            EmpNo = item.EmpNo,
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                            Position = BLHelper.ConvertObjectToString(item.Position),
                            ShiftPatCode = BLHelper.ConvertObjectToString(item.ShiftPatCode),
                            ShiftPointer = BLHelper.ConvertObjectToInt(item.ShiftPointer),
                            CostCenter = BLHelper.ConvertObjectToString(item.CostCenter),
                            CostCenterName = BLHelper.ConvertObjectToString(item.CostCenterName),
                            CostCenterFullName = string.Format("{0} - {1}",
                                BLHelper.ConvertObjectToString(item.CostCenter),
                                BLHelper.ConvertObjectToString(item.CostCenterName)),
                            WorkingCostCenter = BLHelper.ConvertObjectToString(item.WorkingBusinessUnit),
                            WorkingCostCenterName = BLHelper.ConvertObjectToString(item.WorkingBusinessUnitName),
                            WorkingCostCenterFullName = string.Format("{0} - {1}",
                                BLHelper.ConvertObjectToString(item.WorkingBusinessUnit),
                                BLHelper.ConvertObjectToString(item.WorkingBusinessUnitName)),
                            SpecialJobCatg = BLHelper.ConvertObjectToString(item.SpecialJobCatg),
                            SpecialJobCatgDesc = BLHelper.ConvertObjectToString(item.SpecialJobCatgDesc),
                            LastUpdateUser = BLHelper.ConvertObjectToString(item.LastUpdateUser),
                            LastUpdateTime = item.LastUpdateTime,
                            CatgEffectiveDate = item.CatgEffectiveDate,
                            CatgEndingDate = item.CatgEndingDate
                        };

                        // Add item to the collection
                        result.Add(collectionItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<UDCEntity> GetUDCListItem(string udcKey, ref string error, ref string innerError)
        {
            try
            {
                List<UDCEntity> result = null;
                var rawData = TASContext.GetUDCListItem(udcKey);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<UDCEntity>();

                    foreach (var item in rawData)
                    {
                        UDCEntity collectionItem = new UDCEntity()
                        {
                            UDCKey = BLHelper.ConvertObjectToString(item.UDCKey),
                            Code = BLHelper.ConvertObjectToString(item.Code),
                            Description = BLHelper.ConvertObjectToString(item.Description),
                            Description2 = BLHelper.ConvertObjectToString(item.Description2),
                            FieldRef = BLHelper.ConvertObjectToString(item.FieldRef)
                        };

                        // Add item to the collection
                        result.Add(collectionItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void InsertUpdateDeleteWorkingCostCenter(int saveTypeID, List<EmployeeDetail> empDetailList, ref string error, ref string innerError)
        {
            try
            {
                BLHelper.SaveType saveType = (BLHelper.SaveType)Enum.Parse(typeof(BLHelper.SaveType), saveTypeID.ToString());

                switch (saveType)
                {
                    case BLHelper.SaveType.Insert:
                        #region Perform Insert Operation

                        #region Check for duplicate records
                        List<EmployeeDetail> duplicateRecordList = new List<EmployeeDetail>();
                        Master_EmployeeAdditional duplicateRecord = null;
                        StringBuilder sbEmployee = new StringBuilder();

                        foreach (EmployeeDetail item in empDetailList)
                        {
                            duplicateRecord = TASContext.Master_EmployeeAdditional
                                .Where(a => a.EmpNo == item.EmpNo)
                                .FirstOrDefault();

                            if (duplicateRecord != null)
                            {
                                duplicateRecordList.Add(item);

                                if (sbEmployee.Length == 0)
                                {
                                    sbEmployee.Append(string.Format("({0}) on {1}",
                                        item.EmpNo,
                                        item.EmpName));
                                }
                                else
                                {
                                    sbEmployee.Append(string.Format(", ({0}) on {1}",
                                        item.EmpNo,
                                        item.EmpName));
                                }
                            }
                        }

                        if (duplicateRecordList.Count > 0)
                        {
                            error = string.Format("Similar record already exist for the following employees: {0}", sbEmployee.ToString());
                            throw new Exception(string.Format("Similar record already exist for the following employees: {0}", sbEmployee.ToString()));
                        }
                        #endregion

                        #region No duplicate record, proceed in saving to database
                        // Initialize collection
                        List<Master_EmployeeAdditional> recordsToInsertList = new List<Master_EmployeeAdditional>();

                        foreach (EmployeeDetail item in empDetailList)
                        {
                            recordsToInsertList.Add(new Master_EmployeeAdditional()
                            {
                                EmpNo = item.EmpNo,
                                ShiftPatCode = item.ShiftPatCode,
                                ShiftPointer = item.ShiftPointer,
                                WorkingBusinessUnit = item.WorkingCostCenter,
                                SpecialJobCatg = item.SpecialJobCatg,
                                CatgEffectiveDate = item.CatgEffectiveDate,
                                CatgEndingDate = item.CatgEndingDate,
                                LastUpdateUser = item.LastUpdateUser,
                                LastUpdateTime = item.LastUpdateTime
                            });
                        }

                        // Commit changes in the database
                        TASContext.Master_EmployeeAdditional.AddRange(recordsToInsertList);
                        TASContext.SaveChanges();
                        #endregion

                        break;
                        #endregion

                    case BLHelper.SaveType.Update:
                        #region Perform Update Operation
                        foreach (EmployeeDetail item in empDetailList)
                        {
                            Master_EmployeeAdditional recordToUpdate = TASContext.Master_EmployeeAdditional
                                 .Where(a => a.EmpNo == item.EmpNo)
                                .FirstOrDefault();
                            if (recordToUpdate != null)
                            {
                                recordToUpdate.WorkingBusinessUnit = item.WorkingCostCenter;
                                recordToUpdate.SpecialJobCatg = item.SpecialJobCatg;
                                recordToUpdate.CatgEffectiveDate = item.CatgEffectiveDate;
                                recordToUpdate.CatgEndingDate = item.CatgEndingDate;
                                recordToUpdate.LastUpdateUser = item.LastUpdateUser;
                                recordToUpdate.LastUpdateTime = item.LastUpdateTime;

                                // Save to database
                                TASContext.SaveChanges();
                            }
                        }

                        break;
                    #endregion

                    case BLHelper.SaveType.Delete:
                        #region Perform Delete Operation
                        List<Master_EmployeeAdditional> recordsToDeleteList = new List<Master_EmployeeAdditional>();

                        foreach (EmployeeDetail item in empDetailList)
                        {
                            Master_EmployeeAdditional recordToDelete = TASContext.Master_EmployeeAdditional
                                 .Where(a => a.EmpNo == item.EmpNo)
                                .FirstOrDefault();
                            if (recordToDelete != null)
                            {
                                // Add to collection
                                recordsToDeleteList.Add(recordToDelete);
                            }
                        }

                        // Save to database
                        if (recordsToDeleteList.Count > 0)
                        {
                            TASContext.Master_EmployeeAdditional.RemoveRange(recordsToDeleteList);
                            TASContext.SaveChanges();
                        }

                        break;
                        #endregion
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public List<EmployeeAttendanceEntity> GetEmployeeExceptional(DateTime? startDate, DateTime? endDate, int empNo, 
            bool isAbsence, bool isSickLeave, bool isNPH, bool isInjuryLeave, bool isDIL, bool isOvertime, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[9];
                parameters[0] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[1] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[2] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);                
                parameters[3] = new ADONetParameter("@isAbsence", SqlDbType.Bit, isAbsence);
                parameters[4] = new ADONetParameter("@isSickLeave", SqlDbType.Bit, isSickLeave);
                parameters[5] = new ADONetParameter("@isNPH", SqlDbType.Bit, isNPH);
                parameters[6] = new ADONetParameter("@isInjuryLeave", SqlDbType.Bit, isInjuryLeave);
                parameters[7] = new ADONetParameter("@isDIL", SqlDbType.Bit, isDIL);
                parameters[8] = new ADONetParameter("@isOvertime", SqlDbType.Bit, isOvertime);

                DataSet ds = RunSPReturnDataset("tas.Pr_EmployeeExceptional", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            DT = BLHelper.ConvertObjectToDate(row["Date"]),
                            Reason = BLHelper.ConvertObjectToString(row["Reason"])
                        };

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetOnAnnualLeaveBuSwiped(DateTime? startDate, DateTime? endDate, int empNo, string costCenter,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[6];
                parameters[0] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[1] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[2] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[3] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[4] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[5] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetAnnualLeaveButSwiped_V2", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["CostCenterName"]),
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIN"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOUT"]),
                            Duration = BLHelper.ConvertObjectToInt(row["Duration"]),
                            HasMultipleSwipe = BLHelper.ConvertNumberToBolean(row["HasMultipleSwipe"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                            ActualShiftCode = BLHelper.ConvertObjectToString(row["Actual_ShiftCode"]),
                            LeaveType = BLHelper.ConvertObjectToString(row["LeaveType"]),
                            LeaveTypeDesc = BLHelper.ConvertObjectToString(row["LeaveTypeDesc"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                            !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }                

        public List<EmployeeAttendanceEntity> GetResignedBuSwiped(DateTime? startDate, DateTime? endDate, int empNo, string costCenter, 
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[6];
                parameters[0] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[1] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[2] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[3] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[4] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[5] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetResignedButSwiped", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["BusinessUnitName"]),
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIN"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOUT"]),
                            Duration = BLHelper.ConvertObjectToInt(row["Duration"]),
                            DateResigned = BLHelper.ConvertObjectToDate(row["DateResigned"]),
                            PayStatus = BLHelper.ConvertObjectToString(row["PayStatus"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                            !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetResignedBuSwipedV2(DateTime? startDate, DateTime? endDate, int empNo, string costCenter, ref string error, ref string innerError)
        {
            try
            {
                List<EmployeeAttendanceEntity> result = null;
                var rawData = TASContext.GetResignedButSwipedV2(startDate, endDate, empNo, costCenter);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (var item in rawData)
                    {
                        EmployeeAttendanceEntity collectionItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = item.AutoID,
                            RunDate = item.RunDate,
                            EmpNo = BLHelper.ConvertObjectToInt(item.EmpNo),
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                            Position = BLHelper.ConvertObjectToString(item.Position),
                            CostCenter = BLHelper.ConvertObjectToString(item.BusinessUnit),
                            CostCenterName = BLHelper.ConvertObjectToString(item.BusinessUnitName),
                            CostCenterFullName = string.Format("{0} - {1}",
                                BLHelper.ConvertObjectToString(item.BusinessUnit),
                                BLHelper.ConvertObjectToString(item.BusinessUnitName)),
                            DateResigned = item.DateResigned,
                            AttendanceRemarks = BLHelper.ConvertObjectToString(item.AttendanceRemarks)
                        };

                        // Add item to the collection
                        result.Add(collectionItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetLongAbsences(DateTime? processDate, bool showSLP, bool showUL, bool showAbsent,
            ref DateTime? startDate, ref DateTime? endDate, ref string attendanceHistoryTitle, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[4];
                parameters[0] = new ADONetParameter("@processDate", SqlDbType.DateTime, processDate);
                parameters[1] = new ADONetParameter("@showSLP", SqlDbType.Bit, showSLP);
                parameters[2] = new ADONetParameter("@showUL", SqlDbType.Bit, showUL);
                parameters[3] = new ADONetParameter("@showAbsent", SqlDbType.Bit, showAbsent);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetLongAbsence", connectionString, parameters);
                if (ds != null)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                            {
                                EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                                EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                                ActualCostCenter = BLHelper.ConvertObjectToString(row["ActualCostCenter"]),
                                CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]),
                                CostCenterName = BLHelper.ConvertObjectToString(row["CostCenterName"]),
                                AttendanceHistoryValue = BLHelper.ConvertObjectToString(row["AttendanceHistoryValue"])
                            };

                            if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                                !string.IsNullOrEmpty(newItem.CostCenterName))
                            {
                                newItem.CostCenterFullName = string.Format("{0} - {1}",
                                    newItem.CostCenter,
                                    newItem.CostCenterName);
                            }

                            // Add to collection
                            result.Add(newItem);
                        };
                    }

                    #region Get the Start Date, End Date, and Attendance History Title
                    if (ds.Tables[1].Rows.Count > 0)
                    {
                        startDate = BLHelper.ConvertObjectToDate(ds.Tables[1].Rows[0]["FromDate"]);
                        endDate = BLHelper.ConvertObjectToDate(ds.Tables[1].Rows[0]["ToDate"]);
                        attendanceHistoryTitle = BLHelper.ConvertObjectToString(ds.Tables[1].Rows[0]["AttendanceHistoryTitle"]);
                    }
                    #endregion
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<UserDefinedCodes> GetUserDefinedCode(string udcCode, ref string error, ref string innerError)
        {
            try
            {
                List<UserDefinedCodes> result = null;
                var rawData = TASContext.GetUserDefinedCode(udcCode);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<UserDefinedCodes>();

                    foreach (var item in rawData)
                    {
                        UserDefinedCodes collectionItem = new UserDefinedCodes()
                        {
                            UDCGID = item.UDCUDCGID,
                            UDCID = item.UDCID,
                            UDCCode = BLHelper.ConvertObjectToString(item.UDCCode),
                            UDCDesc1 = BLHelper.ConvertObjectToString(item.UDCDesc1),
                            UDCDesc2 = BLHelper.ConvertObjectToString(item.UDCDesc2),
                            UDCSpecialHandlingCode = BLHelper.ConvertObjectToString(item.UDCSpecialHandlingCode),
                            UDCDate = item.UDCDate,
                            UDCAmount = item.UDCAmount,
                            UDCField = BLHelper.ConvertObjectToString(item.UDCField)
                        };

                        // Add item to the collection
                        result.Add(collectionItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetTimesheetIntegrity(string actionCode, DateTime? startDate, DateTime? endDate, int empNo, string costCenter,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[7];
                parameters[0] = new ADONetParameter("@actionCode", SqlDbType.VarChar, 10, actionCode);
                parameters[1] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[2] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[3] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[4] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[5] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[6] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetTimesheetIntegrity_V2", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            CorrectionCode = BLHelper.ConvertObjectToString(row["CorrectionCode"]),
                            CorrectionDesc = BLHelper.ConvertObjectToString(row["CorrectionDesc"]),
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIN"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOUT"]),
                            OTType = BLHelper.ConvertObjectToString(row["OTType"]),
                            OTStartTime = BLHelper.ConvertObjectToDate(row["OTStartTime"]),
                            OTEndTime = BLHelper.ConvertObjectToDate(row["OTEndTime"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["BusinessUnitName"]),
                            CostCenterFullName = string.Format("{0} - {1}",
                                BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                                BLHelper.ConvertObjectToString(row["BusinessUnitName"])),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                            ActualShiftCode = BLHelper.ConvertObjectToString(row["Actual_ShiftCode"]),
                            ShiftAllowance = BLHelper.ConvertObjectToBolean(row["ShiftAllowance"]),
                            DurationShiftAllowanceEvening = BLHelper.ConvertObjectToInt(row["Duration_ShiftAllowance_Evening"]),
                            DurationShiftAllowanceNight = BLHelper.ConvertObjectToInt(row["Duration_ShiftAllowance_Night"]),
                            NoPayHours = BLHelper.ConvertObjectToInt(row["NoPayHours"]),
                            AbsenceReasonCode = BLHelper.ConvertObjectToString(row["AbsenceReasonCode"]),
                            LeaveType = BLHelper.ConvertObjectToString(row["LeaveType"]),
                            DILEntitlement = BLHelper.ConvertObjectToString(row["DIL_Entitlement"]),
                            RemarkCode = BLHelper.ConvertObjectToString(row["RemarkCode"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            Processed = BLHelper.ConvertObjectToBolean(row["Processed"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                            !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetTimesheetIntegrityOld(string actionCode, DateTime? startDate, DateTime? endDate, int empNo, string costCenter, ref string error, ref string innerError)
        {
            try
            {
                List<EmployeeAttendanceEntity> result = null;
                var rawData = TASContext.GetTimesheetIntegrity(actionCode, startDate, endDate, empNo, costCenter);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (var item in rawData)
                    {
                        EmployeeAttendanceEntity collectionItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = item.AutoID,
                            CorrectionCode = BLHelper.ConvertObjectToString(item.CorrectionCode),
                            CorrectionDesc = BLHelper.ConvertObjectToString(item.CorrectionDesc),
                            DT = item.DT,
                            dtIN = item.dtIN,
                            dtOUT = item.dtOUT,
                            OTType = BLHelper.ConvertObjectToString(item.OTType),
                            OTStartTime = item.OTStartTime,
                            OTEndTime = item.OTEndTime,
                            EmpNo = item.EmpNo,
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                            CostCenter = BLHelper.ConvertObjectToString(item.BusinessUnit),
                            CostCenterName = BLHelper.ConvertObjectToString(item.BusinessUnitName),
                            CostCenterFullName = string.Format("{0} - {1}",
                                BLHelper.ConvertObjectToString(item.BusinessUnit),
                                BLHelper.ConvertObjectToString(item.BusinessUnitName)),
                            ShiftPatCode = BLHelper.ConvertObjectToString(item.ShiftPatCode),
                            ShiftCode = BLHelper.ConvertObjectToString(item.ShiftCode),
                            ActualShiftCode = BLHelper.ConvertObjectToString(item.Actual_ShiftCode),
                            ShiftAllowance = item.ShiftAllowance,
                            DurationShiftAllowanceEvening = item.Duration_ShiftAllowance_Evening,
                            DurationShiftAllowanceNight = item.Duration_ShiftAllowance_Night,
                            NoPayHours = item.NoPayHours,
                            AbsenceReasonCode = BLHelper.ConvertObjectToString(item.AbsenceReasonCode),
                            LeaveType = BLHelper.ConvertObjectToString(item.LeaveType),
                            DILEntitlement = BLHelper.ConvertObjectToString(item.DIL_Entitlement),
                            RemarkCode = BLHelper.ConvertObjectToString(item.RemarkCode),
                            LastUpdateUser = BLHelper.ConvertObjectToString(item.LastUpdateUser),
                            LastUpdateTime = item.LastUpdateTime,
                            Processed = item.Processed
                        };

                        // Add item to the collection
                        result.Add(collectionItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetManualTimesheetEntry(int? autoID, int? empNo, string costCenter, DateTime? dateIN, DateTime? dateOUT, 
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[7];
                parameters[0] = new ADONetParameter("@autoID", SqlDbType.Int, autoID);
                parameters[1] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[2] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[3] = new ADONetParameter("@dateIN", SqlDbType.DateTime, dateIN);
                parameters[4] = new ADONetParameter("@dateOUT", SqlDbType.DateTime, dateOUT);
                parameters[5] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[6] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetManualTimesheetEntry", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["BusinessUnitName"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIN"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOUT"]),
                            TimeIn = BLHelper.ConvertObjectToDate(row["timeIN"]),
                            TimeOut = BLHelper.ConvertObjectToDate(row["timeOUT"]),
                            CreatedUser = BLHelper.ConvertObjectToString(row["CreatedUser"]),
                            CreatedTime = BLHelper.ConvertObjectToDate(row["CreatedTime"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"]),
                            IsContractor = BLHelper.ConvertNumberToBolean(row["IsContractor"])
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                            !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }

                        #region Get the Swipe In date and time
                        if (newItem.dtIN.HasValue &&
                            newItem.TimeIn.HasValue)
                        {
                            TimeSpan ts = new TimeSpan(newItem.TimeIn.Value.Hour, newItem.TimeIn.Value.Minute, newItem.TimeIn.Value.Second);
                            newItem.SwipeIn = newItem.dtIN.Value + ts; 
                        }
                        #endregion

                        #region Get the Swipe Out date and time
                        if (newItem.dtOUT.HasValue &&
                            newItem.TimeOut.HasValue)
                        {
                            TimeSpan ts = new TimeSpan(newItem.TimeOut.Value.Hour, newItem.TimeOut.Value.Minute, newItem.TimeOut.Value.Second);
                            newItem.SwipeOut = newItem.dtOUT.Value + ts;
                        }
                        #endregion

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void InsertUpdateDeleteManualTimesheet(int saveTypeNum, List<EmployeeAttendanceEntity> manualTimesheetList, ref string error, ref string innerError)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                BLHelper.SaveType saveType = (BLHelper.SaveType)Enum.Parse(typeof(BLHelper.SaveType), saveTypeNum.ToString());

                switch (saveType)
                {
                    case BLHelper.SaveType.Insert:
                        #region Perform Insert Operation

                        #region Insert new record using ADO.NET                                                
                        foreach (EmployeeAttendanceEntity item in manualTimesheetList)
                        {
                            var resultInsert = TASContext.InsertUpdateDeleteManualAttendance
                            (
                                saveTypeNum,
                                item.AutoID,
                                item.EmpNo,
                                item.dtIN,
                                item.TimeIn,
                                item.dtOUT,
                                item.TimeOut,
                                item.CreatedUser
                            );
                        }
                        #endregion

                        #region Insert new record using Entity Framework
                        //List<Tran_ManualAttendance> recordToInsertList = new List<Tran_ManualAttendance>();
                        //foreach (EmployeeAttendanceEntity item in manualTimesheetList)
                        //{
                        //    recordToInsertList.Add(new Tran_ManualAttendance()
                        //    {
                        //        EmpNo = item.EmpNo,
                        //        dtIN = item.dtIN,
                        //        dtOUT = item.dtOUT,
                        //        timeIN = item.TimeIn.HasValue ? Convert.ToDateTime(item.TimeIn).ToString("HHmm") : null,
                        //        timeOUT = item.TimeOut.HasValue ? Convert.ToDateTime(item.TimeOut).ToString("HHmm") : null,
                        //        CreatedUser = item.CreatedUser,
                        //        CreatedTime = item.CreatedTime
                        //    });
                        //}

                        //// Save to database
                        //if (recordToInsertList.Count > 0)
                        //{
                        //    TASContext.Tran_ManualAttendance.AddRange(recordToInsertList);
                        //    TASContext.SaveChanges();
                        //}
                        #endregion

                        break;
                        #endregion

                    case BLHelper.SaveType.Update:
                        #region Perform Update Operation

                        #region Update record using ADO.NET                                                
                        foreach (EmployeeAttendanceEntity item in manualTimesheetList)
                        {
                            var resultUpdate = TASContext.InsertUpdateDeleteManualAttendance
                            (
                                saveTypeNum,
                                item.AutoID,
                                item.EmpNo,
                                item.dtIN,
                                item.TimeIn,
                                item.dtOUT,
                                item.TimeOut,
                                item.LastUpdateUser
                            );
                        }
                        #endregion

                        #region Update record using Entity Framework
                        //foreach (EmployeeAttendanceEntity item in manualTimesheetList)
                        //{
                        //    Tran_ManualAttendance recordToUpdate = TASContext.Tran_ManualAttendance
                        //        .Where(a => a.AutoID == item.AutoID)
                        //        .FirstOrDefault();
                        //    if (recordToUpdate != null)
                        //    {
                        //        recordToUpdate.dtIN = item.dtIN;
                        //        recordToUpdate.dtOUT = item.dtOUT;
                        //        recordToUpdate.timeIN = item.TimeIn.HasValue ? Convert.ToDateTime(item.TimeIn).ToString("HHmm") : null;
                        //        recordToUpdate.timeOUT = item.TimeOut.HasValue ? Convert.ToDateTime(item.TimeOut).ToString("HHmm") : null;
                        //        recordToUpdate.LastUpdateUser = item.LastUpdateUser;
                        //        recordToUpdate.LastUpdateTime = item.LastUpdateTime;

                        //        // Save to database
                        //        TASContext.SaveChanges();
                        //    }
                        //}
                        #endregion

                        break;
                        #endregion

                    case BLHelper.SaveType.Delete:
                        #region Perform Delete Operation

                        #region Delete record using ADO.NET                                                
                        foreach (EmployeeAttendanceEntity item in manualTimesheetList)
                        {
                            var resultDelete = TASContext.InsertUpdateDeleteManualAttendance
                            (
                                saveTypeNum,
                                item.AutoID,
                                item.EmpNo,
                                item.dtIN,
                                item.TimeIn,
                                item.dtOUT,
                                item.TimeOut,
                                item.LastUpdateUser
                            );
                        }
                        #endregion

                        #region Delete record using Entity Framework
                        //List<Tran_ManualAttendance> recordToDeleteList = new List<Tran_ManualAttendance>();
                        //foreach (EmployeeAttendanceEntity item in manualTimesheetList)
                        //{
                        //    Tran_ManualAttendance recordToDelete = TASContext.Tran_ManualAttendance
                        //        .Where(a => a.AutoID == item.AutoID)
                        //        .FirstOrDefault();
                        //    if (recordToDelete != null)
                        //    {
                        //        // Add to collection
                        //        recordToDeleteList.Add(recordToDelete);
                        //    }
                        //}

                        //// Save to database
                        //if (recordToDeleteList.Count > 0)
                        //{
                        //    TASContext.Tran_ManualAttendance.RemoveRange(recordToDeleteList);
                        //    TASContext.SaveChanges();
                        //}
                        #endregion

                        break;
                        #endregion
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public List<EmployeeDetail> GetContractors(int empNo, string empName, ref string error, ref string innerError)
        {
            List<EmployeeDetail> result = null;

            try
            {
                var rawData = TASContext.GetContractors(empNo, empName);
                if (rawData != null)
                {
                    result = new List<EmployeeDetail>();
                    foreach (var item in rawData)
                    {
                        EmployeeDetail newItem = new EmployeeDetail()
                        {
                            AutoID = item.AutoID,
                            EmpNo = item.EmpNo,
                            ContractorEmpName = BLHelper.ConvertObjectToString(item.ContractorEmpName),
                            ContractorNumber = item.ContractorNumber,
                            GroupCode = BLHelper.ConvertObjectToString(item.GroupCode),
                            ShiftPatCode = BLHelper.ConvertObjectToString(item.ShiftPatCode),
                            ShiftPointer = BLHelper.ConvertObjectToInt(item.ShiftPointer),
                            ReligionCode = BLHelper.ConvertObjectToString(item.ReligionCode),
                            DateJoined = item.DateJoined,
                            DateResigned = item.DateResigned
                        };

                        // Add item to collection
                        result.Add(newItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<ReasonOfAbsenceEntity> GetReasonOfAbsenceEntry(int? autoID, int? empNo, string costCenter, DateTime? effectiveDate, DateTime? endingDate,
            string absenceReasonCode, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<ReasonOfAbsenceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[8];
                parameters[0] = new ADONetParameter("@autoID", SqlDbType.Int, autoID);
                parameters[1] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[2] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[3] = new ADONetParameter("@effectiveDate", SqlDbType.DateTime, effectiveDate);
                parameters[4] = new ADONetParameter("@endingDate", SqlDbType.DateTime, endingDate);
                parameters[5] = new ADONetParameter("@absenceReasonCode", SqlDbType.VarChar, 10, absenceReasonCode);
                parameters[6] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[7] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetReasonOfAbsence", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<ReasonOfAbsenceEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ReasonOfAbsenceEntity newItem = new ReasonOfAbsenceEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["BusinessUnitName"]),
                            EffectiveDate = BLHelper.ConvertObjectToDate(row["EffectiveDate"]),
                            EndingDate = BLHelper.ConvertObjectToDate(row["EndingDate"]),
                            StartTime = BLHelper.ConvertObjectToDate(row["StartTime"]),
                            EndTime = BLHelper.ConvertObjectToDate(row["EndTime"]),
                            DayOfWeek = BLHelper.ConvertObjectToString(row["DayOfWeek"]),
                            AbsenceReasonCode = BLHelper.ConvertObjectToString(row["AbsenceReasonCode"]),
                            AbsenceReasonDesc = BLHelper.ConvertObjectToString(row["AbsenceReasonDesc"]),
                            XID_TS_DIL_ENT = BLHelper.ConvertObjectToInt(row["XID_TS_DIL_ENT"]),
                            XID_TS_DIL_USD = BLHelper.ConvertObjectToInt(row["XID_TS_DIL_USD"]),
                            DIL_ENT_CODE = BLHelper.ConvertObjectToString(row["DIL_ENT_CODE"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                            !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }

                        newItem.AbsenceReasonFullName = string.Format("({0}) {1}",
                            newItem.AbsenceReasonCode,
                            newItem.AbsenceReasonDesc);

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<UserDefinedCodes> GetTimesheetUDCCodes(byte actionType, ref string error, ref string innerError)
        {
            try
            {
                List<UserDefinedCodes> result = null;
                var rawData = TASContext.GetTimesheetUDCCodes(actionType);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<UserDefinedCodes>();

                    foreach (var item in rawData)
                    {
                        UserDefinedCodes newItem = new UserDefinedCodes()
                        {
                            UDCCode = BLHelper.ConvertObjectToString(item.UDCCode),
                            UDCDesc1 = BLHelper.ConvertObjectToString(item.UDCDesc1),
                            UDCDesc2 = BLHelper.ConvertObjectToString(item.UDCDesc2)                            
                        };

                        // Populate other properties
                        newItem.UDCFullName = string.Format("({0}) {1}",
                            newItem.UDCCode,
                            newItem.UDCDesc1);

                        // Add item to the collection
                        result.Add(newItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void InsertUpdateDeleteReasonOfAbsence(int saveTypeNum, List<ReasonOfAbsenceEntity> dataList, ref string error, ref string innerError)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                BLHelper.SaveType saveType = (BLHelper.SaveType)Enum.Parse(typeof(BLHelper.SaveType), saveTypeNum.ToString());

                switch (saveType)
                {
                    case BLHelper.SaveType.Insert:
                        #region Perform Insert Operation
                        List<Tran_Absence> recordToInsertList = new List<Tran_Absence>();
                        foreach (ReasonOfAbsenceEntity item in dataList)
                        {
                            recordToInsertList.Add(new Tran_Absence()
                            {
                                EmpNo = item.EmpNo,
                                EffectiveDate = Convert.ToDateTime(item.EffectiveDate),
                                EndingDate = Convert.ToDateTime(item.EndingDate),
                                StartTime = item.StartTime.HasValue ? Convert.ToDateTime(item.StartTime).ToString("HHmm") : null,
                                EndTime = item.EndTime.HasValue ? Convert.ToDateTime(item.EndTime).ToString("HHmm") : null,
                                DayOfWeek = item.DayOfWeek,
                                AbsenceReasonCode = item.AbsenceReasonCode,
                                XID_TS_DIL_ENT = item.XID_TS_DIL_ENT,
                                XID_TS_DIL_USD = item.XID_TS_DIL_USD,
                                DIL_ENT_CODE = item.DIL_ENT_CODE,
                                LastUpdateTime = item.LastUpdateTime,
                                LastUpdateUser = item.LastUpdateUser
                            });
                        }

                        // Save to database
                        if (recordToInsertList.Count > 0)
                        {
                            TASContext.Tran_Absence.AddRange(recordToInsertList);
                            TASContext.SaveChanges();
                        }

                        break;
                    #endregion

                    case BLHelper.SaveType.Update:
                        #region Perform Update Operation
                        foreach (ReasonOfAbsenceEntity item in dataList)
                        {
                            Tran_Absence recordToUpdate = TASContext.Tran_Absence
                                .Where(a => a.AutoID == item.AutoID)
                                .FirstOrDefault();
                            if (recordToUpdate != null)
                            {
                                recordToUpdate.EmpNo = item.EmpNo;
                                recordToUpdate.EffectiveDate = Convert.ToDateTime(item.EffectiveDate);
                                recordToUpdate.EndingDate = Convert.ToDateTime(item.EndingDate);
                                recordToUpdate.StartTime = item.StartTime.HasValue ? Convert.ToDateTime(item.StartTime).ToString("HHmm") : null;
                                recordToUpdate.EndTime = item.EndTime.HasValue ? Convert.ToDateTime(item.EndTime).ToString("HHmm") : null;
                                recordToUpdate.DayOfWeek = item.DayOfWeek;
                                recordToUpdate.AbsenceReasonCode = item.AbsenceReasonCode;
                                recordToUpdate.XID_TS_DIL_ENT = item.XID_TS_DIL_ENT;
                                recordToUpdate.XID_TS_DIL_USD = item.XID_TS_DIL_USD;
                                recordToUpdate.DIL_ENT_CODE = item.DIL_ENT_CODE;
                                recordToUpdate.LastUpdateTime = item.LastUpdateTime;
                                recordToUpdate.LastUpdateUser = item.LastUpdateUser;

                                // Save to database
                                TASContext.SaveChanges();
                            }
                        }

                        break;
                    #endregion

                    case BLHelper.SaveType.Delete:
                        #region Perform Delete Operation
                        List<Tran_Absence> recordToDeleteList = new List<Tran_Absence>();

                        foreach (ReasonOfAbsenceEntity item in dataList)
                        {
                            Tran_Absence recordToDelete = TASContext.Tran_Absence
                                .Where(a => a.AutoID == item.AutoID)
                                .FirstOrDefault();
                            if (recordToDelete != null)
                            {
                                // Add to collection
                                recordToDeleteList.Add(recordToDelete);
                            }
                        }

                        // Save to database
                        if (recordToDeleteList.Count > 0)
                        {
                            TASContext.Tran_Absence.RemoveRange(recordToDeleteList);
                            TASContext.SaveChanges();
                        }

                        break;
                        #endregion
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public List<EmployeeAttendanceEntity> GetTimesheetExceptional(int empNo, DateTime? startDate, DateTime? endDate, bool withExceptionOnly,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[6];
                parameters[0] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[1] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[2] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[3] = new ADONetParameter("@withExceptionOnly", SqlDbType.Bit, withExceptionOnly);
                parameters[4] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[5] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetTimesheetByPayPeriod", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            CorrectionCode = BLHelper.ConvertObjectToString(row["CorrectionCode"]),
                            CorrectionCodeDesc = BLHelper.ConvertObjectToString(row["CorrectionCodeDesc"]),
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIN"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOUT"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                            ActualShiftCode = BLHelper.ConvertObjectToString(row["Actual_ShiftCode"]),
                            ShiftAllowanceDesc = BLHelper.ConvertObjectToString(row["ShiftAllowance"]),
                            OTStartTime = BLHelper.ConvertObjectToDate(row["OTStartTime"]),
                            OTEndTime = BLHelper.ConvertObjectToDate(row["OTEndTime"]),
                            OTType = BLHelper.ConvertObjectToString(row["OTType"]),
                            NoPayHours = BLHelper.ConvertObjectToInt(row["NoPayHours"]),
                            AbsenceReasonCode = BLHelper.ConvertObjectToString(row["AbsenceReasonCode"]),
                            LeaveType = BLHelper.ConvertObjectToString(row["LeaveType"]),
                            DILEntitlement = BLHelper.ConvertObjectToString(row["DIL_Entitlement"]),
                            RemarkCode = BLHelper.ConvertObjectToString(row["RemarkCode"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            IsLastRow = BLHelper.ConvertObjectToBolean(row["IsLastRow"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"]),
                            IsExceptional = BLHelper.ConvertNumberToBolean(row["IsExceptional"])
                        };

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetTimesheetHistory(int autoID, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[3];
                parameters[0] = new ADONetParameter("@autoID", SqlDbType.Int, autoID);
                parameters[1] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[2] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetTimesheetCorrectionHistory", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            XID_AutoID = BLHelper.ConvertObjectToInt(row["XID_AutoID"]),
                            CorrectionCode = BLHelper.ConvertObjectToString(row["CorrectionCode"]),
                            CorrectionDesc = BLHelper.ConvertObjectToString(row["CorrectionDesc"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIN"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOUT"]),
                            ShiftAllowanceDesc = BLHelper.ConvertObjectToString(row["ShiftAllowance"]),
                            OTType = BLHelper.ConvertObjectToString(row["OTType"]),
                            OTStartTime = BLHelper.ConvertObjectToDate(row["OTStartTime"]),
                            OTEndTime = BLHelper.ConvertObjectToDate(row["OTEndTime"]),
                            NoPayHours = BLHelper.ConvertObjectToInt(row["NoPayHours"]),
                            AbsenceReasonCode = BLHelper.ConvertObjectToString(row["AbsenceReasonCode"]),
                            LeaveType = BLHelper.ConvertObjectToString(row["LeaveType"]),
                            RemarkCode = BLHelper.ConvertObjectToString(row["RemarkCode"]),
                            DILEntitlement = BLHelper.ConvertObjectToString(row["DIL_Entitlement"]),
                            Processed = BLHelper.ConvertObjectToBolean(row["Processed"]),
                            ActionDate = BLHelper.ConvertObjectToDate(row["action_time"]),
                            ActionMachineName = BLHelper.ConvertObjectToString(row["action_machine"]),
                            ActionType = BLHelper.ConvertObjectToString(row["action_type"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["action_time"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<ShiftPatternEntity> GetShiftPatternChangeHistory(int empNo, DateTime? DT, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<ShiftPatternEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[4];
                parameters[0] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[1] = new ADONetParameter("@DT", SqlDbType.DateTime, DT);
                parameters[2] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[3] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetShiftPatternChangeHistory", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<ShiftPatternEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ShiftPatternEntity newItem = new ShiftPatternEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            XID_AutoID = BLHelper.ConvertObjectToInt(row["XID_AutoID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EffectiveDate = BLHelper.ConvertObjectToDate(row["EffectiveDate"]),
                            EndingDate = BLHelper.ConvertObjectToDate(row["EndingDate"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftPointer = BLHelper.ConvertObjectToInt(row["ShiftPointer"]),
                            ChangeType = BLHelper.ConvertObjectToString(row["ChangeType"]),
                            ChangeTypeDesc = BLHelper.ConvertObjectToString(row["ChangeTypeDesc"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            ActionDateTime = BLHelper.ConvertObjectToDate(row["ActionTime"]),
                            ActionMachineName = BLHelper.ConvertObjectToString(row["ActionMachine"]),
                            ActionType = BLHelper.ConvertObjectToString(row["ActionType"]),
                            ActionTypeDesc = BLHelper.ConvertObjectToString(row["ActionTypeDesc"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<ReasonOfAbsenceEntity> GetEmployeeAbsenceHistory(int empNo, DateTime? DT, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<ReasonOfAbsenceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[4];
                parameters[0] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[1] = new ADONetParameter("@DT", SqlDbType.DateTime, DT);
                parameters[2] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[3] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetTimesheetAbsenceHistory", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<ReasonOfAbsenceEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ReasonOfAbsenceEntity newItem = new ReasonOfAbsenceEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EffectiveDate = BLHelper.ConvertObjectToDate(row["EffectiveDate"]),
                            EndingDate = BLHelper.ConvertObjectToDate(row["EndingDate"]),
                            StartTime = BLHelper.ConvertObjectToDate(row["StartTime"]),
                            EndTime = BLHelper.ConvertObjectToDate(row["EndTime"]),
                            DayOfWeek = BLHelper.ConvertObjectToString(row["DayOfWeek"]),
                            AbsenceReasonCode = BLHelper.ConvertObjectToString(row["AbsenceReasonCode"]),
                            AbsenceReasonDesc = BLHelper.ConvertObjectToString(row["AbsenceReasonDesc"]),
                            XID_TS_DIL_ENT = BLHelper.ConvertObjectToInt(row["XID_TS_DIL_ENT"]),
                            XID_TS_DIL_USD = BLHelper.ConvertObjectToInt(row["XID_TS_DIL_USD"]),
                            DIL_ENT_CODE = BLHelper.ConvertObjectToString(row["DIL_ENT_CODE"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        if (!string.IsNullOrEmpty(newItem.AbsenceReasonCode) &&
                            !string.IsNullOrEmpty(newItem.AbsenceReasonDesc))
                        {
                            newItem.AbsenceReasonFullName = string.Format("({0}) {1}",
                                newItem.AbsenceReasonCode,
                                newItem.AbsenceReasonDesc);
                        }
                        else
                            newItem.AbsenceReasonFullName = newItem.AbsenceReasonCode;

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<LeaveEntity> GetEmployeeLeaveHistory(int empNo, DateTime? DT, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<LeaveEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[4];
                parameters[0] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[1] = new ADONetParameter("@DT", SqlDbType.DateTime, DT);
                parameters[2] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[3] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetTimesheetLeaveHistory", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<LeaveEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        LeaveEntity newItem = new LeaveEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            LeaveEmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            FromDate = BLHelper.ConvertObjectToDate(row["FromDate"]),
                            ToDate = BLHelper.ConvertObjectToDate(row["ToDate"]),
                            LeaveCode = BLHelper.ConvertObjectToString(row["LeaveCode"]),
                            LeaveDesc = BLHelper.ConvertObjectToString(row["LeaveDesc"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        if (!string.IsNullOrEmpty(newItem.LeaveCode) &&
                            !string.IsNullOrEmpty(newItem.LeaveDesc))
                        {
                            newItem.LeaveFullName = string.Format("({0}) {1}",
                                newItem.LeaveCode,
                                newItem.LeaveDesc);
                        }
                        else
                            newItem.LeaveFullName = newItem.LeaveCode;

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetTimesheetCorrection(string costCenter, int empNo, DateTime? startDate, DateTime? endDate, int autoID,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[7];
                parameters[0] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[1] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[2] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[3] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[4] = new ADONetParameter("@autoID", SqlDbType.Int, autoID);
                parameters[5] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[6] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetTimesheetCorrection", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["BusinessUnitName"]),
                            CorrectionCode = BLHelper.ConvertObjectToString(row["CorrectionCode"]),
                            CorrectionDesc = BLHelper.ConvertObjectToString(row["CorrectionDesc"]),
                            CorrectionCodeDesc = BLHelper.ConvertObjectToString(row["CorrectionDesc"]),
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIN"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOUT"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                            ActualShiftCode = BLHelper.ConvertObjectToString(row["Actual_ShiftCode"]),
                            ShiftAllowanceDesc = BLHelper.ConvertObjectToString(row["ShiftAllowanceDesc"]),
                            ShiftAllowance = BLHelper.ConvertObjectToBolean(row["ShiftAllowance"]),
                            DurationShiftAllowanceEvening = BLHelper.ConvertObjectToInt(row["Duration_ShiftAllowance_Evening"]),
                            DurationShiftAllowanceNight = BLHelper.ConvertObjectToInt(row["Duration_ShiftAllowance_Night"]),
                            OTType = BLHelper.ConvertObjectToString(row["OTType"]),
                            OTStartTime = BLHelper.ConvertObjectToDate(row["OTStartTime"]),
                            OTEndTime = BLHelper.ConvertObjectToDate(row["OTEndTime"]),
                            OTTypeTE = BLHelper.ConvertObjectToString(row["OTType_TE"]),
                            OTStartTimeTE = BLHelper.ConvertObjectToDate(row["OTStartTime_TE"]),
                            OTEndTimeTE = BLHelper.ConvertObjectToDate(row["OTEndTime_TE"]),
                            NoPayHours = BLHelper.ConvertObjectToInt(row["NoPayHours"]),
                            AbsenceReasonCode = BLHelper.ConvertObjectToString(row["AbsenceReasonCode"]),
                            LeaveType = BLHelper.ConvertObjectToString(row["LeaveType"]),
                            RemarkCode = BLHelper.ConvertObjectToString(row["RemarkCode"]),
                            DILEntitlement = BLHelper.ConvertObjectToString(row["DIL_Entitlement"]),
                            Processed = BLHelper.ConvertObjectToBolean(row["Processed"]),
                            IsLastRow = BLHelper.ConvertObjectToBolean(row["IsLastRow"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            RequiredToSwipeAtWorkplace = BLHelper.ConvertNumberToBolean(row["RequiredToSwipeAtWorkplace"]),
                            TimeInMG = BLHelper.ConvertObjectToDate(row["TimeInMG"]),
                            TimeOutMG = BLHelper.ConvertObjectToDate(row["TimeOutMG"]),
                            TimeInWP = BLHelper.ConvertObjectToDate(row["TimeInWP"]),
                            TimeOutWP = BLHelper.ConvertObjectToDate(row["TimeOutWP"]),
                            IsCorrected = BLHelper.ConvertObjectToString(row["IsCorrected"]),
                            IsCorrectionApproved = BLHelper.ConvertObjectToString(row["IsCorrectionApproved"]),                            
                            Remarks = BLHelper.ConvertObjectToString(row["Remarks"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"]),
                            Duration_Worked_Cumulative = BLHelper.ConvertObjectToInt(row["Duration_Worked_Cumulative"]),
                            IsDriver = BLHelper.ConvertObjectToBolean(row["IsDriver"]),
                            RelativeTypeName = BLHelper.ConvertObjectToString(row["RelativeTypeName"]),
                            DeathRemarks = BLHelper.ConvertObjectToString(row["DeathRemarks"])
                        };

                        // Get the value of NPH in HH:mm format
                        newItem.NoPayHoursDesc = BLHelper.ConvertMinuteToHourString(newItem.NoPayHours);

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                            !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                            newItem.CostCenterFullName = newItem.CostCenter;

                        #region Process "Meal Voucher Approved?"
                        newItem.MealVoucherEligibilityCode = BLHelper.ConvertObjectToString(row["MealVoucherEligibility"]);
                        if (newItem.MealVoucherEligibilityCode == "YA")
                            newItem.MealVoucherEligibility = "Yes";
                        else if (newItem.MealVoucherEligibilityCode == "N")
                            newItem.MealVoucherEligibility = "No";
                        else
                            newItem.MealVoucherEligibility = "-";
                        #endregion

                        #region Process Main Gate and Workplace Swipes
                        if (!newItem.RequiredToSwipeAtWorkplace)
                        {
                            newItem.TimeInMG = null;
                            newItem.TimeOutMG = null;
                            newItem.TimeInWP = null;
                            newItem.TimeOutWP = null;
                            newItem.IsCorrected = string.Empty;
                            newItem.IsCorrectionApproved = string.Empty;
                            newItem.Remarks = "Not required to swipe in plant readers";
                        }
                        else
                        {
                            if (newItem.TimeInMG == null &&
                                newItem.dtIN != null)
                            {
                                newItem.TimeInMG = newItem.dtIN;
                            }

                            if (newItem.TimeOutMG == null &&
                                newItem.dtOUT != null)
                            {
                                newItem.TimeOutMG = newItem.dtOUT;
                            }
                        }
                        #endregion

                        #region Add relative type and HR remarks to the Correction Description for all death related correction codes
                        StringBuilder sb = new StringBuilder();

                        if (newItem.CorrectionCode == "RAD1" ||
                            newItem.CorrectionCode == "RAD2" ||
                            newItem.CorrectionCode == "RAD3" ||
                            newItem.CorrectionCode == "RAD4")
                        {
                            sb.AppendLine(string.Format("<b>{0}</b>", newItem.CorrectionDesc));
                            sb.AppendLine(string.Format("<i>Relative:</i> {0}", newItem.RelativeTypeName));
                            //sb.AppendLine(string.Format("<i>Remarks:</i> {0}", !string.IsNullOrEmpty(newItem.DeathRemarks) ? newItem.DeathRemarks : "-"));

                            // Format text to HTML
                            newItem.CorrectionDesc = string.Format("<HTML><BODY><p>{0}</p></BODY></HTML>", sb.ToString().Trim().Replace("\r\n", "<br />"));
                        }
                        else if (newItem.CorrectionCode == "RAD0")
                        {
                            sb.AppendLine(string.Format("<b>{0}</b>", newItem.CorrectionDesc));
                            sb.AppendLine(string.Format("<i>Other Relative:</i> {0}", newItem.RelativeTypeName));
                            //sb.AppendLine(string.Format("<i>Remarks:</i> {0}", !string.IsNullOrEmpty(newItem.DeathRemarks) ? newItem.DeathRemarks : "-"));

                            // Format text to HTML
                            newItem.CorrectionDesc = string.Format("<HTML><BODY><p>{0}</p></BODY></HTML>", sb.ToString().Trim().Replace("\r\n", "<br />"));
                        }
                        #endregion

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public DatabaseSaveResult InsertUpdateDeleteTimesheet(byte actionType, int autoID, string correctionCode, string otType, DateTime? otStartTime, DateTime? otEndTime,
            int noPayHours, string shiftCode, bool? shiftAllowance, int? durationShiftAllowanceEvening, int? durationShiftAllowanceNight, string dilEntitlement, 
            string remarkCode, string userID, ref string error, ref string innerError)
        {
            DatabaseSaveResult result = null;

            try
            {
                ADONetParameter[] parameters = new ADONetParameter[14];
                parameters[0] = new ADONetParameter("@actionType", SqlDbType.TinyInt, actionType);
                parameters[1] = new ADONetParameter("@autoID", SqlDbType.BigInt, autoID);
                parameters[2] = new ADONetParameter("@correctionCode", SqlDbType.VarChar, 10, correctionCode);
                parameters[3] = new ADONetParameter("@otType", SqlDbType.VarChar, 10, otType);
                parameters[4] = new ADONetParameter("@otStartTime", SqlDbType.DateTime, otStartTime);
                parameters[5] = new ADONetParameter("@otEndTime", SqlDbType.DateTime, otEndTime);
                parameters[6] = new ADONetParameter("@noPayHours", SqlDbType.Int, noPayHours);
                parameters[7] = new ADONetParameter("@shiftCode", SqlDbType.VarChar, 10, shiftCode);
                parameters[8] = new ADONetParameter("@shiftAllowance", SqlDbType.Bit, shiftAllowance);
                parameters[9] = new ADONetParameter("@durationShiftAllowanceEvening", SqlDbType.Int, durationShiftAllowanceEvening);
                parameters[10] = new ADONetParameter("@durationShiftAllowanceNight", SqlDbType.Int, durationShiftAllowanceNight);
                parameters[11] = new ADONetParameter("@dilEntitlement", SqlDbType.VarChar, 10, dilEntitlement);
                parameters[12] = new ADONetParameter("@remarkCode", SqlDbType.VarChar, 10, remarkCode);
                parameters[13] = new ADONetParameter("@userID", SqlDbType.VarChar, 50, userID);

                DataSet ds = RunSPReturnDataset("tas.Pr_TranTimesheet_CRUD", TASContext.Database.Connection.ConnectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    result = new DatabaseSaveResult()
                    {
                        NewIdentityID = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["NewIdentityID"]),
                        RowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["RowsAffected"]),
                        HasError = BLHelper.ConvertObjectToBolean(ds.Tables[0].Rows[0]["HasError"]),
                        ErrorCode = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorCode"]),
                        ErrorDesc = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorDescription"])
                    };

                    if (result.HasError &&
                        !string.IsNullOrEmpty(result.ErrorDesc))
                    {
                        throw new Exception(result.ErrorDesc);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void InsertUpdateDeleteVisitorSwipeLog(int saveTypeID, List<VisitorSwipeEntity> visitorSwipeList, ref string error, ref string innerError)
        {
            try
            {
                BLHelper.SaveType saveType = (BLHelper.SaveType)Enum.Parse(typeof(BLHelper.SaveType), saveTypeID.ToString());

                switch (saveType)
                {
                    case BLHelper.SaveType.Insert:
                        #region Perform Insert Operation

                        #region Check for duplicate records
                        //List<VisitorSwipeEntity> duplicateRecordList = new List<VisitorSwipeEntity>();
                        //VisitorSwipeLog duplicateRecord = null;
                        //StringBuilder sbEmployee = new StringBuilder();

                        //foreach (VisitorSwipeEntity item in visitorSwipeList)
                        //{
                        //    duplicateRecord = TASContext.VisitorSwipeLogs
                        //        .Where(a => a.IDNumber.Trim() == item.IDNumber.Trim() &&
                        //            a.VisitorCardNo.Trim() == item.VisitorCardNo.Trim() &&
                        //            a.VisitDate == item.VisitDate)
                        //        .FirstOrDefault();

                        //    if (duplicateRecord != null)
                        //    {
                        //        duplicateRecordList.Add(item);

                        //        if (sbEmployee.Length == 0)
                        //        {
                        //            sbEmployee.Append(string.Format("({0}) on {1}",
                        //                item.VisitorName,
                        //                Convert.ToDateTime(item.VisitDate).ToString("dd-MMM-yyyy")));
                        //        }
                        //        else
                        //        {
                        //            sbEmployee.Append(string.Format(", ({0}) on {1}",
                        //               item.VisitorName,
                        //                Convert.ToDateTime(item.VisitDate).ToString("dd-MMM-yyyy")));
                        //        }
                        //    }
                        //}

                        //if (duplicateRecordList.Count > 0)
                        //{
                        //    error = string.Format("Similar record already exist for the following employees: {0}", sbEmployee.ToString());
                        //    throw new Exception(string.Format("Similar record already exist for the following employees: {0}", sbEmployee.ToString()));
                        //}
                        #endregion

                        #region No duplicate record, proceed in saving the data
                        // Initialize collection
                        List<VisitorSwipeLog> recordsToInsertList = new List<VisitorSwipeLog>();

                        foreach (VisitorSwipeEntity item in visitorSwipeList)
                        {
                            recordsToInsertList.Add(new VisitorSwipeLog()
                            {
                                LogID = item.LogID,
                                SwipeDate = Convert.ToDateTime(item.SwipeDate),
                                SwipeTime = Convert.ToDateTime(item.SwipeTime),
                                SwipeTypeCode = item.SwipeTypeCode,
                                CreatedDate = item.CreatedDate,
                                CreatedByEmpNo = item.CreatedByEmpNo,
                                CreatedByEmpName = item.CreatedByEmpName,
                                CreatedByEmpEmail = item.CreatedByEmpEmail,
                                CreatedByUserID = item.CreatedByUserID
                            });
                        }

                        // Commit changes in the database
                        TASContext.VisitorSwipeLogs.AddRange(recordsToInsertList);
                        TASContext.SaveChanges();
                        #endregion

                        break;
                    #endregion

                    case BLHelper.SaveType.Update:
                        #region Perform Update Operation
                        foreach (VisitorSwipeEntity item in visitorSwipeList)
                        {
                            VisitorSwipeLog recordToUpdate = TASContext.VisitorSwipeLogs
                                 .Where(a => a.SwipeID == item.SwipeID)
                                .FirstOrDefault();
                            if (recordToUpdate != null)
                            {
                                recordToUpdate.SwipeDate = Convert.ToDateTime(item.SwipeDate);
                                recordToUpdate.SwipeTime = Convert.ToDateTime(item.SwipeTime);
                                recordToUpdate.SwipeTypeCode = item.SwipeTypeCode;
                                recordToUpdate.LastUpdateTime = item.LastUpdateTime;
                                recordToUpdate.LastUpdateEmpNo = item.LastUpdateEmpNo;
                                recordToUpdate.LastUpdateEmpName = item.LastUpdateEmpName;
                                recordToUpdate.LastUpdateEmpEmail = item.LastUpdateEmpEmail;
                                recordToUpdate.LastUpdateUserID = item.LastUpdateUserID;

                                // Save to database
                                TASContext.SaveChanges();
                            }
                        }

                        break;
                    #endregion

                    case BLHelper.SaveType.Delete:
                        #region Perform Delete Operation
                        List<VisitorSwipeLog> recordsToDeleteList = new List<VisitorSwipeLog>();

                        foreach (VisitorSwipeEntity item in visitorSwipeList)
                        {
                            VisitorSwipeLog recordToDelete = TASContext.VisitorSwipeLogs
                                 .Where(a => a.SwipeID == item.SwipeID)
                                .FirstOrDefault();
                            if (recordToDelete != null)
                            {
                                // Add to collection
                                recordsToDeleteList.Add(recordToDelete);
                            }
                        }

                        // Save to database
                        if (recordsToDeleteList.Count > 0)
                        {
                            TASContext.VisitorSwipeLogs.RemoveRange(recordsToDeleteList);
                            TASContext.SaveChanges();
                        }

                        break;
                        #endregion
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public List<VisitorSwipeEntity> GetVisitorSwipeHistory(int empNo, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<VisitorSwipeEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[5];
                parameters[0] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[1] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[2] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[3] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[4] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_VisitorSwipeHistory", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<VisitorSwipeEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        VisitorSwipeEntity newItem = new VisitorSwipeEntity()
                        {
                            SwipeID = BLHelper.ConvertObjectToLong(row["SwipeID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            SwipeDate = BLHelper.ConvertObjectToDate(row["SwipeDate"]),
                            SwipeTime = BLHelper.ConvertObjectToDate(row["SwipeTime"]),
                            SwipeLocation = BLHelper.ConvertObjectToString(row["SwipeLocation"]),
                            SwipeTypeCode = BLHelper.ConvertObjectToString(row["SwipeTypeCode"]),
                            SwipeTypeDesc = BLHelper.ConvertObjectToString(row["SwipeType"]),
                            LocationCode = BLHelper.ConvertObjectToInt(row["LocationCode"]),
                            LocationName = BLHelper.ConvertObjectToString(row["LocationName"]),
                            ReaderNo = BLHelper.ConvertObjectToInt(row["ReaderNo"]),
                            ReaderName = BLHelper.ConvertObjectToString(row["ReaderName"]),
                            SwipeCode = BLHelper.ConvertObjectToString(row["SwipeCode"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<FireTeamMember> GetEmergencyResponseTeam(int actionType, DateTime processDate, int empNo, string costCenter, string imageRootPath, ref string error, ref string innerError)
        {
            List<FireTeamMember> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[4];
                parameters[0] = new ADONetParameter("@actionType", SqlDbType.Int, 0);   //actionType);
                parameters[1] = new ADONetParameter("@processDate", SqlDbType.DateTime, processDate);
                parameters[2] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[3] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetFireTeamAttendance", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    List<FireTeamMember> fireTeamList = BuildFireTeamCollection(ds.Tables[0], imageRootPath, actionType);
                    if (actionType == 1)
                    {
                        if (fireTeamList != null && fireTeamList.Count > 0)
                        {
                            result = fireTeamList.Where(a => a.IsPresent == true).ToList();
                        }
                    }
                    else
                        result = fireTeamList;
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }                

        public int GetLastSwipeStatus(int empNo, DateTime swipeDate, ref string error, ref string innerError)
        {
            BLHelper.SwipeTypes result = BLHelper.SwipeTypes.Unknown;

            try
            {
                var lastSwipe = TASContext.Vw_MainGateSwipeRawData
                    .Where(a => a.EmpNo == empNo && a.SwipeDate == swipeDate)
                    .FirstOrDefault();
                if (lastSwipe != null)
                {
                    string swipeType = BLHelper.ConvertObjectToString(lastSwipe.SwipeType);
                    if (swipeType == BLHelper.SwipeTypes.IN.ToString())
                        result = BLHelper.SwipeTypes.IN;
                    else if (swipeType == BLHelper.SwipeTypes.OUT.ToString())
                        result = BLHelper.SwipeTypes.OUT;
                }

                return Convert.ToInt32(result);
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return Convert.ToInt32(result);
            }
        }

        public EmployeeAttendanceEntity GetEmployeeDetailForManualAttendance(int empNo, ref string error, ref string innerError)
        {
            EmployeeAttendanceEntity result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[1];
                parameters[0] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);

                DataSet ds = RunSPReturnDataset("tas.prGetEmployeeDetailsForManualAttendance_V2", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    result = new EmployeeAttendanceEntity()
                    {
                        AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                        EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                        EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                        Position = BLHelper.ConvertObjectToString(row["EmpName"]),
                        CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]),
                        CostCenterName = BLHelper.ConvertObjectToString(row["CostCenterName"]),
                        dtIN = BLHelper.ConvertObjectToDate(row["DateIn"]),
                        dtOUT = BLHelper.ConvertObjectToDate(row["DateOut"]),
                        SwipeCode = BLHelper.ConvertObjectToString(row["SwipeStatus"])
                    };

                    if (!string.IsNullOrEmpty(result.CostCenter) &&
                        !string.IsNullOrEmpty(result.CostCenterName))
                    {
                        result.CostCenterFullName = string.Format("{0} - {1}",
                            result.CostCenter,
                            result.CostCenterName);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void SaveManualAttendance(int saveTypeID, EmployeeAttendanceEntity attendanceData, ref string error, ref string innerError)
        {
            try
            {
                BLHelper.SaveType saveType = (BLHelper.SaveType)Enum.Parse(typeof(BLHelper.SaveType), saveTypeID.ToString());
                switch (saveType)
                {
                    case BLHelper.SaveType.Insert:
                        #region Perform Insert Operation
                        Tran_ManualAttendance recordToInsert = new Tran_ManualAttendance()
                        {
                            EmpNo = attendanceData.EmpNo,
                            dtIN = attendanceData.dtIN,
                            dtOUT = attendanceData.dtOUT,
                            timeIN = attendanceData.TimeIn.HasValue ? Convert.ToDateTime(attendanceData.TimeIn).ToString("HHmm") : null,
                            timeOUT = attendanceData.TimeOut.HasValue ? Convert.ToDateTime(attendanceData.TimeOut).ToString("HHmm") : null,
                            CreatedUser = attendanceData.CreatedUser,
                            CreatedTime = attendanceData.CreatedTime
                        };

                        // Save to database
                        TASContext.Tran_ManualAttendance.Add(recordToInsert);
                        TASContext.SaveChanges();

                        break;
                        #endregion

                    case BLHelper.SaveType.Update:
                        #region Perform Update Operation
                        // Get the record to update
                        Tran_ManualAttendance recordToUpdate = TASContext.Tran_ManualAttendance
                            .Where(a => a.AutoID == attendanceData.AutoID)
                            .FirstOrDefault();
                        if (recordToUpdate != null)
                        {
                            recordToUpdate.dtOUT = attendanceData.dtOUT;
                            recordToUpdate.timeOUT = attendanceData.TimeOut.HasValue ? Convert.ToDateTime(attendanceData.TimeOut).ToString("HHmm") : null;
                            recordToUpdate.LastUpdateUser = attendanceData.LastUpdateUser;
                            recordToUpdate.LastUpdateTime = attendanceData.LastUpdateTime;

                            // Save to database
                            TASContext.SaveChanges();
                        }

                        break;
                        #endregion

                    case BLHelper.SaveType.Delete:
                        #region Perform Delete Operation
                        Tran_ManualAttendance recordToDelete = TASContext.Tran_ManualAttendance
                            .Where(a => a.AutoID == attendanceData.AutoID)
                            .FirstOrDefault();
                        if (recordToDelete != null)
                        {
                            TASContext.Tran_ManualAttendance.Remove(recordToDelete);
                            TASContext.SaveChanges();
                        }

                        break;
                        #endregion
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public List<ContractorAttendance> GetContractorAttendance(DateTime? startDate, DateTime? endDate, int contractorNo, string contractorName, string costCenter,   
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<ContractorAttendance> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[7];

                parameters[0] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[1] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[2] = new ADONetParameter("@contractorNo", SqlDbType.Int, contractorNo);
                parameters[3] = new ADONetParameter("@contractorName", SqlDbType.VarChar, 100, contractorName);
                parameters[4] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[5] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[6] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetContractorAttendance_V3", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    List<ContractorAttendance> processedAttendanceList = new List<ContractorAttendance>();                    

                    // Initialize collection
                    result = new List<ContractorAttendance>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ContractorAttendance newItem = new ContractorAttendance()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["CostCenterName"]),
                            CPRNo = BLHelper.ConvertObjectToInt(row["CPRNo"]),
                            JobTitle = BLHelper.ConvertObjectToString(row["JobTitle"]),
                            EmployerName = BLHelper.ConvertObjectToString(row["EmployerName"]),
                            StatusID = BLHelper.ConvertObjectToInt(row["StatusID"]),
                            StatusDesc = BLHelper.ConvertObjectToString(row["StatusDesc"]),
                            ContractorTypeID = BLHelper.ConvertObjectToInt(row["ContractorTypeID"]),
                            ContractorTypeDesc = BLHelper.ConvertObjectToString(row["ContractorTypeDesc"]),
                            CreatedDate = BLHelper.ConvertObjectToDate(row["CreatedDate"]),
                            CreatedByNo = BLHelper.ConvertObjectToInt(row["CreatedByNo"]),
                            CreatedByName = BLHelper.ConvertObjectToString(row["CreatedByName"]),
                            ContractStartDate = BLHelper.ConvertObjectToDate(row["ContractStartDate"]),
                            ContractEndDate = BLHelper.ConvertObjectToDate(row["ContractEndDate"]),
                            IDStartDate = BLHelper.ConvertObjectToDate(row["IDStartDate"]),
                            IDEndDate = BLHelper.ConvertObjectToDate(row["IDEndDate"]),
                            RequiredWorkDuration = BLHelper.ConvertObjectToDouble(row["RequiredWorkDuration"]) / 60,
                            SwipeDate = BLHelper.ConvertObjectToDate(row["SwipeDate"]),
                            LocationName = BLHelper.ConvertObjectToString(row["LocationName"]),
                            ReaderName = BLHelper.ConvertObjectToString(row["ReaderName"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                            !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                            newItem.CostCenterFullName = newItem.CostCenter;

                        if (newItem.CreatedByNo > 0 &&
                            !string.IsNullOrEmpty(newItem.CreatedByName))
                        {
                            newItem.CreatedByFullName = string.Format("({0}) {1}",
                                newItem.CreatedByNo,
                                newItem.CreatedByName);
                        }

                        #region Get the Swipe In, Swipe Out, Net Minutes, and Overtime
                        //int workHours = BLHelper.ConvertObjectToInt(row["RequiredWorkDuration"]);
                        //var workDetail = TASContext.GetContractorWorkDuration(newItem.EmpNo, newItem.SwipeDate, workHours).FirstOrDefault();
                        //if (workDetail != null)
                        //{
                        //    newItem.SwipeIn = workDetail.SwipeIn;
                        //    newItem.SwipeOut = workDetail.SwipeOut;
                        //    newItem.NetHour = BLHelper.ConvertObjectToDouble(workDetail.NetMinutes) / 60;
                        //    newItem.NetMinutes = BLHelper.ConvertObjectToDouble(workDetail.NetMinutes);
                        //    newItem.OvertimeHour = BLHelper.ConvertObjectToDouble(workDetail.Overtime) / 60;
                        //    newItem.OvertimeMinutes = BLHelper.ConvertObjectToDouble(workDetail.Overtime);
                        //}

                        newItem.SwipeIn = BLHelper.ConvertObjectToDate(row["SwipeIn"]);
                        newItem.SwipeOut = BLHelper.ConvertObjectToDate(row["SwipeOut"]);
                        newItem.NetHour = BLHelper.ConvertObjectToDouble(row["NetMinutes"]) / 60;
                        newItem.NetMinutes = BLHelper.ConvertObjectToDouble(row["NetMinutes"]);
                        newItem.OvertimeHour = BLHelper.ConvertObjectToDouble(row["Overtime"]) / 60;
                        newItem.OvertimeMinutes = BLHelper.ConvertObjectToDouble(row["Overtime"]);
                        #endregion

                        #region Calculate the work duration
                        //if (processedAttendanceList.Count > 0)
                        //{
                        //    if (processedAttendanceList.Where(a => a.EmpNo == newItem.EmpNo && a.SwipeDate == newItem.SwipeDate).FirstOrDefault() == null)
                        //    {
                        //        // Add to the collection
                        //        processedAttendanceList.Add(newItem);

                        //        string filter = string.Format("EmpNo = {0} AND SwipeDate = '{1}'",
                        //            newItem.EmpNo,
                        //            Convert.ToDateTime(newItem.SwipeDate).ToString("dd/MM/yyyy"));

                        //        DataView attendanceView = new DataView(ds.Tables[0], filter, "SwipeTime ASC", DataViewRowState.CurrentRows);
                        //        if (attendanceView != null)
                        //        {
                        //            DateTime? firstTimeIn = BLHelper.ConvertObjectToDate(attendanceView[0]["SwipeTime"]);
                        //            DateTime? lastTimeOut = BLHelper.ConvertObjectToDate(attendanceView[attendanceView.Count - 1]["SwipeTime"]);

                        //            // Update the collection
                        //            newItem.SwipeIn = firstTimeIn;
                        //            newItem.SwipeOut = lastTimeOut;

                        //            if (firstTimeIn.HasValue && lastTimeOut.HasValue)
                        //            {
                        //                // Calculate the net work duration
                        //                newItem.NetHour = (Convert.ToDateTime(lastTimeOut) - Convert.ToDateTime(firstTimeIn)).TotalHours;
                        //                newItem.NetMinutes = (Convert.ToDateTime(lastTimeOut) - Convert.ToDateTime(firstTimeIn)).TotalMinutes;

                        //                // Calculate the overtime
                        //                if (newItem.RequiredWorkDuration > 0 &&
                        //                    newItem.NetHour > 0 &&
                        //                    newItem.NetHour > newItem.RequiredWorkDuration)
                        //                {
                        //                    newItem.OvertimeHour = newItem.NetHour - newItem.RequiredWorkDuration;
                        //                    newItem.OvertimeMinutes = newItem.NetMinutes - (newItem.RequiredWorkDuration * 60);
                        //                }
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {
                        //        // Move to the next record
                        //        continue;
                        //    }
                        //}
                        //else
                        //{
                        //    // Add to the collection
                        //    processedAttendanceList.Add(newItem);

                        //    string filter = string.Format("EmpNo = {0} AND SwipeDate = '{1}'",
                        //            newItem.EmpNo,
                        //            Convert.ToDateTime(newItem.SwipeDate).ToString("dd/MM/yyyy"));

                        //    DataView attendanceView = new DataView(ds.Tables[0], filter, "SwipeTime ASC", DataViewRowState.CurrentRows);
                        //    if (attendanceView != null)
                        //    {
                        //        DateTime? firstTimeIn = BLHelper.ConvertObjectToDate(attendanceView[0]["SwipeTime"]);
                        //        DateTime? lastTimeOut = BLHelper.ConvertObjectToDate(attendanceView[attendanceView.Count - 1]["SwipeTime"]);

                        //        // Update the collection
                        //        newItem.SwipeIn = firstTimeIn;
                        //        newItem.SwipeOut = lastTimeOut;

                        //        if (firstTimeIn.HasValue && lastTimeOut.HasValue)
                        //        {
                        //            // Calculate the net work duration
                        //            newItem.NetHour = (Convert.ToDateTime(lastTimeOut) - Convert.ToDateTime(firstTimeIn)).TotalHours;
                        //            newItem.NetMinutes = (Convert.ToDateTime(lastTimeOut) - Convert.ToDateTime(firstTimeIn)).TotalMinutes;

                        //            // Calculate the overtime
                        //            if (newItem.RequiredWorkDuration > 0 &&
                        //                newItem.NetHour > 0 &&
                        //                newItem.NetHour > newItem.RequiredWorkDuration)
                        //            {
                        //                newItem.OvertimeHour = newItem.NetHour - newItem.RequiredWorkDuration;
                        //                newItem.OvertimeMinutes = newItem.NetMinutes - (newItem.RequiredWorkDuration * 60);
                        //            }
                        //        }
                        //    }
                        //}
                        #endregion

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<ContractorAttendance> GetContractorAttendanceReport(DateTime? startDate, DateTime? endDate, int contractorNo, string contractorName, string costCenter,
            int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<ContractorAttendance> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[7];

                parameters[0] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[1] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[2] = new ADONetParameter("@contractorNo", SqlDbType.Int, contractorNo);
                parameters[3] = new ADONetParameter("@contractorName", SqlDbType.VarChar, 100, contractorName);
                parameters[4] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[5] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[6] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetContractorAttendance_V2", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    List<ContractorAttendance> processedAttendanceList = new List<ContractorAttendance>();

                    // Initialize collection
                    result = new List<ContractorAttendance>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ContractorAttendance newItem = new ContractorAttendance()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["CostCenterName"]),
                            CPRNo = BLHelper.ConvertObjectToInt(row["CPRNo"]),
                            JobTitle = BLHelper.ConvertObjectToString(row["JobTitle"]),
                            EmployerName = BLHelper.ConvertObjectToString(row["EmployerName"]),
                            StatusID = BLHelper.ConvertObjectToInt(row["StatusID"]),
                            StatusDesc = BLHelper.ConvertObjectToString(row["StatusDesc"]),
                            ContractorTypeID = BLHelper.ConvertObjectToInt(row["ContractorTypeID"]),
                            ContractorTypeDesc = BLHelper.ConvertObjectToString(row["ContractorTypeDesc"]),
                            CreatedDate = BLHelper.ConvertObjectToDate(row["CreatedDate"]),
                            CreatedByNo = BLHelper.ConvertObjectToInt(row["CreatedByNo"]),
                            CreatedByName = BLHelper.ConvertObjectToString(row["CreatedByName"]),
                            ContractStartDate = BLHelper.ConvertObjectToDate(row["ContractStartDate"]),
                            ContractEndDate = BLHelper.ConvertObjectToDate(row["ContractEndDate"]),
                            IDStartDate = BLHelper.ConvertObjectToDate(row["IDStartDate"]),
                            IDEndDate = BLHelper.ConvertObjectToDate(row["IDEndDate"]),
                            RequiredWorkDuration = BLHelper.ConvertObjectToDouble(row["RequiredWorkDuration"]) / 60,
                            SwipeDate = BLHelper.ConvertObjectToDate(row["SwipeDate"]),
                            LocationName = BLHelper.ConvertObjectToString(row["LocationName"]),
                            ReaderName = BLHelper.ConvertObjectToString(row["ReaderName"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                            !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                            newItem.CostCenterFullName = newItem.CostCenter;

                        if (newItem.CreatedByNo > 0 &&
                            !string.IsNullOrEmpty(newItem.CreatedByName))
                        {
                            newItem.CreatedByFullName = string.Format("({0}) {1}",
                                newItem.CreatedByNo,
                                newItem.CreatedByName);
                        }

                        #region Get the Swipe In, Swipe Out, Net Minutes, and Overtime
                        //int workHours  = BLHelper.ConvertObjectToInt(row["RequiredWorkDuration"]);
                        //var workDetail = TASContext.GetContractorWorkDuration(newItem.EmpNo, newItem.SwipeDate, workHours).FirstOrDefault();
                        //if (workDetail != null)
                        //{
                        //    newItem.SwipeIn = workDetail.SwipeIn;
                        //    newItem.SwipeOut = workDetail.SwipeOut;
                        //    newItem.NetHour = BLHelper.ConvertObjectToDouble(workDetail.NetMinutes) / 60;
                        //    newItem.NetMinutes = BLHelper.ConvertObjectToDouble(workDetail.NetMinutes);
                        //    newItem.OvertimeHour = BLHelper.ConvertObjectToDouble(workDetail.Overtime) / 60;
                        //    newItem.OvertimeMinutes = BLHelper.ConvertObjectToDouble(workDetail.Overtime);
                        //}

                        newItem.SwipeIn = BLHelper.ConvertObjectToDate(row["SwipeIn"]);
                        newItem.SwipeOut = BLHelper.ConvertObjectToDate(row["SwipeOut"]);
                        newItem.NetHour = BLHelper.ConvertObjectToDouble(row["NetMinutes"]) / 60;
                        newItem.NetMinutes = BLHelper.ConvertObjectToDouble(row["NetMinutes"]);
                        newItem.OvertimeHour = BLHelper.ConvertObjectToDouble(row["Overtime"]) / 60;
                        newItem.OvertimeMinutes = BLHelper.ConvertObjectToDouble(row["Overtime"]);
                        #endregion

                        #region Calculate the work duration
                        //if (processedAttendanceList.Count > 0)
                        //{
                        //    if (processedAttendanceList
                        //        .Where
                        //        (
                        //            a => a.EmpNo == newItem.EmpNo 
                        //            && a.SwipeDate == newItem.SwipeDate 
                        //            && BLHelper.ConvertObjectToString(a.ReaderName) == newItem.ReaderName
                        //        )
                        //        .FirstOrDefault() == null)
                        //    {
                        //        // Add to the collection
                        //        processedAttendanceList.Add(newItem);

                        //        string filter = string.Format("EmpNo = {0} AND SwipeDate = '{1}'",
                        //            newItem.EmpNo,
                        //            Convert.ToDateTime(newItem.SwipeDate).ToString("dd/MM/yyyy"));

                        //        DataView attendanceView = new DataView(ds.Tables[0], filter, "SwipeTime ASC", DataViewRowState.CurrentRows);
                        //        if (attendanceView != null)
                        //        {
                        //            DateTime? firstTimeIn = BLHelper.ConvertObjectToDate(attendanceView[0]["SwipeTime"]);
                        //            DateTime? lastTimeOut = null;
                        //            if (attendanceView.Count > 1)
                        //            {
                        //                if (attendanceView.Count == 2)
                        //                {
                        //                    lastTimeOut = BLHelper.ConvertObjectToDate(attendanceView[attendanceView.Count - 1]["SwipeTime"]);

                        //                    // Check the time difference between first and second swipe
                        //                    if ((Convert.ToDateTime(lastTimeOut) - Convert.ToDateTime(firstTimeIn)).TotalMinutes < 1)
                        //                        lastTimeOut = null;
                        //                }
                        //                else
                        //                    lastTimeOut = BLHelper.ConvertObjectToDate(attendanceView[attendanceView.Count - 1]["SwipeTime"]);
                        //            }

                        //            // Update the collection
                        //            newItem.SwipeIn = firstTimeIn;
                        //            newItem.SwipeOut = lastTimeOut;

                        //            if (firstTimeIn.HasValue && lastTimeOut.HasValue)
                        //            {
                        //                // Calculate the net work duration
                        //                newItem.NetHour = (Convert.ToDateTime(lastTimeOut) - Convert.ToDateTime(firstTimeIn)).TotalHours;
                        //                newItem.NetMinutes = (Convert.ToDateTime(lastTimeOut) - Convert.ToDateTime(firstTimeIn)).TotalMinutes;

                        //                // Calculate the overtime
                        //                if (newItem.RequiredWorkDuration > 0 &&
                        //                    newItem.NetHour > 0 &&
                        //                    newItem.NetHour > newItem.RequiredWorkDuration)
                        //                {
                        //                    newItem.OvertimeHour = newItem.NetHour - newItem.RequiredWorkDuration;
                        //                    newItem.OvertimeMinutes = newItem.NetMinutes - (newItem.RequiredWorkDuration * 60);
                        //                }
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {
                        //        // Move to the next record
                        //        continue;
                        //    }
                        //}
                        //else
                        //{
                        //    // Add to the collection
                        //    processedAttendanceList.Add(newItem);

                        //    string filter = string.Format("EmpNo = {0} AND SwipeDate = '{1}'",
                        //            newItem.EmpNo,
                        //            Convert.ToDateTime(newItem.SwipeDate).ToString("dd/MM/yyyy"));

                        //    DataView attendanceView = new DataView(ds.Tables[0], filter, "SwipeTime ASC", DataViewRowState.CurrentRows);
                        //    if (attendanceView != null)
                        //    {

                        //        DateTime? firstTimeIn = BLHelper.ConvertObjectToDate(attendanceView[0]["SwipeTime"]);
                        //        DateTime? lastTimeOut = null;
                        //        if (attendanceView.Count > 1)
                        //        {
                        //            if (attendanceView.Count == 2)
                        //            {
                        //                lastTimeOut = BLHelper.ConvertObjectToDate(attendanceView[attendanceView.Count - 1]["SwipeTime"]);

                        //                // Check the time difference between first and second swipe
                        //                if ((Convert.ToDateTime(lastTimeOut) - Convert.ToDateTime(firstTimeIn)).TotalMinutes < 1)
                        //                    lastTimeOut = null;
                        //            }
                        //            else
                        //                lastTimeOut = BLHelper.ConvertObjectToDate(attendanceView[attendanceView.Count - 1]["SwipeTime"]);
                        //        }

                        //        // Update the collection
                        //        newItem.SwipeIn = firstTimeIn;
                        //        newItem.SwipeOut = lastTimeOut;

                        //        if (firstTimeIn.HasValue && lastTimeOut.HasValue)
                        //        {
                        //            // Calculate the net work duration
                        //            newItem.NetHour = (Convert.ToDateTime(lastTimeOut) - Convert.ToDateTime(firstTimeIn)).TotalHours;
                        //            newItem.NetMinutes = (Convert.ToDateTime(lastTimeOut) - Convert.ToDateTime(firstTimeIn)).TotalMinutes;

                        //            // Calculate the overtime
                        //            if (newItem.RequiredWorkDuration > 0 &&
                        //                newItem.NetHour > 0 &&
                        //                newItem.NetHour > newItem.RequiredWorkDuration)
                        //            {
                        //                newItem.OvertimeHour = newItem.NetHour - newItem.RequiredWorkDuration;
                        //                newItem.OvertimeMinutes = newItem.NetMinutes - (newItem.RequiredWorkDuration * 60);
                        //            }
                        //        }
                        //    }
                        //}
                        #endregion

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<ContractorAttendanceExcel> GetContractorAttendanceAll(DateTime? startDate, DateTime? endDate, int contractorNo, string contractorName, string costCenter, ref string error, ref string innerError)
        {
            List<ContractorAttendanceExcel> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[5];

                parameters[0] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[1] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[2] = new ADONetParameter("@contractorNo", SqlDbType.Int, contractorNo);
                parameters[3] = new ADONetParameter("@contractorName", SqlDbType.VarChar, 100, contractorName);
                parameters[4] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetContractorAttendanceAll", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    List<ContractorAttendanceExcel> processedAttendanceList = new List<ContractorAttendanceExcel>();

                    // Initialize collection
                    result = new List<ContractorAttendanceExcel>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ContractorAttendanceExcel newItem = new ContractorAttendanceExcel()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["CostCenterName"]),
                            CPRNo = BLHelper.ConvertObjectToInt(row["CPRNo"]),
                            JobTitle = BLHelper.ConvertObjectToString(row["JobTitle"]),
                            EmployerName = BLHelper.ConvertObjectToString(row["EmployerName"]),
                            StatusDesc = BLHelper.ConvertObjectToString(row["StatusDesc"]),
                            CreatedDate = BLHelper.ConvertObjectToDate(row["CreatedDate"]),
                            ContractStartDate = BLHelper.ConvertObjectToDate(row["ContractStartDate"]),
                            ContractEndDate = BLHelper.ConvertObjectToDate(row["ContractEndDate"]),
                            SwipeDate = BLHelper.ConvertObjectToDate(row["SwipeDate"]),
                            ReaderName = BLHelper.ConvertObjectToString(row["ReaderName"]),
                            WorkHour = BLHelper.ConvertObjectToDouble(row["RequiredWorkDuration"]) / 60
                        };

                        #region Get the Swipe In, Swipe Out, Net Minutes, and Overtime
                        int workHours = BLHelper.ConvertObjectToInt(row["RequiredWorkDuration"]);
                        var workDetail = TASContext.GetContractorWorkDuration(newItem.EmpNo, newItem.SwipeDate, workHours).FirstOrDefault();
                        if (workDetail != null)
                        {
                            newItem.SwipeIn = workDetail.SwipeIn;
                            newItem.SwipeOut = workDetail.SwipeOut;
                            newItem.NetHour = BLHelper.ConvertObjectToDouble(workDetail.NetMinutes) / 60;
                            newItem.OvertimeHour = BLHelper.ConvertObjectToDouble(workDetail.Overtime) / 60;
                        }
                        #endregion

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<ContractorAttendanceExcel> GetContractorAttendanceExcel(DateTime? startDate, DateTime? endDate, 
            int contractorNo, string contractorName, string costCenter, ref string error, ref string innerError)
        {
            List<ContractorAttendanceExcel> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[5];

                parameters[0] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[1] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[2] = new ADONetParameter("@contractorNo", SqlDbType.Int, contractorNo);
                parameters[3] = new ADONetParameter("@contractorName", SqlDbType.VarChar, 100, contractorName);
                parameters[4] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetContractorAttendance_Excel", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    List<ContractorAttendanceExcel> processedAttendanceList = new List<ContractorAttendanceExcel>();

                    // Initialize collection
                    result = new List<ContractorAttendanceExcel>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ContractorAttendanceExcel newItem = new ContractorAttendanceExcel()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["CostCenterName"]),
                            CPRNo = BLHelper.ConvertObjectToInt(row["CPRNo"]),
                            JobTitle = BLHelper.ConvertObjectToString(row["JobTitle"]),
                            EmployerName = BLHelper.ConvertObjectToString(row["EmployerName"]),
                            StatusDesc = BLHelper.ConvertObjectToString(row["StatusDesc"]),
                            CreatedDate = BLHelper.ConvertObjectToDate(row["CreatedDate"]),
                            ContractStartDate = BLHelper.ConvertObjectToDate(row["ContractStartDate"]),
                            ContractEndDate = BLHelper.ConvertObjectToDate(row["ContractEndDate"]),
                            SwipeDate = BLHelper.ConvertObjectToDate(row["SwipeDate"]),
                            ReaderName = BLHelper.ConvertObjectToString(row["ReaderName"]),
                            WorkHour = BLHelper.ConvertObjectToDouble(row["RequiredWorkDuration"]) / 60
                        };

                        #region Calculate the work duration
                        double requiredWorkDuration = BLHelper.ConvertObjectToDouble(row["RequiredWorkDuration"]);

                        if (processedAttendanceList.Count > 0)
                        {
                            if (processedAttendanceList
                                .Where
                                (
                                    a => a.EmpNo == newItem.EmpNo 
                                    && a.SwipeDate == newItem.SwipeDate
                                    && BLHelper.ConvertObjectToString(a.ReaderName) == newItem.ReaderName
                                )
                                .FirstOrDefault() == null)
                            {
                                // Add to the collection
                                processedAttendanceList.Add(newItem);

                                string filter = string.Format("EmpNo = {0} AND SwipeDate = '{1}'",
                                    newItem.EmpNo,
                                    Convert.ToDateTime(newItem.SwipeDate).ToString("dd/MM/yyyy"));

                                DataView attendanceView = new DataView(ds.Tables[0], filter, "SwipeTime ASC", DataViewRowState.CurrentRows);
                                if (attendanceView != null)
                                {
                                    DateTime? firstTimeIn = BLHelper.ConvertObjectToDate(attendanceView[0]["SwipeTime"]);
                                    DateTime? lastTimeOut = null;
                                    if (attendanceView.Count > 1)
                                    {
                                        if (attendanceView.Count == 2)
                                        {
                                            lastTimeOut = BLHelper.ConvertObjectToDate(attendanceView[attendanceView.Count - 1]["SwipeTime"]);

                                            // Check the time difference between first and second swipe
                                            if ((Convert.ToDateTime(lastTimeOut) - Convert.ToDateTime(firstTimeIn)).TotalMinutes < 1)
                                                lastTimeOut = null;
                                        }
                                        else
                                            lastTimeOut = BLHelper.ConvertObjectToDate(attendanceView[attendanceView.Count - 1]["SwipeTime"]);
                                    }

                                    // Update the collection
                                    newItem.SwipeIn = firstTimeIn;
                                    newItem.SwipeOut = lastTimeOut;

                                    if (firstTimeIn.HasValue && lastTimeOut.HasValue)
                                    {
                                        // Calculate the net work duration
                                        newItem.NetHour = (Convert.ToDateTime(lastTimeOut) - Convert.ToDateTime(firstTimeIn)).TotalHours;

                                        // Calculate the overtime
                                        if (requiredWorkDuration > 0 &&
                                            newItem.NetHour > 0 &&
                                            newItem.NetHour > requiredWorkDuration)
                                        {
                                            newItem.OvertimeHour = newItem.NetHour - requiredWorkDuration;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Move to the next record
                                continue;
                            }
                        }
                        else
                        {
                            // Add to the collection
                            processedAttendanceList.Add(newItem);

                            string filter = string.Format("EmpNo = {0} AND SwipeDate = '{1}'",
                                    newItem.EmpNo,
                                    Convert.ToDateTime(newItem.SwipeDate).ToString("dd/MM/yyyy"));

                            DataView attendanceView = new DataView(ds.Tables[0], filter, "SwipeTime ASC", DataViewRowState.CurrentRows);
                            if (attendanceView != null)
                            {
                                DateTime? firstTimeIn = BLHelper.ConvertObjectToDate(attendanceView[0]["SwipeTime"]);
                                DateTime? lastTimeOut = null;
                                if (attendanceView.Count > 1)
                                {
                                    if (attendanceView.Count == 2)
                                    {
                                        lastTimeOut = BLHelper.ConvertObjectToDate(attendanceView[attendanceView.Count - 1]["SwipeTime"]);

                                        // Check the time difference between first and second swipe
                                        if ((Convert.ToDateTime(lastTimeOut) - Convert.ToDateTime(firstTimeIn)).TotalMinutes < 1)
                                            lastTimeOut = null;
                                    }
                                    else
                                        lastTimeOut = BLHelper.ConvertObjectToDate(attendanceView[attendanceView.Count - 1]["SwipeTime"]);
                                }

                                // Update the collection
                                newItem.SwipeIn = firstTimeIn;
                                newItem.SwipeOut = lastTimeOut;

                                if (firstTimeIn.HasValue && lastTimeOut.HasValue)
                                {
                                    // Calculate the net work duration
                                    newItem.NetHour = (Convert.ToDateTime(lastTimeOut) - Convert.ToDateTime(firstTimeIn)).TotalHours;

                                    // Calculate the overtime
                                    if (requiredWorkDuration > 0 &&
                                        newItem.NetHour > 0 &&
                                        newItem.NetHour > requiredWorkDuration)
                                    {
                                        newItem.OvertimeHour = newItem.NetHour - requiredWorkDuration;
                                    }
                                }
                            }
                        }
                        #endregion

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetEmployeeAttendance(string empName, string costCenter, DateTime? attendanceDate, string imageRootPath, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[3];
                parameters[0] = new ADONetParameter("@empName", SqlDbType.VarChar, 100, empName);
                parameters[1] = new ADONetParameter("@CostCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[2] = new ADONetParameter("@attendanceDate", SqlDbType.DateTime, attendanceDate);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetAttendanceDashboard", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<EmployeeAttendanceEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            InquiryDate = BLHelper.ConvertObjectToDate(row["InquiryDate"]),
                            InOutStatus = BLHelper.ConvertObjectToString(row["InOutStatus"]),
                            ExtensionNo = BLHelper.ConvertObjectToString(row["ExtensionNo"]),
                            AttendanceRemarks = BLHelper.ConvertObjectToString(row["Remark"]),
                            EmployeeStatus = BLHelper.ConvertObjectToInt(row["EmployeeStatus"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]),
                            FirstTimeIn = BLHelper.ConvertObjectToDate(row["FirstTimeIn"]),
                            LastTimeOut = BLHelper.ConvertObjectToDate(row["LastTimeOut"]),
                            RequiredTimeOut = BLHelper.ConvertObjectToDate(row["RequiredTimeOut"]),
                            SwipeDate = BLHelper.ConvertObjectToDate(row["SwipeDate"]),
                            AttendanceDate = BLHelper.ConvertObjectToDate(row["AttendanceDate"]),
                            SupervisorEmpNo = BLHelper.ConvertObjectToInt(row["SupervisorEmpNo"])
                        };

                        if (newItem.EmpNo > 0 &&
                            !string.IsNullOrEmpty(newItem.EmpName))
                        {
                            newItem.EmpFullName = string.Format("({0}) {1}",
                                newItem.EmpNo,
                                newItem.EmpName);
                        }

                        #region Set the Attendance Status
                        if (newItem.InOutStatus == BLHelper.CONST_ARRIVAL_NORMAL)
                        {
                            newItem.StatusIconPath = BLHelper.CONST_ARRIVAL_NORMAL_ICON;
                            newItem.StatusIconNotes = BLHelper.CONST_ARRIVAL_NORMAL_NOTES;
                        }
                        else if (newItem.InOutStatus == BLHelper.CONST_ARRIVAL_LATE)
                        {
                            newItem.StatusIconPath = BLHelper.CONST_ARRIVAL_LATE_ICON;
                            newItem.StatusIconNotes = BLHelper.CONST_ARRIVAL_LATE_NOTES;
                        }
                        else if (newItem.InOutStatus == BLHelper.CONST_LEFT_NORMAL)
                        {
                            newItem.StatusIconPath = BLHelper.CONST_LEFT_NORMAL_ICON;
                            newItem.StatusIconNotes = BLHelper.CONST_LEFT_NORMAL_NOTES;
                        }
                        else if (newItem.InOutStatus == BLHelper.CONST_LEFT_EARLY)
                        {
                            newItem.StatusIconPath = BLHelper.CONST_LEFT_EARLY_ICON;
                            newItem.StatusIconNotes = BLHelper.CONST_LEFT_EARLY_NOTES;
                        }
                        else if (newItem.InOutStatus == BLHelper.CONST_NOT_COME_YET)
                        {
                            newItem.StatusIconPath = BLHelper.CONST_NOT_COME_YET_ICON;
                            newItem.StatusIconNotes = BLHelper.CONST_NOT_COME_YET_NOTES;
                        }
                        else if (newItem.InOutStatus == BLHelper.CONST_MANUAL_IN)
                        {
                            newItem.StatusIconPath = BLHelper.CONST_MANUAL_IN_ICON;
                            newItem.StatusIconNotes = BLHelper.CONST_MANUAL_IN_NOTES;
                        }
                        else if (newItem.InOutStatus == BLHelper.CONST_MANUAL_OUT)
                        {
                            newItem.StatusIconPath = BLHelper.CONST_MANUAL_OUT_ICON;
                            newItem.StatusIconNotes = BLHelper.CONST_MANUAL_OUT_NOTES;
                        }
                        #endregion

                        #region Get the employee photo
                        try
                         {                            
                            bool isPhotoFound = false;

                            //string imageFullPath_BMP = string.Format(@"{0}\{1}.bmp", imageRootPath, newItem.EmpNo);
                            //string imageFullPath_JPG = string.Format(@"{0}\{1}.jpg", imageRootPath, newItem.EmpNo);
                            string imageFullPath_BMP = string.Format(@"~/{0}/{1}.bmp", imageRootPath, newItem.EmpNo);
                            string imageFullPath_JPG = string.Format(@"~/{0}/{1}.jpg", imageRootPath, newItem.EmpNo);

                            #region Begin searching for bitmap photo                                
                            if (File.Exists(HostingEnvironment.MapPath(imageFullPath_BMP)))
                            {
                                newItem.EmployeeImagePath = imageFullPath_BMP;
                                isPhotoFound = true;
                            }
                            else
                            {
                                if (newItem.EmpNo > 10000000)
                                {
                                    //imageFullPath_BMP = string.Format(@"{0}\{1}.bmp", imageRootPath, newItem.EmpNo - 10000000);
                                    imageFullPath_BMP = string.Format(@"~/{0}/{1}.bmp", imageRootPath, newItem.EmpNo - 10000000);

                                    if (File.Exists(HostingEnvironment.MapPath(imageFullPath_BMP)))
                                    {
                                        newItem.EmployeeImagePath = imageFullPath_BMP;
                                        isPhotoFound = true;
                                    }
                                    else
                                    {
                                        newItem.EmployeeImagePath = BLHelper.CONST_NO_EMPLOYEE_PHOTO;
                                        newItem.EmployeeImageTooltip = BLHelper.CONST_NO_PHOTO_MESSAGE;
                                    }
                                }
                                else
                                {
                                    newItem.EmployeeImagePath = BLHelper.CONST_NO_EMPLOYEE_PHOTO;
                                    newItem.EmployeeImageTooltip = BLHelper.CONST_NO_PHOTO_MESSAGE;
                                }
                            }
                            #endregion

                            if (!isPhotoFound)
                            {
                                #region Search for JPEG photo
                                if (File.Exists(HostingEnvironment.MapPath(imageFullPath_JPG)))
                                {
                                    newItem.EmployeeImagePath = imageFullPath_JPG;
                                    isPhotoFound = true;
                                }
                                else
                                {
                                    if (newItem.EmpNo > 10000000)
                                    {
                                        //imageFullPath_JPG = string.Format(@"{0}\{1}.jpg", imageRootPath, newItem.EmpNo - 10000000);
                                        imageFullPath_JPG = string.Format(@"~/{0}/{1}.jpg", imageRootPath, newItem.EmpNo - 10000000);

                                        if (File.Exists(HostingEnvironment.MapPath(imageFullPath_JPG)))
                                        {
                                            newItem.EmployeeImagePath = imageFullPath_JPG;
                                            isPhotoFound = true;
                                        }
                                        else
                                        {
                                            newItem.EmployeeImagePath = BLHelper.CONST_NO_EMPLOYEE_PHOTO;
                                            newItem.EmployeeImageTooltip = BLHelper.CONST_NO_PHOTO_MESSAGE;
                                        }
                                    }
                                    else
                                    {
                                        newItem.EmployeeImagePath = BLHelper.CONST_NO_EMPLOYEE_PHOTO;
                                        newItem.EmployeeImageTooltip = BLHelper.CONST_NO_PHOTO_MESSAGE;
                                    }
                                }
                                #endregion
                            }
                        }
                        catch (Exception err)
                        {
                            newItem.EmployeeImagePath = BLHelper.CONST_NO_EMPLOYEE_PHOTO;
                            newItem.EmployeeImageTooltip = BLHelper.CONST_NO_PHOTO_MESSAGE;
                        }
                        #endregion

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<CostCenterEntity> GetCostCenterList(byte loadType, ref string error, ref string innerError)
        {
            List<CostCenterEntity> result = new List<CostCenterEntity>();

            try
            {
                var rawData = TASContext.GetCostCenter(loadType);
                if (rawData != null)
                {
                    foreach (var item in rawData)
                    {
                        CostCenterEntity newItem = new CostCenterEntity()
                        {
                            CompanyCode = BLHelper.ConvertObjectToString(item.CompanyCode),
                            CostCenter = BLHelper.ConvertObjectToString(item.BusinessUnit),
                            CostCenterName = BLHelper.ConvertObjectToString(item.BusinessUnitName),
                            ParentCostCenter = BLHelper.ConvertObjectToString(item.ParentBU),
                            SuperintendentEmpNo = BLHelper.ConvertObjectToInt(item.Superintendent),
                            ManagerEmpNo = BLHelper.ConvertObjectToInt(item.CostCenterManager)
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                            !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                        {
                            newItem.CostCenterFullName = newItem.CostCenterName;
                        }

                        // Add item to the collection
                        result.Add(newItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetSwipeDetails(int empNo, DateTime? attendanceDate, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> result = new List<EmployeeAttendanceEntity>();

            try
            {
                var rawData = TASContext.GetMainGateSwipe(empNo, attendanceDate);
                if (rawData != null)
                {
                    foreach (var item in rawData)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            DT = BLHelper.ConvertObjectToDate(item.DT),
                            SwipeType = BLHelper.ConvertObjectToString(item.SwipeType),
                            SwipeLocation = BLHelper.ConvertObjectToString(item.SwipeLocation)
                        };

                        // Add item to the collection
                        result.Add(newItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<CostCenterEntity> GetManagedCostCenter(int empNo, ref string error, ref string innerError)
        {
            List<CostCenterEntity> result = new List<CostCenterEntity>();

            try
            {
                var rawData = TASContext.GetManagedCostCenter(empNo);
                if (rawData != null)
                {
                    // Initialize collection
                    result = new List<CostCenterEntity>();

                    foreach (var item in rawData)
                    {
                        result.Add(new CostCenterEntity()
                        {
                            CostCenter = BLHelper.ConvertObjectToString(item.CostCenter),
                            CostCenterName = BLHelper.ConvertObjectToString(item.CostCenterName)
                        });
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetOvertimeAttendance(DateTime? startDate, DateTime? endDate, string costCenter, int? empNo, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> overtimeData = new List<EmployeeAttendanceEntity>();

            try
            {
                #region Fetch data using ADO.Net                                
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[4];

                parameters[0] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[1] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[2] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[3] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetAttendanceWithOT", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize the collection
                    overtimeData = new List<EmployeeAttendanceEntity>();

                    #region Populate data in the collection 
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            GradeCode = BLHelper.ConvertObjectToInt(row["GradeCode"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIN"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOUT"]),
                            OTStartTime = BLHelper.ConvertObjectToDate(row["OTstartTime"]),
                            OTEndTime = BLHelper.ConvertObjectToDate(row["OTendTime"]),
                            OTType = BLHelper.ConvertObjectToString(row["OTtype"]),
                            OTDurationMinute = BLHelper.ConvertObjectToInt(row["OTDurationMinute"]),
                            OTDurationHour = BLHelper.ConvertObjectToInt(row["OTDurationHour"]),
                            OTDurationHourClone = BLHelper.ConvertObjectToInt(row["OTDurationHour"]),
                            Approved = BLHelper.ConvertObjectToBolean(row["Approved"]),
                            AttendanceRemarks = BLHelper.ConvertObjectToString(row["Comment"]),
                            OTReasonCode = BLHelper.ConvertObjectToString(row["OTReasonCode"]),
                            OTReason = BLHelper.ConvertObjectToString(row["OTReason"]),
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            XID_AutoID = BLHelper.ConvertObjectToInt(row["XID_AutoID"]),
                            Processed = BLHelper.ConvertObjectToBolean(row["Processed"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"])
                        };

                        if (newItem.OTDurationHour > 0)
                        {
                            newItem.OTDurationText = newItem.OTDurationHour.ToString("0000").Insert(2, ":");
                        }

                        #region Process "OT Approved?"
                        newItem.OTApprovalCode = BLHelper.ConvertObjectToString(row["OTApproved"]);
                        if (newItem.OTApprovalCode == "Y")
                        {
                            newItem.OTApprovalDesc = "Yes";
                            newItem.IsOTAlreadyProcessed = true;
                        }
                        else if (newItem.OTApprovalCode == "N")
                        {
                            newItem.OTApprovalDesc = "No";
                            newItem.IsOTAlreadyProcessed = true;
                        }
                        else
                            newItem.OTApprovalDesc = "-";
                        #endregion

                        #region Process "Meal Voucher Approved?"
                        newItem.MealVoucherEligibilityCode = BLHelper.ConvertObjectToString(row["MealVoucherEligibility"]);
                        if (newItem.MealVoucherEligibilityCode == "YA")
                            newItem.MealVoucherEligibility = "Yes";
                        else if (newItem.MealVoucherEligibilityCode == "N")
                            newItem.MealVoucherEligibility = "No";
                        else
                            newItem.MealVoucherEligibility = "-";
                        #endregion

                        // Add item to the collection
                        overtimeData.Add(newItem);
                    }
                    #endregion
                }
                #endregion

                #region Fetch data using Entity Framework                                
                //var rawData = TASContext.GetAttendanceWithOT(startDate, endDate, costCenter, empNo);
                //if (rawData != null)
                //{
                //    foreach (var item in rawData)
                //    {
                //        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                //        {
                //            DT = item.DT,
                //            CostCenter = BLHelper.ConvertObjectToString(item.BusinessUnit),
                //            CostCenterName = BLHelper.ConvertObjectToString(item.BusinessUnit),
                //            EmpNo = item.EmpNo,
                //            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                //            GradeCode = item.GradeCode,
                //            dtIN = item.dtIN,
                //            dtOUT = item.dtOUT,
                //            OTStartTime = item.OTstartTime,
                //            OTEndTime = item.OTendTime,
                //            OTType = BLHelper.ConvertObjectToString(item.OTtype),
                //            OTDurationMinute = item.OTDurationMinute,
                //            OTDurationHour = BLHelper.ConvertObjectToInt(item.OTDurationHour),
                //            OTDurationHourClone = BLHelper.ConvertObjectToInt(item.OTDurationHour),
                //            Approved = item.Approved,
                //            AttendanceRemarks = BLHelper.ConvertObjectToString(item.Comment),
                //            OTReasonCode = BLHelper.ConvertObjectToString(item.OTReasonCode),
                //            OTReason = BLHelper.ConvertObjectToString(item.OTReason),
                //            AutoID = item.AutoID,
                //            XID_AutoID = item.XID_AutoID,
                //            Processed = item.Processed,
                //            LastUpdateUser = BLHelper.ConvertObjectToString(item.LastUpdateUser),
                //            LastUpdateTime = item.LastUpdateTime
                //        };

                //        if (newItem.OTDurationHour > 0)
                //        {
                //            newItem.OTDurationText = newItem.OTDurationHour.ToString().Insert(2, ":");
                //        }

                //        #region Process "OT Approved?"
                //        newItem.OTApprovalCode = BLHelper.ConvertObjectToString(item.OTApproved);
                //        if (newItem.OTApprovalCode == "Y")
                //        {
                //            newItem.OTApprovalDesc = "Yes";
                //            newItem.IsOTAlreadyProcessed = true;
                //        }
                //        else if (newItem.OTApprovalCode == "N")
                //        {
                //            newItem.OTApprovalDesc = "No";
                //            newItem.IsOTAlreadyProcessed = true;
                //        }
                //        else
                //            newItem.OTApprovalDesc = "-";
                //        #endregion

                //        #region Process "Meal Voucher Approved?"
                //        newItem.MealVoucherEligibilityCode = BLHelper.ConvertObjectToString(item.MealVoucherEligibility);
                //        if (newItem.MealVoucherEligibilityCode == "YA")
                //            newItem.MealVoucherEligibility = "Yes";
                //        else if (newItem.MealVoucherEligibilityCode == "N")
                //            newItem.MealVoucherEligibility = "No";
                //        else
                //            newItem.MealVoucherEligibility = "-";
                //        #endregion

                //        // Add item to the collection
                //        overtimeData.Add(newItem);
                //    }
                //}
                #endregion

                return overtimeData;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }                

        public List<UDCEntity> GetOvertimeReasons(byte loadType, ref string error, ref string innerError)
        {
            List<UDCEntity> result = new List<UDCEntity>();

            try
            {
                var rawData = TASContext.GetOTReason(loadType);
                if (rawData != null)
                {
                    foreach (var item in rawData)
                    {
                        UDCEntity newItem = new UDCEntity()
                        {
                            Code = BLHelper.ConvertObjectToString(item.Code),
                            Description = BLHelper.ConvertObjectToString(item.Description)
                        };

                        // Add item to the collection
                        result.Add(newItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public DatabaseSaveResult SaveEmployeeOvertime(int autoID, string otReasonCode, string comment, string userID, string otApprovalCode, string mealVoucherApprovalCode,
            int otDuration, ref string error, ref string innerError)
        {
            DatabaseSaveResult result = null;

            try
            {
                #region Initialize parameters
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[7];

                parameters[0] = new ADONetParameter("@autoID", SqlDbType.Int, autoID);
                parameters[1] = new ADONetParameter("@otReason", SqlDbType.VarChar, 10, otReasonCode);
                parameters[2] = new ADONetParameter("@comment", SqlDbType.VarChar, 1000, comment);
                parameters[3] = new ADONetParameter("@userID", SqlDbType.VarChar, 30, userID);
                parameters[4] = new ADONetParameter("@otApproved", SqlDbType.VarChar, 1, otApprovalCode);
                parameters[5] = new ADONetParameter("@mealVoucherEligibilityCode", SqlDbType.VarChar, 10, mealVoucherApprovalCode);
                parameters[6] = new ADONetParameter("@otDuration", SqlDbType.Int, otDuration);
                #endregion

                DataSet ds = RunSPReturnDataset("tas.Pr_InsertUpdateDeleteOvertime", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    result = new DatabaseSaveResult()
                    {
                        HasError = BLHelper.ConvertObjectToBolean(ds.Tables[0].Rows[0]["HasError"]),
                        ErrorCode = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorCode"]),
                        ErrorDesc = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorDescription"]),
                        TimesheetRowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["TimesheetRowsAffected"]),
                        TimesheetExtraRowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["TimesheetExtraRowsAffected"])
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();

                result = new DatabaseSaveResult()
                {
                    RowsAffected = 0,
                    HasError = true,
                    ErrorCode = string.Empty,
                    ErrorDesc = ex.Message.ToString()
                };
                return null;
            }
        }

        public List<DutyROTAEntity> GetDutyROTAEntry(int? autoID, int? empNo, DateTime? effectiveDate, DateTime? endingDate,
            string dutyType, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<DutyROTAEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[7];
                parameters[0] = new ADONetParameter("@autoID", SqlDbType.Int, autoID);
                parameters[1] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[2] = new ADONetParameter("@effectiveDate", SqlDbType.DateTime, effectiveDate);
                parameters[3] = new ADONetParameter("@endingDate", SqlDbType.DateTime, endingDate);
                parameters[4] = new ADONetParameter("@dutyType", SqlDbType.VarChar, 10, dutyType);
                parameters[5] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[6] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetDutyROTA", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<DutyROTAEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        DutyROTAEntity newItem = new DutyROTAEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["BusinessUnitName"]),
                            EffectiveDate = BLHelper.ConvertObjectToDate(row["EffectiveDate"]),
                            EndingDate = BLHelper.ConvertObjectToDate(row["EndingDate"]),
                            DutyType = BLHelper.ConvertObjectToString(row["DutyType"]),
                            DutyDescription = BLHelper.ConvertObjectToString(row["DutyDescription"]),
                            DutyAllowance = BLHelper.ConvertObjectToDouble(row["DutyAllowance"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                            !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<DutyROTAEntity> GetDutyROTAType(ref string error, ref string innerError)
        {
            List<DutyROTAEntity> dutyTypeList = null;

            try
            {
                var rawData = from a in TASContext.Master_DutyType
                              orderby a.DutyType
                              select a;
                if (rawData != null)
                {
                    dutyTypeList = new List<DutyROTAEntity>();

                    foreach (var item in rawData)
                    {
                        dutyTypeList.Add(new DutyROTAEntity()
                        {
                            AutoID = item.AutoID,
                            DutyType = item.DutyType,
                            DutyDescription = BLHelper.ConvertObjectToString(item.Description),
                            DutyAllowance = item.DutyAllowance
                        });
                    }
                }

                return dutyTypeList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void InsertUpdateDeleteDutyROTA(int saveTypeNum, List<DutyROTAEntity> dataList, ref string error, ref string innerError)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                BLHelper.SaveType saveType = (BLHelper.SaveType)Enum.Parse(typeof(BLHelper.SaveType), saveTypeNum.ToString());

                switch (saveType)
                {
                    case BLHelper.SaveType.Insert:
                        #region Perform Insert Operation
                        List<Tran_DutyRota> recordToInsertList = new List<Tran_DutyRota>();

                        foreach (DutyROTAEntity item in dataList)
                        {
                            recordToInsertList.Add(new Tran_DutyRota()
                            {
                                EmpNo = item.EmpNo,
                                EffectiveDate = Convert.ToDateTime(item.EffectiveDate),
                                EndingDate = Convert.ToDateTime(item.EndingDate),
                                DutyType = item.DutyType,
                                LastUpdateTime = item.LastUpdateTime,
                                LastUpdateUser = item.LastUpdateUser
                            });
                        }

                        // Save to database
                        if (recordToInsertList.Count > 0)
                        {
                            TASContext.Tran_DutyRota.AddRange(recordToInsertList);
                            TASContext.SaveChanges();
                        }

                        break;
                    #endregion

                    case BLHelper.SaveType.Update:
                        #region Perform Update Operation
                        foreach (DutyROTAEntity item in dataList)
                        {
                            Tran_DutyRota recordToUpdate = TASContext.Tran_DutyRota
                                .Where(a => a.AutoID == item.AutoID)
                                .FirstOrDefault();
                            if (recordToUpdate != null)
                            {
                                recordToUpdate.EmpNo = item.EmpNo;
                                recordToUpdate.EffectiveDate = Convert.ToDateTime(item.EffectiveDate);
                                recordToUpdate.EndingDate = Convert.ToDateTime(item.EndingDate);
                                recordToUpdate.DutyType = item.DutyType;
                                recordToUpdate.LastUpdateTime = item.LastUpdateTime;
                                recordToUpdate.LastUpdateUser = item.LastUpdateUser;

                                // Save to database
                                TASContext.SaveChanges();
                            }
                        }

                        break;
                    #endregion

                    case BLHelper.SaveType.Delete:
                        #region Perform Delete Operation
                        List<Tran_DutyRota> recordToDeleteList = new List<Tran_DutyRota>();

                        foreach (DutyROTAEntity item in dataList)
                        {
                            Tran_DutyRota recordToDelete = TASContext.Tran_DutyRota
                                .Where(a => a.AutoID == item.AutoID)
                                .FirstOrDefault();
                            if (recordToDelete != null)
                            {
                                // Add to collection
                                recordToDeleteList.Add(recordToDelete);
                            }
                        }

                        // Save to database
                        if (recordToDeleteList.Count > 0)
                        {
                            TASContext.Tran_DutyRota.RemoveRange(recordToDeleteList);
                            TASContext.SaveChanges();
                        }

                        break;
                        #endregion
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                        innerError = ex.InnerException.InnerException.Message.ToString();
                    else
                        innerError = ex.InnerException.Message.ToString();
                }
            }
        }

        public List<ShiftPatternEntity> GetEmployeeShiftPattern(int? autoID, int? empNo, string costCenter, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<ShiftPatternEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[5];
                parameters[0] = new ADONetParameter("@autoID", SqlDbType.Int, autoID);
                parameters[1] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[2] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[3] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[4] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetEmpShiftPatternInfo", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<ShiftPatternEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ShiftPatternEntity newItem = new ShiftPatternEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            ParentCostCenter = BLHelper.ConvertObjectToString(row["ParentCostCenter"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["WorkingBusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["WorkingBusinessUnitName"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftPointer = BLHelper.ConvertObjectToInt(row["ShiftPointer"]),
                            SpecialJobCatalog = BLHelper.ConvertObjectToString(row["SpecialJobCatg"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                            !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }

                        if (newItem.EmpNo > 0 &&
                            !string.IsNullOrEmpty(newItem.EmpName))
                        {
                            newItem.EmpFullName = string.Format("({0}) {1}",
                                newItem.EmpNo,
                                newItem.EmpName);
                        }
                        else
                            newItem.EmpFullName = newItem.EmpName;

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<ShiftPatternEntity> GetEmployeeShiftPatternExcel(int? autoID, int? empNo, string costCenter, ref string error, ref string innerError)
        {
            List<ShiftPatternEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[3];
                parameters[0] = new ADONetParameter("@autoID", SqlDbType.Int, autoID);
                parameters[1] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[2] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetEmpShiftPatternInfo_Excel", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<ShiftPatternEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ShiftPatternEntity newItem = new ShiftPatternEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            ParentCostCenter = BLHelper.ConvertObjectToString(row["ParentCostCenter"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["WorkingBusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["WorkingBusinessUnitName"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftPointer = BLHelper.ConvertObjectToInt(row["ShiftPointer"]),
                            SpecialJobCatalog = BLHelper.ConvertObjectToString(row["SpecialJobCatg"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"])
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                            !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }

                        if (newItem.EmpNo > 0 &&
                            !string.IsNullOrEmpty(newItem.EmpName))
                        {
                            newItem.EmpFullName = string.Format("({0}) {1}",
                                newItem.EmpNo,
                                newItem.EmpName);
                        }
                        else
                            newItem.EmpFullName = newItem.EmpName;

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<ShiftProjectionEntity> GetShiftProjection(DateTime? startDate, string costCenter, ref string error, ref string innerError)
        {
            List<ShiftProjectionEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[2];
                parameters[0] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[1] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetShiftProjection", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<ShiftProjectionEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ShiftProjectionEntity newItem = new ShiftProjectionEntity()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            Day1Value = BLHelper.ConvertObjectToString(row["d1"]),
                            Day2Value = BLHelper.ConvertObjectToString(row["d2"]),
                            Day3Value = BLHelper.ConvertObjectToString(row["d3"]),
                            Day4Value = BLHelper.ConvertObjectToString(row["d4"]),
                            Day5Value = BLHelper.ConvertObjectToString(row["d5"]),
                            Day6Value = BLHelper.ConvertObjectToString(row["d6"]),
                            Day7Value = BLHelper.ConvertObjectToString(row["d7"]),
                            Day8Value = BLHelper.ConvertObjectToString(row["d8"]),
                            Day9Value = BLHelper.ConvertObjectToString(row["d9"]),
                            Day10Value = BLHelper.ConvertObjectToString(row["d10"]),
                            Day11Value = BLHelper.ConvertObjectToString(row["d11"]),
                            Day12Value = BLHelper.ConvertObjectToString(row["d12"]),
                            Day13Value = BLHelper.ConvertObjectToString(row["d13"]),
                            Day14Value = BLHelper.ConvertObjectToString(row["d14"]),
                            Day15Value = BLHelper.ConvertObjectToString(row["d15"]),
                            Day16Value = BLHelper.ConvertObjectToString(row["d16"]),
                            Day17Value = BLHelper.ConvertObjectToString(row["d17"]),
                            Day18Value = BLHelper.ConvertObjectToString(row["d18"]),
                            Day19Value = BLHelper.ConvertObjectToString(row["d19"]),
                            Day20Value = BLHelper.ConvertObjectToString(row["d20"]),
                            Day21Value = BLHelper.ConvertObjectToString(row["d21"]),
                            Day22Value = BLHelper.ConvertObjectToString(row["d22"]),
                            Day23Value = BLHelper.ConvertObjectToString(row["d23"]),
                            Day24Value = BLHelper.ConvertObjectToString(row["d24"]),
                            Day25Value = BLHelper.ConvertObjectToString(row["d25"]),
                            Day26Value = BLHelper.ConvertObjectToString(row["d26"]),
                            Day27Value = BLHelper.ConvertObjectToString(row["d27"]),
                            Day28Value = BLHelper.ConvertObjectToString(row["d28"]),
                            Day29Value = BLHelper.ConvertObjectToString(row["d29"]),
                            Day30Value = BLHelper.ConvertObjectToString(row["d30"]),
                            Day31Value = BLHelper.ConvertObjectToString(row["d31"])
                        };

                        #region Calculate the Day of Week
                        newItem.Day1 = string.Format("{0:00}", Convert.ToDateTime(startDate).Day); 
                        newItem.Day1DOW = Convert.ToDateTime(startDate).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day2 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(1).Day); 
                        newItem.Day2DOW = Convert.ToDateTime(startDate).AddDays(1).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day3 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(2).Day);
                        newItem.Day3DOW = Convert.ToDateTime(startDate).AddDays(2).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day4 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(3).Day);
                        newItem.Day4DOW = Convert.ToDateTime(startDate).AddDays(3).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day5 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(4).Day);
                        newItem.Day5DOW = Convert.ToDateTime(startDate).AddDays(4).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day6 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(5).Day);
                        newItem.Day6DOW = Convert.ToDateTime(startDate).AddDays(5).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day7 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(6).Day);
                        newItem.Day7DOW = Convert.ToDateTime(startDate).AddDays(6).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day8 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(7).Day);
                        newItem.Day8DOW = Convert.ToDateTime(startDate).AddDays(7).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day9 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(8).Day);
                        newItem.Day9DOW = Convert.ToDateTime(startDate).AddDays(8).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day10 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(9).Day);
                        newItem.Day10DOW = Convert.ToDateTime(startDate).AddDays(9).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day11 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(10).Day);
                        newItem.Day11DOW = Convert.ToDateTime(startDate).AddDays(10).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day12 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(11).Day);
                        newItem.Day12DOW = Convert.ToDateTime(startDate).AddDays(11).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day13 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(12).Day);
                        newItem.Day13DOW = Convert.ToDateTime(startDate).AddDays(12).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day14 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(13).Day);
                        newItem.Day14DOW = Convert.ToDateTime(startDate).AddDays(13).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day15 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(14).Day);
                        newItem.Day15DOW = Convert.ToDateTime(startDate).AddDays(14).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day16 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(15).Day);
                        newItem.Day16DOW = Convert.ToDateTime(startDate).AddDays(15).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day17 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(16).Day);
                        newItem.Day17DOW = Convert.ToDateTime(startDate).AddDays(16).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day18 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(17).Day);
                        newItem.Day18DOW = Convert.ToDateTime(startDate).AddDays(17).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day19 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(18).Day);
                        newItem.Day19DOW = Convert.ToDateTime(startDate).AddDays(18).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day20 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(19).Day);
                        newItem.Day20DOW = Convert.ToDateTime(startDate).AddDays(19).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day21 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(20).Day);
                        newItem.Day21DOW = Convert.ToDateTime(startDate).AddDays(20).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day22 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(21).Day);
                        newItem.Day22DOW = Convert.ToDateTime(startDate).AddDays(21).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day23 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(22).Day);
                        newItem.Day23DOW = Convert.ToDateTime(startDate).AddDays(22).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day24 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(23).Day);
                        newItem.Day24DOW = Convert.ToDateTime(startDate).AddDays(23).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day25 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(24).Day);
                        newItem.Day25DOW = Convert.ToDateTime(startDate).AddDays(24).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day26 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(25).Day);
                        newItem.Day26DOW = Convert.ToDateTime(startDate).AddDays(25).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day27 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(26).Day);
                        newItem.Day27DOW = Convert.ToDateTime(startDate).AddDays(26).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day28 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(27).Day);
                        newItem.Day28DOW = Convert.ToDateTime(startDate).AddDays(27).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day29 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(28).Day);
                        newItem.Day29DOW = Convert.ToDateTime(startDate).AddDays(28).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day30 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(29).Day);
                        newItem.Day30DOW = Convert.ToDateTime(startDate).AddDays(29).DayOfWeek.ToString().Substring(0, 2).ToUpper();

                        newItem.Day31 = string.Format("{0:00}", Convert.ToDateTime(startDate).AddDays(30).Day);
                        newItem.Day31DOW = Convert.ToDateTime(startDate).AddDays(30).DayOfWeek.ToString().Substring(0, 2).ToUpper();
                        #endregion

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetEmployeeAttendanceHistory(DateTime? startDate, DateTime? endDate, string costCenter, int empNo, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> attendanceList = null;

            try
            {
                #region Fetch data using ADO.Net                                
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[4];

                parameters[0] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[1] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[2] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[3] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetEmpAttendanceHistory", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize the collection
                    attendanceList = new List<EmployeeAttendanceEntity>();

                    #region Populate data in the collection 
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["BusinessUnitName"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                            ActualShiftCode = BLHelper.ConvertObjectToString(row["Actual_ShiftCode"]),
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIn"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOut"]),
                            WorkDurationCumulative = BLHelper.ConvertObjectToInt(row["WorkDurationCumulative"]),
                            WorkDurationMinutes = BLHelper.ConvertObjectToInt(row["WorkDurationMinutes"]),
                            WorkDurationHours = BLHelper.ConvertObjectToString(row["WorkDurationHours"]) != string.Empty
                                ? BLHelper.ConvertObjectToString(row["WorkDurationHours"]).Insert(2, ":")
                                : string.Empty,
                            ShavedWorkDurationMinutes = BLHelper.ConvertObjectToInt(row["ShavedWorkDurationMinutes"]),
                            ShavedWorkDurationHours = BLHelper.ConvertObjectToString(row["ShavedWorkDurationHours"]) != string.Empty
                                ? BLHelper.ConvertObjectToString(row["ShavedWorkDurationHours"]).Insert(2, ":")
                                : string.Empty,

                            OTDurationMinutes = BLHelper.ConvertObjectToInt(row["OTDurationMinutes"]),
                            OvertimeDurationHours = BLHelper.ConvertObjectToString(row["OTDurationHours"]) != string.Empty
                                ? BLHelper.ConvertObjectToString(row["OTDurationHours"]).Insert(2, ":")
                                : string.Empty,

                            NoPayHours = BLHelper.ConvertObjectToInt(row["NoPayHours"]),
                            AttendanceRemarks = BLHelper.ConvertObjectToString(row["Remarks"]),
                            DurationRequired = BLHelper.ConvertObjectToInt(row["Duration_Required"]),
                            DayOffDuration = BLHelper.ConvertObjectToInt(row["DayOffDuration"]),
                            RequiredToSwipeAtWorkplace = BLHelper.ConvertNumberToBolean(row["RequiredToSwipeAtWorkplace"]),
                            TimeInMG = BLHelper.ConvertObjectToDate(row["TimeInMG"]),
                            TimeOutMG = BLHelper.ConvertObjectToDate(row["TimeOutMG"]),
                            TimeInWP = BLHelper.ConvertObjectToDate(row["TimeInWP"]),
                            TimeOutWP = BLHelper.ConvertObjectToDate(row["TimeOutWP"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            CorrectionCode = BLHelper.ConvertObjectToString(row["CorrectionCode"]),
                            RelativeTypeName = BLHelper.ConvertObjectToString(row["RelativeTypeName"]),
                            DeathRemarks = BLHelper.ConvertObjectToString(row["DeathRemarks"])
                        };

                        #region Calculate NPH in hours
                        if (newItem.NoPayHours > 0)
                        {
                            if (newItem.NoPayHours > 60)
                            {
                                int hrs = 0;
                                int min = 0;

                                hrs = Math.DivRem(Convert.ToInt32(newItem.NoPayHours), 60, out min);
                                newItem.NoPayHoursDesc = string.Format("{0}:{1}",
                                    string.Format("{0:00}", hrs),
                                    string.Format("{0:00}", min));
                            }
                            else
                            {
                                newItem.NoPayHoursDesc = string.Format("00:{0}", string.Format("{0:00}", newItem.NoPayHours));
                            }
                        }
                        #endregion

                        #region Identify the value of the workplace swipes
                        if (newItem.RequiredToSwipeAtWorkplace)
                        {
                            newItem.TimeIn = newItem.TimeInMG;
                            newItem.TimeOut = newItem.TimeOutMG;
                            newItem.TimeInWPString = newItem.TimeInWP.HasValue ? newItem.TimeInWP.Value.ToString("HH:mm") : "-";
                            newItem.TimeOutWPString = newItem.TimeOutWP.HasValue ? newItem.TimeOutWP.Value.ToString("HH:mm") : "-";
                        }
                        else
                        {
                            newItem.TimeIn = newItem.dtIN;
                            newItem.TimeOut = newItem.dtOUT;
                            newItem.TimeInWPString = @"N/A";
                            newItem.TimeOutWPString = @"N/A";
                        }
                        #endregion

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                           !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                            newItem.CostCenterFullName = newItem.CostCenter;

                        if (newItem.EmpNo > 0 && !string.IsNullOrEmpty(newItem.EmpName))
                        {
                            newItem.EmpFullName = string.Format("{0} - {1}",
                                newItem.EmpNo,
                                newItem.EmpName);
                        }
                        else
                            newItem.EmpFullName = string.Format("Employee Name: {0}", newItem.EmpName);

                        #region Add relative type and HR remarks to the Correction Description for all death related correction codes
                        StringBuilder sb = new StringBuilder();

                        if (newItem.CorrectionCode == "RAD1" ||
                            newItem.CorrectionCode == "RAD2" ||
                            newItem.CorrectionCode == "RAD3" ||
                            newItem.CorrectionCode == "RAD4")
                        {
                            sb.AppendLine(string.Format("<i>Relative:</i> {0}", newItem.RelativeTypeName));
                            //sb.AppendLine(string.Format("<i>Remarks:</i> {0}", !string.IsNullOrEmpty(newItem.DeathRemarks) ? newItem.DeathRemarks : "-"));

                            // Format text to HTML
                            newItem.CorrectionDesc = string.Format("<HTML><BODY><p>{0}</p></BODY></HTML>", sb.ToString().Trim().Replace("\r\n", "<br />"));
                        }
                        else if (newItem.CorrectionCode == "RAD0")
                        {
                            sb.AppendLine(string.Format("<i>Other Relative:</i> {0}", newItem.RelativeTypeName));
                            //sb.AppendLine(string.Format("<i>Remarks:</i> {0}", !string.IsNullOrEmpty(newItem.DeathRemarks) ? newItem.DeathRemarks : "-"));

                            // Format text to HTML
                            newItem.CorrectionDesc = string.Format("<HTML><BODY><p>{0}</p></BODY></HTML>", sb.ToString().Trim().Replace("\r\n", "<br />"));
                        }
                        #endregion

                        // Add item to the collection
                        attendanceList.Add(newItem);
                    }
                    #endregion
                }
                #endregion

                return attendanceList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<TASFormEntity> GetFormList(string formCode, ref string error, ref string innerError)
        {
            List<TASFormEntity> formList = null;

            try
            {
                var rawData = TASContext.GetTASForms(formCode);
                if (rawData != null)
                {
                    // Initialize the collection
                    formList = new List<TASFormEntity>();

                    foreach (var item in rawData)
                    {
                        TASFormEntity newItem = new TASFormEntity()
                        {
                            FormCode = BLHelper.ConvertObjectToString(item.FormCode),
                            FormName = BLHelper.ConvertObjectToString(item.FormName),
                            FormAppID = BLHelper.ConvertObjectToInt(item.FormAppID),
                            FormFilename = BLHelper.ConvertObjectToString(item.FormFilename),
                            FormPublic = BLHelper.ConvertObjectToBolean(item.FormPublic),
                            FormSeq = BLHelper.ConvertObjectToInt(item.FormSeq)
                        };

                        // Add item to the collection
                        formList.Add(newItem);
                    }
                }

                return formList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<DutyROTAEntity> GetDutyROTAReportData(DateTime? startDate, DateTime? endDate, string costCenterList, int? empNo, ref string error, ref string innerError)
        {
            List<DutyROTAEntity> dutyList = null;

            try
            {
                var rawData = TASContext.GetDutyROTAReport(startDate, endDate, costCenterList, empNo);
                if (rawData != null)
                {
                    // Initialize the collection
                    dutyList = new List<DutyROTAEntity>();

                    foreach (var item in rawData)
                    {
                        DutyROTAEntity newItem = new DutyROTAEntity()
                        {
                            AutoID = item.AutoID,
                            EmpNo = item.EmpNo,
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                            Position = BLHelper.ConvertObjectToString(item.Position),
                            CostCenter = BLHelper.ConvertObjectToString(item.BusinessUnit),
                            CostCenterName = BLHelper.ConvertObjectToString(item.BusinessUnitName),
                            EffectiveDate = item.EffectiveDate,
                            EndingDate = item.EndingDate,
                            DutyType = BLHelper.ConvertObjectToString(item.DutyType),
                            DutyDescription = BLHelper.ConvertObjectToString(item.DutyDescription),
                            DutyAllowance = item.DutyAllowance
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                           !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                            newItem.CostCenterFullName = newItem.CostCenter;

                        // Add item to the collection
                        dutyList.Add(newItem);
                    }
                }

                return dutyList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetDailyAttendanceReportData(byte employeeType, DateTime? startDate, DateTime? endDate, string costCenterList, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> attendanceList = null;

            try
            {
                var rawData = TASContext.GetDailyAttendanceReport(employeeType, startDate, endDate, costCenterList);
                if (rawData != null)
                {
                    // Initialize the collection
                    attendanceList = new List<EmployeeAttendanceEntity>();

                    foreach (var item in rawData)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            EmpNo = item.EmpNo,
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                            Position = BLHelper.ConvertObjectToString(item.Position),
                            CostCenter = BLHelper.ConvertObjectToString(item.BusinessUnit),
                            CostCenterName = BLHelper.ConvertObjectToString(item.BusinessUnitName),
                            DT = item.DT,
                            dtIN = item.dtIN,
                            dtOUT = item.dtOUT,
                            NetMinutes = item.NetMinutes,
                            ShiftPatCode = BLHelper.ConvertObjectToString(item.ShiftPatCode),
                            ShiftCode = BLHelper.ConvertObjectToString(item.ShiftCode),
                            ActualShiftCode = BLHelper.ConvertObjectToString(item.Actual_ShiftCode),
                            OTStartTime = item.OTStartTime,
                            OTEndTime = item.OTEndTime,
                            OTType = BLHelper.ConvertObjectToString(item.OTType),
                            NoPayHours = item.NoPayHours,
                            ShiftAllowance = item.ShiftAllowance,
                            ShiftAllowanceDesc = BLHelper.ConvertObjectToString(item.ShiftAllowanceDesc),
                            DurationShiftAllowanceEvening = item.Duration_ShiftAllowance_Evening,
                            DurationShiftAllowanceNight = item.Duration_ShiftAllowance_Night,
                            AttendanceRemarks = BLHelper.ConvertObjectToString(item.Remark),
                            IsSalaryStaff = item.IsSalStaff,
                            ShiftTiming = item.ShiftTiming
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                           !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                            newItem.CostCenterFullName = newItem.CostCenter;

                        newItem.NoPayHoursDesc = BLHelper.ConvertMinuteToHourString(newItem.NoPayHours);

                        // Add item to the collection
                        attendanceList.Add(newItem);
                    }
                }

                return attendanceList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetAbsenceReasonReportData(DateTime? startDate, DateTime? endDate, byte employeeType, string costCenter, int empNo, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> reportDataList = null;

            try
            {
                var rawData = TASContext.GetAbsenceReasonReport(startDate, endDate, employeeType, costCenter, empNo);
                if (rawData != null)
                {
                    // Initialize the collection
                    List<EmployeeAttendanceEntity> tempData = new List<EmployeeAttendanceEntity>();                    

                    foreach (var item in rawData)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(item.EmpNo),
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                            GradeCode = item.GradeCode,
                            CostCenter = BLHelper.ConvertObjectToString(item.CostCenter),
                            CostCenterName = BLHelper.ConvertObjectToString(item.CostCenterName),
                            DT = item.DT,
                            ShiftPatCode = BLHelper.ConvertObjectToString(item.ShiftPatCode),
                            RemarkCode = BLHelper.ConvertObjectToString(item.RemarkCode),
                            CorrectionCode = BLHelper.ConvertObjectToString(item.CorrectionCode)
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                           !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                            newItem.CostCenterFullName = newItem.CostCenter;

                        if (newItem.EmpNo > 0 &&
                           !string.IsNullOrEmpty(newItem.EmpName))
                        {
                            newItem.EmpFullName = string.Format("({0}) {1}",
                                newItem.EmpNo,
                                newItem.EmpName);
                        }
                        else
                            newItem.EmpFullName = newItem.EmpName;

                        // Add item to the collection
                        tempData.Add(newItem);
                    }

                    #region Filter the records
                    if (tempData.Count > 0)
                    {
                        reportDataList = new List<EmployeeAttendanceEntity>();

                        if (string.IsNullOrEmpty(costCenter) &&
                            empNo == 0)
                        {
                            reportDataList.AddRange(tempData.ToList());
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(costCenter))
                                reportDataList = tempData.Where(a => a.CostCenter == costCenter.Trim()).ToList();

                            if (empNo > 0)
                                reportDataList = tempData.Where(a => a.EmpNo == empNo).ToList();
                        }
                    }
                    #endregion
                }

                return reportDataList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetDILDueToLateEntryOfDutyROTA(DateTime? startDate, DateTime? endDate, string costCenter, int empNo, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> reportDataList = null;

            try
            {
                var rawData = TASContext.GetDILDueToLateEntryDutyROTA(startDate, endDate, costCenter, empNo);
                if (rawData != null)
                {
                    // Initialize the collection
                    List<EmployeeAttendanceEntity> tempData = new List<EmployeeAttendanceEntity>();

                    foreach (var item in rawData)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = item.AutoID,
                            EmpNo = BLHelper.ConvertObjectToInt(item.EmpNo),
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                            CostCenter = BLHelper.ConvertObjectToString(item.BusinessUnit),
                            DT = item.DT,
                            DILEntitlement = BLHelper.ConvertObjectToString(item.DIL_Entitlement),
                            DILDescription = BLHelper.ConvertObjectToString(item.DIL_Desc),
                            EffectiveDate = item.EffectiveDate,
                            EndingDate = item.EndingDate,
                            LastUpdateUser = BLHelper.ConvertObjectToString(item.LastUpdateUser),
                            LastUpdateTime = item.LastUpdateTime,
                            EntryAfterDays = item.EntryAfterDays
                        };

                        if (newItem.EmpNo > 0 &&
                           !string.IsNullOrEmpty(newItem.EmpName))
                        {
                            newItem.EmpFullName = string.Format("({0}) {1}",
                                newItem.EmpNo,
                                newItem.EmpName);
                        }
                        else
                            newItem.EmpFullName = newItem.EmpName;

                        // Add item to the collection
                        tempData.Add(newItem);
                    }

                    #region Filter the records
                    if (tempData.Count > 0)
                    {
                        reportDataList = new List<EmployeeAttendanceEntity>();

                        if (string.IsNullOrEmpty(costCenter) &&
                            empNo == 0)
                        {
                            reportDataList.AddRange(tempData.ToList());
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(costCenter))
                                reportDataList = tempData.Where(a => a.CostCenter == costCenter.Trim()).ToList();

                            if (empNo > 0)
                                reportDataList = tempData.Where(a => a.EmpNo == empNo).ToList();
                        }
                    }
                    #endregion
                }

                return reportDataList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetDILReportData(int empNo, string costCenter, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> reportDataList = null;

            try
            {
                var rawData = TASContext.GetDILReport(empNo, costCenter, startDate, endDate);
                if (rawData != null)
                {
                    // Initialize the collection
                    List<EmployeeAttendanceEntity> tempData = new List<EmployeeAttendanceEntity>();

                    foreach (var item in rawData)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            AutoID = item.AutoID,
                            EmpNo = BLHelper.ConvertObjectToInt(item.EmpNo),
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                            CostCenter = BLHelper.ConvertObjectToString(item.BusinessUnit),
                            CostCenterName = BLHelper.ConvertObjectToString(item.BusinessUnitName),
                            DT = item.DT,
                            DILEntitlement = BLHelper.ConvertObjectToString(item.DIL_Entitlement),
                            DILDescription = BLHelper.ConvertObjectToString(item.DIL_Desc),
                            DateUsed = item.DateUsed,
                            Remarks = BLHelper.ConvertObjectToString(item.Remarks)
                        };

                        if (newItem.EmpNo > 0 &&
                           !string.IsNullOrEmpty(newItem.EmpName))
                        {
                            newItem.EmpFullName = string.Format("({0}) {1}",
                                newItem.EmpNo,
                                newItem.EmpName);
                        }
                        else
                            newItem.EmpFullName = newItem.EmpName;

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                          !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                            newItem.CostCenterFullName = newItem.CostCenter;

                        // Add item to the collection
                        tempData.Add(newItem);
                    }

                    #region Filter the records
                    if (tempData.Count > 0)
                    {
                        reportDataList = new List<EmployeeAttendanceEntity>();

                        if (string.IsNullOrEmpty(costCenter) &&
                            empNo == 0)
                        {
                            reportDataList.AddRange(tempData.ToList());
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(costCenter))
                                reportDataList = tempData.Where(a => a.CostCenter == costCenter.Trim()).ToList();

                            if (empNo > 0)
                                reportDataList = tempData.Where(a => a.EmpNo == empNo).ToList();
                        }
                    }
                    #endregion
                }

                return reportDataList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetWeeklyOvertimeReportData(DateTime? startDate, DateTime? endDate, string costCenterList, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> reportDataList = null;

            try
            {
                var rawData = TASContext.GetWeeklyOvertimeReport(startDate, endDate, costCenterList);
                if (rawData != null)
                {
                    // Initialize the collection
                    reportDataList = new List<EmployeeAttendanceEntity>();

                    foreach (var item in rawData)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            CostCenter = BLHelper.ConvertObjectToString(item.BusinessUnit),
                            CostCenterName = BLHelper.ConvertObjectToString(item.BusinessUnitName),
                            TotalOTMinutes = BLHelper.ConvertObjectToInt(item.TotalOTMinutes)
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                           !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                            newItem.CostCenterFullName = newItem.CostCenter;

                        // Add item to the collection
                        reportDataList.Add(newItem);
                    }
                }

                return reportDataList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeDirectoryEntity> GetEmployeeDirectory(int empNo, string costCenter, string searchString, ref string error, ref string innerError)
        {
            List<EmployeeDirectoryEntity> rawDataList = null;

            try
            {
                var rawData = TASContext.GetEmployeeDirectory(empNo, costCenter, searchString);
                if (rawData != null)
                {
                    // Initialize the collection
                    rawDataList = new List<EmployeeDirectoryEntity>();

                    foreach (var item in rawData)
                    {
                        EmployeeDirectoryEntity newItem = new EmployeeDirectoryEntity()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(item.EmpNo),
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                            SupervisorNo = BLHelper.ConvertObjectToInt(item.SupervisorNo),
                            SupervisorName = BLHelper.ConvertObjectToString(item.SupervisorName),
                            Position = BLHelper.ConvertObjectToString(item.Position),
                            CostCenter = BLHelper.ConvertObjectToString(item.BusinessUnit),
                            CostCenterName = BLHelper.ConvertObjectToString(item.BusinessUnitName),
                            Religion = BLHelper.ConvertObjectToString(item.Religion),
                            Sex = BLHelper.ConvertObjectToString(item.Sex),
                            JobCategory = BLHelper.ConvertObjectToString(item.JobCategory),
                            PayGrade = item.GradeCode,
                            DateJoined = item.DateJoined,
                            YearsOfService = item.YearsOfService,
                            DateOfBirth = item.DateOfBirth,
                            Age = item.Age,
                            TelephoneExt = BLHelper.ConvertObjectToString(item.TelephoneExt),
                            MobileNo = BLHelper.ConvertObjectToString(item.MobileNo),
                            TelNo = BLHelper.ConvertObjectToString(item.TelNo),
                            FaxNo = BLHelper.ConvertObjectToString(item.FaxNo),
                            Email = BLHelper.ConvertObjectToString(item.EmpEmail)
                        };

                        if (newItem.SupervisorNo > 0 &&
                            !string.IsNullOrEmpty(newItem.SupervisorName))
                        {
                            newItem.SupervisorFullName = string.Format("{0} - {1}",
                                newItem.SupervisorNo,
                                newItem.SupervisorName);
                        }
                        else
                            newItem.SupervisorFullName = newItem.SupervisorName;

                        // Add item to the collection
                        rawDataList.Add(newItem);
                    }
                }

                return rawDataList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetAspirePayrolReport(DateTime? startDate, DateTime? endDate, int processType, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> rawDataList = null;

            try
            {
                var rawData = TASContext.GetAspirePayrollReport(startDate, endDate, processType);
                if (rawData != null)
                {
                    // Initialize the collection
                    rawDataList = new List<EmployeeAttendanceEntity>();

                    foreach (var item in rawData)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(item.EmpNo),
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                            DT = item.DT,
                            PayGrade = BLHelper.ConvertObjectToString(item.PayGrade),
                            PayHour = BLHelper.ConvertObjectToString(item.PayHour),
                            PayMinute = BLHelper.ConvertObjectToInt(item.PayMinute),
                            PayDescription = BLHelper.ConvertObjectToString(item.PayDescription),
                            PaymentStatus = BLHelper.ConvertObjectToString(item.PaymentStatus)
                        };

                        // Add item to the collection
                        rawDataList.Add(newItem);
                    }
                }

                return rawDataList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<AttendanceStatisticsEntity> GetPunctualityReport(DateTime? startDate, DateTime? endDate, string costCenter, bool showDayOff, bool showCount, ref string error, ref string innerError)
        {
            List<AttendanceStatisticsEntity> rawDataList = null;

            try
            {
                var rawData = TASContext.GetPunctualityReport(startDate, endDate, costCenter, showDayOff, showCount);
                if (rawData != null)
                {
                    // Initialize the collection
                    rawDataList = new List<AttendanceStatisticsEntity>();

                    foreach (var item in rawData)
                    {
                        AttendanceStatisticsEntity newItem = new AttendanceStatisticsEntity()
                        {
                            CostCenter = BLHelper.ConvertObjectToString(item.BusinessUnit),
                            AttendanceDate = BLHelper.ConvertObjectToString(item.AttendanceDate),
                            Total_0700_Below = Math.Round(BLHelper.ConvertObjectToDouble(item.Total_0700_Below), 2),
                            Total_0700_0710 = Math.Round(BLHelper.ConvertObjectToDouble(item.Total_0700_0710), 2),
                            Total_0710_0720 = Math.Round(BLHelper.ConvertObjectToDouble(item.Total_0710_0720), 2),
                            Total_0720_0730 = Math.Round(BLHelper.ConvertObjectToDouble(item.Total_0720_0730), 2),
                            Total_0730_0740 = Math.Round(BLHelper.ConvertObjectToDouble(item.Total_0730_0740), 2),
                            Total_0740_0750 = Math.Round(BLHelper.ConvertObjectToDouble(item.Total_0740_0750), 2),
                            Total_0750_0800 = Math.Round(BLHelper.ConvertObjectToDouble(item.Total_0750_0800), 2),
                            Total_0800_Above = Math.Round(BLHelper.ConvertObjectToDouble(item.Total_0800_Above), 2),
                            Total_Absent = Math.Round(BLHelper.ConvertObjectToDouble(item.Total_Absent), 2)
                        };

                        // Get the total
                        //newItem.TotalNumber = newItem.Total_0700_Below + newItem.Total_0700_0710 + newItem.Total_0710_0720 + newItem.Total_0720_0730
                        //    + newItem.Total_0730_0740 + newItem.Total_0740_0750 + newItem.Total_0750_0800 + newItem.Total_0800_Above + newItem.Total_Absent;

                        newItem.TotalNumber = Math.Round(BLHelper.ConvertObjectToDouble(item.Total_0700_Below) +
                                                BLHelper.ConvertObjectToDouble(item.Total_0700_0710) +
                                                BLHelper.ConvertObjectToDouble(item.Total_0710_0720) +
                                                BLHelper.ConvertObjectToDouble(item.Total_0720_0730) +
                                                BLHelper.ConvertObjectToDouble(item.Total_0730_0740) +
                                                BLHelper.ConvertObjectToDouble(item.Total_0740_0750) +
                                                BLHelper.ConvertObjectToDouble(item.Total_0750_0800) +
                                                BLHelper.ConvertObjectToDouble(item.Total_0800_Above) +
                                                BLHelper.ConvertObjectToDouble(item.Total_Absent), 2);

                        // Add item to the collection
                        rawDataList.Add(newItem);
                    }
                }

                return rawDataList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<TASJDEComparisonEntity> GetTASJDEComparisonReport(ref string error, ref string innerError)
        {
            List<TASJDEComparisonEntity> rawDataList = null;

            try
            {
                var rawData = TASContext.GetTASJDEComparisonReport();
                if (rawData != null)
                {
                    // Initialize the collection
                    rawDataList = new List<TASJDEComparisonEntity>();

                    foreach (var item in rawData)
                    {
                        TASJDEComparisonEntity newItem = new TASJDEComparisonEntity()
                        {
                            PDBA = BLHelper.ConvertObjectToString(item.PDBA),
                            PDBAName = BLHelper.ConvertObjectToString(item.PDBA_Name),
                            TASCount = BLHelper.ConvertObjectToInt(item.TAS_cnt),
                            DiffTAS = BLHelper.ConvertObjectToInt(item.Diff_TAS),
                            JDECount = BLHelper.ConvertObjectToInt(item.JDE_cnt),
                            DiffJDE = BLHelper.ConvertObjectToInt(item.Diff_JDE),
                            TotalDiff = BLHelper.ConvertObjectToInt(item.Diff)
                        };

                        // Add item to the collection
                        rawDataList.Add(newItem);
                    }
                }

                return rawDataList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void GetTASJDETransactionHistory(string PDBA, out List<TASJDEComparisonEntity> tasHistoryList, out List<TASJDEComparisonEntity> jdeHistoryList, 
            ref string error, ref string innerError)
        {
            // Initialize collection
            tasHistoryList = new List<TASJDEComparisonEntity>();
            jdeHistoryList = new List<TASJDEComparisonEntity>();

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[1];
                parameters[0] = new ADONetParameter("@PDBA", SqlDbType.VarChar, 10, PDBA);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetTASJDETransHistory", connectionString, parameters);
                if (ds != null && ds.Tables.Count == 2)
                {                    
                    #region Get TAS transaction history
                    if (ds.Tables[0].Rows.Count > 0)
                    {                        
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            TASJDEComparisonEntity newItem = new TASJDEComparisonEntity()
                            {
                                AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                                EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                                DT = BLHelper.ConvertObjectToDate(row["DT"])
                            };

                            if (ds.Tables[0].Columns.Contains("PDBA"))
                                newItem.PDBA = BLHelper.ConvertObjectToString(row["PDBA"]);

                            if (ds.Tables[0].Columns.Contains("txt"))
                                newItem.Txt = BLHelper.ConvertObjectToString(row["txt"]);

                            if (ds.Tables[0].Columns.Contains("OTFrom"))
                                newItem.OTFrom = BLHelper.ConvertObjectToDate(row["OTFrom"]);

                            if (ds.Tables[0].Columns.Contains("OTTo"))
                                newItem.OTTo = BLHelper.ConvertObjectToDate(row["OTTo"]);

                            // Add item to collection
                            tasHistoryList.Add(newItem);
                        };
                    }
                    #endregion

                    #region Get JDE transaction history
                    if (ds.Tables[1].Rows.Count > 0)
                    {                                                
                        foreach (DataRow row in ds.Tables[1].Rows)
                        {
                            TASJDEComparisonEntity newItem = new TASJDEComparisonEntity()
                            {
                                Txt = BLHelper.ConvertObjectToString(row["txt"]),
                                AutoID = BLHelper.ConvertObjectToInt(row["autoid"]),
                                DT = BLHelper.ConvertObjectToDate(row["DT"]),
                                EmpNo = BLHelper.ConvertObjectToInt(row["empno"]),
                                XXXXX = BLHelper.ConvertObjectToString(row["XXXXX"]),
                                JPDBA = BLHelper.ConvertObjectToString(row["Jpdba"]),
                                JEmpNo = BLHelper.ConvertObjectToInt(row["Jempno"]),
                                JAutoID = BLHelper.ConvertObjectToInt(row["Jautoid"]),
                                Jhours = BLHelper.ConvertObjectToDouble(row["JHrs"])
                            };

                            // Add item to collection
                            jdeHistoryList.Add(newItem);
                        };
                    }
                    #endregion
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public List<ContractorEntity> GetContractorShiftPattern(int? autoID, int? empNo, string empName, DateTime? dateJoinedStart, DateTime? dateJoinedEnd,
            DateTime? dateResignedStart, DateTime? dateResignedEnd, ref string error, ref string innerError)
        {
            List<ContractorEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[7];
                parameters[0] = new ADONetParameter("@autoID", SqlDbType.Int, autoID);
                parameters[1] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[2] = new ADONetParameter("empName", SqlDbType.VarChar, 40, empName);
                parameters[3] = new ADONetParameter("@dateJoinedStart", SqlDbType.DateTime, dateJoinedStart);
                parameters[4] = new ADONetParameter("@dateJoinedEnd", SqlDbType.DateTime, dateJoinedEnd);
                parameters[5] = new ADONetParameter("@dateResignedStart", SqlDbType.DateTime, dateResignedStart);
                parameters[6] = new ADONetParameter("@dateResignedEnd", SqlDbType.DateTime, dateResignedEnd);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetContractorShiftPattern", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<ContractorEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ContractorEntity newItem = new ContractorEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            ContractorNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            ContractorName = BLHelper.ConvertObjectToString(row["ContractorEmpName"]),
                            GroupCode = BLHelper.ConvertObjectToString(row["GroupCode"]),
                            GroupDesc = BLHelper.ConvertObjectToString(row["GroupDesc"]),
                            SupplierNo = BLHelper.ConvertObjectToInt(row["SupplierNo"]),
                            SupplierName = BLHelper.ConvertObjectToString(row["SupplierName"]),
                            DateJoined = BLHelper.ConvertObjectToDate(row["DateJoined"]),
                            DateResigned = BLHelper.ConvertObjectToDate(row["DateResigned"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftPointer = BLHelper.ConvertObjectToInt(row["ShiftPointer"]),
                            ReligionCode = BLHelper.ConvertObjectToString(row["ReligionCode"]),
                            ReligionDesc = BLHelper.ConvertObjectToString(row["ReligionDesc"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"])
                        };

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<ContractorEntity> GetContractorShiftPatternV2(int? autoID, int? empNo, string empName, DateTime? dateJoinedStart, DateTime? dateJoinedEnd,
            DateTime? dateResignedStart, DateTime? dateResignedEnd, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<ContractorEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[9];
                parameters[0] = new ADONetParameter("@autoID", SqlDbType.Int, autoID);
                parameters[1] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[2] = new ADONetParameter("@empName", SqlDbType.VarChar, 40, empName);
                parameters[3] = new ADONetParameter("@dateJoinedStart", SqlDbType.DateTime, dateJoinedStart);
                parameters[4] = new ADONetParameter("@dateJoinedEnd", SqlDbType.DateTime, dateJoinedEnd);
                parameters[5] = new ADONetParameter("@dateResignedStart", SqlDbType.DateTime, dateResignedStart);
                parameters[6] = new ADONetParameter("@dateResignedEnd", SqlDbType.DateTime, dateResignedEnd);
                parameters[7] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[8] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetContractorShiftPattern_V2", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<ContractorEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ContractorEntity newItem = new ContractorEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            ContractorNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            ContractorName = BLHelper.ConvertObjectToString(row["ContractorEmpName"]),
                            GroupCode = BLHelper.ConvertObjectToString(row["GroupCode"]),
                            GroupDesc = BLHelper.ConvertObjectToString(row["GroupDesc"]),
                            SupplierNo = BLHelper.ConvertObjectToInt(row["SupplierNo"]),
                            SupplierName = BLHelper.ConvertObjectToString(row["SupplierName"]),
                            DateJoined = BLHelper.ConvertObjectToDate(row["DateJoined"]),
                            DateResigned = BLHelper.ConvertObjectToDate(row["DateResigned"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftPointer = BLHelper.ConvertObjectToInt(row["ShiftPointer"]),
                            ReligionCode = BLHelper.ConvertObjectToString(row["ReligionCode"]),
                            ReligionDesc = BLHelper.ConvertObjectToString(row["ReligionDesc"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"])
                        };

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void InsertUpdateDeleteContractorShiftPattern(int saveTypeNum, List<ContractorEntity> dataList, ref string error, ref string innerError)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                BLHelper.SaveType saveType = (BLHelper.SaveType)Enum.Parse(typeof(BLHelper.SaveType), saveTypeNum.ToString());

                switch (saveType)
                {
                    case BLHelper.SaveType.Insert:
                        #region Perform Insert Operation

                        #region Insert new record using Entity Framework                                                
                        List<Master_ContractEmployee> recordToInsertList = new List<Master_ContractEmployee>();
                        foreach (ContractorEntity item in dataList)
                        {
                            #region Check if Contractor No. already exist
                            var findExistingRecord = (from a in TASContext.Master_ContractEmployee
                                                      where a.EmpNo == item.ContractorNo
                                                      select a).FirstOrDefault();
                            if (findExistingRecord != null)
                            {
                                throw new Exception(string.Format("Unable to insert new record because Contractor No. {0} already exist in the database. Please specify a unique Contractor No. then try to save the record again!", item.ContractorNo));
                            }
                            #endregion

                            recordToInsertList.Add(new Master_ContractEmployee()
                            {
                                EmpNo = item.ContractorNo,
                                ContractorEmpName = item.ContractorName,
                                GroupCode = item.GroupCode,
                                ContractorNumber = item.SupplierNo,
                                DateJoined = item.DateJoined,
                                DateResigned = item.DateResigned,
                                ShiftPatCode = item.ShiftPatCode,
                                ShiftPointer = item.ShiftPointer,
                                ReligionCode = item.ReligionCode,
                                LastUpdateTime = item.LastUpdateTime,
                                LastUpdateUser = item.LastUpdateUser
                            });
                        }

                        // Save to database
                        if (recordToInsertList.Count > 0)
                        {
                            TASContext.Master_ContractEmployee.AddRange(recordToInsertList);
                            TASContext.SaveChanges();
                        }
                        #endregion

                        #region Insert new record using ADO.NET                                                
                        //foreach (ContractorEntity item in dataList)
                        //{
                        //    #region Check if Contractor No. already exist
                        //    var findExistingRecord = (from a in TASContext.Master_ContractEmployee
                        //                              where a.EmpNo == item.ContractorNo
                        //                              select a).FirstOrDefault();
                        //    if (findExistingRecord != null)
                        //    {
                        //        throw new Exception(string.Format("Unable to insert new record because Contractor No. {0} already exist in the database. Please specify a unique Contractor No. then try to save the record again!", item.ContractorNo));
                        //    }
                        //    #endregion

                        //    var resultInsert = TASContext.InsertUpdateDeleteContractEmployee
                        //    (
                        //        saveTypeNum,
                        //        item.AutoID,
                        //        item.ContractorNo,
                        //        item.ContractorName,
                        //        item.GroupCode,
                        //        item.SupplierNo,
                        //        item.DateJoined,
                        //        item.DateResigned,
                        //        item.ShiftPatCode,
                        //        item.ShiftPointer,
                        //        item.ReligionCode,
                        //        item.LastUpdateUser
                        //    );

                        //    // Return the rows affected
                        //    int rowsAffected = BLHelper.ConvertObjectToInt(resultInsert.FirstOrDefault().RowsAffected);
                        //}
                        #endregion

                        break;
                        #endregion

                    case BLHelper.SaveType.Update:
                        #region Perform Update Operation
                        foreach (ContractorEntity item in dataList)
                        {
                            Master_ContractEmployee recordToUpdate = TASContext.Master_ContractEmployee
                                .Where(a => a.AutoID == item.AutoID)
                                .FirstOrDefault();
                            if (recordToUpdate != null)
                            {
                                recordToUpdate.ContractorEmpName = item.ContractorName;
                                recordToUpdate.GroupCode = item.GroupCode;
                                recordToUpdate.ContractorNumber = item.SupplierNo;
                                recordToUpdate.DateJoined = item.DateJoined;
                                recordToUpdate.DateResigned = item.DateResigned;
                                recordToUpdate.ShiftPatCode = item.ShiftPatCode;
                                recordToUpdate.ShiftPointer = item.ShiftPointer;
                                recordToUpdate.ReligionCode = item.ReligionCode;
                                recordToUpdate.LastUpdateTime = item.LastUpdateTime;
                                recordToUpdate.LastUpdateUser = item.LastUpdateUser;

                                // Save to database
                                TASContext.SaveChanges();
                            }
                        }

                        break;
                        #endregion

                    case BLHelper.SaveType.Delete:
                        #region Perform Delete Operation
                        List<Master_ContractEmployee> recordToDeleteList = new List<Master_ContractEmployee>();

                        foreach (ContractorEntity item in dataList)
                        {
                            Master_ContractEmployee recordToDelete = TASContext.Master_ContractEmployee
                                .Where(a => a.AutoID == item.AutoID)
                                .FirstOrDefault();
                            if (recordToDelete != null)
                            {
                                // Add to collection
                                recordToDeleteList.Add(recordToDelete);
                            }
                        }

                        // Save to database
                        if (recordToDeleteList.Count > 0)
                        {
                            TASContext.Master_ContractEmployee.RemoveRange(recordToDeleteList);
                            TASContext.SaveChanges();
                        }

                        break;
                        #endregion
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public List<TrainingRecordEntity> GetTrainingRecord(long? trainingRecordID, int? empNo, string costCenter, int? trainingProgramID, int? trainingProviderID,
            string qualificationCode, string typeOfTrainingCode, string statusCode, DateTime? fromDate, DateTime? toDate, byte? createdByTypeID, int? userEmpNo, int? createdByOtherEmpNo,
            DateTime? createdStartDate, DateTime? createdEndDate, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<TrainingRecordEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[17];

                parameters[0] = new ADONetParameter("@trainingRecordID", SqlDbType.BigInt, trainingRecordID);
                parameters[1] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[2] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[3] = new ADONetParameter("@trainingProgramID", SqlDbType.Int, trainingProgramID);
                parameters[4] = new ADONetParameter("@trainingProviderID", SqlDbType.Int, trainingProviderID);
                parameters[5] = new ADONetParameter("@qualificationCode", SqlDbType.VarChar, 10, qualificationCode);
                parameters[6] = new ADONetParameter("@typeOfTrainingCode", SqlDbType.VarChar, 10, typeOfTrainingCode);
                parameters[7] = new ADONetParameter("@statusCode", SqlDbType.VarChar, 10, statusCode);
                parameters[8] = new ADONetParameter("@fromDate", SqlDbType.DateTime, fromDate);
                parameters[9] = new ADONetParameter("@toDate", SqlDbType.DateTime, toDate);
                parameters[10] = new ADONetParameter("@createdByTypeID", SqlDbType.TinyInt, createdByTypeID);
                parameters[11] = new ADONetParameter("@userEmpNo", SqlDbType.Int, userEmpNo);
                parameters[12] = new ADONetParameter("@createdByOtherEmpNo", SqlDbType.Int, createdByOtherEmpNo);
                parameters[13] = new ADONetParameter("@createdStartDate", SqlDbType.DateTime, createdStartDate);
                parameters[14] = new ADONetParameter("@createdEndDate", SqlDbType.DateTime, createdEndDate);
                parameters[15] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[16] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetTrainingRecord_V2", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<TrainingRecordEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        TrainingRecordEntity newItem = new TrainingRecordEntity()
                        {
                            AttendeeeType = BLHelper.ConvertObjectToByte(row["AttendeeeType"]),
                            TrainingRecordID = BLHelper.ConvertObjectToLong(row["TrainingRecordID"]),
                            TrainingProgramID = BLHelper.ConvertObjectToInt(row["TrainingProgramID"]),
                            CourseTitle = BLHelper.ConvertObjectToString(row["CourseTitle"]),
                            CourseDescription = BLHelper.ConvertObjectToString(row["CourseDescription"]),
                            CourseCode = BLHelper.ConvertObjectToString(row["CourseCode"]),

                            CourseSessionID = BLHelper.ConvertObjectToInt(row["CourseSessionID"]),
                            FromDate = BLHelper.ConvertObjectToDate(row["FromDate"]),
                            ToDate = BLHelper.ConvertObjectToDate(row["ToDate"]),
                            Duration = BLHelper.ConvertObjectToInt(row["Duration"]),
                            DurationCode = BLHelper.ConvertObjectToString(row["DurationType"]),
                            DurationDesc = BLHelper.ConvertObjectToString(row["DurationDesc"]),
                            Location = BLHelper.ConvertObjectToString(row["DurationDesc"]),
                            Cost = BLHelper.ConvertObjectToDouble(row["Cost"]),

                            TrainingProviderID = BLHelper.ConvertObjectToInt(row["TrainingProviderID"]),
                            TrainingProviderName = BLHelper.ConvertObjectToString(row["TrainingProviderName"]),
                            QualificationCode = BLHelper.ConvertObjectToString(row["QualificationCode"]),
                            QualificationDesc = BLHelper.ConvertObjectToString(row["QualificationDesc"]),
                            TypeOfTrainingCode = BLHelper.ConvertObjectToString(row["TypeOfTrainingCode"]),
                            TypeOfTrainingDesc = BLHelper.ConvertObjectToString(row["TypeOfTrainingDesc"]),
                            StatusCode = BLHelper.ConvertObjectToString(row["StatusCode"]),
                            StatusDesc = BLHelper.ConvertObjectToString(row["StatusDesc"]),
                            Remarks = BLHelper.ConvertObjectToString(row["Remarks"]),
                            DatesAttendedArray = BLHelper.ConvertObjectToString(row["DatesAttendedArray"]),
                            DurationAttended = BLHelper.ConvertObjectToInt(row["DurationAttended"]),
                            DurationAttendedCode = BLHelper.ConvertObjectToString(row["DurationType"]),
                            DurationAttendedDesc = BLHelper.ConvertObjectToString(row["DurationDesc"]),

                            CreatedDate = BLHelper.ConvertObjectToDate(row["CreatedDate"]),
                            CreatedByEmpNo = BLHelper.ConvertObjectToInt(row["CreatedByEmpNo"]),
                            CreatedByUserID = BLHelper.ConvertObjectToString(row["CreatedByUserID"]),
                            CreatedByEmpName = BLHelper.ConvertObjectToString(row["CreatedByEmpName"]),
                            CreatedByEmpEmail = BLHelper.ConvertObjectToString(row["CreatedByEmpEmail"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            LastUpdateEmpNo = BLHelper.ConvertObjectToInt(row["LastUpdateEmpNo"]),
                            LastUpdateUserID = BLHelper.ConvertObjectToString(row["LastUpdateUserID"]),
                            LastUpdateEmpName = BLHelper.ConvertObjectToString(row["LastUpdateEmpName"]),
                            LastUpdateEmpEmail = BLHelper.ConvertObjectToString(row["LastUpdateEmpEmail"]),
                            TotalRecords = BLHelper.ConvertObjectToInt(row["TotalRecords"]),
                        };

                        if (newItem.AttendeeeType == Convert.ToByte(BLHelper.AttendeeType.Employee))
                        {
                            newItem.EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]);
                            newItem.EmpName = BLHelper.ConvertObjectToString(row["EmpName"]);
                            newItem.EmpPosition = BLHelper.ConvertObjectToString(row["EmpPosition"]);
                            newItem.SupervisorNo = BLHelper.ConvertObjectToInt(row["SupervisorNo"]);
                            newItem.SupervisorName = BLHelper.ConvertObjectToString(row["SupervisorName"]);
                            newItem.ManagerNo = BLHelper.ConvertObjectToInt(row["ManagerNo"]);
                            newItem.ManagerName = BLHelper.ConvertObjectToString(row["ManagerName"]);
                            newItem.CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]);
                            newItem.CostCenterName = BLHelper.ConvertObjectToString(row["CostCenterName"]);

                            if (newItem.EmpNo > 0)
                                newItem.EmpFullName = string.Format("{0} - {1}", newItem.EmpNo, newItem.EmpName);
                            else
                                newItem.EmpFullName = newItem.EmpName;

                            if (newItem.SupervisorNo > 0)
                                newItem.SupervisorFullName = string.Format("{0} - {1}", newItem.SupervisorNo, newItem.SupervisorName);

                            if (newItem.ManagerNo > 0)
                                newItem.ManagerFullName = string.Format("{0} - {1}", newItem.ManagerNo, newItem.ManagerName);

                            if (newItem.CostCenter != string.Empty)
                                newItem.CostCenterFullName = string.Format("{0} - {1}", newItem.CostCenter, newItem.CostCenterName);
                            else
                                newItem.CostCenterFullName = newItem.CostCenterName;

                            newItem.TraineeID = newItem.EmpNo;
                            newItem.TraineeName = newItem.EmpName;
                            newItem.TraineeFullName = newItem.EmpFullName;
                            newItem.TraineePosition = newItem.EmpPosition;
                        }
                        else
                        {
                            newItem.ContractorID = BLHelper.ConvertObjectToInt(row["EmpNo"]);
                            newItem.ContractorOtherID = BLHelper.ConvertObjectToString(row["ContractorOtherID"]);
                            newItem.ContractorName = BLHelper.ConvertObjectToString(row["ContractorName"]);
                            newItem.ContractorOccupation = BLHelper.ConvertObjectToString(row["ContractorOccupation"]);

                            newItem.TraineeID = newItem.ContractorID;
                            newItem.TraineeName = newItem.ContractorName;
                            newItem.TraineeFullName = newItem.ContractorName;
                            newItem.TraineePosition = newItem.ContractorOccupation;
                        }

                        if (newItem.CreatedByEmpNo > 0)
                            newItem.CreatedByFullName = string.Format("{0} - {1}", newItem.CreatedByEmpNo, newItem.CreatedByEmpName);

                        if (newItem.LastUpdateEmpNo > 0)
                            newItem.LastUpdateFullName = string.Format("{0} - {1}", newItem.LastUpdateEmpNo, newItem.LastUpdateEmpName);

                        if (newItem.SupervisorNo > 0)
                            newItem.SupervisorFullName = string.Format("{0} - {1}", newItem.SupervisorNo, newItem.SupervisorName);

                        newItem.DurationDetails = string.Format("{0} {1}", newItem.Duration, newItem.DurationDesc);
                        newItem.DurationAttendedDetails = string.Format("{0} {1}", newItem.DurationAttended, newItem.DurationAttendedDesc);

                        // Add to collection
                        result.Add(newItem);
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public bool CheckIfTimesheetProcessingInProgress(ref string error, ref string innerError)
        {
            bool result = false;

            try
            {
                var rawData = (from a in TASContext.Master_ProcessLocks
                              select a).FirstOrDefault();
                if (rawData != null)
                {
                    result = BLHelper.ConvertNumberToBolean(rawData.TimesheetProcessing);
                }

                return result;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return false;
            }
        }

        public List<EmployeeAttendanceEntity> GetDailyAttendanceForSalaryStaff(DateTime? startDate, DateTime? endDate, string costCenterList, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> attendanceList = null;

            try
            {
                var rawData = TASContext.GetDailyAttendanceSalaryStaff(startDate, endDate, costCenterList);
                if (rawData != null)
                {
                    // Initialize the collection
                    attendanceList = new List<EmployeeAttendanceEntity>();

                    foreach (var item in rawData)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            EmpNo = item.EmpNo,
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                            Position = BLHelper.ConvertObjectToString(item.Position),
                            CostCenter = BLHelper.ConvertObjectToString(item.BusinessUnit),
                            CostCenterName = BLHelper.ConvertObjectToString(item.BusinessUnitName),
                            DT = item.DT,
                            dtIN = item.dtIN,
                            dtOUT = item.dtOUT,
                            NetMinutes = item.NetMinutes,
                            ShiftPatCode = BLHelper.ConvertObjectToString(item.ShiftPatCode),
                            ShiftCode = BLHelper.ConvertObjectToString(item.ShiftCode),
                            ActualShiftCode = BLHelper.ConvertObjectToString(item.Actual_ShiftCode),
                            OTStartTime = item.OTStartTime,
                            OTEndTime = item.OTEndTime,
                            OTType = BLHelper.ConvertObjectToString(item.OTType),
                            NoPayHours = item.NoPayHours,
                            ShiftAllowance = item.ShiftAllowance,
                            ShiftAllowanceDesc = BLHelper.ConvertObjectToString(item.ShiftAllowanceDesc),
                            DurationShiftAllowanceEvening = item.Duration_ShiftAllowance_Evening,
                            DurationShiftAllowanceNight = item.Duration_ShiftAllowance_Night,
                            AttendanceRemarks = BLHelper.ConvertObjectToString(item.Remark),
                            IsSalaryStaff = item.IsSalStaff,
                            ShiftTiming = item.ShiftTiming
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                           !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                            newItem.CostCenterFullName = newItem.CostCenter;

                        // Add item to the collection
                        attendanceList.Add(newItem);
                    }
                }

                return attendanceList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<CostCenterAccessEntity> GetCostCenterPermission(byte loadType, int? empNo, string costCenter, ref string error, ref string innerError)
        {
            List<CostCenterAccessEntity> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[3];
                parameters[0] = new ADONetParameter("@loadType", SqlDbType.TinyInt, loadType);
                parameters[1] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[2] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 23, costCenter);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetCostCenterPermission", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<CostCenterAccessEntity>();

                    if (loadType == 0)
                    {
                        #region Get cost center permission summary                                                
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            CostCenterAccessEntity newItem = new CostCenterAccessEntity()
                            {
                                EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                                EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                                CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]),
                                CostCenterName = BLHelper.ConvertObjectToString(row["CostCenterName"])
                            };

                            // Add to collection
                            result.Add(newItem);
                        };
                        #endregion
                    }
                    else
                    {
                        #region Get list of allowed cost centers
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            CostCenterAccessEntity newItem = new CostCenterAccessEntity()
                            {
                                PermitID = BLHelper.ConvertObjectToInt(row["PermitID"]),
                                EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                                EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                                CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]),
                                CostCenterName = BLHelper.ConvertObjectToString(row["CostCenterName"]),
                                CreatedByEmpNo = BLHelper.ConvertObjectToInt(row["CreatedByEmpNo"]),
                                CreatedByEmpName = BLHelper.ConvertObjectToString(row["CreatedByEmpName"]),
                                CreatedDate = BLHelper.ConvertObjectToDate(row["CreatedDate"]),
                                ModifiedByEmpNo = BLHelper.ConvertObjectToInt(row["ModifiedByEmpNo"]),
                                ModifiedByEmpName = BLHelper.ConvertObjectToString(row["ModifiedByEmpName"]),
                                ModifiedDate = BLHelper.ConvertObjectToDate(row["ModifiedDate"])
                            };

                            if (newItem.CreatedByEmpNo > 0)
                                newItem.CreatedByFullName = string.Format("{0} - {1}", newItem.CreatedByEmpNo, newItem.CreatedByEmpName);
                            else
                                newItem.CreatedByFullName = newItem.CreatedByEmpName;

                            if (newItem.ModifiedByEmpNo > 0)
                                newItem.ModifiedByFullName = string.Format("{0} - {1}", newItem.ModifiedByEmpNo, newItem.ModifiedByEmpName);
                            else
                                newItem.ModifiedByFullName = newItem.ModifiedByEmpName;

                            // Add to collection
                            result.Add(newItem);
                        };
                        #endregion
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public DatabaseSaveResult InsertUpdateDeleteCostCenterPermission(byte actionType, int permitID, int permitEmpNo, string permitCostCenter, int userEmpNo, ref string error, ref string innerError)
        {
            DatabaseSaveResult result = null;

            try
            {
                #region Initialize parameters
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[5];

                parameters[0] = new ADONetParameter("@actionType", SqlDbType.TinyInt, actionType);
                parameters[1] = new ADONetParameter("@permitID", SqlDbType.Int, permitID);
                parameters[2] = new ADONetParameter("@permitEmpNo", SqlDbType.Int, permitEmpNo);
                parameters[3] = new ADONetParameter("@permitCostCenter", SqlDbType.VarChar, 12, permitCostCenter);
                parameters[4] = new ADONetParameter("@userEmpNo", SqlDbType.Int, userEmpNo);
                #endregion

                DataSet ds = RunSPReturnDataset("tas.Pr_PermitCostCenter_CRUD", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    result = new DatabaseSaveResult()
                    {
                        HasError = BLHelper.ConvertObjectToBolean(ds.Tables[0].Rows[0]["HasError"]),
                        ErrorCode = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorCode"]),
                        ErrorDesc = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorDescription"]),
                        RowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["RowsAffected"]),
                        NewIdentityID = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["NewIdentityID"])
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();

                result = new DatabaseSaveResult()
                {
                    RowsAffected = 0,
                    HasError = true,
                    ErrorCode = string.Empty,
                    ErrorDesc = ex.Message.ToString()
                };
                return null;
            }
        }

        public void InsertAllowedCostCenter(int empNo, List<CostCenterAccessEntity> costCenterList, ref string error, ref string innerError)
        {
            try
            {
                #region Get the TAS Application ID
                int applicationID = 0;
                var applicationCode = GenPurposeContext.UserDefinedCodes
                                    .Where(a => a.UDCUDCGID == 17 && a.UDCCode.Trim() == "TAS3")
                                    .FirstOrDefault(); 
                if (applicationCode != null)
                {
                    applicationID = applicationCode.UDCID;
                }
                #endregion

                if (applicationID == 0)
                {
                    throw new Exception("Failed saving changes because the Application ID could not be determined!");
                }

                #region Remove existing records                
                var recordToDeleteList = GenPurposeContext.PermitCostCenters
                    .Where(a => a.PermitEmpNo == empNo && a.PermitAppID == applicationID)
                    .ToList();
                if (recordToDeleteList != null)
                {
                    GenPurposeContext.PermitCostCenters.RemoveRange(recordToDeleteList);
                    GenPurposeContext.SaveChanges();
                }
                #endregion

                List<PermitCostCenter> recordToInsertList = new List<PermitCostCenter>();
                foreach (CostCenterAccessEntity item in costCenterList)
                {
                    recordToInsertList.Add(new PermitCostCenter()
                    {
                        PermitEmpNo = item.EmpNo,
                        PermitCostCenter1 = item.CostCenter,
                        PermitAppID = applicationID,
                        PermitCreatedBy = item.CreatedByEmpNo,
                        PermitCreatedDate = item.CreatedDate
                    });
                }

                // Save to database
                if (recordToInsertList.Count > 0)
                {
                    GenPurposeContext.PermitCostCenters.AddRange(recordToInsertList);
                    GenPurposeContext.SaveChanges();
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public List<EmployeeDetail> GetWorkflowActionMember(int empNo, string distListCode, string costCenter, ref string error, ref string innerError)
        {
            List<EmployeeDetail> result = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[3];
                parameters[0] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[1] = new ADONetParameter("@distListCode", SqlDbType.VarChar, 10, distListCode);
                parameters[2] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetWFActionMember", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize collection
                    result = new List<EmployeeDetail>();

                    foreach (DataRow item in ds.Tables[0].Rows)
                    {
                        result.Add(new EmployeeDetail()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(item["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(item["EmpName"]),
                            EmpEmail = BLHelper.ConvertObjectToString(item["EmpEmail"]),
                            CostCenter = BLHelper.ConvertObjectToString(item["BusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(item["BusinessUnitName"])
                        });
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<PunctualityEntity> GetPunctualitySummaryReport(byte loadType, DateTime? startDate, DateTime? endDate, string costCenter, 
            int occurenceLimit, int lateAttendanceThreshold, int earlyLeavingThreshold, bool hideDayOffHoliday, ref string error, ref string innerError)
        {
            List<PunctualityEntity> puctualityList = null;

            try
            {
                #region Fetch data using ADO.NET
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[8];
                parameters[0] = new ADONetParameter("@loadType", SqlDbType.TinyInt, loadType);
                parameters[1] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[2] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[3] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[4] = new ADONetParameter("@occurenceLimit", SqlDbType.Int, occurenceLimit);
                parameters[5] = new ADONetParameter("@lateAttendanceThreshold", SqlDbType.Int, lateAttendanceThreshold);
                parameters[6] = new ADONetParameter("@earlyLeavingThreshold", SqlDbType.Int, earlyLeavingThreshold);
                parameters[7] = new ADONetParameter("@hideDayOffHoliday", SqlDbType.Bit, hideDayOffHoliday);

                DataSet ds = RunSPReturnDataset("tas.Pr_PunctualitySummaryReport", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    #region Build the raw data source                                        
                    List<PunctualityEntity> rawDataSource = new List<PunctualityEntity>();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        rawDataSource.Add(new PunctualityEntity()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["CostCenterName"]),
                            CostCenterFullName = string.Format("{0} - {1}",
                                                    BLHelper.ConvertObjectToString(row["CostCenter"]),
                                                    BLHelper.ConvertObjectToString(row["CostCenterName"])),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                            ActualShiftCode = BLHelper.ConvertObjectToString(row["Actual_ShiftCode"]),
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIN"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOUT"]),
                            MaxArrivalTime = BLHelper.ConvertObjectToDate(row["MaxArrivalTime"]),
                            RequiredTimeOut = BLHelper.ConvertObjectToDate(row["RequiredTimeOut"]),
                            ArrivalTimeDiff = BLHelper.ConvertObjectToInt(row["ArrivalTimeDiff"]),
                            DepartureTimeDiff = BLHelper.ConvertObjectToInt(row["DepartureTimeDiff"]),
                            Remarks = BLHelper.ConvertObjectToString(row["Remarks"]),
                            LeaveType = BLHelper.ConvertObjectToString(row["LeaveType"]),
                            UDCCode = BLHelper.ConvertObjectToString(row["UDCCode"]),
                            UDCDescription = BLHelper.ConvertObjectToString(row["UDCDescription"])
                        });
                    }
                    #endregion

                    if (rawDataSource.Count > 0)
                    {
                        // Initialize the collection
                        puctualityList = new List<PunctualityEntity>();

                        #region Get the distinct employee names
                        var employeeList = (from a in rawDataSource
                                            select new
                                            {
                                                EmpNo = a.EmpNo,
                                                EmpName = a.EmpName,
                                                CostCenter = a.CostCenter,
                                                CostCenterName = a.CostCenterName,
                                                CostCenterFullName = a.CostCenterFullName,
                                            })
                                           .Distinct()
                                           .OrderBy(a => a.EmpNo);
                        #endregion

                        if (employeeList != null)
                        {
                            foreach (var item in employeeList)
                            {
                                PunctualityEntity punctualityItem = new PunctualityEntity()
                                {
                                    EmpNo = item.EmpNo,
                                    EmpName = item.EmpName,
                                    CostCenter = item.CostCenter,
                                    CostCenterName = item.CostCenterName,
                                    CostCenterFullName = item.CostCenterFullName
                                };

                                #region Populate the weekdays                                                        
                                int counter = 1;
                                DateTime attendanceDate = Convert.ToDateTime(startDate);
                                int totalWorkMissed = 0;

                                while (attendanceDate <= endDate)
                                {
                                    if (counter == 1)
                                    {
                                        #region Process First day of week - Sunday                                                                         
                                        punctualityItem.Day1 = attendanceDate;

                                        // Get the time-in and time-out
                                        var day1Record = rawDataSource
                                            .Where(a => a.EmpNo == item.EmpNo && a.DT == attendanceDate)
                                            .FirstOrDefault();
                                        if (day1Record != null)
                                        {
                                            punctualityItem.Day1TimeIn = day1Record.dtIN;
                                            punctualityItem.Day1TimeOut = day1Record.dtOUT;

                                            if (day1Record.Remarks == BLHelper.ArrivalStatus.Dayoff.ToString())
                                                punctualityItem.Day1Remarks = "Off";
                                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.Absent.ToString())
                                                punctualityItem.Day1Remarks = "Abs";
                                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.OnLeave.ToString())
                                                punctualityItem.Day1Remarks = day1Record.LeaveType;
                                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.MissingSwipe.ToString())
                                                punctualityItem.Day1Remarks = "MS";
                                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.Holiday.ToString())
                                                punctualityItem.Day1Remarks = "Hol";
                                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.DIL.ToString())
                                                punctualityItem.Day1Remarks = "DIL";
                                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.Late.ToString())
                                            {
                                                punctualityItem.Day1TimeInLate = true;

                                                // Get the total duration of missed work
                                                totalWorkMissed = totalWorkMissed + day1Record.ArrivalTimeDiff;
                                            }
                                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.LeftEarly.ToString())
                                            {
                                                punctualityItem.Day1TimeOutEarly = true;

                                                // Get the total duration of missed work
                                                totalWorkMissed = totalWorkMissed + day1Record.DepartureTimeDiff;
                                            }
                                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.LateAndLeftEarly.ToString())
                                            {
                                                punctualityItem.Day1TimeInLate = true;
                                                punctualityItem.Day1TimeOutEarly = true;

                                                // Get the total duration of missed work
                                                //totalWorkMissed = totalWorkMissed + day1Record.ArrivalTimeDiff + day1Record.DepartureTimeDiff;
                                                totalWorkMissed = totalWorkMissed + day1Record.DepartureTimeDiff;
                                            }
                                            else
                                            {
                                                punctualityItem.Day1Remarks = day1Record.UDCCode;
                                            }
                                        }
                                        #endregion
                                    }
                                    else if (counter == 2)
                                    {
                                        #region Process Second day of week - Monday                                                                         
                                        punctualityItem.Day2 = attendanceDate;

                                        // Get the time-in and time-out
                                        var day2Record = rawDataSource
                                            .Where(a => a.EmpNo == item.EmpNo && a.DT == attendanceDate)
                                            .FirstOrDefault();
                                        if (day2Record != null)
                                        {
                                            punctualityItem.Day2TimeIn = day2Record.dtIN;
                                            punctualityItem.Day2TimeOut = day2Record.dtOUT;

                                            if (day2Record.Remarks == BLHelper.ArrivalStatus.Dayoff.ToString())
                                                punctualityItem.Day2Remarks = "Off";
                                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.Absent.ToString())
                                                punctualityItem.Day2Remarks = "Abs";
                                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.OnLeave.ToString())
                                                punctualityItem.Day2Remarks = day2Record.LeaveType;
                                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.MissingSwipe.ToString())
                                                punctualityItem.Day2Remarks = "MS";
                                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.Holiday.ToString())
                                                punctualityItem.Day2Remarks = "Hol";
                                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.DIL.ToString())
                                                punctualityItem.Day2Remarks = "DIL";
                                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.Late.ToString())
                                            {
                                                punctualityItem.Day2TimeInLate = true;

                                                // Get the total duration of missed work
                                                totalWorkMissed = totalWorkMissed + day2Record.ArrivalTimeDiff;
                                            }
                                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.LeftEarly.ToString())
                                            {
                                                punctualityItem.Day2TimeOutEarly = true;

                                                // Get the total duration of missed work
                                                totalWorkMissed = totalWorkMissed + day2Record.DepartureTimeDiff;
                                            }
                                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.LateAndLeftEarly.ToString())
                                            {
                                                punctualityItem.Day2TimeInLate = true;
                                                punctualityItem.Day2TimeOutEarly = true;

                                                // Get the total duration of missed work
                                                //totalWorkMissed = totalWorkMissed + day2Record.ArrivalTimeDiff + day2Record.DepartureTimeDiff;
                                                totalWorkMissed = totalWorkMissed + day2Record.DepartureTimeDiff;
                                            }
                                            else
                                            {
                                                punctualityItem.Day2Remarks = day2Record.UDCCode;
                                            }
                                        }
                                        #endregion
                                    }
                                    else if (counter == 3)
                                    {
                                        #region Process Third day of week - Tuesday                                                                         
                                        punctualityItem.Day3 = attendanceDate;

                                        // Get the time-in and time-out
                                        var day3Record = rawDataSource
                                            .Where(a => a.EmpNo == item.EmpNo && a.DT == attendanceDate)
                                            .FirstOrDefault();
                                        if (day3Record != null)
                                        {
                                            punctualityItem.Day3TimeIn = day3Record.dtIN;
                                            punctualityItem.Day3TimeOut = day3Record.dtOUT;

                                            if (day3Record.Remarks == BLHelper.ArrivalStatus.Dayoff.ToString())
                                                punctualityItem.Day3Remarks = "Off";
                                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.Absent.ToString())
                                                punctualityItem.Day3Remarks = "Abs";
                                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.OnLeave.ToString())
                                                punctualityItem.Day3Remarks = day3Record.LeaveType;
                                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.MissingSwipe.ToString())
                                                punctualityItem.Day3Remarks = "MS";
                                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.Holiday.ToString())
                                                punctualityItem.Day3Remarks = "Hol";
                                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.DIL.ToString())
                                                punctualityItem.Day3Remarks = "DIL";
                                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.Late.ToString())
                                            {
                                                punctualityItem.Day3TimeInLate = true;

                                                // Get the total duration of missed work
                                                totalWorkMissed = totalWorkMissed + day3Record.ArrivalTimeDiff;
                                            }
                                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.LeftEarly.ToString())
                                            {
                                                punctualityItem.Day3TimeOutEarly = true;

                                                // Get the total duration of missed work
                                                totalWorkMissed = totalWorkMissed + day3Record.DepartureTimeDiff;
                                            }
                                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.LateAndLeftEarly.ToString())
                                            {
                                                punctualityItem.Day3TimeInLate = true;
                                                punctualityItem.Day3TimeOutEarly = true;

                                                // Get the total duration of missed work
                                                //totalWorkMissed = totalWorkMissed + day3Record.ArrivalTimeDiff + day3Record.DepartureTimeDiff;
                                                totalWorkMissed = totalWorkMissed + day3Record.DepartureTimeDiff;
                                            }
                                            else
                                            {
                                                punctualityItem.Day3Remarks = day3Record.UDCCode;
                                            }
                                        }
                                        #endregion
                                    }
                                    else if (counter == 4)
                                    {
                                        #region Process Fourth day of week - Wednesday                                                                         
                                        punctualityItem.Day4 = attendanceDate;

                                        // Get the time-in and time-out
                                        var day4Record = rawDataSource
                                            .Where(a => a.EmpNo == item.EmpNo && a.DT == attendanceDate)
                                            .FirstOrDefault();
                                        if (day4Record != null)
                                        {
                                            punctualityItem.Day4TimeIn = day4Record.dtIN;
                                            punctualityItem.Day4TimeOut = day4Record.dtOUT;

                                            if (day4Record.Remarks == BLHelper.ArrivalStatus.Dayoff.ToString())
                                                punctualityItem.Day4Remarks = "Off";
                                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.Absent.ToString())
                                                punctualityItem.Day4Remarks = "Abs";
                                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.OnLeave.ToString())
                                                punctualityItem.Day4Remarks = day4Record.LeaveType;
                                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.MissingSwipe.ToString())
                                                punctualityItem.Day4Remarks = "MS";
                                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.Holiday.ToString())
                                                punctualityItem.Day4Remarks = "Hol";
                                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.DIL.ToString())
                                                punctualityItem.Day4Remarks = "DIL";
                                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.Late.ToString())
                                            {
                                                punctualityItem.Day4TimeInLate = true;

                                                // Get the total duration of missed work
                                                totalWorkMissed = totalWorkMissed + day4Record.ArrivalTimeDiff;
                                            }
                                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.LeftEarly.ToString())
                                            {
                                                punctualityItem.Day4TimeOutEarly = true;

                                                // Get the total duration of missed work
                                                totalWorkMissed = totalWorkMissed + day4Record.DepartureTimeDiff;
                                            }
                                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.LateAndLeftEarly.ToString())
                                            {
                                                punctualityItem.Day4TimeInLate = true;
                                                punctualityItem.Day4TimeOutEarly = true;

                                                // Get the total duration of missed work
                                                //totalWorkMissed = totalWorkMissed + day4Record.ArrivalTimeDiff + day4Record.DepartureTimeDiff;
                                                totalWorkMissed = totalWorkMissed + day4Record.DepartureTimeDiff;
                                            }
                                            else
                                            {
                                                punctualityItem.Day4Remarks = day4Record.UDCCode;
                                            }
                                        }
                                        #endregion
                                    }
                                    else if (counter == 5)
                                    {
                                        #region Process Fifth day of week - Thursday                                                                         
                                        punctualityItem.Day5 = attendanceDate;

                                        // Get the time-in and time-out
                                        var day5Record = rawDataSource
                                            .Where(a => a.EmpNo == item.EmpNo && a.DT == attendanceDate)
                                            .FirstOrDefault();
                                        if (day5Record != null)
                                        {
                                            punctualityItem.Day5TimeIn = day5Record.dtIN;
                                            punctualityItem.Day5TimeOut = day5Record.dtOUT;

                                            if (day5Record.Remarks == BLHelper.ArrivalStatus.Dayoff.ToString())
                                                punctualityItem.Day5Remarks = "Off";
                                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.Absent.ToString())
                                                punctualityItem.Day5Remarks = "Abs";
                                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.OnLeave.ToString())
                                                punctualityItem.Day5Remarks = day5Record.LeaveType;
                                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.MissingSwipe.ToString())
                                                punctualityItem.Day5Remarks = "MS";
                                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.Holiday.ToString())
                                                punctualityItem.Day5Remarks = "Hol";
                                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.DIL.ToString())
                                                punctualityItem.Day5Remarks = "DIL";
                                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.Late.ToString())
                                            {
                                                punctualityItem.Day5TimeInLate = true;

                                                // Get the total duration of missed work
                                                totalWorkMissed = totalWorkMissed + day5Record.ArrivalTimeDiff;
                                            }
                                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.LeftEarly.ToString())
                                            {
                                                punctualityItem.Day5TimeOutEarly = true;

                                                // Get the total duration of missed work
                                                totalWorkMissed = totalWorkMissed + day5Record.DepartureTimeDiff;
                                            }
                                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.LateAndLeftEarly.ToString())
                                            {
                                                punctualityItem.Day5TimeInLate = true;
                                                punctualityItem.Day5TimeOutEarly = true;

                                                // Get the total duration of missed work
                                                //totalWorkMissed = totalWorkMissed + day5Record.ArrivalTimeDiff + day5Record.DepartureTimeDiff;
                                                totalWorkMissed = totalWorkMissed + day5Record.DepartureTimeDiff;
                                            }
                                            else
                                            {
                                                punctualityItem.Day5Remarks = day5Record.UDCCode;
                                            }
                                        }
                                        #endregion
                                    }
                                    else if (counter == 6)
                                    {
                                        #region Process Sixth day of week - Friday                                                                         
                                        punctualityItem.Day6 = attendanceDate;

                                        // Get the time-in and time-out
                                        var day6Record = rawDataSource
                                            .Where(a => a.EmpNo == item.EmpNo && a.DT == attendanceDate)
                                            .FirstOrDefault();
                                        if (day6Record != null)
                                        {
                                            punctualityItem.Day6TimeIn = day6Record.dtIN;
                                            punctualityItem.Day6TimeOut = day6Record.dtOUT;

                                            if (day6Record.Remarks == BLHelper.ArrivalStatus.Dayoff.ToString())
                                                punctualityItem.Day6Remarks = "Off";
                                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.Absent.ToString())
                                                punctualityItem.Day6Remarks = "Abs";
                                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.OnLeave.ToString())
                                                punctualityItem.Day6Remarks = day6Record.LeaveType;
                                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.MissingSwipe.ToString())
                                                punctualityItem.Day6Remarks = "MS";
                                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.Holiday.ToString())
                                                punctualityItem.Day6Remarks = "Hol";
                                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.DIL.ToString())
                                                punctualityItem.Day6Remarks = "DIL";
                                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.Late.ToString())
                                            {
                                                punctualityItem.Day6TimeInLate = true;

                                                // Get the total duration of missed work
                                                totalWorkMissed = totalWorkMissed + day6Record.ArrivalTimeDiff;
                                            }
                                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.LeftEarly.ToString())
                                            {
                                                punctualityItem.Day6TimeOutEarly = true;

                                                // Get the total duration of missed work
                                                totalWorkMissed = totalWorkMissed + day6Record.DepartureTimeDiff;
                                            }
                                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.LateAndLeftEarly.ToString())
                                            {
                                                punctualityItem.Day6TimeInLate = true;
                                                punctualityItem.Day6TimeOutEarly = true;

                                                // Get the total duration of missed work
                                                //totalWorkMissed = totalWorkMissed + day6Record.ArrivalTimeDiff + day6Record.DepartureTimeDiff;
                                                totalWorkMissed = totalWorkMissed + day6Record.DepartureTimeDiff;
                                            }
                                            else
                                            {
                                                punctualityItem.Day6Remarks = day6Record.UDCCode;
                                            }
                                        }
                                        #endregion
                                    }
                                    else if (counter == 7)
                                    {
                                        #region Process Seventh day of week - Saturday                                                                         
                                        punctualityItem.Day7 = attendanceDate;

                                        // Get the time-in and time-out
                                        var day7Record = rawDataSource
                                            .Where(a => a.EmpNo == item.EmpNo && a.DT == attendanceDate)
                                            .FirstOrDefault();
                                        if (day7Record != null)
                                        {
                                            punctualityItem.Day7TimeIn = day7Record.dtIN;
                                            punctualityItem.Day7TimeOut = day7Record.dtOUT;

                                            if (day7Record.Remarks == BLHelper.ArrivalStatus.Dayoff.ToString())
                                                punctualityItem.Day7Remarks = "Off";
                                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.Absent.ToString())
                                                punctualityItem.Day7Remarks = "Abs";
                                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.OnLeave.ToString())
                                                punctualityItem.Day7Remarks = day7Record.LeaveType;
                                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.MissingSwipe.ToString())
                                                punctualityItem.Day7Remarks = "MS";
                                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.Holiday.ToString())
                                                punctualityItem.Day7Remarks = "Hol";
                                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.DIL.ToString())
                                                punctualityItem.Day7Remarks = "DIL";
                                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.Late.ToString())
                                            {
                                                punctualityItem.Day7TimeInLate = true;

                                                // Get the total duration of missed work
                                                totalWorkMissed = totalWorkMissed + day7Record.ArrivalTimeDiff;
                                            }
                                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.LeftEarly.ToString())
                                            {
                                                punctualityItem.Day7TimeOutEarly = true;

                                                // Get the total duration of missed work
                                                totalWorkMissed = totalWorkMissed + day7Record.DepartureTimeDiff;
                                            }
                                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.LateAndLeftEarly.ToString())
                                            {
                                                punctualityItem.Day7TimeInLate = true;
                                                punctualityItem.Day7TimeOutEarly = true;

                                                // Get the total duration of missed work
                                                //totalWorkMissed = totalWorkMissed + day7Record.ArrivalTimeDiff + day7Record.DepartureTimeDiff;
                                                totalWorkMissed = totalWorkMissed + day7Record.DepartureTimeDiff;
                                            }
                                            else
                                            {
                                                punctualityItem.Day7Remarks = day7Record.UDCCode;
                                            }
                                        }
                                        #endregion
                                    }

                                    attendanceDate = attendanceDate.AddDays(1);
                                    counter++;
                                }

                                // Calculate the total duration of missed workf for the entire week
                                punctualityItem.TotalLateForWeek = totalWorkMissed;
                                #endregion

                                // Get the employee shift pattern
                                var employeeShiftPattern = (from a in rawDataSource
                                                            where a.EmpNo == item.EmpNo
                                                            select new
                                                            {
                                                                EmpNo = a.EmpNo,
                                                                EmpName = a.EmpName,
                                                                ShiftPatCode = a.ShiftPatCode,
                                                                DT = a.DT
                                                            })
                                                            .OrderByDescending(a => a.DT)
                                                            .FirstOrDefault();
                                if (employeeShiftPattern != null)
                                {
                                    punctualityItem.ShiftPatCode = employeeShiftPattern.ShiftPatCode;
                                }

                                // Add item to the collection
                                puctualityList.Add(punctualityItem);
                            }
                        }
                    }
                }
                #endregion

                #region Fetch data using Entity Framework                                
                //var rawData = TASContext.GetPunctualitySummaryReport(startDate, endDate, costCenter, occurenceLimit, lateAttendanceThreshold, earlyLeavingThreshold, hideDayOffHoliday);
                //if (rawData != null)
                //{
                //    #region Build the raw data source                                        
                //    List<PunctualityEntity> rawDataSource = new List<PunctualityEntity>();
                //    foreach (var item in rawData)
                //    {
                //        rawDataSource.Add(new PunctualityEntity()
                //        {
                //            EmpNo = item.EmpNo,
                //            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                //            CostCenter = BLHelper.ConvertObjectToString(item.CostCenter),
                //            CostCenterName = BLHelper.ConvertObjectToString(item.CostCenterName),
                //            CostCenterFullName = string.Format("{0} - {1}",
                //                                BLHelper.ConvertObjectToString(item.CostCenter),
                //                                BLHelper.ConvertObjectToString(item.CostCenterName)),
                //            ShiftPatCode = BLHelper.ConvertObjectToString(item.ShiftPatCode),
                //            ShiftCode = BLHelper.ConvertObjectToString(item.ShiftCode),
                //            ActualShiftCode = BLHelper.ConvertObjectToString(item.Actual_ShiftCode),
                //            DT = BLHelper.ConvertObjectToDate(item.DT),
                //            dtIN = BLHelper.ConvertObjectToDate(item.dtIN),
                //            dtOUT = BLHelper.ConvertObjectToDate(item.dtOUT),
                //            MaxArrivalTime = BLHelper.ConvertObjectToDate(item.MaxArrivalTime),
                //            RequiredTimeOut = BLHelper.ConvertObjectToDate(item.RequiredTimeOut),
                //            ArrivalTimeDiff = item.ArrivalTimeDiff,
                //            DepartureTimeDiff = BLHelper.ConvertObjectToInt(item.DepartureTimeDiff),
                //            Remarks = BLHelper.ConvertObjectToString(item.Remarks),
                //            LeaveType = BLHelper.ConvertObjectToString(item.LeaveType),
                //        });
                //    }
                //    #endregion

                //    if (rawDataSource.Count > 0)
                //    {
                //        // Initialize the collection
                //        puctualityList = new List<PunctualityEntity>();

                //        #region Get the distinct employee names
                //        var employeeList = (from a in rawDataSource
                //                            select new
                //                            {
                //                                EmpNo = a.EmpNo,
                //                                EmpName = a.EmpName,
                //                                CostCenter = a.CostCenter,
                //                                CostCenterName = a.CostCenterName,
                //                                CostCenterFullName = a.CostCenterFullName,
                //                            })
                //                           .Distinct()
                //                           .OrderBy(a => a.EmpNo);
                //        #endregion

                //        if (employeeList != null)
                //        {
                //            foreach (var item in employeeList)
                //            {
                //                PunctualityEntity punctualityItem = new PunctualityEntity()
                //                {
                //                    EmpNo = item.EmpNo,
                //                    EmpName = item.EmpName,
                //                    CostCenter = item.CostCenter,
                //                    CostCenterName = item.CostCenterName,
                //                    CostCenterFullName = item.CostCenterFullName
                //                };

                //                #region Populate the weekdays                                                        
                //                int counter = 1;
                //                DateTime attendanceDate = Convert.ToDateTime(startDate);
                //                int totalWorkMissed = 0;

                //                while (attendanceDate <= endDate)
                //                {
                //                    if (counter == 1)
                //                    {
                //                        #region Process First day of week - Sunday                                                                         
                //                        punctualityItem.Day1 = attendanceDate;

                //                        // Get the time-in and time-out
                //                        var day1Record = rawDataSource
                //                            .Where(a => a.EmpNo == item.EmpNo && a.DT == attendanceDate)
                //                            .FirstOrDefault();
                //                        if (day1Record != null)
                //                        {
                //                            punctualityItem.Day1TimeIn = day1Record.dtIN;
                //                            punctualityItem.Day1TimeOut = day1Record.dtOUT;

                //                            if (day1Record.Remarks == BLHelper.ArrivalStatus.Dayoff.ToString())
                //                                punctualityItem.Day1Remarks = "Off";
                //                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.Absent.ToString())
                //                                punctualityItem.Day1Remarks = "Abs";
                //                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.OnLeave.ToString())
                //                                punctualityItem.Day1Remarks = day1Record.LeaveType;
                //                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.MissingSwipe.ToString())
                //                                punctualityItem.Day1Remarks = "MS";
                //                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.Holiday.ToString())
                //                                punctualityItem.Day1Remarks = "Hol";
                //                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.Late.ToString())
                //                            {
                //                                punctualityItem.Day1TimeInLate = true;

                //                                // Get the total duration of missed work
                //                                totalWorkMissed = totalWorkMissed + day1Record.ArrivalTimeDiff;
                //                            }
                //                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.LeftEarly.ToString())
                //                            {
                //                                punctualityItem.Day1TimeOutEarly = true;

                //                                // Get the total duration of missed work
                //                                totalWorkMissed = totalWorkMissed + day1Record.DepartureTimeDiff;
                //                            }
                //                            else if (day1Record.Remarks == BLHelper.ArrivalStatus.LateAndLeftEarly.ToString())
                //                            {
                //                                punctualityItem.Day1TimeInLate = true;
                //                                punctualityItem.Day1TimeOutEarly = true;

                //                                // Get the total duration of missed work
                //                                //totalWorkMissed = totalWorkMissed + day1Record.ArrivalTimeDiff + day1Record.DepartureTimeDiff;
                //                                totalWorkMissed = totalWorkMissed + day1Record.DepartureTimeDiff;
                //                            }
                //                        }
                //                        #endregion
                //                    }
                //                    else if (counter == 2)
                //                    {
                //                        #region Process Second day of week - Monday                                                                         
                //                        punctualityItem.Day2 = attendanceDate;

                //                        // Get the time-in and time-out
                //                        var day2Record = rawDataSource
                //                            .Where(a => a.EmpNo == item.EmpNo && a.DT == attendanceDate)
                //                            .FirstOrDefault();
                //                        if (day2Record != null)
                //                        {
                //                            punctualityItem.Day2TimeIn = day2Record.dtIN;
                //                            punctualityItem.Day2TimeOut = day2Record.dtOUT;

                //                            if (day2Record.Remarks == BLHelper.ArrivalStatus.Dayoff.ToString())
                //                                punctualityItem.Day2Remarks = "Off";
                //                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.Absent.ToString())
                //                                punctualityItem.Day2Remarks = "Abs";
                //                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.OnLeave.ToString())
                //                                punctualityItem.Day2Remarks = day2Record.LeaveType;
                //                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.MissingSwipe.ToString())
                //                                punctualityItem.Day2Remarks = "MS";
                //                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.Holiday.ToString())
                //                                punctualityItem.Day2Remarks = "Hol";
                //                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.Late.ToString())
                //                            {
                //                                punctualityItem.Day2TimeInLate = true;

                //                                // Get the total duration of missed work
                //                                totalWorkMissed = totalWorkMissed + day2Record.ArrivalTimeDiff;
                //                            }
                //                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.LeftEarly.ToString())
                //                            {
                //                                punctualityItem.Day2TimeOutEarly = true;

                //                                // Get the total duration of missed work
                //                                totalWorkMissed = totalWorkMissed + day2Record.DepartureTimeDiff;
                //                            }
                //                            else if (day2Record.Remarks == BLHelper.ArrivalStatus.LateAndLeftEarly.ToString())
                //                            {
                //                                punctualityItem.Day2TimeInLate = true;
                //                                punctualityItem.Day2TimeOutEarly = true;

                //                                // Get the total duration of missed work
                //                                //totalWorkMissed = totalWorkMissed + day2Record.ArrivalTimeDiff + day2Record.DepartureTimeDiff;
                //                                totalWorkMissed = totalWorkMissed + day2Record.DepartureTimeDiff;
                //                            }
                //                        }
                //                        #endregion
                //                    }
                //                    else if (counter == 3)
                //                    {
                //                        #region Process Third day of week - Tuesday                                                                         
                //                        punctualityItem.Day3 = attendanceDate;

                //                        // Get the time-in and time-out
                //                        var day3Record = rawDataSource
                //                            .Where(a => a.EmpNo == item.EmpNo && a.DT == attendanceDate)
                //                            .FirstOrDefault();
                //                        if (day3Record != null)
                //                        {
                //                            punctualityItem.Day3TimeIn = day3Record.dtIN;
                //                            punctualityItem.Day3TimeOut = day3Record.dtOUT;

                //                            if (day3Record.Remarks == BLHelper.ArrivalStatus.Dayoff.ToString())
                //                                punctualityItem.Day3Remarks = "Off";
                //                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.Absent.ToString())
                //                                punctualityItem.Day3Remarks = "Abs";
                //                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.OnLeave.ToString())
                //                                punctualityItem.Day3Remarks = day3Record.LeaveType;
                //                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.MissingSwipe.ToString())
                //                                punctualityItem.Day3Remarks = "MS";
                //                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.Holiday.ToString())
                //                                punctualityItem.Day3Remarks = "Hol";
                //                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.Late.ToString())
                //                            {
                //                                punctualityItem.Day3TimeInLate = true;

                //                                // Get the total duration of missed work
                //                                totalWorkMissed = totalWorkMissed + day3Record.ArrivalTimeDiff;
                //                            }
                //                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.LeftEarly.ToString())
                //                            {
                //                                punctualityItem.Day3TimeOutEarly = true;

                //                                // Get the total duration of missed work
                //                                totalWorkMissed = totalWorkMissed + day3Record.DepartureTimeDiff;
                //                            }
                //                            else if (day3Record.Remarks == BLHelper.ArrivalStatus.LateAndLeftEarly.ToString())
                //                            {
                //                                punctualityItem.Day3TimeInLate = true;
                //                                punctualityItem.Day3TimeOutEarly = true;

                //                                // Get the total duration of missed work
                //                                //totalWorkMissed = totalWorkMissed + day3Record.ArrivalTimeDiff + day3Record.DepartureTimeDiff;
                //                                totalWorkMissed = totalWorkMissed + day3Record.DepartureTimeDiff;
                //                            }
                //                        }
                //                        #endregion
                //                    }
                //                    else if (counter == 4)
                //                    {
                //                        #region Process Fourth day of week - Wednesday                                                                         
                //                        punctualityItem.Day4 = attendanceDate;

                //                        // Get the time-in and time-out
                //                        var day4Record = rawDataSource
                //                            .Where(a => a.EmpNo == item.EmpNo && a.DT == attendanceDate)
                //                            .FirstOrDefault();
                //                        if (day4Record != null)
                //                        {
                //                            punctualityItem.Day4TimeIn = day4Record.dtIN;
                //                            punctualityItem.Day4TimeOut = day4Record.dtOUT;

                //                            if (day4Record.Remarks == BLHelper.ArrivalStatus.Dayoff.ToString())
                //                                punctualityItem.Day4Remarks = "Off";
                //                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.Absent.ToString())
                //                                punctualityItem.Day4Remarks = "Abs";
                //                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.OnLeave.ToString())
                //                                punctualityItem.Day4Remarks = day4Record.LeaveType;
                //                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.MissingSwipe.ToString())
                //                                punctualityItem.Day4Remarks = "MS";
                //                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.Holiday.ToString())
                //                                punctualityItem.Day4Remarks = "Hol";
                //                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.Late.ToString())
                //                            {
                //                                punctualityItem.Day4TimeInLate = true;

                //                                // Get the total duration of missed work
                //                                totalWorkMissed = totalWorkMissed + day4Record.ArrivalTimeDiff;
                //                            }
                //                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.LeftEarly.ToString())
                //                            {
                //                                punctualityItem.Day4TimeOutEarly = true;

                //                                // Get the total duration of missed work
                //                                totalWorkMissed = totalWorkMissed + day4Record.DepartureTimeDiff;
                //                            }
                //                            else if (day4Record.Remarks == BLHelper.ArrivalStatus.LateAndLeftEarly.ToString())
                //                            {
                //                                punctualityItem.Day4TimeInLate = true;
                //                                punctualityItem.Day4TimeOutEarly = true;

                //                                // Get the total duration of missed work
                //                                //totalWorkMissed = totalWorkMissed + day4Record.ArrivalTimeDiff + day4Record.DepartureTimeDiff;
                //                                totalWorkMissed = totalWorkMissed + day4Record.DepartureTimeDiff;
                //                            }
                //                        }
                //                        #endregion
                //                    }
                //                    else if (counter == 5)
                //                    {
                //                        #region Process Fifth day of week - Thursday                                                                         
                //                        punctualityItem.Day5 = attendanceDate;

                //                        // Get the time-in and time-out
                //                        var day5Record = rawDataSource
                //                            .Where(a => a.EmpNo == item.EmpNo && a.DT == attendanceDate)
                //                            .FirstOrDefault();
                //                        if (day5Record != null)
                //                        {
                //                            punctualityItem.Day5TimeIn = day5Record.dtIN;
                //                            punctualityItem.Day5TimeOut = day5Record.dtOUT;

                //                            if (day5Record.Remarks == BLHelper.ArrivalStatus.Dayoff.ToString())
                //                                punctualityItem.Day5Remarks = "Off";
                //                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.Absent.ToString())
                //                                punctualityItem.Day5Remarks = "Abs";
                //                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.OnLeave.ToString())
                //                                punctualityItem.Day5Remarks = day5Record.LeaveType;
                //                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.MissingSwipe.ToString())
                //                                punctualityItem.Day5Remarks = "MS";
                //                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.Holiday.ToString())
                //                                punctualityItem.Day5Remarks = "Hol";
                //                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.Late.ToString())
                //                            {
                //                                punctualityItem.Day5TimeInLate = true;

                //                                // Get the total duration of missed work
                //                                totalWorkMissed = totalWorkMissed + day5Record.ArrivalTimeDiff;
                //                            }
                //                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.LeftEarly.ToString())
                //                            {
                //                                punctualityItem.Day5TimeOutEarly = true;

                //                                // Get the total duration of missed work
                //                                totalWorkMissed = totalWorkMissed + day5Record.DepartureTimeDiff;
                //                            }
                //                            else if (day5Record.Remarks == BLHelper.ArrivalStatus.LateAndLeftEarly.ToString())
                //                            {
                //                                punctualityItem.Day5TimeInLate = true;
                //                                punctualityItem.Day5TimeOutEarly = true;

                //                                // Get the total duration of missed work
                //                                //totalWorkMissed = totalWorkMissed + day5Record.ArrivalTimeDiff + day5Record.DepartureTimeDiff;
                //                                totalWorkMissed = totalWorkMissed + day5Record.DepartureTimeDiff;
                //                            }
                //                        }
                //                        #endregion
                //                    }
                //                    else if (counter == 6)
                //                    {
                //                        #region Process Sixth day of week - Friday                                                                         
                //                        punctualityItem.Day6 = attendanceDate;

                //                        // Get the time-in and time-out
                //                        var day6Record = rawDataSource
                //                            .Where(a => a.EmpNo == item.EmpNo && a.DT == attendanceDate)
                //                            .FirstOrDefault();
                //                        if (day6Record != null)
                //                        {
                //                            punctualityItem.Day6TimeIn = day6Record.dtIN;
                //                            punctualityItem.Day6TimeOut = day6Record.dtOUT;

                //                            if (day6Record.Remarks == BLHelper.ArrivalStatus.Dayoff.ToString())
                //                                punctualityItem.Day6Remarks = "Off";
                //                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.Absent.ToString())
                //                                punctualityItem.Day6Remarks = "Abs";
                //                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.OnLeave.ToString())
                //                                punctualityItem.Day6Remarks = day6Record.LeaveType;
                //                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.MissingSwipe.ToString())
                //                                punctualityItem.Day6Remarks = "MS";
                //                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.Holiday.ToString())
                //                                punctualityItem.Day6Remarks = "Hol";
                //                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.Late.ToString())
                //                            {
                //                                punctualityItem.Day6TimeInLate = true;

                //                                // Get the total duration of missed work
                //                                totalWorkMissed = totalWorkMissed + day6Record.ArrivalTimeDiff;
                //                            }
                //                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.LeftEarly.ToString())
                //                            {
                //                                punctualityItem.Day6TimeOutEarly = true;

                //                                // Get the total duration of missed work
                //                                totalWorkMissed = totalWorkMissed + day6Record.DepartureTimeDiff;
                //                            }
                //                            else if (day6Record.Remarks == BLHelper.ArrivalStatus.LateAndLeftEarly.ToString())
                //                            {
                //                                punctualityItem.Day6TimeInLate = true;
                //                                punctualityItem.Day6TimeOutEarly = true;

                //                                // Get the total duration of missed work
                //                                //totalWorkMissed = totalWorkMissed + day6Record.ArrivalTimeDiff + day6Record.DepartureTimeDiff;
                //                                totalWorkMissed = totalWorkMissed + day6Record.DepartureTimeDiff;
                //                            }
                //                        }
                //                        #endregion
                //                    }
                //                    else if (counter == 7)
                //                    {
                //                        #region Process Seventh day of week - Saturday                                                                         
                //                        punctualityItem.Day7 = attendanceDate;

                //                        // Get the time-in and time-out
                //                        var day7Record = rawDataSource
                //                            .Where(a => a.EmpNo == item.EmpNo && a.DT == attendanceDate)
                //                            .FirstOrDefault();
                //                        if (day7Record != null)
                //                        {
                //                            punctualityItem.Day7TimeIn = day7Record.dtIN;
                //                            punctualityItem.Day7TimeOut = day7Record.dtOUT;

                //                            if (day7Record.Remarks == BLHelper.ArrivalStatus.Dayoff.ToString())
                //                                punctualityItem.Day7Remarks = "Off";
                //                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.Absent.ToString())
                //                                punctualityItem.Day7Remarks = "Abs";
                //                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.OnLeave.ToString())
                //                                punctualityItem.Day7Remarks = day7Record.LeaveType;
                //                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.MissingSwipe.ToString())
                //                                punctualityItem.Day7Remarks = "MS";
                //                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.Holiday.ToString())
                //                                punctualityItem.Day7Remarks = "Hol";
                //                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.Late.ToString())
                //                            {
                //                                punctualityItem.Day7TimeInLate = true;

                //                                // Get the total duration of missed work
                //                                totalWorkMissed = totalWorkMissed + day7Record.ArrivalTimeDiff;
                //                            }
                //                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.LeftEarly.ToString())
                //                            {
                //                                punctualityItem.Day7TimeOutEarly = true;

                //                                // Get the total duration of missed work
                //                                totalWorkMissed = totalWorkMissed + day7Record.DepartureTimeDiff;
                //                            }
                //                            else if (day7Record.Remarks == BLHelper.ArrivalStatus.LateAndLeftEarly.ToString())
                //                            {
                //                                punctualityItem.Day7TimeInLate = true;
                //                                punctualityItem.Day7TimeOutEarly = true;

                //                                // Get the total duration of missed work
                //                                //totalWorkMissed = totalWorkMissed + day7Record.ArrivalTimeDiff + day7Record.DepartureTimeDiff;
                //                                totalWorkMissed = totalWorkMissed + day7Record.DepartureTimeDiff;
                //                            }
                //                        }
                //                        #endregion
                //                    }

                //                    attendanceDate = attendanceDate.AddDays(1);
                //                    counter++;
                //                }

                //                // Calculate the total duration of missed workf for the entire week
                //                punctualityItem.TotalLateForWeek = totalWorkMissed;
                //                #endregion

                //                // Get the employee shift pattern
                //                var employeeShiftPattern = (from a in rawDataSource
                //                                            where a.EmpNo == item.EmpNo
                //                                            select new
                //                                            {
                //                                                EmpNo = a.EmpNo,
                //                                                EmpName = a.EmpName,
                //                                                ShiftPatCode = a.ShiftPatCode,
                //                                                DT = a.DT
                //                                            })
                //                                            .OrderByDescending(a => a.DT)
                //                                            .FirstOrDefault();
                //                if (employeeShiftPattern != null)
                //                {
                //                    punctualityItem.ShiftPatCode = employeeShiftPattern.ShiftPatCode;
                //                }

                //                // Add item to the collection
                //                puctualityList.Add(punctualityItem);
                //            }
                //        }
                //    }
                //}
                #endregion

                return puctualityList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetOvertimeByPeriod(int currentUserEmpNo, byte dataFilterID, DateTime? startDate, DateTime? endDate, string costCenter, int? empNo,
            long? otRequestNo, int pageNumber, int pageSize, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> overtimeData = new List<EmployeeAttendanceEntity>();

            try
            {
                #region Fetch data using ADO.Net                                
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[9];

                parameters[0] = new ADONetParameter("@currentUserEmpNo", SqlDbType.Int, currentUserEmpNo);
                parameters[1] = new ADONetParameter("@dataFilterID", SqlDbType.TinyInt, dataFilterID);
                parameters[2] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[3] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[4] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[5] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[6] = new ADONetParameter("@otRequestNo", SqlDbType.BigInt, otRequestNo);
                parameters[7] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[8] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetOvertimeByPeriod", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize the collection
                    overtimeData = new List<EmployeeAttendanceEntity>();

                    #region Populate data in the collection 
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["BusinessUnitName"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            GradeCode = BLHelper.ConvertObjectToInt(row["GradeCode"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                            ActualShiftCode = BLHelper.ConvertObjectToString(row["Actual_ShiftCode"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIN"]),
                            dtINLastRow = BLHelper.ConvertObjectToDate(row["dtIN_LastRow"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOUT"]),
                            OTStartTime = BLHelper.ConvertObjectToDate(row["OTstartTime"]),
                            OTEndTime = BLHelper.ConvertObjectToDate(row["OTendTime"]),
                            OTType = BLHelper.ConvertObjectToString(row["OTtype"]),
                            OTDurationMinute = BLHelper.ConvertObjectToInt(row["OTDurationMinute"]),
                            OTDurationHour = BLHelper.ConvertObjectToInt(row["OTDurationHour"]),
                            OTDurationHourOrig = BLHelper.ConvertObjectToDouble(row["OTDurationHourOrig"]),

                            //OTDurationHourClone = BLHelper.ConvertObjectToInt(row["TotalWorkDuration"]),  // Uncomment if maximum overtime duration should be equal to the total work duration
                            OTDurationHourClone = BLHelper.ConvertObjectToInt(row["OTDurationHour"]),

                            TotalWorkDuration = BLHelper.ConvertObjectToInt(row["TotalWorkDuration"]).ToString("0000").Insert(2, ":"),
                            RequiredWorkDuration = BLHelper.ConvertObjectToInt(row["RequiredWorkDuration"]).ToString("0000").Insert(2, ":"),
                            OTShavedTimeDuration = BLHelper.ConvertObjectToInt(row["OTShavedTimeDuration"]),
                            ExcessWorkDuration = BLHelper.ConvertObjectToInt(row["ExcessWorkDuration"]),
                            Approved = BLHelper.ConvertObjectToBolean(row["Approved"]),
                            AttendanceRemarks = BLHelper.ConvertObjectToString(row["Comment"]),
                            OTReasonCode = BLHelper.ConvertObjectToString(row["OTReasonCode"]),
                            OTReason = BLHelper.ConvertObjectToString(row["OTReason"]),
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            XID_AutoID = BLHelper.ConvertObjectToInt(row["XID_AutoID"]),
                            Processed = BLHelper.ConvertObjectToBolean(row["Processed"]),
                            CreatedByEmpNo = BLHelper.ConvertObjectToInt(row["CreatedByEmpNo"]),
                            CreatedByEmpName = BLHelper.ConvertObjectToString(row["CreatedByEmpName"]),
                            CreatedByEmail = BLHelper.ConvertObjectToString(row["CreatedByEmail"]),
                            LastUpdateEmpNo = BLHelper.ConvertObjectToInt(row["LastUpdateEmpNo"]),
                            LastUpdateEmpName = BLHelper.ConvertObjectToString(row["LastUpdateEmpName"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUserID"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            OTRequestNo = BLHelper.ConvertObjectToLong(row["OTRequestNo"]),
                            StatusCode = BLHelper.ConvertObjectToString(row["StatusCode"]),
                            StatusDesc = BLHelper.ConvertObjectToString(row["StatusDesc"]),
                            StatusHandlingCode = BLHelper.ConvertObjectToString(row["StatusHandlingCode"]),
                            CurrentlyAssignedEmpNo = BLHelper.ConvertObjectToInt(row["CurrentlyAssignedEmpNo"]),
                            CurrentlyAssignedEmpName = BLHelper.ConvertObjectToString(row["CurrentlyAssignedEmpName"]),
                            ServiceProviderTypeCode = BLHelper.ConvertObjectToString(row["ServiceProviderTypeCode"]),
                            DistListCode = BLHelper.ConvertObjectToString(row["DistListCode"]),
                            DistListDesc = BLHelper.ConvertObjectToString(row["DistListDesc"]),
                            DistListMembers = BLHelper.ConvertObjectToString(row["DistListMembers"]),
                            RequestSubmissionDate = BLHelper.ConvertObjectToDate(row["RequestSubmissionDate"]),
                            TotalRecords = ds.Tables[0].Columns.Contains("TotalRecords") 
                                ? BLHelper.ConvertObjectToInt(row["TotalRecords"]) 
                                : ds.Tables[0].Rows.Count,
                            IsOTDueToShiftSpan = BLHelper.ConvertObjectToBolean(row["IsOTDueToShiftSpan"]),
                            IsArrivedEarly = BLHelper.ConvertObjectToBolean(row["IsArrivedEarly"]),
                            ArrivalSchedule = BLHelper.ConvertObjectToString(row["ArrivalSchedule"]),
                            IsOTExceedOrig = BLHelper.ConvertNumberToBolean(row["IsOTExceedOrig"]),
                            IsPublicHoliday = BLHelper.ConvertObjectToBolean(row["IsPublicHoliday"]),
                            IsOTRamadanExceedLimit = BLHelper.ConvertNumberToBolean(row["IsOTRamadanExceedLimit"]),
                            IsRamadan = BLHelper.ConvertObjectToBolean(row["isRamadan"])
                        };

                        #region Set the maximum OT duration and tooltip 
                        //if (!newItem.IsArrivedEarly)
                        //{
                        //    newItem.OTDurationHourClone = newItem.OTShavedTimeDuration;
                        //    newItem.OTDurationTooltip = string.Format("(Note: Maximum OT duration than can be entered is {0}", newItem.OTShavedTimeDuration.ToString("0000").Insert(2, ":"));
                        //}
                        //else
                        //{
                        //    newItem.OTDurationHourClone = newItem.OTDurationHour;
                        //    newItem.OTDurationTooltip = string.Format("(Note: Maximum OT duration than can be entered is {0}", newItem.OTDurationHour.ToString("0000").Insert(2, ":"));
                        //}
                        #endregion

                        // Set the Cost Center Fullname
                        if (newItem.CostCenter != string.Empty &&
                            newItem.CostCenterName != string.Empty)
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                            newItem.CostCenterFullName = newItem.CostCenterName;

                        // Set the Currently Assigned Fullname
                        if (newItem.CurrentlyAssignedEmpNo > 0)
                        {
                            newItem.CurrentlyAssignedFullName = string.Format("({0}) {1}",
                                newItem.CurrentlyAssignedEmpNo,
                                newItem.CurrentlyAssignedEmpName);
                        }
                        else
                            newItem.CurrentlyAssignedFullName = newItem.DistListMembers;

                        // Set the Last Update Fullname
                        if (newItem.LastUpdateEmpNo > 0 &&
                            newItem.LastUpdateEmpName != string.Empty)
                        {
                            newItem.LastUpdateFullName = string.Format("({0}) {1}",
                                newItem.LastUpdateEmpNo,
                                newItem.LastUpdateEmpName);
                        }
                        else
                            newItem.LastUpdateFullName = newItem.LastUpdateEmpName;

                        if (newItem.OTDurationHour > 0)
                        {
                            newItem.OTDurationText = newItem.OTDurationHour.ToString("0000").Insert(2, ":");                            
                        }

                        #region Process "OT Approved?"
                        newItem.OTApprovalCode = BLHelper.ConvertObjectToString(row["OTApproved"]);
                        if (newItem.OTApprovalCode == "Y")
                        {
                            newItem.OTApprovalDesc = "Yes";
                            newItem.IsOTAlreadyProcessed = true;
                        }
                        else if (newItem.OTApprovalCode == "N")
                        {
                            newItem.OTApprovalDesc = "No";
                            newItem.IsOTAlreadyProcessed = true;
                        }
                        else
                            newItem.OTApprovalDesc = "-";
                        #endregion

                        #region Process "Meal Voucher Approved?"
                        newItem.MealVoucherEligibilityCode = BLHelper.ConvertObjectToString(row["MealVoucherEligibility"]);
                        if (newItem.MealVoucherEligibilityCode == "YA")
                            newItem.MealVoucherEligibility = "Yes";
                        else if (newItem.MealVoucherEligibilityCode == "N")
                            newItem.MealVoucherEligibility = "No";
                        else
                            newItem.MealVoucherEligibility = "-";
                        #endregion

                        // Add item to the collection
                        overtimeData.Add(newItem);
                    }
                    #endregion
                }
                #endregion

                return overtimeData;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public DatabaseSaveResult SaveEmployeeOvertimeByClerk(int autoID, string otReasonCode, string comment, int userEmpNo, string userEmpName, string userID, string otApprovalCode, 
            string mealVoucherApprovalCode, int otDuration, ref string error, ref string innerError)
        {
            DatabaseSaveResult dbResult = null;

            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    #region Save the overtime requisition
                    
                    #region Initialize parameters
                    string connectionString = TASContext.Database.Connection.ConnectionString;
                    ADONetParameter[] parameters = new ADONetParameter[9];

                    parameters[0] = new ADONetParameter("@autoID", SqlDbType.Int, autoID);
                    parameters[1] = new ADONetParameter("@otReason", SqlDbType.VarChar, 10, otReasonCode);
                    parameters[2] = new ADONetParameter("@comment", SqlDbType.VarChar, 1000, comment);
                    parameters[3] = new ADONetParameter("@userEmpNo", SqlDbType.Int, userEmpNo);
                    parameters[4] = new ADONetParameter("@userEmpName", SqlDbType.VarChar, 100, userEmpName);
                    parameters[5] = new ADONetParameter("@userID", SqlDbType.VarChar, 50, userID);
                    parameters[6] = new ADONetParameter("@otApproved", SqlDbType.VarChar, 1, otApprovalCode);
                    parameters[7] = new ADONetParameter("@mealVoucherEligibilityCode", SqlDbType.VarChar, 10, mealVoucherApprovalCode);
                    parameters[8] = new ADONetParameter("@otDuration", SqlDbType.Int, otDuration);
                    #endregion

                    #region Save data to database
                    DataSet ds = RunSPReturnDataset("tas.Pr_InsertUpdateDeleteOvertime_Clerk", connectionString, parameters);
                    if (ds != null && ds.Tables[0].Rows.Count > 0)
                    {
                        dbResult = new DatabaseSaveResult()
                        {
                            HasError = BLHelper.ConvertObjectToBolean(ds.Tables[0].Rows[0]["HasError"]),
                            ErrorCode = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorCode"]),
                            ErrorDesc = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorDescription"]),
                            OvertimeRowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["OvertimeRowsAffected"]),
                            OvertimeRequestRowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["OvertimeRequestRowsAffected"]),
                            OTRequestNo = BLHelper.ConvertObjectToLong(ds.Tables[0].Rows[0]["OTRequestNo"]),
                            RequestSubmissionDate = BLHelper.ConvertObjectToDate(ds.Tables[0].Rows[0]["RequestSubmissionDate"])
                        };

                        if (!dbResult.HasError)
                        {
                            #region Instantiate the workflow
                            error = innerError = string.Empty;
                            DateTime requestSubmissionDate = dbResult.RequestSubmissionDate.HasValue
                                ? Convert.ToDateTime(dbResult.RequestSubmissionDate)
                                : DateTime.Now;

                            DatabaseSaveResult wfResult = ProcessOvertimeWorflowInternal(1, dbResult.OTRequestNo, autoID, userID, userEmpNo, userEmpName, 
                                null, null, null, null, requestSubmissionDate, ref error, ref innerError);
                            if (wfResult != null)
                            {
                                // Save the flag that determines whether the workflow has been closed automatically in the backend database 
                                dbResult.IsWorkflowCompleted = wfResult.IsWorkflowCompleted;

                                if (!wfResult.HasError)
                                {
                                    // Commit the transaction
                                    scope.Complete();
                                }
                                else
                                {
                                    // Pass the error information to the caller
                                    dbResult.HasError = wfResult.HasError;
                                    dbResult.ErrorCode = wfResult.ErrorCode;
                                    dbResult.ErrorDesc = wfResult.ErrorDesc;                                    
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion

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
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();

                dbResult = new DatabaseSaveResult()
                {
                    RowsAffected = 0,
                    HasError = true,
                    ErrorCode = string.Empty,
                    ErrorDesc = ex.Message.ToString()
                };

                return dbResult;
            }
        }

        public DatabaseSaveResult ManageOvertimeRequest(byte actionType, long otRequestNo, int userEmpNo, string userEmpName, string userID, ref string error, ref string innerError)
        {
            DatabaseSaveResult result = null;

            try
            {
                #region Initialize parameters
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[5];

                parameters[0] = new ADONetParameter("@actionType", SqlDbType.TinyInt, actionType);
                parameters[1] = new ADONetParameter("@otRequestNo", SqlDbType.BigInt, otRequestNo);
                parameters[2] = new ADONetParameter("@userEmpNo", SqlDbType.Int, userEmpNo);
                parameters[3] = new ADONetParameter("@userEmpName", SqlDbType.VarChar, 100, userEmpName);
                parameters[4] = new ADONetParameter("@userID", SqlDbType.VarChar, 50, userID);
                #endregion

                DataSet ds = RunSPReturnDataset("tas.Pr_ManageOvertimeRequest", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    result = new DatabaseSaveResult()
                    {
                        HasError = BLHelper.ConvertObjectToBolean(ds.Tables[0].Rows[0]["HasError"]),
                        ErrorCode = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorCode"]),
                        ErrorDesc = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorDescription"]),
                        OvertimeRowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["OvertimeRowsAffected"]),
                        OvertimeRequestRowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["OvertimeRequestRowsAffected"])
                    };

                    if (result.HasError)
                        throw new Exception(result.ErrorDesc);
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();

                result = new DatabaseSaveResult()
                {
                    RowsAffected = 0,
                    HasError = true,
                    ErrorCode = string.Empty,
                    ErrorDesc = ex.Message.ToString()
                };
                return null;
            }
        }

        public List<WorkflowEmailDeliveryEntity> GetWFEmailDueForDelivery(byte actionType, int? createdByEmpNo, int? assignedToEmpNo, DateTime? startDate, DateTime? endDate, ref string error, ref string innerError)
        {
            try
            {
                List<WorkflowEmailDeliveryEntity> result = null;
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[5];

                parameters[0] = new ADONetParameter("@actionType", SqlDbType.TinyInt, actionType);
                parameters[1] = new ADONetParameter("@createdByEmpNo", SqlDbType.Int, createdByEmpNo);
                parameters[2] = new ADONetParameter("@assignedToEmpNo", SqlDbType.Int, assignedToEmpNo);
                parameters[3] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[4] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetOvertimeWFEmailDelivery", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    #region Populate items in the collection
                    result = new List<WorkflowEmailDeliveryEntity>();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        WorkflowEmailDeliveryEntity item = new WorkflowEmailDeliveryEntity()
                        {
                            DeliveryID = ds.Tables[0].Columns.Contains("DeliveryID")
                                    ? BLHelper.ConvertObjectToInt(row["DeliveryID"])
                                    : 0,
                            OTRequestNo = ds.Tables[0].Columns.Contains("OTRequestNo")
                                ? BLHelper.ConvertObjectToLong(row["OTRequestNo"])
                                : 0,
                            TS_AutoID = ds.Tables[0].Columns.Contains("TS_AutoID")
                                ? BLHelper.ConvertObjectToInt(row["TS_AutoID"])
                                : 0,
                            CurrentlyAssignedEmpNo = ds.Tables[0].Columns.Contains("CurrentlyAssignedEmpNo")
                                ? BLHelper.ConvertObjectToInt(row["CurrentlyAssignedEmpNo"])
                                : 0,
                            CurrentlyAssignedEmpName = ds.Tables[0].Columns.Contains("CurrentlyAssignedEmpName")
                                ? BLHelper.ConvertObjectToString(row["CurrentlyAssignedEmpName"])
                                : string.Empty,
                            CurrentlyAssignedEmpEmail = ds.Tables[0].Columns.Contains("CurrentlyAssignedEmpEmail")
                                ? BLHelper.ConvertObjectToString(row["CurrentlyAssignedEmpEmail"])
                                : string.Empty,
                            ActivityCode = ds.Tables[0].Columns.Contains("ActivityCode")
                                ? BLHelper.ConvertObjectToString(row["ActivityCode"])
                                : string.Empty,
                            ActionMemberCode = ds.Tables[0].Columns.Contains("ActionMemberCode")
                                ? BLHelper.ConvertObjectToString(row["ActionMemberCode"])
                                : string.Empty,
                            EmailSourceName = ds.Tables[0].Columns.Contains("EmailSourceName")
                                ? BLHelper.ConvertObjectToString(row["EmailSourceName"])
                                : string.Empty,
                            EmailCCRecipient = ds.Tables[0].Columns.Contains("EmailCCRecipient")
                                ? BLHelper.ConvertObjectToString(row["EmailCCRecipient"])
                                : string.Empty,
                            EmailCCRecipientType = ds.Tables[0].Columns.Contains("EmailCCRecipientType")
                                ? BLHelper.ConvertObjectToInt(row["EmailCCRecipientType"])
                                : 0,
                            IsDelivered = ds.Tables[0].Columns.Contains("IsDelivered")
                                ? BLHelper.ConvertObjectToBolean(row["IsDelivered"])
                                : false,
                            CreatedByEmpNo = ds.Tables[0].Columns.Contains("CreatedByEmpNo")
                                ? BLHelper.ConvertObjectToInt(row["CreatedByEmpNo"])
                                : 0,
                            CreatedByEmpName = ds.Tables[0].Columns.Contains("CreatedByEmpName")
                                ? BLHelper.ConvertObjectToString(row["CreatedByEmpName"])
                                : string.Empty,
                            CreatedDate = ds.Tables[0].Columns.Contains("CreatedDate")
                                ? BLHelper.ConvertObjectToDate(row["CreatedDate"])
                                : null,
                            EmpNo = ds.Tables[0].Columns.Contains("EmpNo")
                                ? BLHelper.ConvertObjectToInt(row["EmpNo"])
                                : 0,
                            EmpName = ds.Tables[0].Columns.Contains("EmpName")
                                ? BLHelper.ConvertObjectToString(row["EmpName"])
                                : string.Empty,
                            Position = ds.Tables[0].Columns.Contains("Position")
                                ? BLHelper.ConvertObjectToString(row["Position"])
                                : string.Empty,
                            PayGrade = ds.Tables[0].Columns.Contains("GradeCode")
                                ? BLHelper.ConvertObjectToInt(row["GradeCode"])
                                : 0,
                            CostCenter = ds.Tables[0].Columns.Contains("CostCenter")
                                ? BLHelper.ConvertObjectToString(row["CostCenter"])
                                : string.Empty,
                            CostCenterName = ds.Tables[0].Columns.Contains("CostCenterName")
                                ? BLHelper.ConvertObjectToString(row["CostCenterName"])
                                : string.Empty,                            
                            ShiftPatCode = ds.Tables[0].Columns.Contains("ShiftPatCode")
                                ? BLHelper.ConvertObjectToString(row["ShiftPatCode"])
                                : string.Empty,
                            ShiftCode = ds.Tables[0].Columns.Contains("ShiftCode")
                                ? BLHelper.ConvertObjectToString(row["ShiftCode"])
                                : string.Empty,
                            ActualShiftCode = ds.Tables[0].Columns.Contains("Actual_ShiftCode")
                                ? BLHelper.ConvertObjectToString(row["Actual_ShiftCode"])
                                : string.Empty,
                            DT = ds.Tables[0].Columns.Contains("DT")
                                ? BLHelper.ConvertObjectToDate(row["DT"])
                                : null,
                            OTStartTime = ds.Tables[0].Columns.Contains("OTStartTime")
                                ? BLHelper.ConvertObjectToDate(row["OTStartTime"])
                                : null,
                            OTEndTime = ds.Tables[0].Columns.Contains("OTEndTime")
                                ? BLHelper.ConvertObjectToDate(row["OTEndTime"])
                                : null,
                            OTType = ds.Tables[0].Columns.Contains("OTType")
                                ? BLHelper.ConvertObjectToString(row["OTType"])
                                : string.Empty,
                            MealVoucherEligibility = ds.Tables[0].Columns.Contains("MealVoucherEligibility")
                               ? BLHelper.ConvertObjectToString(row["MealVoucherEligibility"])
                               : string.Empty,
                            OTApproved = ds.Tables[0].Columns.Contains("OTApproved")
                               ? BLHelper.ConvertObjectToString(row["OTApproved"])
                               : string.Empty,
                            CorrectionCode = ds.Tables[0].Columns.Contains("CorrectionCode")
                               ? BLHelper.ConvertObjectToString(row["CorrectionCode"])
                               : string.Empty,
                            CorrectionDesc = ds.Tables[0].Columns.Contains("CorrectionDesc")
                               ? BLHelper.ConvertObjectToString(row["CorrectionDesc"])
                               : string.Empty,
                            OTComment = ds.Tables[0].Columns.Contains("OTComment")
                               ? BLHelper.ConvertObjectToString(row["OTComment"])
                               : string.Empty
                        };

                        // Set the employee full name
                        if (item.EmpNo > 0 && !string.IsNullOrEmpty(item.EmpName))
                            item.EmpFullName = string.Format("({0}) {1}", item.EmpNo, item.EmpName);

                        // Set the cost center name
                        if (!string.IsNullOrEmpty(item.CostCenter) && !string.IsNullOrEmpty(item.CostCenterName))
                            item.CostCenterFullName = string.Format("{0} - {1}", item.CostCenter, item.CostCenterName);

                        // Add item to collection
                        result.Add(item);
                    }
                    #endregion
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void CloseEmailDelivery(List<WorkflowEmailDeliveryEntity> emailDeliveryList, ref string error, ref string innerError)
        {
            try
            {
                foreach (WorkflowEmailDeliveryEntity item in emailDeliveryList)
                {
                    // Find the record to update
                    OvertimeWFEmailDelivery recordToUpdate = TASContext.OvertimeWFEmailDeliveries
                        .Where(a => a.DeliveryID == item.DeliveryID)
                        .FirstOrDefault();
                    if (recordToUpdate != null)
                    {
                        recordToUpdate.IsDelivered = true;
                        recordToUpdate.LastUpdateTime = item.LastUpdateTime;
                        recordToUpdate.LastUpdateEmpNo = item.LastUpdateEmpNo;
                        recordToUpdate.LastUpdateEmpName = item.LastUpdateEmpName;

                        // Save to database
                        TASContext.SaveChanges();
                    }
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        private DatabaseSaveResult ProcessOvertimeWorflowInternal(byte actionType, long otRequestNo, int tsAutoID, string currentUserID, int? createdByEmpNo, string createdByEmpName,
            int? assigneeEmpNo, string assigneeEmpName, bool? isApproved, string appRemarks, DateTime? requestSubmissionDate, ref string error, ref string innerError)
        {
            DatabaseSaveResult result = null;

            try
            {
                #region Initialize parameters
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[11];

                parameters[0] = new ADONetParameter("@actionType", SqlDbType.TinyInt, actionType);
                parameters[1] = new ADONetParameter("@otRequestNo", SqlDbType.BigInt, otRequestNo);
                parameters[2] = new ADONetParameter("@tsAutoID", SqlDbType.Int, tsAutoID);
                parameters[3] = new ADONetParameter("@currentUserID", SqlDbType.VarChar, 50, currentUserID);
                parameters[4] = new ADONetParameter("@createdByEmpNo", SqlDbType.Int, createdByEmpNo);
                parameters[5] = new ADONetParameter("@createdByEmpName", SqlDbType.VarChar, 50, createdByEmpName);
                parameters[6] = new ADONetParameter("@assigneeEmpNo", SqlDbType.Int, assigneeEmpNo);
                parameters[7] = new ADONetParameter("@assigneeEmpName", SqlDbType.VarChar, 50, assigneeEmpName);
                parameters[8] = new ADONetParameter("@isApproved", SqlDbType.Bit, isApproved);
                parameters[9] = new ADONetParameter("@appRemarks", SqlDbType.VarChar, 300, appRemarks);
                parameters[10] = new ADONetParameter("@requestSubmissionDate", SqlDbType.DateTime, requestSubmissionDate);
                #endregion

                DataSet ds = RunSPReturnDataset("tas.Pr_SetOvertimeWorkflowState", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    result = new DatabaseSaveResult()
                    {
                        HasError = BLHelper.ConvertObjectToBolean(ds.Tables[0].Rows[0]["HasError"]),
                        ErrorCode = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorCode"]),
                        ErrorDesc = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorDescription"]),
                        OvertimeRequestRowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["OTRequestRecordProcessed"]),
                        TimesheetRowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["TimeSheetRecordProcessed"]),
                        OvertimeRowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["OTDetailRecordProcessed"]),
                        IsWorkflowCompleted = BLHelper.ConvertObjectToBolean(ds.Tables[0].Rows[0]["IsWorkflowCompleted"]),
                        CurrentlyAssignedEmpNo = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["CurrentlyAssignedEmpNo"]),
                        CurrentlyAssignedEmpName = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["CurrentlyAssignedEmpName"]),
                        CurrentlyAssignedEmpEmail = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["CurrentlyAssignedEmpEmail"]),
                        EmailSourceName = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["EmailSourceName"]),
                        EmailCCRecipient = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["EmailCCRecipient"]),
                        EmailCCRecipientType = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["EmailCCRecipientType"])
                    };

                    //if (result.HasError &&
                    //    !string.IsNullOrEmpty(result.ErrorDesc))
                    //{
                    //    throw new Exception(result.ErrorDesc);
                    //}
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public DatabaseSaveResult ProcessOvertimeWorflow(byte actionType, long otRequestNo, int tsAutoID, string currentUserID, int? createdByEmpNo, string createdByEmpName,
            int? assigneeEmpNo, string assigneeEmpName, bool? isApproved, string appRemarks, DateTime? requestSubmissionDate, string otComment, ref string error, ref string innerError)
        {
            DatabaseSaveResult result = null;

            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    #region Initialize parameters
                    string connectionString = TASContext.Database.Connection.ConnectionString;
                    ADONetParameter[] parameters = new ADONetParameter[12];

                    parameters[0] = new ADONetParameter("@actionType", SqlDbType.TinyInt, actionType);
                    parameters[1] = new ADONetParameter("@otRequestNo", SqlDbType.BigInt, otRequestNo);
                    parameters[2] = new ADONetParameter("@tsAutoID", SqlDbType.Int, tsAutoID);
                    parameters[3] = new ADONetParameter("@currentUserID", SqlDbType.VarChar, 50, currentUserID);
                    parameters[4] = new ADONetParameter("@createdByEmpNo", SqlDbType.Int, createdByEmpNo);
                    parameters[5] = new ADONetParameter("@createdByEmpName", SqlDbType.VarChar, 50, createdByEmpName);
                    parameters[6] = new ADONetParameter("@assigneeEmpNo", SqlDbType.Int, assigneeEmpNo);
                    parameters[7] = new ADONetParameter("@assigneeEmpName", SqlDbType.VarChar, 50, assigneeEmpName);
                    parameters[8] = new ADONetParameter("@isApproved", SqlDbType.Bit, isApproved);
                    parameters[9] = new ADONetParameter("@appRemarks", SqlDbType.VarChar, 300, appRemarks);
                    parameters[10] = new ADONetParameter("@requestSubmissionDate", SqlDbType.DateTime, requestSubmissionDate);
                    parameters[11] = new ADONetParameter("@otComment", SqlDbType.VarChar, 1000, otComment);
                    #endregion

                    DataSet ds = RunSPReturnDataset("tas.Pr_SetOvertimeWorkflowState", connectionString, parameters);
                    if (ds != null && ds.Tables[0].Rows.Count > 0)
                    {
                        result = new DatabaseSaveResult()
                        {
                            HasError = BLHelper.ConvertObjectToBolean(ds.Tables[0].Rows[0]["HasError"]),
                            ErrorCode = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorCode"]),
                            ErrorDesc = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorDescription"]),
                            OvertimeRequestRowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["OTRequestRecordProcessed"]),
                            TimesheetRowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["TimeSheetRecordProcessed"]),
                            OvertimeRowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["OTDetailRecordProcessed"]),
                            IsWorkflowCompleted = BLHelper.ConvertObjectToBolean(ds.Tables[0].Rows[0]["IsWorkflowCompleted"]),
                            CurrentlyAssignedEmpNo = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["CurrentlyAssignedEmpNo"]),
                            CurrentlyAssignedEmpName = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["CurrentlyAssignedEmpName"]),
                            CurrentlyAssignedEmpEmail = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["CurrentlyAssignedEmpEmail"]),
                            EmailSourceName = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["EmailSourceName"]),
                            EmailCCRecipient = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["EmailCCRecipient"]),
                            EmailCCRecipientType = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["EmailCCRecipientType"])
                        };

                        if (result.HasError)
                        {
                            if (!string.IsNullOrEmpty(result.ErrorDesc))
                                throw new Exception(result.ErrorDesc);
                            else
                                throw new Exception("An unknown error has accoured in the database!");
                        }
                        else
                        {
                            // Commit the transaction
                            scope.Complete();
                        }
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public DatabaseSaveResult SubmitOvertimeChanges(long otRequestNo, string otReasonCode, string comment, int userEmpNo, string userEmpName, string userID, string otApprovalCode,
           string mealVoucherApprovalCode, int? otDuration, ref string error, ref string innerError)
        {
            DatabaseSaveResult dbResult = null;

            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    #region Initialize parameters
                    string connectionString = TASContext.Database.Connection.ConnectionString;
                    ADONetParameter[] parameters = new ADONetParameter[9];

                    parameters[0] = new ADONetParameter("@otRequestNo", SqlDbType.BigInt, otRequestNo);
                    parameters[1] = new ADONetParameter("@otReason", SqlDbType.VarChar, 10, otReasonCode);
                    parameters[2] = new ADONetParameter("@comment", SqlDbType.VarChar, 1000, comment);
                    parameters[3] = new ADONetParameter("@userEmpNo", SqlDbType.Int, userEmpNo);
                    parameters[4] = new ADONetParameter("@userEmpName", SqlDbType.VarChar, 100, userEmpName);
                    parameters[5] = new ADONetParameter("@userID", SqlDbType.VarChar, 50, userID);
                    parameters[6] = new ADONetParameter("@otApproved", SqlDbType.VarChar, 1, otApprovalCode);
                    parameters[7] = new ADONetParameter("@mealVoucherEligibilityCode", SqlDbType.VarChar, 10, mealVoucherApprovalCode);
                    parameters[8] = new ADONetParameter("@otDuration", SqlDbType.Int, otDuration);
                    #endregion

                    #region Save data to database
                    DataSet ds = RunSPReturnDataset("tas.Pr_UpdateOvertimeRequestByHR", connectionString, parameters);
                    if (ds != null && ds.Tables[0].Rows.Count > 0)
                    {
                        dbResult = new DatabaseSaveResult()
                        {
                            HasError = BLHelper.ConvertObjectToBolean(ds.Tables[0].Rows[0]["HasError"]),
                            ErrorCode = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorCode"]),
                            ErrorDesc = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["ErrorDescription"]),
                            OvertimeRequestRowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["OvertimeRequestRowsAffected"]),
                            OvertimeRowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["OvertimeDetailRowsAffected"]),
                            OTStartTime = BLHelper.ConvertObjectToDate(ds.Tables[0].Rows[0]["OTStartTime"]),
                            OTEndTime = BLHelper.ConvertObjectToDate(ds.Tables[0].Rows[0]["OTEndTime"]),
                            OTType = BLHelper.ConvertObjectToString(ds.Tables[0].Rows[0]["OTType"]),
                        };

                        if (!dbResult.HasError)
                        {
                            // Commit the transaction
                            scope.Complete();
                        }
                    }
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
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();

                dbResult = new DatabaseSaveResult()
                {
                    RowsAffected = 0,
                    HasError = true,
                    ErrorCode = string.Empty,
                    ErrorDesc = ex.Message.ToString()
                };

                return dbResult;
            }
        }

        public List<EmployeeDetail> GetEmployeeEmailInfo(int empNo, string costCenter, ref string error, ref string innerError)
        {
            List<EmployeeDetail> employeeList = null;

            try
            {
                var rawData = TASContext.GetEmployeeEmailAddress(empNo, costCenter);
                if (rawData != null)
                {
                    // Initialize the collection
                    employeeList = new List<EmployeeDetail>();

                    foreach (var item in rawData)
                    {
                        EmployeeDetail newItem = new EmployeeDetail()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(item.EmpNo),
                            EmpName = BLHelper.ConvertObjectToString(item.EmpName),
                            Position = BLHelper.ConvertObjectToString(item.Position),
                            EmpEmail = BLHelper.ConvertObjectToString(item.EmpEmail)
                        };

                        // Add item to the collection
                        employeeList.Add(newItem);
                    }
                }

                return employeeList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetAssignedOvertimeRequest(int currentUserEmpNo, byte assignTypeID, int @assignedToEmpNo,
            DateTime? startDate, DateTime? endDate, string costCenter, int? empNo, bool show12HourShift, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> overtimeData = new List<EmployeeAttendanceEntity>();

            try
            {
                #region Fetch data using ADO.Net                                
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[8];

                parameters[0] = new ADONetParameter("@currentUserEmpNo", SqlDbType.Int, currentUserEmpNo);
                parameters[1] = new ADONetParameter("@assignTypeID", SqlDbType.TinyInt, @assignTypeID);
                parameters[2] = new ADONetParameter("@assignedToEmpNo", SqlDbType.Int, assignedToEmpNo);
                parameters[3] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[4] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[5] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[6] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[7] = new ADONetParameter("@show12HourShift", SqlDbType.Bit, show12HourShift);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetAssignedOvertimeRequest_V2", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize the collection
                    overtimeData = new List<EmployeeAttendanceEntity>();

                    #region Populate data in the collection 
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["BusinessUnitName"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            GradeCode = BLHelper.ConvertObjectToInt(row["GradeCode"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                            ActualShiftCode = BLHelper.ConvertObjectToString(row["Actual_ShiftCode"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIN"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOUT"]),
                            OTStartTime = BLHelper.ConvertObjectToDate(row["OTstartTime"]),
                            OTEndTime = BLHelper.ConvertObjectToDate(row["OTendTime"]),
                            OTType = BLHelper.ConvertObjectToString(row["OTtype"]),
                            OTDurationMinute = BLHelper.ConvertObjectToInt(row["OTDurationMinute"]),
                            OTDurationHour = BLHelper.ConvertObjectToInt(row["OTDurationHour"]),
                            OTDurationHourOrig = BLHelper.ConvertObjectToDouble(row["OTDurationHourOrig"]),
                            OTDurationHourClone = BLHelper.ConvertObjectToInt(row["OTDurationHour"]),
                            Approved = BLHelper.ConvertObjectToBolean(row["Approved"]),
                            AttendanceRemarks = BLHelper.ConvertObjectToString(row["Comment"]),
                            OTReasonCode = BLHelper.ConvertObjectToString(row["OTReasonCode"]),
                            OTReason = BLHelper.ConvertObjectToString(row["OTReason"]),
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            XID_AutoID = BLHelper.ConvertObjectToInt(row["XID_AutoID"]),
                            Processed = BLHelper.ConvertObjectToBolean(row["Processed"]),
                            CreatedByEmpNo = BLHelper.ConvertObjectToInt(row["CreatedByEmpNo"]),
                            CreatedByEmpName = BLHelper.ConvertObjectToString(row["CreatedByEmpName"]),
                            CreatedByEmail = BLHelper.ConvertObjectToString(row["CreatedByEmail"]),
                            LastUpdateEmpNo = BLHelper.ConvertObjectToInt(row["LastUpdateEmpNo"]),
                            LastUpdateEmpName = BLHelper.ConvertObjectToString(row["LastUpdateEmpName"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUserID"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            OTRequestNo = BLHelper.ConvertObjectToLong(row["OTRequestNo"]),
                            StatusCode = BLHelper.ConvertObjectToString(row["StatusCode"]),
                            StatusDesc = BLHelper.ConvertObjectToString(row["StatusDesc"]),
                            StatusHandlingCode = BLHelper.ConvertObjectToString(row["StatusHandlingCode"]),
                            CurrentlyAssignedEmpNo = BLHelper.ConvertObjectToInt(row["CurrentlyAssignedEmpNo"]),
                            CurrentlyAssignedEmpName = BLHelper.ConvertObjectToString(row["CurrentlyAssignedEmpName"]),
                            ServiceProviderTypeCode = BLHelper.ConvertObjectToString(row["ServiceProviderTypeCode"]),
                            DistListCode = BLHelper.ConvertObjectToString(row["DistListCode"]),
                            DistListDesc = BLHelper.ConvertObjectToString(row["DistListDesc"]),
                            DistListMembers = BLHelper.ConvertObjectToString(row["DistListMembers"]),
                            RequestSubmissionDate = BLHelper.ConvertObjectToDate(row["SubmittedDate"]),
                            IsCallOut = BLHelper.ConvertNumberToBolean(row["IsCallOut"]),
                            IsOTDueToShiftSpan = BLHelper.ConvertObjectToBolean(row["IsOTDueToShiftSpan"]),
                            ArrivalSchedule = BLHelper.ConvertObjectToString(row["ArrivalSchedule"]),
                            TotalWorkDuration = BLHelper.ConvertObjectToInt(row["TotalWorkDuration"]).ToString("0000").Insert(2, ":"),
                            RequiredWorkDuration = BLHelper.ConvertObjectToInt(row["RequiredWorkDuration"]).ToString("0000").Insert(2, ":"),
                            IsOTExceedOrig = BLHelper.ConvertNumberToBolean(row["IsOTExceedOrig"]),
                            OTStartTimeOrig = BLHelper.ConvertObjectToDate(row["OTStartTime_Orig"]),
                            OTEndTimeOrig = BLHelper.ConvertObjectToDate(row["OTEndTime_Orig"]),
                            IsHold = BLHelper.ConvertObjectToBolean(row["IsHold"]),
                            IsOTRamadanExceedLimit = BLHelper.ConvertNumberToBolean(row["IsOTRamadanExceedLimit"])
                        };

                        // Set the Cost Center Fullname
                        if (newItem.CostCenter != string.Empty &&
                            newItem.CostCenterName != string.Empty)
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                            newItem.CostCenterFullName = newItem.CostCenterName;

                        // Set the Employee fullname
                        if (newItem.EmpNo > 0 &&
                            newItem.EmpName != string.Empty)
                        {
                            newItem.EmpFullName = string.Format("({0}) {1}",
                                newItem.EmpNo,
                                newItem.EmpName);
                        }
                        else
                            newItem.EmpFullName = newItem.EmpName;

                        // Set the Currently Assigned Fullname
                        if (newItem.CurrentlyAssignedEmpNo > 0)
                        {
                            newItem.CurrentlyAssignedFullName = string.Format("({0}) {1}",
                                newItem.CurrentlyAssignedEmpNo,
                                newItem.CurrentlyAssignedEmpName);
                        }
                        else
                            newItem.CurrentlyAssignedFullName = newItem.DistListMembers;

                        // Set the Last Update Fullname
                        if (newItem.LastUpdateEmpNo > 0 &&
                            newItem.LastUpdateEmpName != string.Empty)
                        {
                            newItem.LastUpdateFullName = string.Format("({0}) {1}",
                                newItem.LastUpdateEmpNo,
                                newItem.LastUpdateEmpName);
                        }
                        else
                            newItem.LastUpdateFullName = newItem.LastUpdateEmpName;

                        if (newItem.OTDurationHour > 0)
                        {
                            newItem.OTDurationText = newItem.OTDurationHour.ToString("0000").Insert(2, ":");
                        }

                        #region Process "OT Approved?"
                        newItem.OTApprovalCode = BLHelper.ConvertObjectToString(row["OTApproved"]);
                        if (newItem.OTApprovalCode == "Y")
                        {
                            newItem.OTApprovalDesc = "Yes";
                            newItem.IsOTAlreadyProcessed = true;
                        }
                        else if (newItem.OTApprovalCode == "N")
                        {
                            newItem.OTApprovalDesc = "No";
                            newItem.IsOTAlreadyProcessed = true;
                        }
                        else
                            newItem.OTApprovalDesc = "-";
                        #endregion

                        #region Process "Meal Voucher Approved?"
                        newItem.MealVoucherEligibilityCode = BLHelper.ConvertObjectToString(row["MealVoucherEligibility"]);
                        if (newItem.MealVoucherEligibilityCode == "YA")
                            newItem.MealVoucherEligibility = "Yes";
                        else if (newItem.MealVoucherEligibilityCode == "N")
                            newItem.MealVoucherEligibility = "No";
                        else
                            newItem.MealVoucherEligibility = "-";
                        #endregion

                        #region Process "Approve?" field
                        if (newItem.IsHold)
                            newItem.OTWFApprovalCode = "0";
                        else
                            newItem.OTWFApprovalCode = "Y";     // Default selection is Yes
                        #endregion

                        // Add item to the collection
                        overtimeData.Add(newItem);
                    }
                    #endregion
                }
                #endregion

                return overtimeData;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAttendanceEntity> GetOvertimeRequisition(int currentUserEmpNo, long otRequestNo, int empNo, string costCenter, int createdByEmpNo, int assignedToEmpNo,
            DateTime? startDate, DateTime? endDate,  string statusCode, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> overtimeData = new List<EmployeeAttendanceEntity>();

            try
            {
                #region Fetch data using ADO.Net                                
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[9];

                parameters[0] = new ADONetParameter("@currentUserEmpNo", SqlDbType.Int, currentUserEmpNo);
                parameters[1] = new ADONetParameter("@otRequestNo", SqlDbType.BigInt, otRequestNo);
                parameters[2] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[3] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[4] = new ADONetParameter("@createdByEmpNo", SqlDbType.Int, createdByEmpNo);
                parameters[5] = new ADONetParameter("@assignedToEmpNo", SqlDbType.Int, assignedToEmpNo);
                parameters[6] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[7] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[8] = new ADONetParameter("@statusCode", SqlDbType.VarChar, 10, statusCode);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetOvertimeRequisition", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize the collection
                    overtimeData = new List<EmployeeAttendanceEntity>();

                    #region Populate data in the collection 
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["CostCenterName"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            GradeCode = BLHelper.ConvertObjectToInt(row["GradeCode"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                            ActualShiftCode = BLHelper.ConvertObjectToString(row["Actual_ShiftCode"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIN"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOUT"]),
                            OTStartTime = BLHelper.ConvertObjectToDate(row["OTstartTime"]),
                            OTEndTime = BLHelper.ConvertObjectToDate(row["OTendTime"]),
                            OTType = BLHelper.ConvertObjectToString(row["OTtype"]),
                            OTDurationMinute = BLHelper.ConvertObjectToInt(row["OTDurationMinute"]),
                            OTDurationHour = BLHelper.ConvertObjectToInt(row["OTDurationHour"]),
                            OTDurationHourClone = BLHelper.ConvertObjectToInt(row["OTDurationHour"]),
                            Approved = BLHelper.ConvertObjectToBolean(row["Approved"]),
                            AttendanceRemarks = BLHelper.ConvertObjectToString(row["Comment"]),
                            OTReasonCode = BLHelper.ConvertObjectToString(row["OTReasonCode"]),
                            OTReason = BLHelper.ConvertObjectToString(row["OTReason"]),
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            Processed = BLHelper.ConvertObjectToBolean(row["Processed"]),
                            CreatedByEmpNo = BLHelper.ConvertObjectToInt(row["CreatedByEmpNo"]),
                            CreatedByEmpName = BLHelper.ConvertObjectToString(row["CreatedByEmpName"]),
                            CreatedByEmail = BLHelper.ConvertObjectToString(row["CreatedByEmail"]),
                            CreatedDate = BLHelper.ConvertObjectToDate(row["CreatedDate"]),
                            LastUpdateEmpNo = BLHelper.ConvertObjectToInt(row["LastUpdateEmpNo"]),
                            LastUpdateEmpName = BLHelper.ConvertObjectToString(row["LastUpdateEmpName"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUserID"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            OTRequestNo = BLHelper.ConvertObjectToLong(row["OTRequestNo"]),
                            StatusCode = BLHelper.ConvertObjectToString(row["StatusCode"]),
                            StatusDesc = BLHelper.ConvertObjectToString(row["StatusDesc"]),
                            StatusHandlingCode = BLHelper.ConvertObjectToString(row["StatusHandlingCode"]),
                            CurrentlyAssignedEmpNo = BLHelper.ConvertObjectToInt(row["CurrentlyAssignedEmpNo"]),
                            CurrentlyAssignedEmpName = BLHelper.ConvertObjectToString(row["CurrentlyAssignedEmpName"]),
                            ServiceProviderTypeCode = BLHelper.ConvertObjectToString(row["ServiceProviderTypeCode"]),
                            DistListCode = BLHelper.ConvertObjectToString(row["DistListCode"]),
                            DistListDesc = BLHelper.ConvertObjectToString(row["DistListDesc"]),
                            DistListMembers = BLHelper.ConvertObjectToString(row["DistListMembers"]),
                            RequestSubmissionDate = BLHelper.ConvertObjectToDate(row["SubmittedDate"]),
                            IsOTDueToShiftSpan = BLHelper.ConvertObjectToBolean(row["IsOTDueToShiftSpan"]),
                            IsArrivedEarly = BLHelper.ConvertObjectToBolean(row["IsArrivedEarly"]),
                            ArrivalSchedule = BLHelper.ConvertObjectToString(row["ArrivalSchedule"]),
                            TotalWorkDuration = BLHelper.ConvertObjectToInt(row["TotalWorkDuration"]).ToString("0000").Insert(2, ":"),
                            RequiredWorkDuration = BLHelper.ConvertObjectToInt(row["RequiredWorkDuration"]).ToString("0000").Insert(2, ":"),
                            IsOTExceedOrig = BLHelper.ConvertNumberToBolean(row["IsOTExceedOrig"]),
                            IsOTRamadanExceedLimit = BLHelper.ConvertNumberToBolean(row["IsOTRamadanExceedLimit"])
                        };

                        // Set the Cost Center Fullname
                        if (newItem.CostCenter != string.Empty &&
                            newItem.CostCenterName != string.Empty)
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                            newItem.CostCenterFullName = newItem.CostCenterName;

                        // Set the Employee fullname
                        if (newItem.EmpNo > 0 &&
                            newItem.EmpName != string.Empty)
                        {
                            newItem.EmpFullName = string.Format("({0}) {1}",
                                newItem.EmpNo,
                                newItem.EmpName);
                        }
                        else
                            newItem.EmpFullName = newItem.EmpName;

                        // Set the Currently Assigned Fullname
                        if (newItem.CurrentlyAssignedEmpNo > 0)
                        {
                            newItem.CurrentlyAssignedFullName = string.Format("({0}) {1}",
                                newItem.CurrentlyAssignedEmpNo,
                                newItem.CurrentlyAssignedEmpName);
                        }
                        else
                            newItem.CurrentlyAssignedFullName = newItem.DistListMembers;

                        // Set the Created By Fullname
                        if (newItem.CreatedByEmpNo > 0 &&
                            newItem.CreatedByEmpName != string.Empty)
                        {
                            newItem.CreatedByFullName = string.Format("({0}) {1}",
                                newItem.CreatedByEmpNo,
                                newItem.CreatedByEmpName);
                        }
                        else
                            newItem.CreatedByFullName = newItem.CreatedByEmpName;

                        // Set the Last Update Fullname
                        if (newItem.LastUpdateEmpNo > 0 &&
                            newItem.LastUpdateEmpName != string.Empty)
                        {
                            newItem.LastUpdateFullName = string.Format("({0}) {1}",
                                newItem.LastUpdateEmpNo,
                                newItem.LastUpdateEmpName);
                        }
                        else
                            newItem.LastUpdateFullName = newItem.LastUpdateEmpName;

                        if (newItem.OTDurationHour > 0)
                        {
                            newItem.OTDurationText = newItem.OTDurationHour.ToString("0000").Insert(2, ":");                            
                        }

                        #region Process "OT Approved?"
                        newItem.OTApprovalCode = BLHelper.ConvertObjectToString(row["OTApproved"]);
                        if (newItem.OTApprovalCode == "Y")
                        {
                            newItem.OTApprovalDesc = "Yes";
                            newItem.IsOTAlreadyProcessed = true;
                        }
                        else if (newItem.OTApprovalCode == "N")
                        {
                            newItem.OTApprovalDesc = "No";
                            newItem.IsOTAlreadyProcessed = true;
                        }
                        else
                            newItem.OTApprovalDesc = "-";
                        #endregion

                        #region Process "Meal Voucher Approved?"
                        newItem.MealVoucherEligibilityCode = BLHelper.ConvertObjectToString(row["MealVoucherEligibility"]);
                        if (newItem.MealVoucherEligibilityCode == "YA")
                            newItem.MealVoucherEligibility = "Yes";
                        else if (newItem.MealVoucherEligibilityCode == "N")
                            newItem.MealVoucherEligibility = "No";
                        else
                            newItem.MealVoucherEligibility = "-";
                        #endregion

                        // Add item to the collection
                        overtimeData.Add(newItem);
                    }
                    #endregion
                }
                #endregion

                return overtimeData;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<CostCenterEntity> GetCostCenterOTAllowed(int userEmpNo, ref string error, ref string innerError)
        {
            List<CostCenterEntity> result = new List<CostCenterEntity>();

            try
            {
                var rawData = TASContext.GetCostCenterOTAllowed(userEmpNo);
                if (rawData != null)
                {
                    foreach (var item in rawData)
                    {
                        CostCenterEntity newItem = new CostCenterEntity()
                        {
                            CompanyCode = BLHelper.ConvertObjectToString(item.CompanyCode),
                            CostCenter = BLHelper.ConvertObjectToString(item.BusinessUnit),
                            CostCenterName = BLHelper.ConvertObjectToString(item.BusinessUnitName),
                            ParentCostCenter = BLHelper.ConvertObjectToString(item.ParentBU),
                            SuperintendentEmpNo = BLHelper.ConvertObjectToInt(item.Superintendent),
                            ManagerEmpNo = BLHelper.ConvertObjectToInt(item.CostCenterManager)
                        };

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                            !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                        {
                            newItem.CostCenterFullName = newItem.CostCenterName;
                        }

                        // Add item to the collection
                        result.Add(newItem);
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void InsertSystemErrorLog(byte actionType, int logID, long requisitionNo, byte errorCode, string errorDesc, int userEmpNo, string userID)
        {
            try
            {
                #region Insert record using Entity Framework                                
                //SystemErrorLog newErrorLog = new SystemErrorLog()
                //{
                //    ErrorCode = errorCode,
                //    ErrorDscription = errorDesc,
                //    CreatedDate = DateTime.Now,
                //    CreatedByEmpNo = userEmpNo,
                //    CreatedByUserID = userID,
                //    RequisitionNo = requisitionNo
                //};

                //// Commit changes in the database
                //TASContext.SystemErrorLogs.Add(newErrorLog);
                //TASContext.SaveChanges();
                #endregion

                #region Insert log record using ADO.Net
                // Initialize parameters
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[7];

                parameters[0] = new ADONetParameter("@actionType", SqlDbType.TinyInt, actionType);
                parameters[1] = new ADONetParameter("@logID", SqlDbType.Int, logID);
                parameters[2] = new ADONetParameter("@requisitionNo", SqlDbType.BigInt, requisitionNo);
                parameters[3] = new ADONetParameter("@errorCode", SqlDbType.TinyInt, errorCode);
                parameters[4] = new ADONetParameter("@errorDscription", SqlDbType.VarChar, 2000, errorDesc);
                parameters[5] = new ADONetParameter("@userEmpNo", SqlDbType.Int, userEmpNo);
                parameters[6] = new ADONetParameter("@userID", SqlDbType.VarChar, 50, userID);

                DataSet ds = RunSPReturnDataset("tas.Pr_SystemErrorLog_CRUD", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    DatabaseSaveResult dbResult = new DatabaseSaveResult()
                    {
                        NewIdentityID = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["NewIdentityID"]),
                        RowsAffected = BLHelper.ConvertObjectToInt(ds.Tables[0].Rows[0]["RowsAffected"])
                    };
                }
                #endregion
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

        public bool? CheckIfHRApprover(int empNo, ref string error, ref string innerError)
        {
            try
            {
                bool? isHRApprover = TASContext.IsHROTApprover(empNo).FirstOrDefault();
                
                return isHRApprover;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void HoldOvertimeRequest(List<EmployeeAttendanceEntity> otRequisitionList, ref string error, ref string innerError)
        {
            try
            {
                foreach (EmployeeAttendanceEntity item in otRequisitionList)
                {
                    OvertimeRequest recordToUpdate = TASContext.OvertimeRequests
                        .Where(a => a.OTRequestNo == item.OTRequestNo)
                        .FirstOrDefault();
                    if (recordToUpdate != null)
                    {
                        recordToUpdate.IsHold = true;

                        // Save to database
                        TASContext.SaveChanges();
                    }
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public bool CheckIfPayrollProcessing(ref string error, ref string innerError)
        {
            try
            {
                bool isPayrollProcessing = BLHelper.ConvertObjectToBolean(TASContext.CheckIfPayrollProcessing().FirstOrDefault());

                return isPayrollProcessing;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();

                return false;
            }
        }

        public bool CheckIfPayrollProcessingByEmpNo(int empNo, ref string error, ref string innerError)
        {
            try
            {
                bool isPayrollProcessing = BLHelper.ConvertObjectToBolean(TASContext.CheckIfPayrollProcessingByEmpNo(empNo).FirstOrDefault());

                return isPayrollProcessing;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();

                return false;
            }
        }

        public List<FireTeamMember> GetFireTeamAndFireWatchWithPaging(byte loadType, DateTime processDate, string shiftCodeArray, int empNo, string costCenter, int pageNumber, int pageSize,
            string imageRootPath, ref string error, ref string innerError)
        {
            List<FireTeamMember> result = null;
            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[6];
                parameters[0] = new ADONetParameter("@loadType", SqlDbType.TinyInt, loadType);
                parameters[1] = new ADONetParameter("@processDate", SqlDbType.DateTime, processDate);
                parameters[2] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[3] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[4] = new ADONetParameter("@pageNumber", SqlDbType.Int, pageNumber);
                parameters[5] = new ADONetParameter("@pageSize", SqlDbType.Int, pageSize);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetFireTeamAndFireWatch", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    List<FireTeamMember> fireTeamList = BuildFireTeamFireWatchCollection(ds.Tables[0], imageRootPath);
                    if (loadType == 1 ||    // Currently Available Fire Team 
                        loadType == 2 ||    // Currently Available Fire Watch
                        loadType == 3)      // Currently Available Fire Team & Fire Watch
                    {
                        if (fireTeamList != null && fireTeamList.Count > 0)
                        {
                            result = fireTeamList.Where(a => a.IsPresent == true).ToList();
                        }
                    }
                    else
                        result = fireTeamList;
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<FireTeamMember> GetFireTeamAndFireWatch(byte actionType, DateTime processDate, string shiftCode, int empNo, string costCenter, string imageRootPath, ref string error, ref string innerError)
        {
            List<FireTeamMember> fireTeamList = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;
                byte loadType = 0;

                if (actionType == 4)    // All Fire Team Members
                    loadType = 1;
                else if (actionType == 5)   // All Fire Watch Members
                    loadType = 2;
                else if (actionType == 6)   // All Fire Team and Fire Watch Members
                    loadType = 3;
                else
                    loadType = actionType;

                ADONetParameter[] parameters = new ADONetParameter[4];
                parameters[0] = new ADONetParameter("@loadType", SqlDbType.TinyInt, loadType);
                parameters[1] = new ADONetParameter("@processDate", SqlDbType.DateTime, processDate);
                parameters[2] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[3] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetFireTeamAttendance_V2", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    List<FireTeamMember> processedfireTeamList = BuildFireTeamFireWatchCollection(ds.Tables[0], imageRootPath);
                    if (actionType == 1 ||    // Currently Available Fire Team 
                        actionType == 2 ||    // Currently Available Fire Watch
                        actionType == 3)      // Currently Available Fire Team & Fire Watch
                    {
                        if (processedfireTeamList != null && processedfireTeamList.Count > 0)
                        {
                            fireTeamList = processedfireTeamList.Where(a => a.IsPresent == true).ToList();
                        }
                    }
                    else
                        fireTeamList = processedfireTeamList;

                    if (!string.IsNullOrEmpty(shiftCode) &&
                        shiftCode != "valAll")
                    {
                        #region Filter by Shift Code 
                        if (shiftCode == "valDayShift")
                            fireTeamList = fireTeamList.Where(a => a.ShiftCode == "D").ToList();
                        else if (shiftCode == "valMorningShift")
                            fireTeamList = fireTeamList.Where(a => a.ShiftCode == "M").ToList();
                        else if (shiftCode == "valEveningShift")
                            fireTeamList = fireTeamList.Where(a => a.ShiftCode == "E").ToList();
                        else if (shiftCode == "valNightShift")
                            fireTeamList = fireTeamList.Where(a => a.ShiftCode == "N").ToList();
                        else if (shiftCode == "valDayOff")
                            fireTeamList = fireTeamList.Where(a => a.ShiftCode == "O").ToList();                        
                        #endregion
                    }
                }

                return fireTeamList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<PunctualityEntity> GetUnpunctualEmployeeSummary(DateTime? startDate, DateTime? endDate, string costCenter, int empNo, ref string error, ref string innerError)
        {
            List<PunctualityEntity> reportDataList = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[4];
                parameters[0] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[1] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[2] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[3] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);

                DataSet ds = RunSPReturnDataset("tas.Pr_EmpPunctualityByPeriod", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize the collection
                    reportDataList = new List<PunctualityEntity>();

                    #region Build the datasource for the report
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        PunctualityEntity reportItem = new PunctualityEntity()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["CostCenterName"]),
                            CostCenterFullName = string.Format("{0} - {1}",
                                                BLHelper.ConvertObjectToString(row["CostCenter"]),
                                                BLHelper.ConvertObjectToString(row["CostCenterName"])),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                            ActualShiftCode = BLHelper.ConvertObjectToString(row["Actual_ShiftCode"]),
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIN"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOUT"]),
                            Remarks = BLHelper.ConvertObjectToString(row["Remarks"]),
                            TotalLostTime = BLHelper.ConvertObjectToInt(row["TotalLostTime"]),
                            ReportOccurenceCount = BLHelper.ConvertObjectToInt(row["ReportOccurenceCount"]),
                            PunctualityTypeID = BLHelper.ConvertObjectToByte(row["PunctualityTypeID"]),
                            ArrivalTimeDiff = BLHelper.ConvertObjectToInt(row["ArrivalTimeDiff"]),
                            DepartureTimeDiff = BLHelper.ConvertObjectToInt(row["DepartureTimeDiff"])
                        };

                        #region Calculate the total time lost
                        if (reportItem.PunctualityTypeID == 1)          // Late
                        {
                            reportItem.TotalLostTime = reportItem.ArrivalTimeDiff;
                        }
                        else if (reportItem.PunctualityTypeID == 2)     // Left early
                        {
                            reportItem.TotalLostTime = reportItem.DepartureTimeDiff;
                        }
                        //else if (reportItem.PunctualityTypeID == 3)     // Late and left early
                        //{
                        //    reportItem.TotalLostTime = reportItem.ArrivalTimeDiff + reportItem.DepartureTimeDiff;
                        //}
                        #endregion

                        #region Determine whether the Time-in or Time-out is unpunctual
                        if (reportItem.PunctualityTypeID == 1)       // Late 
                        {
                            reportItem.TimeInUnpunctual = true;
                        }
                        else if (reportItem.PunctualityTypeID == 2)  // Left early
                        {
                            reportItem.TimeOutUnpunctual = true;
                        }
                        else if (reportItem.PunctualityTypeID == 3) // Late & left early
                        {
                            reportItem.TimeInUnpunctual = true;
                            reportItem.TimeOutUnpunctual = true;
                        }
                        #endregion

                        // Add item to the collection
                        reportDataList.Add(reportItem);
                    }
                    #endregion
                }

                return reportDataList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<OvertimeBudgetEntity> GetOvertimeBudgetStatistics(byte loadType, int fiscalYear, string costCenter, ref string error, ref string innerError)
        {
            List<OvertimeBudgetEntity> otBudgetDataList = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[3];
                parameters[0] = new ADONetParameter("@loadType", SqlDbType.TinyInt, loadType);
                parameters[1] = new ADONetParameter("@fiscalYear", SqlDbType.Int, fiscalYear);
                parameters[2] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 200, costCenter);

                DataSet ds = RunSPReturnDataset("tas.Pr_OvertimeBudgetStatistics", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize the collection
                    otBudgetDataList = new List<OvertimeBudgetEntity>();

                    #region Populate items to the collection
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        OvertimeBudgetEntity budgetItem = new OvertimeBudgetEntity();

                        budgetItem.FiscalYear = BLHelper.ConvertObjectToInt(row["FiscalYear"]);
                        budgetItem.FiscalYearDesc = BLHelper.ConvertObjectToString(row["FiscalYear"]);

                        #region Populate OT Budget                                                
                        if (ds.Tables[0].Columns.Contains("JanBudget"))
                            budgetItem.JanBudget = BLHelper.ConvertObjectToDecimal(row["JanBudget"]);

                        if (ds.Tables[0].Columns.Contains("FebBudget"))
                            budgetItem.FebBudget = BLHelper.ConvertObjectToDecimal(row["FebBudget"]);

                        if (ds.Tables[0].Columns.Contains("MarBudget"))
                            budgetItem.MarBudget = BLHelper.ConvertObjectToDecimal(row["MarBudget"]);

                        if (ds.Tables[0].Columns.Contains("AprBudget"))
                            budgetItem.AprBudget = BLHelper.ConvertObjectToDecimal(row["AprBudget"]);

                        if (ds.Tables[0].Columns.Contains("MayBudget"))
                            budgetItem.MayBudget = BLHelper.ConvertObjectToDecimal(row["MayBudget"]);

                        if (ds.Tables[0].Columns.Contains("JunBudget"))
                            budgetItem.JunBudget = BLHelper.ConvertObjectToDecimal(row["JunBudget"]);

                        if (ds.Tables[0].Columns.Contains("JulBudget"))
                            budgetItem.JulBudget = BLHelper.ConvertObjectToDecimal(row["JulBudget"]);

                        if (ds.Tables[0].Columns.Contains("AugBudget"))
                            budgetItem.AugBudget = BLHelper.ConvertObjectToDecimal(row["AugBudget"]);

                        if (ds.Tables[0].Columns.Contains("SepBudget"))
                            budgetItem.SepBudget = BLHelper.ConvertObjectToDecimal(row["SepBudget"]);

                        if (ds.Tables[0].Columns.Contains("OctBudget"))
                            budgetItem.OctBudget = BLHelper.ConvertObjectToDecimal(row["OctBudget"]);

                        if (ds.Tables[0].Columns.Contains("NovBudget"))
                            budgetItem.NovBudget = BLHelper.ConvertObjectToDecimal(row["NovBudget"]);

                        if (ds.Tables[0].Columns.Contains("DecBudget"))
                            budgetItem.DecBudget = BLHelper.ConvertObjectToDecimal(row["DecBudget"]);
                        #endregion

                        #region Populate OT Actuals                                                
                        if (ds.Tables[0].Columns.Contains("JanActual"))
                            budgetItem.JanActual = BLHelper.ConvertObjectToDecimal(row["JanActual"]);

                        if (ds.Tables[0].Columns.Contains("FebActual"))
                            budgetItem.FebActual = BLHelper.ConvertObjectToDecimal(row["FebActual"]);

                        if (ds.Tables[0].Columns.Contains("MarActual"))
                            budgetItem.MarActual = BLHelper.ConvertObjectToDecimal(row["MarActual"]);

                        if (ds.Tables[0].Columns.Contains("AprActual"))
                            budgetItem.AprActual = BLHelper.ConvertObjectToDecimal(row["AprActual"]);

                        if (ds.Tables[0].Columns.Contains("MayActual"))
                            budgetItem.MayActual = BLHelper.ConvertObjectToDecimal(row["MayActual"]);

                        if (ds.Tables[0].Columns.Contains("JunActual"))
                            budgetItem.JunActual = BLHelper.ConvertObjectToDecimal(row["JunActual"]);

                        if (ds.Tables[0].Columns.Contains("JulActual"))
                            budgetItem.JulActual = BLHelper.ConvertObjectToDecimal(row["JulActual"]);

                        if (ds.Tables[0].Columns.Contains("AugActual"))
                            budgetItem.AugActual = BLHelper.ConvertObjectToDecimal(row["AugActual"]);

                        if (ds.Tables[0].Columns.Contains("SepActual"))
                            budgetItem.SepActual = BLHelper.ConvertObjectToDecimal(row["SepActual"]);

                        if (ds.Tables[0].Columns.Contains("OctActual"))
                            budgetItem.OctActual = BLHelper.ConvertObjectToDecimal(row["OctActual"]);

                        if (ds.Tables[0].Columns.Contains("NovActual"))
                            budgetItem.NovActual = BLHelper.ConvertObjectToDecimal(row["NovActual"]);

                        if (ds.Tables[0].Columns.Contains("DecActual"))
                            budgetItem.DecActual = BLHelper.ConvertObjectToDecimal(row["DecActual"]);
                        #endregion

                        if (ds.Tables[0].Columns.Contains("TotalBudgetAmount"))
                            budgetItem.TotalBudgetAmount = BLHelper.ConvertObjectToDecimal(row["TotalBudgetAmount"]);

                        if (ds.Tables[0].Columns.Contains("TotalActualAmount"))
                            budgetItem.TotalActualAmount = BLHelper.ConvertObjectToDecimal(row["TotalActualAmount"]);

                        if (ds.Tables[0].Columns.Contains("TotalBalanceAmount"))
                            budgetItem.TotalBalanceAmount = BLHelper.ConvertObjectToDecimal(row["TotalBalanceAmount"]);

                        if (ds.Tables[0].Columns.Contains("CostCenter"))
                            budgetItem.CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]);

                        if (ds.Tables[0].Columns.Contains("TotalBudgetHour"))
                            budgetItem.TotalBudgetHour = BLHelper.ConvertObjectToDecimal(row["TotalBudgetHour"]);

                        if (ds.Tables[0].Columns.Contains("TotalActualHour"))
                            budgetItem.TotalActualHour = BLHelper.ConvertObjectToDecimal(row["TotalActualHour"]);

                        if (ds.Tables[0].Columns.Contains("TotalBalanceHour"))
                            budgetItem.TotalBalanceHour = BLHelper.ConvertObjectToDecimal(row["TotalBalanceHour"]);

                        // Add item to the collection
                        otBudgetDataList.Add(budgetItem);
                    }
                    #endregion
                }

                return otBudgetDataList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeDetail> GetWorkingCostCenterHistory(int empNo, ref string error, ref string innerError)
        {
            List<EmployeeDetail> historyList = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[1];
                parameters[0] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetWorkingCostCenterHistory", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize the collection
                    historyList = new List<EmployeeDetail>();

                    #region Populate items to the collection
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeDetail collectionItem = new EmployeeDetail()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            //ShiftPointer = BLHelper.ConvertObjectToInt(row["ShiftPointer"]),
                            WorkingCostCenter = BLHelper.ConvertObjectToString(row["WorkingBusinessUnit"]),
                            WorkingCostCenterName = BLHelper.ConvertObjectToString(row["WorkingBusinessUnitName"]),
                            WorkingCostCenterFullName = string.Format("{0} - {1}",
                               BLHelper.ConvertObjectToString(row["WorkingBusinessUnit"]),
                               BLHelper.ConvertObjectToString(row["WorkingBusinessUnitName"])),
                            SpecialJobCatg = BLHelper.ConvertObjectToString(row["SpecialJobCatg"]),
                            SpecialJobCatgDesc = BLHelper.ConvertObjectToString(row["SpecialJobCatgDesc"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"]),
                            CatgEffectiveDate = BLHelper.ConvertObjectToDate(row["CatgEffectiveDate"]),
                            CatgEndingDate = BLHelper.ConvertObjectToDate(row["CatgEndingDate"])
                        };

                        // Add item to the collection
                        historyList.Add(collectionItem);
                    }
                    #endregion
                }

                return historyList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }                

        public List<UserFormAccessEntity> GetCommonAdminComboData(byte loadType, string appCode, string formCode, ref string error, ref string innerError)
        {
            List<UserFormAccessEntity> comboDataList = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[3];
                parameters[0] = new ADONetParameter("@loadType", SqlDbType.TinyInt, loadType);
                parameters[1] = new ADONetParameter("@appCode", SqlDbType.VarChar, 10, appCode);
                parameters[2] = new ADONetParameter("@formCode", SqlDbType.VarChar, 10, formCode);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetCommonAdminCombo", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize the collection
                    comboDataList = new List<UserFormAccessEntity>();

                    #region Populate items to the collection
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        UserFormAccessEntity collectionItem = new UserFormAccessEntity();

                        if (ds.Tables[0].Columns.Contains("ApplicationID"))
                            collectionItem.ApplicationID = BLHelper.ConvertObjectToInt(row["ApplicationID"]);

                        if (ds.Tables[0].Columns.Contains("ApplicationCode"))
                            collectionItem.ApplicationCode = BLHelper.ConvertObjectToString(row["ApplicationCode"]);

                        if (ds.Tables[0].Columns.Contains("ApplicationName"))
                            collectionItem.ApplicationName = BLHelper.ConvertObjectToString(row["ApplicationName"]);

                        if (ds.Tables[0].Columns.Contains("FormAppID"))
                            collectionItem.FormAppID = BLHelper.ConvertObjectToInt(row["FormAppID"]);

                        if (ds.Tables[0].Columns.Contains("FormCode"))
                            collectionItem.FormCode = BLHelper.ConvertObjectToString(row["FormCode"]);

                        if (ds.Tables[0].Columns.Contains("FormName"))
                            collectionItem.FormName = BLHelper.ConvertObjectToString(row["FormName"]);

                        // Add item to the collection
                        comboDataList.Add(collectionItem);
                    }
                    #endregion
                }

                return comboDataList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }
       
        public List<RoutineHistoryEntity> GetRoutineHistory(long otRequestNo, int tsAutoID, DateTime? reqSubmissionDate, ref string error, ref string innerError)
        {
            List<RoutineHistoryEntity> routineHistoryList = null;

            try
            {
                var rawData = from a in TASContext.OvertimeWFRoutineHistories
                              where a.OTRequestNo == otRequestNo && a.TS_AutoID == tsAutoID &&
                                (a.RequestSubmissionDate == reqSubmissionDate || reqSubmissionDate == null)
                              select a;
                if (rawData != null)
                {
                    // Initialize the collection
                    routineHistoryList = new List<RoutineHistoryEntity>();

                    #region Populate the collection      
                    foreach (var item in rawData)
                    {
                        routineHistoryList.Add(new RoutineHistoryEntity()
                        {
                            AutoID = item.AutoID,
                            OTRequestNo = item.OTRequestNo,
                            TS_AutoID = item.TS_AutoID,
                            RequestSubmissionDate = item.RequestSubmissionDate,
                            HistDescription = BLHelper.ConvertObjectToString(item.HistDesc),
                            HistCreatedBy = item.HistCreatedBy,
                            HistCreatedName = BLHelper.ConvertObjectToString(item.HistCreatedName).ToUpper(),
                            HistCreatedFullName = item.HistCreatedBy > 0
                                ? string.Format("({0}) {1}", item.HistCreatedBy, BLHelper.ConvertObjectToString(item.HistCreatedName).ToUpper())
                                : BLHelper.ConvertObjectToString(item.HistCreatedName),
                            HistCreatedDate = item.HistCreatedDate
                        });
                    }
                    #endregion
                }

                return routineHistoryList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<ApprovalEntity> GetApprovalHistory(long otRequestNo, int tsAutoID, DateTime? reqSubmissionDate, ref string error, ref string innerError)
        {
            List<ApprovalEntity> approvalHistoryList = null;

            try
            {
                var rawData = TASContext.GetOTApprovalHistory(otRequestNo, tsAutoID, reqSubmissionDate);
                if (rawData != null)
                {
                    // Initialize the collection
                    approvalHistoryList = new List<ApprovalEntity>();

                    #region Populate the collection      
                    foreach (var item in rawData)
                    {
                        approvalHistoryList.Add(new ApprovalEntity()
                        {
                            AutoID = item.AutoID,
                            OTRequestNo = item.OTRequestNo,
                            TS_AutoID = item.TS_AutoID,
                            RequestSubmissionDate = item.RequestSubmissionDate,
                            AppApproved = item.AppApproved,
                            AppRemarks = BLHelper.ConvertObjectToString(item.AppRemarks),
                            AppRoutineSeq = item.AppRoutineSeq,
                            AppCreatedBy = item.AppCreatedBy,
                            AppCreatedName = BLHelper.ConvertObjectToString(item.AppCreatedName),
                            ApproverPosition = BLHelper.ConvertObjectToString(item.AppCreatedPosition),
                            AppCreatedFullName = string.Format("({0}) {1}",
                                item.AppCreatedBy,
                                BLHelper.ConvertObjectToString(item.AppCreatedName)),
                            AppCreatedDate = item.AppCreatedDate,
                            AppModifiedBy = item.AppModifiedBy,
                            AppModifiedName = BLHelper.ConvertObjectToString(item.AppModifiedName),
                            AppModifiedDate = item.AppModifiedDate,
                            ApprovalRole = BLHelper.ConvertObjectToString(item.ApprovalRole),
                        });
                    }
                    #endregion
                }

                return approvalHistoryList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<WFTransActivityEntity> GetWorkflowHistory(long otRequestNo, int tsAutoID, DateTime? reqSubmissionDate, ref string error, ref string innerError)
        {
            List<WFTransActivityEntity> workflowHistoryList = null;

            try
            {
                var rawData = from a in TASContext.OvertimeWFTransactionActivities
                              where a.OTRequestNo == otRequestNo && a.TS_AutoID == tsAutoID &&
                                (a.RequestSubmissionDate == reqSubmissionDate || reqSubmissionDate == null)
                              select a;
                if (rawData != null)
                {
                    // Initialize the collection
                    workflowHistoryList = new List<WFTransActivityEntity>();

                    #region Populate the collection                                        
                    foreach (var item in rawData)
                    {
                        WFTransActivityEntity newItem = new WFTransActivityEntity()
                        {
                            WorkflowTransactionID = item.WorkflowTransactionID,
                            OTRequestNo = item.OTRequestNo,
                            TS_AutoID = item.TS_AutoID,
                            WFModuleCode = BLHelper.ConvertObjectToString(item.WFModuleCode),
                            ActivityCode = BLHelper.ConvertObjectToString(item.ActivityCode),
                            NextActivityCode = BLHelper.ConvertObjectToString(item.NextActivityCode),
                            ActivityDesc1 = BLHelper.ConvertObjectToString(item.ActivityDesc1),
                            ActivityDesc2 = BLHelper.ConvertObjectToString(item.ActivityDesc2),
                            ActivityTypeCode = BLHelper.ConvertObjectToString(item.WFActivityTypeCode),
                            ActivityTypeDesc = string.Format("{0} Activity", BLHelper.ConvertStringToTitleCase(item.WFActivityTypeCode)),
                            SequenceNo = BLHelper.ConvertObjectToInt(item.SequenceNo),
                            SequenceType = BLHelper.ConvertObjectToInt(item.SequenceType),
                            ApprovalType = BLHelper.ConvertObjectToInt(item.ApprovalType),
                            ActionRole = BLHelper.ConvertObjectToInt(item.ActionRole),
                            ActionMemberCode = BLHelper.ConvertObjectToString(item.ActionMemberCode),
                            ParameterSourceTable = BLHelper.ConvertObjectToString(item.ParameterSourceTable),
                            ParameterName = BLHelper.ConvertObjectToString(item.ParameterName),
                            ParameterDataType = BLHelper.ConvertObjectToString(item.ParameterDataType),
                            ConditionCheckValue = BLHelper.ConvertObjectToString(item.ConditionCheckValue),
                            ConditionCheckDataType = BLHelper.ConvertObjectToString(item.ConditionCheckDataType),
                            EmailSourceName = BLHelper.ConvertObjectToString(item.EmailSourceName),
                            EmailCCRecipient = BLHelper.ConvertObjectToString(item.EmailCCRecipient),
                            EmailCCRecipientType = BLHelper.ConvertObjectToString(item.EmailCCRecipientType),
                            IsCurrent = BLHelper.ConvertObjectToBolean(item.IsCurrent),
                            IsCompleted = BLHelper.ConvertObjectToBolean(item.IsCompleted),
                            IsFinalAct = BLHelper.ConvertObjectToBolean(item.IsFinalAct),
                            ActStatusID = BLHelper.ConvertObjectToInt(item.ActStatusID),
                            RequestSubmissionDate = BLHelper.ConvertObjectToDate(item.RequestSubmissionDate),
                            CreatedByUser = BLHelper.ConvertObjectToString(item.CreatedByUser),
                            CreatedDate = BLHelper.ConvertObjectToDate(item.CreatedDate),
                            CreatedByUserEmpNo = BLHelper.ConvertObjectToInt(item.CreatedByUserEmpNo),
                            CreatedByUserEmpName = BLHelper.ConvertObjectToString(item.CreatedByUserEmpName),
                            LastUpdateUser = BLHelper.ConvertObjectToString(item.LastUpdateUser),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(item.LastUpdateTime),
                            LastUpdateEmpNo = BLHelper.ConvertObjectToInt(item.LastUpdateEmpNo),
                            LastUpdateEmpName = BLHelper.ConvertObjectToString(item.LastUpdateEmpName)
                        };

                        // Set the completion date
                        if (item.ActStatusID == Convert.ToInt32(BLHelper.WorkflowProgressStatuses.Completed))
                            newItem.CompletionDate = item.LastUpdateTime;

                        // Set status description
                        if (item.ActStatusID == Convert.ToInt32(BLHelper.WorkflowProgressStatuses.Completed))
                            newItem.StatusDesc = "Completed";
                        else if (item.ActStatusID == Convert.ToInt32(BLHelper.WorkflowProgressStatuses.InProgress))
                            newItem.StatusDesc = "In progress";
                        else if (item.ActStatusID == Convert.ToInt32(BLHelper.WorkflowProgressStatuses.Pending))
                            newItem.StatusDesc = "Pending";
                        else if (item.ActStatusID == Convert.ToInt32(BLHelper.WorkflowProgressStatuses.ByPassed))
                            newItem.StatusDesc = "Skipped";

                        // Add to collection
                        workflowHistoryList.Add(newItem);
                    }
                    #endregion
                }
                return workflowHistoryList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeDetail> GetRequestApprovers(long otRequestNo, int tsAutoID, DateTime? reqSubmissionDate, ref string error, ref string innerError)
        {
            List<EmployeeDetail> approverList = null;

            try
            {
                var rawData = TASContext.GetRequisitionApprover(otRequestNo, tsAutoID, reqSubmissionDate);
                if (rawData != null)
                {
                    // Initialize the collection
                    approverList = new List<EmployeeDetail>();

                    #region Populate the collection      
                    foreach (var item in rawData)
                    {
                        approverList.Add(new EmployeeDetail()
                        {
                            EmpNo = item.ApprovedByEmpNo,
                            EmpName = BLHelper.ConvertObjectToString(item.ApprovedByEmpName),
                            EmpEmail = BLHelper.ConvertObjectToString(item.ApprovedByEmpEmail)
                        });
                    }
                    #endregion
                }

                return approverList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public bool? IsOTBudgetAdmin(int empNo, ref string error, ref string innerError)
        {
            try
            {
                bool? isOTAdmin = TASContext.IsOTBudgetAdmin(empNo).FirstOrDefault();

                return isOTAdmin;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<string> GetAllocatedCostCenter(int empNo, ref string error, ref string innerError)
        {
            List<string> costCenterList = null;

            try
            {
                var rawData = TASContext.GetAssignedCostCenter(empNo);
                if (rawData != null)
                {
                    // Initialize the collection
                    costCenterList = new List<string>();

                    foreach (var item in rawData)
                    {
                        costCenterList.Add(item.CostCenter);
                    }
                }

                return costCenterList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<UserFormAccessEntity> GetUserFormAccess(string appCode, int empNo, string formCode, ref string error, ref string innerError)
        {
            List<UserFormAccessEntity> result = new List<UserFormAccessEntity>();

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[3];
                parameters[0] = new ADONetParameter("@appCode", SqlDbType.VarChar, 10, appCode);
                parameters[1] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[2] = new ADONetParameter("@formCode", SqlDbType.VarChar, 10, formCode);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetUserFormAccessInfo", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    #region Populate items to the collection
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        UserFormAccessEntity newItem = new UserFormAccessEntity()
                        {
                            ApplicationName = BLHelper.ConvertObjectToString(row["ApplicationName"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["CostCenter"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            FormCode = BLHelper.ConvertObjectToString(row["FormCode"]),
                            FormName = BLHelper.ConvertObjectToString(row["FormName"]),
                            FormPublic = BLHelper.ConvertObjectToBolean(row["FormPublic"]),
                            UserFrmCRUDP = BLHelper.ConvertObjectToString(row["UserFrmCRUDP"]),
                            UserFrmFormCode = BLHelper.ConvertObjectToString(row["UserFrmFormCode"]),
                            CreatedByEmpNo = BLHelper.ConvertObjectToInt(row["CreatedByEmpNo"]),
                            CreatedByEmpName = BLHelper.ConvertObjectToString(row["CreatedByEmpName"]),
                            CreatedDate = BLHelper.ConvertObjectToDate(row["CreatedDate"]),
                            LastUpdatedByEmpNo = BLHelper.ConvertObjectToInt(row["LastUpdatedByEmpNo"]),
                            LastUpdatedByEmpName = BLHelper.ConvertObjectToString(row["LastUpdatedByEmpName"]),
                            LastUpdatedDate = BLHelper.ConvertObjectToDate(row["LastUpdatedDate"])
                        };

                        if (newItem.UserFrmCRUDP != string.Empty)
                        {
                            newItem.HasViewAccess = CheckFormAccess(newItem.UserFrmCRUDP, FormAccessIndex.Retrieve);
                            newItem.HasCreateAccess = CheckFormAccess(newItem.UserFrmCRUDP, FormAccessIndex.Create);
                            newItem.HasUpdateAccess = CheckFormAccess(newItem.UserFrmCRUDP, FormAccessIndex.Update);
                            newItem.HasDeleteAccess = CheckFormAccess(newItem.UserFrmCRUDP, FormAccessIndex.Delete);
                            newItem.HasPrintAccess = CheckFormAccess(newItem.UserFrmCRUDP, FormAccessIndex.Print);
                        }

                        if (newItem.FormPublic)
                        {
                            newItem.ViewAccessEnable = false;
                            newItem.CreateAccessEnable = false;
                            newItem.UpdateAccessEnable = false;
                            newItem.DeleteAccessEnable = false;
                            newItem.PrintAccessEnable = false;
                        }
                        else
                        {
                            if (!newItem.HasViewAccess)
                            {
                                newItem.ViewAccessEnable = true;
                                newItem.CreateAccessEnable = false;
                                newItem.UpdateAccessEnable = false;
                                newItem.DeleteAccessEnable = false;
                                newItem.PrintAccessEnable = false;
                            }
                            else
                            {
                                newItem.ViewAccessEnable = true;
                                newItem.CreateAccessEnable = true;
                                newItem.UpdateAccessEnable = true;
                                newItem.DeleteAccessEnable = true;
                                newItem.PrintAccessEnable = true;
                            }
                        }

                        if (newItem.CreatedByEmpNo > 0 && newItem.CreatedByEmpName != string.Empty)
                            newItem.CreatedByFullName = string.Format("({0}) {1}", newItem.CreatedByEmpNo, newItem.CreatedByEmpName);
                        else
                            newItem.CreatedByFullName = newItem.CreatedByEmpName;

                        if (newItem.LastUpdatedByEmpNo > 0 && newItem.LastUpdatedByEmpName != string.Empty)
                            newItem.LastUpdatedByFullName = string.Format("({0}) {1}", newItem.LastUpdatedByEmpNo, newItem.LastUpdatedByEmpName);
                        else
                            newItem.LastUpdatedByFullName = newItem.LastUpdatedByEmpName;

                        // Add item to the collection
                        result.Add(newItem);
                    }
                    #endregion
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void InsertUpdateDeleteUserFormAccess(List<UserFormAccessEntity> dirtyUserAccessList, ref string error, ref string innerError)
        {
            try
            {
                if (dirtyUserAccessList != null)
                {
                    #region Filter records for insert operation                                        
                    List<UserFormAccessEntity> recordToInsertList = dirtyUserAccessList.Where(a => string.IsNullOrEmpty(a.UserFrmFormCode)).ToList();
                    if (recordToInsertList != null &&
                        recordToInsertList.Count > 0)
                    {
                        // Initialize collection
                        List<UserFormAccess> insertList = new List<UserFormAccess>();

                        foreach (UserFormAccessEntity item in recordToInsertList)
                        {
                            insertList.Add(new UserFormAccess()
                            {
                                UserFrmFormCode = item.FormCode,
                                UserFrmEmpNo = item.EmpNo,
                                UserFrmCRUDP = item.UserFrmCRUDP,
                                UserFrmCreatedBy = item.CreatedByEmpNo,
                                UserFrmCreatedDate = item.CreatedDate
                            });
                        }

                        // Commit changes in the database
                        GenPurposeContext.UserFormAccesses.AddRange(insertList);
                        GenPurposeContext.SaveChanges();
                    }
                    #endregion

                    #region Filter records for update operation                                        
                    List<UserFormAccessEntity> recordToUpdateList = dirtyUserAccessList.Where(a => !string.IsNullOrEmpty(a.UserFrmFormCode)).ToList();
                    if (recordToUpdateList != null &&
                        recordToUpdateList.Count > 0)
                    {
                        foreach (UserFormAccessEntity item in recordToUpdateList)
                        {
                            UserFormAccess recordToUpdate = GenPurposeContext.UserFormAccesses
                                  .Where(a => a.UserFrmFormCode.Trim() == item.FormCode.Trim() && a.UserFrmEmpNo == item.EmpNo)
                                 .FirstOrDefault();
                            if (recordToUpdate != null)
                            {
                                recordToUpdate.UserFrmCRUDP = item.UserFrmCRUDP;
                                recordToUpdate.UserFrmModifiedBy = item.LastUpdatedByEmpNo;
                                recordToUpdate.UserFrmModifiedDate = item.LastUpdatedDate;

                                // Save to database
                                GenPurposeContext.SaveChanges();
                            }
                        }
                    }
                    #endregion
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public List<MasterShiftPatternEntity> GetShiftPatternList(ref string error, ref string innerError)
        {
            List<MasterShiftPatternEntity> shiftPatternList = new List<MasterShiftPatternEntity>();

            try
            {
                var rawData = from a in TASContext.Master_ShiftPatternTitles
                            select new
                            {
                                AutoID = a.AutoID,
                                ShiftPatCode = a.ShiftPatCode,
                                ShiftPatDescription = a.ShiftPatDescription,
                                IsDayShift = a.IsDayShift
                            };

                if (rawData != null)
                {
                    foreach (var item in rawData)
                    {
                        MasterShiftPatternEntity newItem = new MasterShiftPatternEntity()
                        {
                            AutoID = item.AutoID,
                            ShiftPatCode = BLHelper.ConvertObjectToString(item.ShiftPatCode),
                            ShiftPatDescription = BLHelper.ConvertObjectToString(item.ShiftPatDescription),
                            IsDayShift = BLHelper.ConvertObjectToBolean(item.IsDayShift)
                        };

                        if (newItem.ShiftPatDescription != string.Empty)
                            newItem.ShiftPatternFullName = string.Format("{0} - {1}", newItem.ShiftPatCode, newItem.ShiftPatDescription);
                        else
                            newItem.ShiftPatternFullName = newItem.ShiftPatCode;

                        // Add item to the collection
                        shiftPatternList.Add(newItem);
                    }
                }

                return shiftPatternList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<MasterShiftPatternEntity> GetShiftCodeList(string shiftCode, ref string error, ref string innerError)
        {
            List<MasterShiftPatternEntity> shiftCodeList = new List<MasterShiftPatternEntity>();

            try
            {
                var rawData = TASContext.GetShiftCode(shiftCode);
                if (rawData != null)
                {
                    // Initialize the collection
                    shiftCodeList = new List<MasterShiftPatternEntity>();

                    foreach (var item in rawData)
                    {
                        MasterShiftPatternEntity newItem = new MasterShiftPatternEntity()
                        {
                            ShiftCode = BLHelper.ConvertObjectToString(item.ShiftCode),
                            ShiftDescription = BLHelper.ConvertObjectToString(item.ShiftDesc),
                            ArrivalFrom = BLHelper.ConvertObjectToDate(item.ArrivalFrom),
                            ArrivalTo = BLHelper.ConvertObjectToDate(item.ArrivalTo),
                            DepartFrom = BLHelper.ConvertObjectToDate(item.DepartFrom),
                            DepartTo = BLHelper.ConvertObjectToDate(item.DepartTo),
                            DurationNormalDay = BLHelper.ConvertObjectToInt(item.DurationNormalDay),
                            RArrivalFrom = BLHelper.ConvertObjectToDate(item.RArrivalFrom),
                            RArrivalTo = BLHelper.ConvertObjectToDate(item.RArrivalTo),
                            RDepartFrom = BLHelper.ConvertObjectToDate(item.RDepartFrom),
                            RDepartTo = BLHelper.ConvertObjectToDate(item.RDepartTo),
                            DurationRamadanDay = BLHelper.ConvertObjectToInt(item.DurationRamadanDay)
                        };

                        if (newItem.ShiftDescription != string.Empty)
                            newItem.ShiftFullDescription = string.Format("{0} - {1}", newItem.ShiftCode, newItem.ShiftDescription);
                        else
                            newItem.ShiftFullDescription = newItem.ShiftCode;

                        // Get the duration 24-hour format
                        if (newItem.DurationNormalDay > 0)
                            newItem.DurationNormalDayString = BLHelper.ConvertMinuteToHourString(newItem.DurationNormalDay);
                        else
                            newItem.DurationNormalDayString = "-";

                        if (newItem.DurationRamadanDay > 0)
                            newItem.DurationRamadanDayString = BLHelper.ConvertMinuteToHourString(newItem.DurationRamadanDay);
                        else
                            newItem.DurationRamadanDayString = "-";

                        // Add item to the collection
                        shiftCodeList.Add(newItem);
                    }
                }

                return shiftCodeList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<MasterShiftPatternEntity> GetShiftPatternDetail(byte loadType, string shiftPatCode, byte isDayShift, byte isFlexitime, ref string error, ref string innerError)
        {
            List<MasterShiftPatternEntity> result = new List<MasterShiftPatternEntity>();

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[4];
                parameters[0] = new ADONetParameter("@loadType", SqlDbType.TinyInt, loadType);
                parameters[1] = new ADONetParameter("@shiftPatCode", SqlDbType.VarChar, 10, shiftPatCode);
                parameters[2] = new ADONetParameter("@isDayShift", SqlDbType.TinyInt, isDayShift);
                parameters[3] = new ADONetParameter("@isFlexitime", SqlDbType.TinyInt, isFlexitime);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetShiftPatternDetail", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    if (loadType == 1)
                    { 
                        #region Populate Shift Timing Schedule
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            MasterShiftPatternEntity newItem = new MasterShiftPatternEntity()
                            {
                                AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                                ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                                ShiftPatDescription = BLHelper.ConvertObjectToString(row["ShiftPatDescription"]),
                                ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                                ShiftDescription = BLHelper.ConvertObjectToString(row["ShiftDescription"]),
                                IsDayShift = BLHelper.ConvertObjectToBolean(row["IsDayShift"]),
                                IsFlexitime = BLHelper.ConvertObjectToBolean(row["IsFlexitime"]),
                                ArrivalFrom = BLHelper.ConvertObjectToDate(row["ArrivalFrom"]),
                                ArrivalTo = BLHelper.ConvertObjectToDate(row["ArrivalTo"]),
                                DepartFrom = BLHelper.ConvertObjectToDate(row["DepartFrom"]),
                                DepartTo = BLHelper.ConvertObjectToDate(row["DepartTo"]),
                                DurationNormalDay = BLHelper.ConvertObjectToInt(row["DurationNormalDay"]),
                                RArrivalFrom = BLHelper.ConvertObjectToDate(row["RArrivalFrom"]),
                                RArrivalTo = BLHelper.ConvertObjectToDate(row["RArrivalTo"]),
                                RDepartFrom = BLHelper.ConvertObjectToDate(row["RDepartFrom"]),
                                RDepartTo = BLHelper.ConvertObjectToDate(row["RDepartTo"]),
                                DurationRamadanDay = BLHelper.ConvertObjectToInt(row["DurationRamadanDay"]),
                                CreatedByEmpNo = BLHelper.ConvertObjectToInt(row["CreatedByEmpNo"]),
                                CreatedByEmpName = BLHelper.ConvertObjectToString(row["CreatedByEmpName"]),
                                CreatedDate = BLHelper.ConvertObjectToDate(row["CreatedDate"]),
                                LastUpdateEmpNo = BLHelper.ConvertObjectToInt(row["LastUpdateEmpNo"]),
                                LastUpdateEmpName = BLHelper.ConvertObjectToString(row["LastUpdateEmpName"]),
                                LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"])
                            };

                            if (newItem.ShiftDescription != string.Empty)
                                newItem.ShiftFullDescription = string.Format("{0} - {1}", newItem.ShiftCode, newItem.ShiftDescription);
                            else
                                newItem.ShiftFullDescription = newItem.ShiftCode;

                            // Get the duration 24-hour format
                            if (newItem.DurationNormalDay > 0)
                                newItem.DurationNormalDayString = BLHelper.ConvertMinuteToHourString(newItem.DurationNormalDay);
                            else
                                newItem.DurationNormalDayString = "-";

                            if (newItem.DurationRamadanDay > 0)
                                newItem.DurationRamadanDayString = BLHelper.ConvertMinuteToHourString(newItem.DurationRamadanDay);
                            else
                                newItem.DurationRamadanDayString = "-";

                            if (newItem.CreatedByEmpNo > 0 && newItem.CreatedByEmpName != string.Empty)
                                newItem.CreatedByFullName = string.Format("({0}) {1}", newItem.CreatedByEmpNo, newItem.CreatedByEmpName);
                            else
                                newItem.CreatedByFullName = newItem.CreatedByEmpName;

                            if (newItem.LastUpdateEmpNo > 0 && newItem.LastUpdateEmpName != string.Empty)
                                newItem.LastUpdateFullName = string.Format("({0}) {1}", newItem.LastUpdateEmpNo, newItem.LastUpdateEmpName);
                            else
                                newItem.LastUpdateFullName = newItem.LastUpdateEmpName;

                            // Add item to the collection
                            result.Add(newItem);
                        }
                    #endregion
                    }
                    else if (loadType == 2)
                    {
                        #region Populate Shift Timing Sequence
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            MasterShiftPatternEntity newItem = new MasterShiftPatternEntity()
                            {
                                AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                                ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                                ShiftPointer = BLHelper.ConvertObjectToInt(row["ShiftPointer"]),
                                ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                                ShiftDescription = BLHelper.ConvertObjectToString(row["ShiftDescription"])
                            };

                            if (newItem.ShiftDescription != string.Empty)
                                newItem.ShiftFullDescription = string.Format("{0} - {1}", newItem.ShiftCode, newItem.ShiftDescription);
                            else
                                newItem.ShiftFullDescription = newItem.ShiftCode;

                            // Add item to the collection
                            result.Add(newItem);
                        }
                        #endregion
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void SaveMasterShiftPattern(string shiftPatCode, List<MasterShiftPatternEntity> shiftTimingScheduleList, List<MasterShiftPatternEntity> shiftTimingPointerList, ref string error, ref string innerError)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    #region Save changes to shift timing schedule
                    if (shiftTimingScheduleList.Count > 0)
                    {
                        #region Delete no matching records
                        List<Master_ShiftTimes> shiftTimingDeleteList = new List<Master_ShiftTimes>();

                        var rawData = from a in TASContext.Master_ShiftTimes
                                      where a.ShiftPatCode.Trim() == shiftPatCode
                                      select a;
                        if (rawData != null)
                        {
                            foreach (var item in rawData)
                            {
                                if (shiftTimingScheduleList.Where(a => a.AutoID == item.AutoID).FirstOrDefault() == null)
                                    shiftTimingDeleteList.Add(item);
                            }

                            // Save to database
                            if (shiftTimingDeleteList.Count > 0)
                            {
                                TASContext.Master_ShiftTimes.RemoveRange(shiftTimingDeleteList);
                                TASContext.SaveChanges();
                            }
                        }
                        #endregion

                        #region Save changes to the database         
                        List<Master_ShiftTimes> shiftTimingInsertList = new List<Master_ShiftTimes>();

                        foreach (MasterShiftPatternEntity item in shiftTimingScheduleList)
                        {
                            if (item.AutoID > 0)
                            {
                                #region Update existing record                                                        
                                if (item.IsDirty)
                                {
                                    Master_ShiftTimes itemToUpdate = TASContext.Master_ShiftTimes.Where(a => a.AutoID == item.AutoID).FirstOrDefault();
                                    if (itemToUpdate != null)
                                    {
                                        itemToUpdate.ArrivalFrom = item.ArrivalFrom;
                                        itemToUpdate.ArrivalTo = item.ArrivalTo;
                                        itemToUpdate.DepartFrom = item.DepartFrom;
                                        itemToUpdate.DepartTo = item.DepartTo;
                                        itemToUpdate.TotalHrs = item.DurationNormalDay;
                                        itemToUpdate.RArrivalFrom = item.RArrivalFrom;
                                        itemToUpdate.RArrivalTo = item.RArrivalTo;
                                        itemToUpdate.RDepartFrom = item.RDepartFrom;
                                        itemToUpdate.RDepartTo = item.RDepartTo;
                                        itemToUpdate.RTotalHrs = item.DurationRamadanDay;
                                        itemToUpdate.LastUpdateEmpNo = item.LastUpdateEmpNo;
                                        itemToUpdate.LastUpdateEmpName = item.LastUpdateEmpName;
                                        itemToUpdate.LastUpdateUser = item.LastUpdateUserID;
                                        itemToUpdate.LastUpdateTime = item.LastUpdateTime;

                                        // Save to database
                                        TASContext.SaveChanges();
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                #region Insert new record                                 
                                shiftTimingInsertList.Add(new Master_ShiftTimes()
                                {
                                    ShiftPatCode = item.ShiftPatCode,
                                    ShiftCode = item.ShiftCode,
                                    ArrivalFrom = item.ArrivalFrom,
                                    ArrivalTo = item.ArrivalTo,
                                    DepartFrom = item.DepartFrom,
                                    DepartTo = item.DepartTo,
                                    TotalHrs = item.DurationNormalDay,
                                    RArrivalFrom = item.RArrivalFrom,
                                    RArrivalTo = item.RArrivalTo,
                                    RDepartFrom = item.RDepartFrom,
                                    RDepartTo = item.RDepartTo,
                                    RTotalHrs = item.DurationRamadanDay,
                                    CreatedByEmpNo = item.CreatedByEmpNo,
                                    CreatedByEmpName = item.CreatedByEmpName,
                                    CreatedByUser = item.CreatedByUserID,
                                    CreatedDate = item.CreatedDate
                                });
                                #endregion
                            }
                        }

                        if (shiftTimingInsertList.Count > 0)
                        {
                            // Commit changes in the database
                            TASContext.Master_ShiftTimes.AddRange(shiftTimingInsertList);
                            TASContext.SaveChanges();
                        }
                        #endregion
                    }
                    #endregion

                    #region Save changes to shift pointer sequence
                    DatabaseSaveResult shiftPatternDBResult = new DatabaseSaveResult();

                    if (shiftTimingPointerList.Count > 0)
                    {
                        #region Delete existing records
                        var rawData = TASContext.InsertUpdateDeleteMasterShiftPattern(3, shiftPatCode, 0, string.Empty);
                        if (rawData != null)
                        {
                            var dbTransResult = rawData.FirstOrDefault();
                            if (dbTransResult != null)
                            {
                                shiftPatternDBResult.HasError = BLHelper.ConvertObjectToBolean(dbTransResult.HasError);
                                shiftPatternDBResult.ErrorCode = BLHelper.ConvertObjectToString(dbTransResult.ErrorCode);
                                shiftPatternDBResult.ErrorDesc = BLHelper.ConvertObjectToString(dbTransResult.ErrorDescription);
                                shiftPatternDBResult.NewIdentityID = BLHelper.ConvertObjectToInt(dbTransResult.NewIdentityID);
                                shiftPatternDBResult.RowsAffected = BLHelper.ConvertObjectToInt(dbTransResult.RowsAffected);
                            }
                        }
                        #endregion

                        if (!shiftPatternDBResult.HasError)
                        {
                            #region Insert new shift pointer sequence
                            foreach (MasterShiftPatternEntity item in shiftTimingPointerList)
                            {
                                var rawDataInsert = TASContext.InsertUpdateDeleteMasterShiftPattern(1, item.ShiftPatCode, item.ShiftPointer, item.ShiftCode);

                                var dbTransResult = rawDataInsert.FirstOrDefault();
                                if (dbTransResult != null)
                                {
                                    shiftPatternDBResult.HasError = BLHelper.ConvertObjectToBolean(dbTransResult.HasError);
                                    shiftPatternDBResult.ErrorDesc = BLHelper.ConvertObjectToString(dbTransResult.ErrorDescription);

                                    if (shiftPatternDBResult.HasError)
                                    {
                                        throw new Exception(shiftPatternDBResult.ErrorDesc);
                                    }
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            throw new Exception(shiftPatternDBResult.ErrorDesc);
                        }
                    }
                    #endregion

                    // Commit the transaction
                    scope.Complete();
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public void InsertUpdateDeleteShiftPattern(int saveTypeID, MasterShiftPatternEntity shiftPatternInfo, ref string error, ref string innerError)
        {
            try
            {
                BLHelper.SaveType saveType = (BLHelper.SaveType)Enum.Parse(typeof(BLHelper.SaveType), saveTypeID.ToString());

                switch (saveType)
                {
                    case BLHelper.SaveType.Insert:
                        #region Perform Insert Operation

                        #region Check for duplicate records
                        Master_ShiftPatternTitles duplicateRecord = TASContext.Master_ShiftPatternTitles
                            .Where(a => a.ShiftPatCode.Trim() == shiftPatternInfo.ShiftPatCode)
                            .FirstOrDefault();
                        if (duplicateRecord != null)
                        {
                            throw new Exception("Unable to save changes because the specified Shift Pattern Code already exist in the database.");
                        }
                        #endregion

                        #region No duplicate record, proceed in saving the data
                        // Initialize collection
                        Master_ShiftPatternTitles recordToInsert = new Master_ShiftPatternTitles()
                        {
                            ShiftPatCode = shiftPatternInfo.ShiftPatCode,
                            ShiftPatDescription = shiftPatternInfo.ShiftPatDescription,
                            IsDayShift = shiftPatternInfo.IsDayShift,
                            LastUpdateUser = shiftPatternInfo.CreatedByUserID,
                            LastUpdateTime = shiftPatternInfo.CreatedDate
                        };

                        // Commit changes in the database
                        TASContext.Master_ShiftPatternTitles.Add(recordToInsert);
                        TASContext.SaveChanges();
                        #endregion

                        break;
                        #endregion

                    case BLHelper.SaveType.Update:
                        #region Perform Update Operation
                        Master_ShiftPatternTitles recordToUpdate = TASContext.Master_ShiftPatternTitles
                            .Where(a => a.ShiftPatCode.Trim() == shiftPatternInfo.ShiftPatCode)
                            .FirstOrDefault();
                        if (recordToUpdate != null)
                        {
                            recordToUpdate.ShiftPatDescription = shiftPatternInfo.ShiftPatDescription;
                            recordToUpdate.IsDayShift = shiftPatternInfo.IsDayShift;
                            recordToUpdate.LastUpdateTime = shiftPatternInfo.LastUpdateTime;
                            recordToUpdate.LastUpdateUser = shiftPatternInfo.LastUpdateUserID;

                            // Save to database
                            TASContext.SaveChanges();
                        }

                        break;
                        #endregion

                    case BLHelper.SaveType.Delete:
                        #region Perform Delete Operation

                        #region Delete all associated shift timings
                        List<Master_ShiftTimes> shifTimingList = TASContext.Master_ShiftTimes
                           .Where(a => a.ShiftPatCode.Trim() == shiftPatternInfo.ShiftPatCode)
                           .ToList();
                        if (shifTimingList != null)
                        {
                            TASContext.Master_ShiftTimes.RemoveRange(shifTimingList);
                            TASContext.SaveChanges();
                        }
                        #endregion

                        #region Delete all associated shift pointer sequence
                        List<Master_ShiftPattern> shifPointerList = TASContext.Master_ShiftPattern
                           .Where(a => a.ShiftPatCode.Trim() == shiftPatternInfo.ShiftPatCode)
                           .ToList();
                        if (shifPointerList != null)
                        {
                            TASContext.Master_ShiftPattern.RemoveRange(shifPointerList);
                            TASContext.SaveChanges();
                        }
                        #endregion

                        #region Delete the Master Shift Pattern code                                                
                        Master_ShiftPatternTitles recordToDelete = TASContext.Master_ShiftPatternTitles
                            .Where(a => a.ShiftPatCode.Trim() == shiftPatternInfo.ShiftPatCode)
                            .FirstOrDefault();
                        if (recordToDelete != null)
                        {
                            TASContext.Master_ShiftPatternTitles.Remove(recordToDelete);
                            TASContext.SaveChanges();
                        }
                        #endregion

                        break;
                        #endregion
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }

        public List<ServiceLogDetail> GetTimesheetAndSPULogDetail(byte loadType, DateTime? processDate, ref string error, ref string innerError)
        {
            List<ServiceLogDetail> result = new List<ServiceLogDetail>();

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[2];
                parameters[0] = new ADONetParameter("@loadType", SqlDbType.TinyInt, loadType);
                parameters[1] = new ADONetParameter("@processDate", SqlDbType.DateTime, processDate);

                DataSet ds = RunSPReturnDataset("tas.Pr_TimesheetLogDetail", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    if (loadType == 1)
                    {
                        #region Get SPU Service logs
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            ServiceLogDetail newItem = new ServiceLogDetail()
                            {
                                AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                                LogDate = BLHelper.ConvertObjectToDate(row["Log_Date"]),
                                SPUDate = BLHelper.ConvertObjectToDate(row["SPU_Date"]),
                                ProcessID = BLHelper.ConvertObjectToInt(row["SPU_TxID"]),
                                LogDescription = BLHelper.ConvertObjectToString(row["Log_Description"]),
                                LogErrorDesc = BLHelper.ConvertObjectToString(row["ErrorDescription"])
                            };

                            // Add item to the collection
                            result.Add(newItem);
                        }
                        #endregion
                    }
                    else if (loadType == 2)
                    {
                        #region Get Timesheet Processing Service logs
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            ServiceLogDetail newItem = new ServiceLogDetail()
                            {
                                MessageID = BLHelper.ConvertObjectToInt(row["MessageID"]),
                                ProcessDate = BLHelper.ConvertObjectToDate(row["ProcessDate"]),
                                ProcessID = BLHelper.ConvertObjectToInt(row["ProcessID"]),
                                Message = BLHelper.ConvertObjectToString(row["Message"])
                            };

                            // Add item to the collection
                            result.Add(newItem);
                        }
                        #endregion
                    }
                    else if (loadType == 3)
                    {
                        #region Get Shift Pointer counts
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            ServiceLogDetail newItem = new ServiceLogDetail()
                            {                                
                                ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                                ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                                ShiftPointer = BLHelper.ConvertObjectToInt(row["ShiftPointer"]),
                                EmpCount = BLHelper.ConvertObjectToInt(row["EmpCount"])
                            };

                            // Add item to the collection
                            result.Add(newItem);
                        }
                        #endregion
                    }
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public SystemValueEntity GetSystemValues(ref string error, ref string innerError)
        {
            SystemValueEntity result = null;

            try
            {
                var rawData = (from a in TASContext.System_Values
                              select new
                              {
                                  ShiftPatternLastUpdated = a.DT_ShiftPatternLastUpdated,
                                  ManualTimeSheetLastEntered = a.DT_ManualTimeSheetLastEntered,
                                  SwipeLastProcessDate = a.DT_SwipeLastProcessed,
                                  SwipeNewProcessDate = a.DT_SwipeNewProcess
                              }).FirstOrDefault();

                if (rawData != null)
                {
                    result = new SystemValueEntity()
                    {
                        ShiftPatternLastUpdated = rawData.ShiftPatternLastUpdated,
                        ManualTimeSheetLastEntered = rawData.ManualTimeSheetLastEntered,
                        SwipeLastProcessDate = rawData.SwipeLastProcessDate,
                        SwipeNewProcessDate = rawData.SwipeNewProcessDate
                    };
                }

                return result;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmployeeAbsentEntity> GetEmployeeAbsences(DateTime? startDate, DateTime? endDate, string costCenter, int empNo, ref string error, ref string innerError)
        {
            List<EmployeeAbsentEntity> absenceList = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[4];
                parameters[0] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[1] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[2] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[3] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetEmployeeAbsences", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize the collection
                    absenceList = new List<EmployeeAbsentEntity>();

                    #region Build the datasource for the report
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAbsentEntity reportItem = new EmployeeAbsentEntity()
                        {
                            AutoID = BLHelper.ConvertObjectToInt(row["AutoID"]),
                            BusinessUnit = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            BusinessUnitName = BLHelper.ConvertObjectToString(row["BusinessUnitName"]),
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            PayGrade = BLHelper.ConvertObjectToInt(row["PayGrade"]),
                            SupervisorNo = BLHelper.ConvertObjectToInt(row["SupervisorNo"]),
                            SupervisorName = BLHelper.ConvertObjectToString(row["SupervisorName"]),
                            AbsentDate = BLHelper.ConvertObjectToDate(row["DT"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                            Remarks = BLHelper.ConvertObjectToString(row["Remarks"]),
                        };

                        // Add item to the collection
                        absenceList.Add(reportItem);
                    }
                    #endregion
                }

                return absenceList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<EmergencyContactEntity> GetEmployeeEmergencyContact(byte loadType, string costCenter, int empNo, string searchString, int userEmpNo, ref string error, ref string innerError)
        {
            List<EmergencyContactEntity> contactList = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;

                ADONetParameter[] parameters = new ADONetParameter[5];
                parameters[0] = new ADONetParameter("@loadType", SqlDbType.TinyInt, loadType);
                parameters[1] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[2] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);
                parameters[3] = new ADONetParameter("@searchString", SqlDbType.VarChar, 100, searchString);
                parameters[4] = new ADONetParameter("@userEmpNo", SqlDbType.Int, userEmpNo);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetEmpEmergencyContact", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize the collection
                    contactList = new List<EmergencyContactEntity>();

                    #region Build the datasource for the report
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmergencyContactEntity contactItem = new EmergencyContactEntity()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = ds.Tables[0].Columns.Contains("EmpName") ? BLHelper.ConvertObjectToString(row["EmpName"]) : string.Empty,
                            RelatedPersonID = ds.Tables[0].Columns.Contains("RelatedPersonID") ? BLHelper.ConvertObjectToInt(row["RelatedPersonID"]) : 0,
                            RelatedPersonName = ds.Tables[0].Columns.Contains("RelatedPersonName") ? BLHelper.ConvertObjectToString(row["RelatedPersonName"]) : string.Empty,
                            RelationTypeID = ds.Tables[0].Columns.Contains("RelationTypeID") ? BLHelper.ConvertObjectToInt(row["RelationTypeID"]) : 0,
                            RelationTypeDesc = ds.Tables[0].Columns.Contains("RelationTypeDesc") ? BLHelper.ConvertObjectToString(row["RelationTypeDesc"]) : string.Empty,
                            LineNumberID = ds.Tables[0].Columns.Contains("LineNumberID") ? BLHelper.ConvertObjectToInt(row["LineNumberID"]) : 0,
                            PhoneNumberType = ds.Tables[0].Columns.Contains("PhoneNumberType") ? BLHelper.ConvertObjectToString(row["PhoneNumberType"]) : string.Empty,
                            PhoneNumberDesc = ds.Tables[0].Columns.Contains("PhoneNumberDesc") ? BLHelper.ConvertObjectToString(row["PhoneNumberDesc"]) : string.Empty,
                            PhonePrefix = ds.Tables[0].Columns.Contains("PhonePrefix") ? BLHelper.ConvertObjectToString(row["PhonePrefix"]) : string.Empty,
                            PhoneNumber = ds.Tables[0].Columns.Contains("PhoneNumber") ? BLHelper.ConvertObjectToString(row["PhoneNumber"]) : string.Empty,

                            SupervisorNo = ds.Tables[0].Columns.Contains("SupervisorNo") ? BLHelper.ConvertObjectToInt(row["SupervisorNo"]) : 0,
                            SupervisorName = ds.Tables[0].Columns.Contains("SupervisorName") ? BLHelper.ConvertObjectToString(row["SupervisorName"]) : string.Empty,
                            Position = ds.Tables[0].Columns.Contains("Position") ? BLHelper.ConvertObjectToString(row["Position"]) : string.Empty,
                            CostCenter = ds.Tables[0].Columns.Contains("BusinessUnit") ? BLHelper.ConvertObjectToString(row["BusinessUnit"]) : string.Empty,
                            CostCenterName = ds.Tables[0].Columns.Contains("BusinessUnitName") ? BLHelper.ConvertObjectToString(row["BusinessUnitName"]) : string.Empty,
                            Religion = ds.Tables[0].Columns.Contains("Religion") ? BLHelper.ConvertObjectToString(row["Religion"]) : string.Empty,
                            Sex = ds.Tables[0].Columns.Contains("Sex") ? BLHelper.ConvertObjectToString(row["Sex"]) : string.Empty,
                            JobCategory = ds.Tables[0].Columns.Contains("JobCategory") ? BLHelper.ConvertObjectToString(row["JobCategory"]) : string.Empty,
                            PayGrade = ds.Tables[0].Columns.Contains("GradeCode") ? BLHelper.ConvertObjectToInt(row["GradeCode"]) : 0,
                            DateJoined = ds.Tables[0].Columns.Contains("DateJoined") ? BLHelper.ConvertObjectToDate(row["DateJoined"]) : null,
                            YearsOfService = ds.Tables[0].Columns.Contains("YearsOfService") ? BLHelper.ConvertObjectToDouble(row["YearsOfService"]) : 0,
                            DateOfBirth = ds.Tables[0].Columns.Contains("DateOfBirth") ? BLHelper.ConvertObjectToDate(row["DateOfBirth"]) : null,
                            Age = ds.Tables[0].Columns.Contains("Age") ? BLHelper.ConvertObjectToInt(row["Age"]) : 0,
                            TelephoneExt = ds.Tables[0].Columns.Contains("TelephoneExt") ? BLHelper.ConvertObjectToString(row["TelephoneExt"]) : string.Empty,
                            MobileNo = ds.Tables[0].Columns.Contains("MobileNo") ? BLHelper.ConvertObjectToString(row["MobileNo"]) : string.Empty,
                            TelNo = ds.Tables[0].Columns.Contains("TelNo") ? BLHelper.ConvertObjectToString(row["TelNo"]) : string.Empty,
                            FaxNo = ds.Tables[0].Columns.Contains("FaxNo") ? BLHelper.ConvertObjectToString(row["FaxNo"]) : string.Empty,
                            Email = ds.Tables[0].Columns.Contains("EmpEmail") ? BLHelper.ConvertObjectToString(row["EmpEmail"]) : string.Empty
                        };

                        if (contactItem.SupervisorNo > 0 &&
                            !string.IsNullOrEmpty(contactItem.SupervisorName))
                        {
                            contactItem.SupervisorFullName = string.Format("{0} - {1}",
                                contactItem.SupervisorNo,
                                contactItem.SupervisorName);
                        }
                        else
                            contactItem.SupervisorFullName = contactItem.SupervisorName;

                        // Add item to the collection
                        contactList.Add(contactItem);
                    }
                    #endregion
                }

                return contactList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public bool CheckIfCanAccessEmergencyContact(int empNo, ref string error, ref string innerError)
        {
            try
            {
                bool hasAccess = BLHelper.ConvertObjectToBolean(TASContext.CanAccessEmergencyContact(empNo).FirstOrDefault());

                return hasAccess;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return false;
            }
        }

        public bool CheckIfCanAccessDependentInfo(int empNo, ref string error, ref string innerError)
        {
            try
            {
                bool hasAccess = BLHelper.ConvertObjectToBolean(TASContext.CanAccessDependentInfo(empNo).FirstOrDefault());

                return hasAccess;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return false;
            }
        }

        public List<EmployeeAttendanceEntity> GetEmployeeAttendanceHistoryCompact(DateTime? startDate, DateTime? endDate, string costCenter, int empNo, ref string error, ref string innerError)
        {
            List<EmployeeAttendanceEntity> attendanceList = null;

            try
            {
                string connectionString = TASContext.Database.Connection.ConnectionString;
                ADONetParameter[] parameters = new ADONetParameter[4];

                parameters[0] = new ADONetParameter("@startDate", SqlDbType.DateTime, startDate);
                parameters[1] = new ADONetParameter("@endDate", SqlDbType.DateTime, endDate);
                parameters[2] = new ADONetParameter("@costCenter", SqlDbType.VarChar, 12, costCenter);
                parameters[3] = new ADONetParameter("@empNo", SqlDbType.Int, empNo);

                DataSet ds = RunSPReturnDataset("tas.Pr_GetAttendanceHistoryCompact", connectionString, parameters);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    // Initialize the collection
                    attendanceList = new List<EmployeeAttendanceEntity>();

                    #region Populate data in the collection 
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EmployeeAttendanceEntity newItem = new EmployeeAttendanceEntity()
                        {
                            EmpNo = BLHelper.ConvertObjectToInt(row["EmpNo"]),
                            EmpName = BLHelper.ConvertObjectToString(row["EmpName"]),
                            Position = BLHelper.ConvertObjectToString(row["Position"]),
                            CostCenter = BLHelper.ConvertObjectToString(row["BusinessUnit"]),
                            CostCenterName = BLHelper.ConvertObjectToString(row["BusinessUnitName"]),
                            ShiftPatCode = BLHelper.ConvertObjectToString(row["ShiftPatCode"]),
                            ShiftCode = BLHelper.ConvertObjectToString(row["ShiftCode"]),
                            ActualShiftCode = BLHelper.ConvertObjectToString(row["Actual_ShiftCode"]),
                            DT = BLHelper.ConvertObjectToDate(row["DT"]),
                            dtIN = BLHelper.ConvertObjectToDate(row["dtIn"]),
                            dtOUT = BLHelper.ConvertObjectToDate(row["dtOut"]),
                            WorkDurationCumulative = BLHelper.ConvertObjectToInt(row["WorkDurationCumulative"]),
                            WorkDurationMinutes = BLHelper.ConvertObjectToInt(row["WorkDurationMinutes"]),
                            WorkDurationHours = BLHelper.ConvertObjectToString(row["WorkDurationHours"]) != string.Empty
                                ? BLHelper.ConvertObjectToString(row["WorkDurationHours"]).Insert(2, ":")
                                : string.Empty,
                            //ShavedWorkDurationMinutes = BLHelper.ConvertObjectToInt(row["ShavedWorkDurationMinutes"]),
                            //ShavedWorkDurationHours = BLHelper.ConvertObjectToString(row["ShavedWorkDurationHours"]) != string.Empty
                            //    ? BLHelper.ConvertObjectToString(row["ShavedWorkDurationHours"]).Insert(2, ":")
                            //    : string.Empty,

                            OTDurationMinutes = BLHelper.ConvertObjectToInt(row["OTDurationMinutes"]),
                            OvertimeDurationHours = BLHelper.ConvertObjectToString(row["OTDurationHours"]) != string.Empty
                                ? BLHelper.ConvertObjectToString(row["OTDurationHours"]).Insert(2, ":")
                                : string.Empty,

                            NoPayHours = BLHelper.ConvertObjectToInt(row["NoPayHours"]),
                            AttendanceRemarks = BLHelper.ConvertObjectToString(row["Remarks"]),
                            //DurationRequired = BLHelper.ConvertObjectToInt(row["Duration_Required"]),
                            //DayOffDuration = BLHelper.ConvertObjectToInt(row["DayOffDuration"]),
                            //RequiredToSwipeAtWorkplace = BLHelper.ConvertNumberToBolean(row["RequiredToSwipeAtWorkplace"]),
                            //TimeInMG = BLHelper.ConvertObjectToDate(row["TimeInMG"]),
                            //TimeOutMG = BLHelper.ConvertObjectToDate(row["TimeOutMG"]),
                            //TimeInWP = BLHelper.ConvertObjectToDate(row["TimeInWP"]),
                            //TimeOutWP = BLHelper.ConvertObjectToDate(row["TimeOutWP"]),
                            LastUpdateUser = BLHelper.ConvertObjectToString(row["LastUpdateUser"]),
                            LastUpdateTime = BLHelper.ConvertObjectToDate(row["LastUpdateTime"])
                        };

                        #region Calculate NPH in hours
                        if (newItem.NoPayHours > 0)
                        {
                            if (newItem.NoPayHours > 60)
                            {
                                int hrs = 0;
                                int min = 0;

                                hrs = Math.DivRem(Convert.ToInt32(newItem.NoPayHours), 60, out min);
                                newItem.NoPayHoursDesc = string.Format("{0}:{1}",
                                    string.Format("{0:00}", hrs),
                                    string.Format("{0:00}", min));
                            }
                            else
                            {
                                newItem.NoPayHoursDesc = string.Format("00:{0}", string.Format("{0:00}", newItem.NoPayHours));
                            }
                        }
                        #endregion

                        if (!string.IsNullOrEmpty(newItem.CostCenter) &&
                           !string.IsNullOrEmpty(newItem.CostCenterName))
                        {
                            newItem.CostCenterFullName = string.Format("{0} - {1}",
                                newItem.CostCenter,
                                newItem.CostCenterName);
                        }
                        else
                            newItem.CostCenterFullName = newItem.CostCenter;

                        if (newItem.EmpNo > 0 && !string.IsNullOrEmpty(newItem.EmpName))
                        {
                            newItem.EmpFullName = string.Format("{0} - {1}",
                                newItem.EmpNo,
                                newItem.EmpName);
                        }
                        else
                            newItem.EmpFullName = string.Format("Employee Name: {0}", newItem.EmpName);

                        // Add item to the collection
                        attendanceList.Add(newItem);
                    }
                    #endregion
                }

                return attendanceList;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public List<RelativeType> GetFamilyRelativeTypes(byte degreeLevel, string relativeTypeCode, ref string error, ref string innerError)
        {
            List<RelativeType> relativeTypeList = new List<RelativeType>();

            try
            {
                var rawData = TASContext.GetFamilyRelativeSetting(degreeLevel, relativeTypeCode);
                if (rawData != null)
                {
                    // Initialize the collection
                    relativeTypeList = new List<RelativeType>();

                    foreach (var item in rawData)
                    {
                        RelativeType newItem = new RelativeType()
                        {
                            SettingID = BLHelper.ConvertObjectToInt(item.SettingID),
                            DegreeLevel = BLHelper.ConvertObjectToByte(item.DegreeLevel),
                            RelativeTypeCode = BLHelper.ConvertObjectToString(item.RelativeTypeCode),
                            RelativeTypeName = BLHelper.ConvertObjectToString(item.RelativeTypeName),
                            SequenceNo = BLHelper.ConvertObjectToByte(item.SequenceNo)
                        };

                        // Add item to the collection
                        relativeTypeList.Add(newItem);
                    }
                }

                return relativeTypeList;
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
                return null;
            }
        }

        public void InsertUpdateDeleteDeathReasonOfAbsence(int saveTypeID, DeathReasonOfAbsenceEntity deathEntity, ref string error, ref string innerError)
        {
            try
            {
                BLHelper.SaveType saveType = (BLHelper.SaveType)Enum.Parse(typeof(BLHelper.SaveType), saveTypeID.ToString());

                switch (saveType)
                {
                    case BLHelper.SaveType.Insert:
                        #region Perform Insert Operation

                        #region Delete existing records
                        DeathReasonOfAbsence duplicateRecord = TASContext.DeathReasonOfAbsences
                            .Where(a => a.EmpNo == deathEntity.EmpNo && a.DT == deathEntity.DT && a.CostCenter.Trim() == deathEntity.CostCenter)   
                            .FirstOrDefault();
                        if (duplicateRecord != null)
                        {
                            TASContext.DeathReasonOfAbsences.Remove(duplicateRecord);
                            TASContext.SaveChanges();
                        }
                        #endregion

                        #region No duplicate record, proceed in saving the data
                        // Initialize collection
                        DeathReasonOfAbsence recordToInsert = new DeathReasonOfAbsence()
                        {
                            EmpNo = deathEntity.EmpNo,
                            DT = Convert.ToDateTime(deathEntity.DT),
                            CostCenter = deathEntity.CostCenter,
                            CorrectionCode = deathEntity.CorrectionCode,
                            ShiftPatCode = deathEntity.ShiftPatCode,
                            ShiftCode = deathEntity.ShiftCode,
                            RelativeTypeCode = deathEntity.RelativeTypeCode,
                            OtherRelativeType = deathEntity.OtherRelativeType,
                            Remarks = deathEntity.Remarks,
                            CreatedDate = deathEntity.CreatedDate,
                            CreatedByEmpNo = deathEntity.CreatedByEmpNo,
                            CreatedByEmpName = deathEntity.CreatedByEmpName,
                            CreatedByUserID = deathEntity.CreatedByUserID
                        };

                        // Commit changes in the database
                        TASContext.DeathReasonOfAbsences.Add(recordToInsert);
                        TASContext.SaveChanges();
                        #endregion

                        break;
                        #endregion

                    case BLHelper.SaveType.Update:
                        #region Perform Update Operation
                        DeathReasonOfAbsence recordToUpdate = TASContext.DeathReasonOfAbsences
                            .Where(a => a.ReasonAbsenceID == deathEntity.ReasonAbsenceID)
                            .FirstOrDefault();
                        if (recordToUpdate != null)
                        {
                            recordToUpdate.ShiftPatCode = deathEntity.ShiftPatCode;
                            recordToUpdate.ShiftCode = deathEntity.ShiftCode;
                            recordToUpdate.RelativeTypeCode = deathEntity.RelativeTypeCode;
                            recordToUpdate.OtherRelativeType = deathEntity.OtherRelativeType;
                            recordToUpdate.Remarks = deathEntity.Remarks;
                            recordToUpdate.LastUpdateDate = deathEntity.LastUpdateDate;
                            recordToUpdate.LastUpdateEmpNo = deathEntity.LastUpdateEmpNo;
                            recordToUpdate.LastUpdateEmpName = deathEntity.LastUpdateEmpName;
                            recordToUpdate.LastUpdateUserID = deathEntity.LastUpdateUserID;

                            // Save to database
                            TASContext.SaveChanges();
                        }

                        break;
                        #endregion

                    case BLHelper.SaveType.Delete:
                        #region Perform Delete Operation

                        DeathReasonOfAbsence recordToDelete = TASContext.DeathReasonOfAbsences
                            .Where(a => a.ReasonAbsenceID == deathEntity.ReasonAbsenceID)
                            .FirstOrDefault();
                        if (recordToDelete != null)
                        {
                            TASContext.DeathReasonOfAbsences.Remove(recordToDelete);
                            TASContext.SaveChanges();
                        }

                        break;
                        #endregion
                }
            }
            catch (SqlException sqlErr)
            {
                throw new Exception(sqlErr.Message.ToString());
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                if (ex.InnerException != null)
                    innerError = ex.InnerException.Message.ToString();
            }
        }
        #endregion

        #region Web Service Methods
        public List<object> GetContractorRegistrationLookup()
        {
            List<object> result = null;

            try
            {
                string sql = "EXEC tas.Pr_GetContRegistrationLookup";
                List<CostCenterEntity> costCenterList = new List<CostCenterEntity>();
                List<UserDefinedCode> licenseList = new List<UserDefinedCode>();
                DbCommand cmd;
                DbDataReader reader;

                // Build the command object
                cmd = TASContext.Database.Connection.CreateCommand();
                cmd.CommandText = sql;

                // Open database connection
                TASContext.Database.Connection.Open();

                // Create a DataReader  
                reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                #region Get the cost center list
                CostCenterEntity costCenterItem = null;
                while (reader.Read())
                {
                    costCenterItem = new CostCenterEntity
                    {
                        CompanyCode = BLHelper.ConvertObjectToString(reader["Company"]),
                        CostCenter = BLHelper.ConvertObjectToString(reader["CostCenter"]),
                        CostCenterName = BLHelper.ConvertObjectToString(reader["CostCenterName"]),
                        ParentCostCenter = BLHelper.ConvertObjectToString(reader["ParentBU"]),
                        SuperintendentEmpNo = BLHelper.ConvertObjectToInt(reader["Superintendent"]),
                        ManagerEmpNo = BLHelper.ConvertObjectToInt(reader["CostCenterManager"])
                    };

                    if (!string.IsNullOrWhiteSpace(costCenterItem.CostCenter))
                        costCenterItem.CostCenterFullName = string.Format("{0} - {1}", costCenterItem.CostCenter, costCenterItem.CostCenterName);
                    else
                        costCenterItem.CostCenterFullName = costCenterItem.CostCenterName;

                    costCenterList.Add(costCenterItem);
                }
                #endregion

                // Advance to the next result set  
                reader.NextResult();

                #region Get License Types
                while (reader.Read())
                {
                    licenseList.Add(new UserDefinedCode
                    {
                        UDCCode = BLHelper.ConvertObjectToString(reader["LicenseCode"]),
                        UDCDesc1 = BLHelper.ConvertObjectToString(reader["LicenseDesc"])
                    });
                }
                #endregion

                // Close reader and database connection
                reader.Close();

                result = new List<object>
                {
                    costCenterList,
                    licenseList
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

        public async Task<List<object>> GetContractorRegistrationLookupAsync()
        {
            List<object> result = null;

            try
            {
                string sql = "EXEC tas.Pr_GetContRegistrationLookup";
                List<CostCenterEntity> costCenterList = new List<CostCenterEntity>();
                DbCommand cmd;
                DbDataReader reader;

                // Build the command object
                cmd = TASContext.Database.Connection.CreateCommand();
                cmd.CommandText = sql;

                // Open database connection
                await TASContext.Database.Connection.OpenAsync();

                // Create a DataReader  
                reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                #region Get the cost center list
                CostCenterEntity costCenterItem = null;
                while (await reader.ReadAsync())
                {
                    costCenterItem = new CostCenterEntity
                    {
                        CompanyCode = BLHelper.ConvertObjectToString(reader["Company"]),
                        CostCenter = BLHelper.ConvertObjectToString(reader["CostCenter"]),
                        CostCenterName = BLHelper.ConvertObjectToString(reader["CostCenterName"]),
                        ParentCostCenter = BLHelper.ConvertObjectToString(reader["ParentBU"]),
                        SuperintendentEmpNo = BLHelper.ConvertObjectToInt(reader["Superintendent"]),
                        ManagerEmpNo = BLHelper.ConvertObjectToInt(reader["CostCenterManager"])
                    };

                    if (!string.IsNullOrWhiteSpace(costCenterItem.CostCenter))
                        costCenterItem.CostCenterFullName = string.Format("{0} - {1}", costCenterItem.CostCenter, costCenterItem.CostCenterName);
                    else
                        costCenterItem.CostCenterFullName = costCenterItem.CostCenterName;

                    costCenterList.Add(costCenterItem);
                }
                #endregion

                // Close reader and database connection
                reader.Close();

                result = new List<object>
                {
                    costCenterList
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
        #endregion
    }
}
