/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetDailyAttendanceReport
*	Description: Get data for the Daily Attendance Report
*
*	Date			Author		Rev. #		Comments:
*	13/11/2016		Ervin		1.0			Created
*	04/12/2016		Ervin		1.1			Modified the Order By clause
*	11/01/2017		Ervin		1.2			Modified the filter clause for Salary Staff. Return attendance records of employees whose pay grade <= 9
*	19/01/2017		Ervin		1.3			Commented the filter for Salary Staff where PayGrade <= 9
*	13/04/2017		Ervin		1.4			Fetch the cost center from "Master_Employee_JDE_View_V2" view
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetDailyAttendanceReport
(   
	@employeeType		TINYINT,
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

	IF @employeeType = 0	--Non-Salary Staff
	BEGIN
    
		SELECT	a.BusinessUnit,
				c.BUname AS BusinessUnitName,
				a.EmpNo,
				b.EmpName,
				b.Position,
				a.DT,
				a.dtIN,
				a.dtOUT,
				a.NetMinutes,
				--CASE WHEN a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL
				--	THEN DATEDIFF(n, a.dtIN, a.dtOUT)
				--	ELSE NULL
				--END AS NetMinutes,
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
				CASE WHEN b.GradeCode = 0 
						--AND (CASE WHEN a.dtIN IS NULL AND b.GradeCode = 0 THEN e.SwipeTime ELSE a.dtIn END) IS NOT NULL
						AND a.dtIn IS NOT NULL

						--AND (CASE WHEN a.dtOUT IS NULL AND b.GradeCode = 0 THEN f.SwipeTime ELSE a.dtOUT END) IS NOT NULL
						AND a.dtOUT IS NOT NULL
					THEN ''
					ELSE 
						CASE ISNULL(a.MealVoucherEligibility,'N')
							WHEN 'Y' THEN '[Eligible for Meal Voucher?] '
							WHEN 'YA' THEN '[Meal Voucher Granted] '
							ELSE ''
						END +  LVDesc + RMdesc + RAdesc + TxDesc +  TxtShiftSpan + DayOff + Resigned + OtherRemarks + ShiftCodeDifference + CustomRemarks
				END AS Remark,
				CONVERT(VARCHAR(8), g.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), g.DepartFrom, 108) AS ShiftTiming,
				a.GradeCode 
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_view c ON RTRIM(a.BusinessUnit) = RTRIM(c.BU)
			LEFT JOIN tas.GetRemark02 d ON a.AutoId = d.AutoId
			--LEFT JOIN tas.Vw_ContractorSwipe e ON (a.EmpNo = e.EmpNo OR UPPER(RTRIM(b.EmpName)) = UPPER(RTRIM(e.LName))) AND e.SwipeType = 'IN' AND a.DT = e.SwipeDate
			--LEFT JOIN tas.Vw_ContractorSwipe f ON (a.EmpNo = f.EmpNo OR UPPER(RTRIM(b.EmpName)) = UPPER(RTRIM(f.LName))) AND f.SwipeType = 'OUT' AND a.DT = f.SwipeDate
			LEFT JOIN tas.Master_ShiftTimes g ON RTRIM(a.ShiftPatCode) = RTRIM(g.ShiftPatCode) AND RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)) = RTRIM(g.ShiftCode)
		WHERE 
			ISNULL(a.IsSalStaff, 0) = 0
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
				RTRIM(b.BusinessUnit) IN (SELECT GenericNo FROM tas.fnParseStringArrayToInt(@costCenterList, ','))	--Rev. #1.4
				OR
				@costCenterList IS NULL
			)
		ORDER BY a.BusinessUnit, a.EmpNo, a.dtIN
	END

	ELSE IF @employeeType = 1	--Salary Staff
	BEGIN
    
		SELECT	a.BusinessUnit,
				c.BUname AS BusinessUnitName,
				a.EmpNo,
				b.EmpName,
				b.Position,
				a.DT,
				a.dtIN,
				a.dtOUT,
				a.NetMinutes,
				--CASE WHEN a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL
				--	THEN DATEDIFF(n, a.dtIN, a.dtOUT)
				--	ELSE NULL
				--END AS NetMinutes,
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
			AND ISNULL(a.IsDayWorker_OR_Shifter, 0) = 0
			AND ISNUMERIC(b.PayStatus) = 1
			--AND CONVERT(INT, a.GradeCode) <= 9	--Rev. #1.3
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
				RTRIM(b.BusinessUnit) IN (SELECT GenericNo FROM tas.fnParseStringArrayToInt(@costCenterList, ','))	--Rev. #1.4
				OR
				@costCenterList IS NULL
			)			
		ORDER BY a.BusinessUnit, a.EmpNo, a.dtIN
	END

	ELSE
    BEGIN

		--Non Salary Staff
		SELECT	a.BusinessUnit,
				c.BUname AS BusinessUnitName,
				a.EmpNo,
				b.EmpName,
				b.Position,
				a.DT,
				a.dtIN,
				a.dtOUT,
				a.NetMinutes,
				--CASE WHEN a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL
				--	THEN DATEDIFF(n, a.dtIN, a.dtOUT)
				--	ELSE NULL
				--END AS NetMinutes,
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
				CASE WHEN b.GradeCode = 0 
						--AND (CASE WHEN a.dtIN IS NULL AND b.GradeCode = 0 THEN e.SwipeTime ELSE a.dtIn END) IS NOT NULL
						AND a.dtIn IS NOT NULL

						--AND (CASE WHEN a.dtOUT IS NULL AND b.GradeCode = 0 THEN f.SwipeTime ELSE a.dtOUT END) IS NOT NULL
						AND a.dtOUT IS NOT NULL
					THEN ''
					ELSE 
						CASE ISNULL(a.MealVoucherEligibility,'N')
							WHEN 'Y' THEN '[Eligible for Meal Voucher?] '
							WHEN 'YA' THEN '[Meal Voucher Granted] '
							ELSE ''
						END +  LVDesc + RMdesc + RAdesc + TxDesc +  TxtShiftSpan + DayOff + Resigned + OtherRemarks + ShiftCodeDifference + CustomRemarks
				END AS Remark,
				CONVERT(VARCHAR(8), g.ArrivalTo, 108) + ' - ' + CONVERT(VARCHAR(8), g.DepartFrom, 108) AS ShiftTiming,
				a.GradeCode  
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_view c ON RTRIM(a.BusinessUnit) = RTRIM(c.BU)
			LEFT JOIN tas.GetRemark02 d ON a.AutoId = d.AutoId
			--LEFT JOIN tas.Vw_ContractorSwipe e ON (a.EmpNo = e.EmpNo OR UPPER(RTRIM(b.EmpName)) = UPPER(RTRIM(e.LName))) AND e.SwipeType = 'IN' AND a.DT = e.SwipeDate
			--LEFT JOIN tas.Vw_ContractorSwipe f ON (a.EmpNo = f.EmpNo OR UPPER(RTRIM(b.EmpName)) = UPPER(RTRIM(f.LName))) AND f.SwipeType = 'OUT' AND a.DT = f.SwipeDate
			LEFT JOIN tas.Master_ShiftTimes g ON RTRIM(a.ShiftPatCode) = RTRIM(g.ShiftPatCode) AND RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)) = RTRIM(g.ShiftCode)
		WHERE 
			ISNULL(a.IsSalStaff, 0) = 0
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
				RTRIM(b.BusinessUnit) IN (SELECT GenericNo FROM tas.fnParseStringArrayToInt(@costCenterList, ','))	--Rev. #1.4
				OR
				@costCenterList IS NULL
			)

		UNION

		--Salary Staff
		SELECT	a.BusinessUnit,
				c.BUname AS BusinessUnitName,
				a.EmpNo,
				b.EmpName,
				b.Position,
				a.DT,
				a.dtIN,
				a.dtOUT,
				a.NetMinutes,
				--CASE WHEN a.dtIN IS NOT NULL AND a.dtOUT IS NOT NULL
				--	THEN DATEDIFF(n, a.dtIN, a.dtOUT)
				--	ELSE NULL
				--END AS NetMinutes,
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
				a.GradeCode
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_view c ON RTRIM(a.BusinessUnit) = RTRIM(c.BU)
			LEFT JOIN tas.GetRemark02 d ON a.AutoId = d.AutoId
			LEFT JOIN tas.Master_ShiftTimes e ON RTRIM(a.ShiftPatCode) = RTRIM(e.ShiftPatCode) AND RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)) = RTRIM(e.ShiftCode)
		WHERE 
			a.IsSalStaff = 1
			AND ISNULL(a.IsDayWorker_OR_Shifter, 0) = 0
			AND ISNUMERIC(b.PayStatus) = 1
			--AND CONVERT(INT, a.GradeCode) <= 9	--Rev. #1.3
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
				RTRIM(b.BusinessUnit) IN (SELECT GenericNo FROM tas.fnParseStringArrayToInt(@costCenterList, ','))	--Rev. #1.4
				OR
				@costCenterList IS NULL
			)
		ORDER BY IsSalStaff, BusinessUnit, EmpNo, a.dtIN
    END 

GO 
            

/*	Debug:

PARAMETERS:
	@employeeType		TINYINT,
	@startDate			DATETIME,
	@endDate			DATETIME,
	@costCenterList		VARCHAR(500) = ''

	EXEC tas.Pr_GetDailyAttendanceReport 0, '01/06/2017', '01/06/2017', '4100'			--Non-salary Staff (4 sec)
	EXEC tas.Pr_GetDailyAttendanceReport 1, '11/30/2016', '11/30/2016', '7600'			--Salary Staff (1 sec)
	EXEC tas.Pr_GetDailyAttendanceReport 2, '04/12/2017', '04/12/2017', '4200'			--Both	(6 sec)

*/