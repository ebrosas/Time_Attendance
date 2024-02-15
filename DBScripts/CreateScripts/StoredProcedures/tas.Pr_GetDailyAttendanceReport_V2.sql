/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetDailyAttendanceReport_V2
*	Description: Get data for the Daily Attendance Report
*
*	Date			Author		Rev. #		Comments:
*	13/11/2016		Ervin		1.0			Created
*	04/12/2016		Ervin		1.1			Modified the Order By clause
*	13/04/2017		Ervin		1.2			Fetch the cost center from "Master_Employee_JDE_View_V2" view
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetDailyAttendanceReport_V2
(   
	@employeeType		TINYINT,	
	@startDate			DATETIME,
	@endDate			DATETIME,
	@costCenter			VARCHAR(12)
)
AS

	DECLARE	@CONST_MIN_SHIFT_ALLOWANCE INT
    SELECT	@CONST_MIN_SHIFT_ALLOWANCE = a.Minutes_MinShiftAllowance
	FROM tas.System_Values a

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
				END AS Remark 
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_view c ON RTRIM(a.BusinessUnit) = RTRIM(c.BU)
			LEFT JOIN tas.GetRemark02 d ON a.AutoId = d.AutoId
			--LEFT JOIN tas.Vw_ContractorSwipe e ON (a.EmpNo = e.EmpNo OR UPPER(RTRIM(b.EmpName)) = UPPER(RTRIM(e.LName))) AND e.SwipeType = 'IN' AND a.DT = e.SwipeDate
			--LEFT JOIN tas.Vw_ContractorSwipe f ON (a.EmpNo = f.EmpNo OR UPPER(RTRIM(b.EmpName)) = UPPER(RTRIM(f.LName))) AND f.SwipeType = 'OUT' AND a.DT = f.SwipeDate
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
			AND RTRIM(b.BusinessUnit) = RTRIM(@costCenter)	--Rev #1.2
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
				d.LVDesc + d.RMdesc + d.RAdesc + d.TxDesc + d.TxtShiftSpan + d.DayOff + d.Resigned + d.OtherRemarks +  d.ShiftCodeDifference + d.CustomRemarks AS Remark
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_view c ON RTRIM(a.BusinessUnit) = RTRIM(c.BU)
			LEFT JOIN tas.GetRemark02 d ON a.AutoId = d.AutoId
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
			AND RTRIM(b.BusinessUnit) = RTRIM(@costCenter)	--Rev #1.2
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
				END AS Remark 
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_view c ON RTRIM(a.BusinessUnit) = RTRIM(c.BU)
			LEFT JOIN tas.GetRemark02 d ON a.AutoId = d.AutoId
			--LEFT JOIN tas.Vw_ContractorSwipe e ON (a.EmpNo = e.EmpNo OR UPPER(RTRIM(b.EmpName)) = UPPER(RTRIM(e.LName))) AND e.SwipeType = 'IN' AND a.DT = e.SwipeDate
			--LEFT JOIN tas.Vw_ContractorSwipe f ON (a.EmpNo = f.EmpNo OR UPPER(RTRIM(b.EmpName)) = UPPER(RTRIM(f.LName))) AND f.SwipeType = 'OUT' AND a.DT = f.SwipeDate
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
			AND RTRIM(b.BusinessUnit) = RTRIM(@costCenter)	--Rev #1.2

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
				d.LVDesc + d.RMdesc + d.RAdesc + d.TxDesc + d.TxtShiftSpan + d.DayOff + d.Resigned + d.OtherRemarks +  d.ShiftCodeDifference + d.CustomRemarks AS Remark
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
			LEFT JOIN tas.Master_BusinessUnit_JDE_view c ON RTRIM(a.BusinessUnit) = RTRIM(c.BU)
			LEFT JOIN tas.GetRemark02 d ON a.AutoId = d.AutoId
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
			AND RTRIM(b.BusinessUnit) = RTRIM(@costCenter)	--Rev #1.2
		ORDER BY IsSalStaff, BusinessUnit, EmpNo, a.dtIN
    END 

GO 
            

/*	Debug:

PARAMETERS:
	@employeeType		TINYINT,	
	@startDate			DATETIME,
	@endDate			DATETIME,
	@costCenter			VARCHAR(12)

	EXEC tas.Pr_GetDailyAttendanceReport_V2 0, '01/03/2016', '31/03/2016', '5200'		--Non-salary Staff (2 sec)
	EXEC tas.Pr_GetDailyAttendanceReport_V2 1, '11/30/2016', '11/30/2016', '7600'		--Salary Staff (1 sec)
	EXEC tas.Pr_GetDailyAttendanceReport_V2 2, '01/03/2016', '31/03/2016', '5200'		--Both	(6 sec)

*/