/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_EmployeeExceptional
*	Description: Get the list of employees with different working cost center
*
*	Date			Author		Revision No.	Comments:
*	23/06/2016		Ervin		1.0				Created
*	02/01/2017		Ervin		1.1				Modified the ORDER BY clause to sort by DT in ascending order
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_EmployeeExceptional
(   	
	@empNo			INT,
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@isAbsence		BIT = 0,
	@isSickLeave	BIT = 0,
	@isNPH			BIT = 0,
	@isInjuryLeave	BIT = 0,
	@isDIL			BIT = 0,
	@isOvertime		BIT = 0
)
AS

	--Validate parameters
	IF ISNULL(@startDate, '') = CONVERT(DATETIME, '')
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = CONVERT(DATETIME, '')
		SET @endDate = NULL

	Create table #attendanceTable 
	(
		DT     datetime, 
		EmpNo  int, 
		Reason varchar(50)
	)

	--Absent
	IF @isAbsence = 1
	BEGIN

		INSERT INTO #attendanceTable 
		SELECT a.DT, a.EmpNo, 'Absent' 
		FROM tas.Tran_Timesheet a 
		WHERE UPPER(RTRIM(a.RemarkCode)) = 'A'
			AND a.EmpNo = @empNo
			AND 
			(
				a.DT BETWEEN @startDate AND @endDate
				OR
                (@startDate IS NULL AND @endDate IS NULL)
			)
	END
	
	--Sick Leave
	IF @isSickLeave = 1
	BEGIN

		INSERT INTO #attendanceTable 
		SELECT a.DT, a.EmpNo, 'Sick Leave - ' + LeaveType 
		FROM tas.Tran_Timesheet a 
		WHERE UPPER(RTRIM(a.LeaveType)) IN ('SLP', 'SLU') 
			AND a.EmpNo = @empNo
			AND 
			(
				a.DT BETWEEN @startDate AND @endDate
				OR
                (@startDate IS NULL AND @endDate IS NULL)
			)
	END
	
	--Injury Leave
	IF @isInjuryLeave = 1	
	BEGIN
		INSERT INTO #attendanceTable 
		SELECT a.DT, a.EmpNo,  'Injury Leave - ' + LeaveType 
		FROM tas.Tran_Timesheet a
		WHERE UPPER(RTRIM(a.LeaveType)) IN ('IL','ILU')
			AND a.EmpNo = @empNo
			AND 
			(	
				a.DT BETWEEN @startDate and @endDate
				OR
                (@startDate IS NULL AND @endDate IS NULL)
			)
	END
	
	--No Pay Hour
	IF @isNPH = 1
	BEGIN

		INSERT INTO #attendanceTable 
		SELECT a.DT, a.EmpNo, 'NPH - ' + tas.cstr(a.NoPayHours) + ' mins.'    
		FROM tas.Tran_Timesheet a
		WHERE a.NoPayHours > 0
			AND a.EmpNo = @empNo
			AND 
			(	
				a.DT BETWEEN @startDate AND @endDate
				OR
                (@startDate IS NULL AND @endDate IS NULL)
			)
	END
	
	--Day In Lieu
	IF @isDIL = 1
	BEGIN

		INSERT INTO #attendanceTable 
		SELECT a.DT, a.EmpNo, 'DIL' 
		FROM tas.Tran_Timesheet a
		WHERE UPPER(RTRIM(a.AbsenceReasonCode)) IN ('DD','UD','UA')
			AND a.EmpNo = @empNo
			AND 
			(
				a.DT BETWEEN @startDate AND @endDate
				OR
                (@startDate IS NULL AND @endDate IS NULL)
			)
	END
	
	--Overtime
	IF @isOvertime = 1
	BEGIN

		INSERT INTO #attendanceTable 
		SELECT a.DT, a.EmpNo, 'OT - ' + tas.cstr(SUM(DATEDIFF(n, a.OTStartTime, a.OTEndTime))) + ' mins.'  
		FROM tas.Tran_Timesheet a
		WHERE a.OTStartTime IS NOT NULL
			AND a.OTEndTime IS NOT NULL
			AND tas.fmtTime(a.OTStartTime) <> tas.fmtTime(a.OTEndTime)
			AND a.EmpNo = @empNo
			AND 
			(
				a.DT BETWEEN @startDate AND @endDate
				OR
                (@startDate IS NULL AND @endDate IS NULL)
			)
		GROUP BY a.EmpNo, a.DT 
	End

	SELECT	a.EmpNo,
			a.DT AS [Date], 
			a.Reason 
	FROM #attendanceTable a
	ORDER BY a.DT ASC, a.Reason	

GO 

/*	Debugging:

PARAMETERS:
	@empNo			INT,
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@isAbsence		BIT = 0,
	@isSickLeave	BIT = 0,
	@isNPH			BIT = 0,
	@isInjuryLeave	BIT = 0,
	@isDIL			BIT = 0,
	@isOvertime		BIT = 0

	EXEC tas.Pr_EmployeeExceptional 10003631, NULL, NULL, 1, 1, 1, 1, 1, 1 
	EXEC tas.Pr_EmployeeExceptional 10003512, NULL, NULL, 1, 0, 0, 0, 0, 0 

*/


