/***********************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.External_DSX_evnlog
*	Description: Get the swipe data from the swipe system
*
*	Date			Author		Revision No.		Comments
*	18/07/2006		Khuzema		1.0					Created
*	30/10/2016		Ervin		1.1					Refactored the logic to get the correct Time In/Out of contractors
*************************************************************************************************************************************************************************/

ALTER VIEW tas.RPT_DailyAttendance01 
AS 

	SELECT DISTINCT
		b.empName,
       	a.EmpNo,
       	a.DT,

		--Start of Rev. #1.1
		CASE WHEN a.dtIN IS NULL AND b.GradeCode = 0
			THEN e.SwipeTime
			ELSE a.dtIn
		END AS dtIn,
		CASE WHEN a.dtOUT IS NULL AND b.GradeCode = 0
			THEN f.SwipeTime
			ELSE a.dtOUT
		END AS DtOUT,
		--End of Rev. #1.1

		a.ShiftPatCode,
       	a.Actual_ShiftCode,
       	a.OTType,
		tas.lpad(DATEPART(HH,OTStartTime) , 2, '0') + tas.lpad(DATEPART( n,OTStartTime) , 2, '0') OTStartTime,
		tas.lpad(DATEPART(HH,OTEndTime)   , 2, '0') + tas.lpad(DATEPART( n,OTEndTime)   , 2, '0') OTEndTime,
       	a.NoPayHours,
 		RTRIM(LTRIM(a.BusinessUnit)) BusinessUnit,
		a.ShiftAllowance,
		a.Duration_ShiftAllowance_Evening,
		a.Duration_ShiftAllowance_Night,

		--Allowance---------------
		 (CASE 	WHEN a.Duration_ShiftAllowance_Evening  >=Minutes_MinShiftAllowance 
					 AND a.Duration_ShiftAllowance_Night>=Minutes_MinShiftAllowance
					 THEN CONVERT(VARCHAR(1),a.ShiftCode) + ' '+ CONVERT(VARCHAR(1), '3')--'Eve/Nght'
			WHEN a.Duration_ShiftAllowance_Night    >=Minutes_MinShiftAllowance 
					 THEN CONVERT(VARCHAR(1),a.ShiftCode) + ' '+ CONVERT(VARCHAR(1), '2')--'Night'
			WHEN a.Duration_ShiftAllowance_Evening  >=Minutes_MinShiftAllowance 
					 THEN CONVERT(VARCHAR(1),a.ShiftCode) + ' '+ CONVERT(VARCHAR(1), '1')--'Evening'
				ELSE      CONVERT(VARCHAR(1),a.ShiftCode) + ' '+ CONVERT(VARCHAR(1), '0')--'nothing'
		 END) [ShiftCode],
		--------------------------

		--Start of Rev. #1.1
		CASE WHEN b.GradeCode = 0 
				AND (CASE WHEN a.dtIN IS NULL AND b.GradeCode = 0 THEN e.SwipeTime ELSE a.dtIn END) IS NOT NULL
				AND (CASE WHEN a.dtOUT IS NULL AND b.GradeCode = 0 THEN f.SwipeTime ELSE a.dtOUT END) IS NOT NULL
			THEN ''
			ELSE 
				CASE ISNULL(a.MealVoucherEligibility,'N')
					WHEN 'Y' THEN '[Eligible for Meal Voucher?] '
					WHEN 'YA' THEN '[Meal Voucher Granted] '
					ELSE ''
				END +  LVDesc + RMdesc + RAdesc + TxDesc +  TxtShiftSpan + DayOff + Resigned + OtherRemarks + ShiftCodeDifference + CustomRemarks
		END AS Remark, 
		--End of Rev. #1.1

       	c.BusinessUnitName,
		' ' IsItNextDay,
		a.IsSalStaff,
		a.IsDayWorker_OR_Shifter,
		a.ShiftSpanDate,
		a.ShiftSpan,

		CASE 	WHEN ShiftSpanDate IS NOT NULL 				THEN 1
	  		WHEN ShiftSpan IS NULL AND ShiftspanDate IS NULL 	THEN 2
	  		WHEN ShiftSpan = 1  					THEN 3
		END AS Sort1,
		CASE 	WHEN dtin  IS NOT NULL THEN dtin 
			WHEN dtout IS NOT NULL THEN dtout 
		END AS Sort2,	
		CASE 	WHEN dtin IS NOT NULL AND dtout IS NULL     THEN 1
			WHEN dtin IS NOT NULL AND dtout IS NOT NULL THEN 2 
			WHEN dtin IS NULL AND dtout IS NOT NULL     THEN 3 
			WHEN dtin IS NULL AND dtout IS NULL THEN 4 
		END AS Sort3
	FROM tas.Tran_Timesheet a 
		INNER JOIN tas.Master_Employee_JDE b ON a.EmpNo = b.EmpNo
		INNER JOIN tas.Master_BusinessUnit_JDE c ON LTRIM(RTRIM(a.BusinessUnit)) = LTRIM(RTRIM(c.BusinessUnit))
		LEFT JOIN tas.GetRemark02 d ON a.AutoID = d.AutoId
		LEFT JOIN tas.Vw_ContractorSwipe e ON (a.EmpNo = e.EmpNo OR UPPER(RTRIM(b.EmpName)) = UPPER(RTRIM(e.LName))) AND e.SwipeType = 'IN' AND a.DT = e.SwipeDate
		LEFT JOIN tas.Vw_ContractorSwipe f ON (a.EmpNo = f.EmpNo OR UPPER(RTRIM(b.EmpName)) = UPPER(RTRIM(f.LName))) AND f.SwipeType = 'OUT' AND a.DT = f.SwipeDate,
		tas.System_Values
	WHERE 	
		a.IsSalStaff = 0

GO

/*	Debug:

	SELECT * FROM tas.RPT_DailyAttendance01 a
	WHERE RTRIM(a.BusinessUnit) = '7600'
		AND a.DT = '10/30/2016'
	ORDER BY a.DT DESC, a.EmpNo

*/


