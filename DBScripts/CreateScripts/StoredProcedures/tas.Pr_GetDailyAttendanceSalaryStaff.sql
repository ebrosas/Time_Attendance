/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetDailyAttendanceSalaryStaff
*	Description: Get data for "Daily Attendance for Salary Staff Report"
*
*	Date			Author		Rev. #		Comments:
*	15/01/2017		Ervin		1.0			Created
*	13/04/2017		Ervin		1.1			Fetch the cost center from "Master_Employee_JDE_View_V2" view
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetDailyAttendanceSalaryStaff
(   
	@startDate			DATETIME,
	@endDate			DATETIME,
	@costCenterList		VARCHAR(500) = ''
)
AS

	--Validate parameters
	IF ISNULL(@endDate, '') = '' OR @endDate = CONVERT(DATETIME, '')
		SET @endDate = @startDate

	DECLARE	@CONST_MIN_SHIFT_ALLOWANCE INT
    SELECT	@CONST_MIN_SHIFT_ALLOWANCE = a.Minutes_MinShiftAllowance
	FROM tas.System_Values a

	--Validate parameters
	IF ISNULL(@costCenterList, '') = ''
		SET @costCenterList = NULL

	SELECT	a.BusinessUnit,
			c.BUname AS BusinessUnitName,
			a.EmpNo,
			b.EmpName,
			b.Position,
			a.DT,
			a.dtIN,
			a.dtOUT,
			a.NetMinutes,
			a.ShiftPatCode,
			a.ShiftCode,
			a.Actual_ShiftCode,
			a.OTStartTime,
			a.OTEndTime,
			a.OTType,
			a.NoPayHours,
			a.ShiftAllowance,
			CASE 
				WHEN a.Duration_ShiftAllowance_Evening  >= @CONST_MIN_SHIFT_ALLOWANCE AND a.Duration_ShiftAllowance_Night >= @CONST_MIN_SHIFT_ALLOWANCE THEN 'Eve/Night' 
				WHEN a.Duration_ShiftAllowance_Night >= @CONST_MIN_SHIFT_ALLOWANCE THEN 'Night'
				WHEN a.Duration_ShiftAllowance_Evening >=@CONST_MIN_SHIFT_ALLOWANCE THEN 'Evening'
				ELSE ''
			END AS ShiftAllowanceDesc,
			a.Duration_ShiftAllowance_Evening,
			a.Duration_ShiftAllowance_Night,				
			a.IsSalStaff,
			d.LVDesc + d.RMdesc + d.RAdesc + d.TxDesc + d.TxtShiftSpan + d.DayOff + d.Resigned + d.OtherRemarks +  d.ShiftCodeDifference + d.CustomRemarks AS Remark,
			CONVERT(VARCHAR(8), e.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), e.DepartFrom, 108) AS ShiftTiming,
			a.GradeCode,
			a.IsDayWorker_OR_Shifter
	FROM tas.Tran_Timesheet a
		INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
		LEFT JOIN tas.Master_BusinessUnit_JDE_view c ON RTRIM(a.BusinessUnit) = RTRIM(c.BU)
		LEFT JOIN tas.GetRemark02 d ON a.AutoId = d.AutoId
		LEFT JOIN tas.Master_ShiftTimes e ON RTRIM(a.ShiftPatCode) = RTRIM(e.ShiftPatCode) AND RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)) = RTRIM(e.ShiftCode)
	WHERE 
		a.IsSalStaff = 1
		AND ISNUMERIC(b.PayStatus) = 1
		AND 
		(
			(
				a.DT BETWEEN @startDate AND @endDate 
				AND @startDate < @endDate
			)
			OR
            (
				a.DT = @startDate
				AND @startDate = @endDate
			)
		)
		AND 
		(
			RTRIM(b.BusinessUnit) IN (SELECT GenericNo FROM tas.fnParseStringArrayToInt(@costCenterList, ','))	--Rev. #1.1
			OR
			@costCenterList IS NULL
		)			
	ORDER BY a.BusinessUnit, a.EmpNo, a.dtIN

GO 
            

/*	Debug:

PARAMETERS:
	@startDate			DATETIME,
	@endDate			DATETIME,
	@costCenterList		VARCHAR(500) = ''

	EXEC tas.Pr_GetDailyAttendanceSalaryStaff '01/06/2017', '01/06/2017', '4100'
	EXEC tas.Pr_GetDailyAttendanceSalaryStaff '01/06/2017', '01/06/2017', '7600'

*/