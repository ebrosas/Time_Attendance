DECLARE	@EmpName     varchar(100),           
		@CostCenter  varchar(12),
		@Date        Datetime 

SELECT	@EmpName		= '',           
		@CostCenter		= '7500',
		@Date			= '03/27/2016'

	--Get and insert all emplouee attance details for today
	exec tas.prInsertUpdateCurrentAttendance 'Insert'

	DECLARE @shiftCode            varchar(20),
			@InOutStatus          char(1),
			@Remark               varchar(500),
			@empno                int,
			@ExtensionNo          varchar(20)

	CREATE TABLE #EmpAttendance
	(
		  EmpName               varchar(100),
		  EmpNo                 varchar(10),
		  InquiryDate           dateTime,
		  InOutStatus           varchar(2),
		  ExtensionNo           varchar(20),
		  Remark                varchar(100),
		  EmployeeStatus		varchar(5),
		  CostCenter			varchar(12)
	)

	SET @EmpName = rtrim(ltrim(@EmpName))

	IF (isnull(@Date,'') = '' or convert(datetime,'') = @Date)
		SET @Date = GETDATE()

	--Get employee attance details
	IF (isnull(@EmpName,'') = '')
	BEGIN

		INSERT INTO #EmpAttendance 
		(
				EmpName, 
				EmpNo, 
				InquiryDate, 
				InOutStatus, 
				ExtensionNo, 
				Remark, 
				EmployeeStatus, 
				CostCenter
		)
		SELECT	EmpName, 
				EmpNo, 
				GETDATE(), 
				'', 
				Extension, 
				'', 
				EmployeeStatus,
				BusinessUnit 
		FROM
		(
			SELECT DISTINCT 
				a.EmpName, a.EmpNo, isnull(convert(varchar(20),d.WPPH1),'') as Extension,
				'EmployeeStatus' = CASE WHEN c.YAPAST IN ('R', 'T', 'E', 'X') and GETDATE() < tas.ConvertFromJulian(c.YADT) THEN '0' ELSE c.YAPAST END,
				a.BusinessUnit
			FROM tas.Master_Employee_JDE_View a
				INNER JOIN tas.master_Businessunit_JDE b on rtrim(a.businessunit) = rtrim(b.businessunit) 
				INNER JOIN tas.syJDE_F060116 c on a.EmpNo = c.YAAN8
				LEFT JOIN tas.syJDE_F0115 d on a.EmpNo = d.WPAN8 and upper(ltrim(rtrim(d.WPPHTP))) = 'EXT'
			WHERE 
				--a.PayStatus in ('0','1','2','3','4','5','6','7','8','9','i')
				--and c.DateResigned IS NULL and 
				RTRIM(a.BusinessUnit) = RTRIM(@CostCenter)
				AND a.EmpNo NOT IN (10003669, 10003668, 10002191, 10002133, 10001253, 10002136)
		) tblMain
		WHERE (isnumeric(EmployeeStatus) = 1 and convert(int,EmployeeStatus) between 0 and 9) or EmployeeStatus = 'I'			
		ORDER BY EmpName
	END

	ELSE 
	BEGIN

		INSERT INTO #EmpAttendance 
		(
				EmpName, 
				EmpNo, 
				InquiryDate, 
				InOutStatus, 
				ExtensionNo, 
				Remark, 
				EmployeeStatus,
				CostCenter
		)
		SELECT	EmpName, 
				EmpNo, 
				GETDATE(),
				'', 
				Extension, 
				'', 
				EmployeeStatus,
				BusinessUnit 
		FROM
		(
			SELECT DISTINCT 
				a.EmpName + ' - ' + b.businessunitname as EmpName, a.EmpNo, isnull(convert(varchar(20),d.WPPH1),'') as Extension,
				'EmployeeStatus' = CASE WHEN c.YAPAST IN ('R', 'T', 'E', 'X') and GETDATE() < tas.ConvertFromJulian(c.YADT) THEN '0' ELSE c.YAPAST END,
				a.BusinessUnit
			FROM tas.Master_Employee_JDE_View a
				INNER JOIN tas.master_Businessunit_JDE b on a.businessunit=b.businessunit 
				INNER JOIN tas.syJDE_F060116 c on a.EmpNo = c.YAAN8
				LEFT JOIN tas.syJDE_F0115 d on a.EmpNo = d.WPAN8 and upper(ltrim(rtrim(d.WPPHTP))) = 'EXT'
			WHERE 
				--a.PayStatus in ('0','1','2','3','4','5','6','7','8','9','i')
				--and a.DateResigned IS NULL and 
				UPPER(RTRIM(a.EmpName)) LIKE '%'+ UPPER(RTRIM(@EmpName)) +'%'
		) tblMain
		WHERE ISNUMERIC(EmployeeStatus) = 1 
			AND CONVERT(int, EmployeeStatus) BETWEEN 0 AND 9
	END

	--Update attendance details
	Update #EmpAttendance
	SET InOutStatus = tas.fnGetAttendanceStatusEx(a.EmpNo),
		--ExtensionNo = [tas].[fnGetEmployeeExtension](a.EmpNo),
		Remark = tas.fnGetAttendanceStatusRemark(a.EmpNo, tas.fnGetAttendanceStatus(a.EmpNo))
	FROM #EmpAttendance a	
	
	--Ensure correct status for Early Leaving
	Update #EmpAttendance
	SET InOutStatus = 'e'	
	WHERE rtrim(Remark) = 'Early leaving' 

	Update #EmpAttendance
	SET Remark = 'Early leaving'	
	WHERE upper(rtrim(InOutStatus)) = 'E' 

	Update #EmpAttendance
	SET Remark = 'Contractor'	
	WHERE upper(rtrim(InOutStatus)) = 'X' and EmployeeStatus = 'I'
		
	--Select all records
	SELECT * From #EmpAttendance a
	WHERE RTRIM(a.Remark) <> 'Contractor'
	
	--Drop the temporary table
	Drop Table #EmpAttendance
	
	--Drop the attendance temporary data
	exec tas.prInsertUpdateCurrentAttendance 'Delete'