USE [tas2]
GO

/****** Object:  View [tas].[RPT_DailyAttendance01]    Script Date: 31/10/2016 13:34:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER VIEW [tas].[RPT_DailyAttendance01] AS 
SELECT DISTINCT
	m.empName,
       	t.EmpNo,
       	t.DT,
       	t.dtIn,
       	t.DtOUT,
	t.ShiftPatCode,
       	t.Actual_ShiftCode,
       	--t.ShiftCode,
       	t.OTType,
	tas.lpad(DATEPART(HH,OTStartTime) , 2, '0') + tas.lpad(DATEPART( n,OTStartTime) , 2, '0') OTStartTime,
	tas.lpad(DATEPART(HH,OTEndTime)   , 2, '0') + tas.lpad(DATEPART( n,OTEndTime)   , 2, '0') OTEndTime,
       	t.NoPayHours,
 	RTRIM(LTRIM(t.BusinessUnit)) BusinessUnit,
	t.ShiftAllowance,
	t.Duration_ShiftAllowance_Evening,
	t.Duration_ShiftAllowance_Night,

--Allowance---------------
 (CASE 	WHEN t.Duration_ShiftAllowance_Evening  >=Minutes_MinShiftAllowance 
             AND t.Duration_ShiftAllowance_Night>=Minutes_MinShiftAllowance
             THEN CONVERT(VARCHAR(1),t.ShiftCode) + ' '+ CONVERT(VARCHAR(1), '3')--'Eve/Nght'
	WHEN t.Duration_ShiftAllowance_Night    >=Minutes_MinShiftAllowance 
             THEN CONVERT(VARCHAR(1),t.ShiftCode) + ' '+ CONVERT(VARCHAR(1), '2')--'Night'
	WHEN t.Duration_ShiftAllowance_Evening  >=Minutes_MinShiftAllowance 
             THEN CONVERT(VARCHAR(1),t.ShiftCode) + ' '+ CONVERT(VARCHAR(1), '1')--'Evening'
        ELSE      CONVERT(VARCHAR(1),t.ShiftCode) + ' '+ CONVERT(VARCHAR(1), '0')--'nothing'
 END) [ShiftCode],
--------------------------

	(CASE ISNULL(t.MealVoucherEligibility,'N')
		WHEN 'Y' THEN '[Eligible for Meal Voucher?] '
		WHEN 'YA' THEN '[Meal Voucher Granted] '
		ELSE ''
	 END)
       	+  LVDesc + RMdesc + RAdesc + TxDesc +  TxtShiftSpan + DayOff + Resigned + OtherRemarks + ShiftCodeDifference + CustomRemarks  AS Remark, 
       	B.BusinessUnitName,
	' ' IsItNextDay,
       t.IsSalStaff,
       t.IsDayWorker_OR_Shifter,
	t.ShiftSpanDate,
	T.ShiftSpan,

	(CASE 	WHEN ShiftSpanDate IS NOT NULL 				THEN 1
	  	WHEN ShiftSpan IS NULL AND ShiftspanDate IS NULL 	THEN 2
	  	WHEN ShiftSpan = 1  					THEN 3
	END) Sort1
	,
	(CASE 	WHEN dtin  IS NOT NULL THEN dtin 
		WHEN dtout IS NOT NULL THEN dtout 
	END) Sort2
	,	
	(CASE 	WHEN dtin IS NOT NULL AND dtout IS NULL     THEN 1
		WHEN dtin IS NOT NULL AND dtout IS NOT NULL THEN 2 
		WHEN dtin IS NULL AND dtout IS NOT NULL     THEN 3 
		WHEN dtin IS NULL AND dtout IS NULL THEN 4 
	END ) Sort3

FROM  
	tran_timesheet t, 
	Master_Employee_JDE M ,
	Master_BusinessUnit_JDE B,
	GetRemark02 R,
	system_Values

WHERE 	m.empno=t.empno
	AND   LTRIM(t.businessUnit)=LTRIM(B.businessUnit)
	AND   T.AutoID=R.AutoID
	AND t.IsSalStaff=0
	

GO


