USE [tas2]
GO
/****** Object:  UserDefinedFunction [tas].[fnGetNotPuctualManagers]    Script Date: 02/02/2022 13:48:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetNotPuctualManagers
*	Description: This UDF is used to fetch the unpunctual managers
*
*	Date:			Author:		Rev.#:		Comments:
*	27/07/2017		Ervin		1.0			Created
*	11/09/2017		Ervin		1.1			Modified the logic in identifying the shift code for Shift Worker employees. Added In-lieu as holiday
*	11/07/2019		Ervin		1.2			Modified the filter condition to return only Grade 12 Managers
*******************************************************************************************************************************************************/

ALTER FUNCTION [tas].[fnGetNotPuctualManagers]
(
	@startDate					DATETIME,
	@endDate					DATETIME,
	@costCenter					VARCHAR(12),
	@occurenceLimit				INT,
	@lateAttendanceThreshold	INT,
	@earlyLeavingThreshold		INT = 0 
)
RETURNS @rtnTable
TABLE	
(	
	CostCenter VARCHAR(12),
	EmpNo INT NOT NULL,
	PayGrade INT NOT NULL,
	Occurence INT  
)
AS
BEGIN

	DECLARE @myTable TABLE 
	(		
		CostCenter VARCHAR(12),
		EmpNo INT,
		PayGrade INT,
		Occurence INT 
	)

	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@occurenceLimit, 0) = 0			--(Note: The default occurence limit is 3)
		SET @occurenceLimit = 3			

	IF ISNULL(@lateAttendanceThreshold, 0) = 0	--(Note: Default value is 5 minutes)
		SET @lateAttendanceThreshold = 5	

	IF ISNULL(@earlyLeavingThreshold, 0) = 0	--(Note: Default value is 5 minutes)
		SET @earlyLeavingThreshold = 5	

	--Populate data to the table
	INSERT INTO @myTable  
	SELECT	x.CostCenter, x.EmpNo, x.GradeCode, COUNT(x.EmpNo) AS Occurence			
	FROM
    (
		--Get managers who are late
		SELECT DISTINCT 
			a.EmpNo,
			c.GradeCode,
			a.BusinessUnit AS CostCenter,
			a.DT
		FROM tas.Tran_Timesheet a
			CROSS APPLY
			(
				SELECT TOP 1 dtIN, dtOUT 
				FROM tas.Tran_Timesheet 
				WHERE DT = a.DT
					AND dtIN IS NOT NULL 
					AND dtOUT IS NOT NULL 
					AND EmpNo = a.EmpNo
			) b
			INNER JOIN tas.Master_Employee_JDE_View_V2 c ON a.EmpNo = c.EmpNo
			LEFT JOIN tas.FlexiTimeSetting e ON RTRIM(a.ShiftPatCode) = RTRIM(e.ShiftPatCode)
			LEFT JOIN tas.Master_ShiftPatternTitles g ON RTRIM(a.ShiftPatCode) = RTRIM(g.ShiftPatCode)
			LEFT JOIN tas.Master_ShiftTimes f ON RTRIM(a.ShiftPatCode) = RTRIM(f.ShiftPatCode) --AND RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)) = RTRIM(f.ShiftCode)
				AND CASE WHEN g.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O'	--Rev. #1.1
						THEN a.ShiftCode
						ELSE 
							CASE WHEN (DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, f.ArrivalTo)) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))  
								THEN RTRIM(a.Actual_ShiftCode)
								ELSE RTRIM(a.ShiftCode)
							END
					END = RTRIM(f.ShiftCode)
		WHERE 
			a.DT BETWEEN @startDate AND @endDate
			AND a.dtIN IS NOT NULL 
			AND a.dtOUT IS NOT NULL 
			AND ISNULL(a.LeaveType, '') = ''
			AND ISNULL(a.AbsenceReasonCode, '') = ''
			AND ISNULL(a.DIL_Entitlement, '') = ''
			AND ISNULL(a.IsPublicHoliday, 0) = 0
			AND NOT (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1)		--Rev. #1.1
			AND ISNULL(a.IsDriver, 0) = 0
			AND ISNULL(a.IsLiasonOfficer, 0) = 0
			AND ISNULL(a.IsHedger, 0) = 0
			AND ISNULL(a.ShiftSpan, 0) = 0	--Rev. #1.1
			AND 
			(
				CASE WHEN g.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
					THEN a.ShiftCode
					ELSE RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode))
				END <> 'O'
			)
			AND ISNUMERIC(c.PayStatus) = 1
			AND 
			(
				DATEDIFF(MINUTE, CAST(CASE WHEN e.SettingID IS NOT NULL THEN e.NormalArrivalTo ELSE f.ArrivalTo END AS TIME), CAST(b.dtIN AS TIME)) > @lateAttendanceThreshold
				AND
                DATEDIFF(MINUTE, CAST(CASE WHEN e.SettingID IS NOT NULL THEN e.NormalArrivalTo ELSE f.ArrivalTo END AS TIME), CAST(b.dtIN AS TIME)) <
					(
						CASE WHEN CAST(f.DepartFrom AS TIME) > CAST(f.ArrivalTo AS TIME)
							THEN DATEDIFF(MINUTE, f.ArrivalTo, f.DepartFrom)
							ELSE 1440 + DATEDIFF(MINUTE, f.ArrivalTo, f.DepartFrom)
						END
					)
			)			
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND c.GradeCode = 12

		UNION 	
        
		--Get employees who leave early from work
		SELECT	a.EmpNo,
				b.GradeCode,
				RTRIM(a.BusinessUnit) AS CostCenter,
				a.DT
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_Employee_JDE_View_V2 b ON a.EmpNo = b.EmpNo
			CROSS APPLY
			(
				SELECT TOP 1 dtIN
				FROM tas.Tran_Timesheet 
				WHERE DT = a.DT
					AND dtIN IS NOT NULL 
					AND dtOUT IS NOT NULL 
					AND EmpNo = a.EmpNo
			) c
			LEFT JOIN tas.Master_ShiftPatternTitles f ON RTRIM(a.ShiftPatCode) = RTRIM(f.ShiftPatCode)
			LEFT JOIN tas.Master_ShiftTimes d ON RTRIM(a.ShiftPatCode) = RTRIM(d.ShiftPatCode) --AND RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode)) = RTRIM(d.ShiftCode)
				AND CASE WHEN f.IsDayShift = 1	
						THEN a.ShiftCode
						ELSE 
							--CASE WHEN a.Duration_Required > 0 AND a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2))  --(Note: If total work duration is greater than or equals to the required work duration plus 1/2 of it then shift code will be based on the value of "Actual_ShiftCode" field)
							CASE WHEN (DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, d.ArrivalTo)) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))  
								THEN RTRIM(a.Actual_ShiftCode)
								ELSE RTRIM(a.ShiftCode)
							END
					END = RTRIM(d.ShiftCode)
			LEFT JOIN tas.FlexiTimeSetting e ON RTRIM(a.ShiftPatCode) = RTRIM(e.ShiftPatCode)
		WHERE 
			a.dtIN IS NOT NULL 
			AND a.dtOUT IS NOT NULL 
			AND ISNULL(a.LeaveType, '') = ''
			AND ISNULL(a.AbsenceReasonCode, '') = ''
			AND ISNULL(a.DIL_Entitlement, '') = ''
			AND ISNULL(a.IsPublicHoliday, 0) = 0
			AND NOT (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1)		--Rev. #1.1
			AND ISNULL(a.IsDriver, 0) = 0
			AND ISNULL(a.IsLiasonOfficer, 0) = 0
			AND ISNULL(a.IsHedger, 0) = 0
			AND 
			(
				CASE WHEN f.IsDayShift = 1 
					THEN a.ShiftCode
					ELSE RTRIM(ISNULL(a.Actual_ShiftCode, a.ShiftCode))
				END <> 'O'
			)
			AND ISNUMERIC(b.PayStatus) = 1
			AND a.IsLastRow = 1
			AND ISNULL(a.ShiftSpan, 0) = 0	--Rev. #1.1
			AND DATEDIFF
				(
					MINUTE, 
					CONVERT(TIME, A.dtOUT), 
					CONVERT(TIME, CASE WHEN e.SettingID IS NOT NULL
										THEN DATEADD
											(
												MINUTE, 
												CASE WHEN CAST(d.DepartFrom AS TIME) > CAST(d.ArrivalTo AS TIME)
													THEN DATEDIFF(MINUTE, d.ArrivalTo, d.DepartFrom)
													ELSE 1440 + DATEDIFF(MINUTE, d.ArrivalTo, d.DepartFrom)
												END, 
												c.dtIN
											)
										ELSE d.DepartFrom
									END)
				) > @earlyLeavingThreshold 
			AND a.DT BETWEEN @startDate AND @endDate
			AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND b.GradeCode = 12
	) x
	GROUP BY x.CostCenter, x.EmpNo, x.GradeCode 
	HAVING (COUNT(x.EmpNo) >= @occurenceLimit)

	INSERT INTO @rtnTable 
	SELECT * FROM @mytable 
	ORDER BY CostCenter, EmpNo

	RETURN 
END 

/*	Debugging:

PARAMETERS:
	@startDate					DATETIME,
	@endDate					DATETIME,
	@costCenter					VARCHAR(12),
	@occurenceLimit				INT,
	@lateAttendanceThreshold	INT,
	@earlyLeavingThreshold		INT = 0 

	--Test server
	SELECT * FROM tas.fnGetNotPuctualManagers('29/01/2016', '04/02/2016', '', 0, 0, 0)			--By cost center
	SELECT * FROM tas.fnGetNotPuctualManagers('29/01/2016', '04/02/2016', '', 0, 0, 0)			--All

	--Production server
	SELECT * FROM tas.fnGetNotPuctualManagers('07/07/2019', '07/13/2019', '', 0, 0, 0)			--By cost center
	SELECT * FROM tas.fnGetNotPuctualManagers('07/07/2019', '07/13/2019', '', 0, 0, 0)			--All

*/
	

	