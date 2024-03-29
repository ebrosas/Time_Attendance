USE [tas2]
GO
/****** Object:  UserDefinedFunction [tas].[fnGetNotPuctualEmployees]    Script Date: 26/05/2018 11:09:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/******************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetNotPuctualEmployees
*	Description: Converts minute input value into hours with the following format: HH:mm
*
*	Date:			Author:		Rev.#:		Comments:
*	10/07/2017		Ervin		1.0			Created
*	22/07/2017		Ervin		1.1			Refactored the logic in indetifying the shift code. Also, exclude records where ShiftSpan = 1
*	26/07/2017		Ervin		1.2			Added filter to exclude all Grade 12 and Above 
*	11/09/2017		Ervin		1.3			Modified the logic in identifying the shift code for Shift Worker employees. Added In-lieu as holiday
*******************************************************************************************************************************************************/

ALTER FUNCTION [tas].[fnGetNotPuctualEmployees]
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
	Occurence INT  
)
AS
BEGIN

	DECLARE @myTable TABLE 
	(		
		CostCenter VARCHAR(12),
		EmpNo INT,
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
	SELECT	x.CostCenter, x.EmpNo, COUNT(x.EmpNo) AS Occurence			
	FROM
    (
		--Get employees who are late
		SELECT DISTINCT 
			a.EmpNo,
			RTRIM(c.BusinessUnit) AS CostCenter,
			a.DT
			--'Late' AS Remarks  			
			--b.dtIN,
			--b.dtOUT,
			--CASE WHEN e.SettingID IS NOT NULL 
			--	THEN e.NormalArrivalTo
			--	ELSE f.ArrivalTo
			--END AS MaxArrivalTime,
			--DATEDIFF(MINUTE, CAST(CASE WHEN e.SettingID IS NOT NULL THEN e.NormalArrivalTo ELSE f.ArrivalTo END AS TIME), CAST(b.dtIN AS TIME)) AS ArrivalTimeDiff
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
			LEFT JOIN tas.Master_ShiftPatternTitles f ON RTRIM(a.ShiftPatCode) = RTRIM(f.ShiftPatCode)
			LEFT JOIN tas.Master_ShiftTimes g ON RTRIM(a.ShiftPatCode) = RTRIM(g.ShiftPatCode) AND RTRIM(a.ShiftCode) = RTRIM(g.ShiftCode)	--Based on the scheduled shift
			LEFT JOIN tas.Master_ShiftTimes h ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) 
				AND
				(
					CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O'	--Rev. #1.1
						THEN a.ShiftCode
						ELSE 
							CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, g.ArrivalTo))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))  
								THEN RTRIM(a.Actual_ShiftCode)
								ELSE RTRIM(a.ShiftCode)
							END
					END
				) = RTRIM(h.ShiftCode)
		WHERE 
			a.DT BETWEEN @startDate AND @endDate
			AND a.dtIN IS NOT NULL 
			AND a.dtOUT IS NOT NULL 
			AND ISNULL(a.LeaveType, '') = ''
			AND ISNULL(a.AbsenceReasonCode, '') = ''
			AND ISNULL(a.DIL_Entitlement, '') = ''
			AND ISNULL(a.IsPublicHoliday, 0) = 0
			AND NOT (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1)		--Rev. #1.3
			AND ISNULL(a.IsDriver, 0) = 0
			AND ISNULL(a.IsLiasonOfficer, 0) = 0
			AND ISNULL(a.IsHedger, 0) = 0
			AND ISNULL(a.ShiftSpan, 0) = 0	--Rev. #1.1
			AND 
			(
				CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
					THEN a.ShiftCode
					ELSE 
						CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, h.ArrivalTo))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
							THEN RTRIM(a.Actual_ShiftCode)
							ELSE RTRIM(a.ShiftCode)
						END		--Rev. #1.3
				END <> 'O'
			)
			AND ISNUMERIC(c.PayStatus) = 1
			AND 
			(
				DATEDIFF(MINUTE, CAST(CASE WHEN e.SettingID IS NOT NULL THEN e.NormalArrivalTo ELSE h.ArrivalTo END AS TIME), CAST(b.dtIN AS TIME)) > @lateAttendanceThreshold
				AND
                DATEDIFF(MINUTE, CAST(CASE WHEN e.SettingID IS NOT NULL THEN e.NormalArrivalTo ELSE h.ArrivalTo END AS TIME), CAST(b.dtIN AS TIME)) <
					(
						CASE WHEN CAST(h.DepartFrom AS TIME) > CAST(h.ArrivalTo AS TIME)
							THEN DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
							ELSE 1440 + DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
						END
					)
			)			
			AND (RTRIM(c.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND c.GradeCode <= 11	--Rev. #1.2

		UNION 	
        
		--Get employees who leave early from work
		SELECT	a.EmpNo,
				RTRIM(b.BusinessUnit) AS CostCenter,
				a.DT
				--'Left early' AS Remarks
				--a.ShiftPatCode,
				--ISNULL(a.Actual_ShiftCode, a.ShiftPatCode) AS ShiftCode,				
				--c.dtIN AS FirstTimeIn,
				--a.dtOUT AS LastTimeOut,
				--CASE WHEN CAST(d.DepartFrom AS TIME) > CAST(d.ArrivalTo AS TIME)
				--	THEN DATEDIFF(MINUTE, d.ArrivalTo, d.DepartFrom)
				--	ELSE 1440 + DATEDIFF(MINUTE, d.ArrivalTo, d.DepartFrom)
				--END AS Duration_Required,
				--CASE WHEN e.SettingID IS NOT NULL
				--	THEN DATEADD
				--		(
				--			MINUTE, 
				--			CASE WHEN CAST(d.DepartFrom AS TIME) > CAST(d.ArrivalTo AS TIME)
				--				THEN DATEDIFF(MINUTE, d.ArrivalTo, d.DepartFrom)
				--				ELSE 1440 + DATEDIFF(MINUTE, d.ArrivalTo, d.DepartFrom)
				--			END, 
				--			c.dtIN
				--		)
				--	ELSE d.DepartFrom
				--END AS Required_TimeOut
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
			LEFT JOIN tas.Master_ShiftTimes g ON RTRIM(a.ShiftPatCode) = RTRIM(g.ShiftPatCode) AND RTRIM(a.ShiftCode) = RTRIM(g.ShiftCode)	--Based on the scheduled shift
			LEFT JOIN tas.Master_ShiftTimes h ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode) 
				AND 
				(
					CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 	--Rev. #1.1
						THEN a.ShiftCode
						ELSE 
							CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, g.ArrivalTo))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
								THEN RTRIM(a.Actual_ShiftCode)
								ELSE RTRIM(a.ShiftCode)
							END		
					END
				) = RTRIM(h.ShiftCode)
			LEFT JOIN tas.FlexiTimeSetting e ON RTRIM(a.ShiftPatCode) = RTRIM(e.ShiftPatCode)
		WHERE 
			a.dtIN IS NOT NULL 
			AND a.dtOUT IS NOT NULL 
			AND ISNULL(a.LeaveType, '') = ''
			AND ISNULL(a.AbsenceReasonCode, '') = ''
			AND ISNULL(a.DIL_Entitlement, '') = ''
			AND ISNULL(a.IsPublicHoliday, 0) = 0
			AND NOT (a.IsDILdayWorker = 1 AND a.IsDayWorker_OR_Shifter = 1)		--Rev. #1.3
			AND ISNULL(a.IsDriver, 0) = 0
			AND ISNULL(a.IsLiasonOfficer, 0) = 0
			AND ISNULL(a.IsHedger, 0) = 0
			AND 
			(
				CASE WHEN f.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
					THEN a.ShiftCode
					ELSE 
						CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, a.dtIN), CONVERT(TIME, h.ArrivalTo))) > a.Duration_Required / 2) OR (a.Duration_Worked_Cumulative >= (a.Duration_Required + (a.Duration_Required / 2)))
							THEN RTRIM(a.Actual_ShiftCode)
							ELSE RTRIM(a.ShiftCode)
						END		--Rev. #1.3
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
												CASE WHEN CAST(h.DepartFrom AS TIME) > CAST(h.ArrivalTo AS TIME)
													THEN DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
													ELSE 1440 + DATEDIFF(MINUTE, h.ArrivalTo, h.DepartFrom)
												END, 
												c.dtIN
											)
										ELSE h.DepartFrom
									END)
				) > @earlyLeavingThreshold 
			AND a.DT BETWEEN @startDate AND @endDate
			AND (RTRIM(b.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
			AND b.GradeCode <= 11	--Rev. #1.2
	) x
	GROUP BY x.EmpNo, x.CostCenter
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
	SELECT * FROM tas.fnGetNotPuctualEmployees('01/01/2016', '07/01/2016', '7550', 0, 0, 0)		--By cost center
	SELECT * FROM tas.fnGetNotPuctualEmployees('01/01/2016', '07/01/2016', '', 0, 0, 0)			--All

	--Production server
	SELECT * FROM tas.fnGetNotPuctualEmployees('09/03/2017', '09/09/2017', '3320', 0, 0, 0)		--By cost center
	SELECT * FROM tas.fnGetNotPuctualEmployees('07/16/2017', '07/22/2017', '', 0, 0, 0)			--All

*/
	

	