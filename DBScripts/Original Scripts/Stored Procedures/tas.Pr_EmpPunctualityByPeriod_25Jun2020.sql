USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_EmpPunctualityByPeriod]    Script Date: 25/06/2020 15:37:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_EmpPunctualityByPeriod
*	Description: This stored procedure is used to fetch the data for the Employee Monthly Punctuality Report
*
*	Date:			Author:		Rev.#:		Comments:
*	12/02/2018		Ervin		1.0			Created
*	15/05/2018		Ervin		1.1			Implemented Ramadan timing logic in the code
*	26/05/2018		Ervin		1.2			Added filter condition to return records where occurence is equal or greater than the Occurence Limit
*	26/07/2018		Ervin		1.3			Added extra logic to determine the shift code to use
*	06/01/2019		Ervin		1.4			Added filter condition to exclude attendance records where LeaveType is not null
*	20/05/2019		Ervin		1.5			Added "SET NOCOUNT ON" header declaration
*	10/06/2020		Ervin		1.6			Commented block of code
**************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_EmpPunctualityByPeriod]
(
	@startDate			DATETIME,
	@endDate			DATETIME,
	@costCenter			VARCHAR(12) = '',
	@empNo				INT = 0
)
AS 
	
	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	DECLARE	@CONST_OCCURENCE_LIMIT	INT
	SET @CONST_OCCURENCE_LIMIT = 2

	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL 

	SELECT	DISTINCT
			a.EmpNo, 
			b.EmpName, 
			a.CostCenter, 
			RTRIM(c.BUname) AS CostCenterName,
			a.DT,
			CASE WHEN e.dtIN IS NULL 
				THEN CONVERT(DATETIME, '')
				ELSE e.dtIN
			END AS dtIN,
		
			CASE WHEN d.dtOUT IS NULL 
				THEN CONVERT(DATETIME, '')
				ELSE d.dtOUT
			END AS dtOUT,
			d.ShiftPatCode,
			d.ShiftCode,
			d.Actual_ShiftCode,
			a.CreatedDate,
			CASE 
				WHEN a.PunctualityTypeID = 1 THEN 'Late' 
				WHEN a.PunctualityTypeID = 2 THEN 'Left Early'
				WHEN a.PunctualityTypeID = 3 THEN 'Late and Left Early'
				ELSE ''
			END AS Remarks,
			a.TotalDuration AS TotalLostTime,
			f.ReportOccurenceCount,
			a.PunctualityTypeID,
			ISNULL
			(
				DATEDIFF
				(
					MINUTE, 
					CAST
					(
						CASE WHEN g.SettingID IS NOT NULL 
							THEN CASE WHEN d.isRamadan = 1 THEN g.RamadanArrivalTo ELSE g.NormalArrivalTo END 
							ELSE CASE WHEN d.isRamadan = 1 THEN j.RArrivalTo ELSE j.ArrivalTo END 
						END AS TIME
					), 
					CAST(e.dtIN AS TIME)
				)
			, 0) AS ArrivalTimeDiff,
			DATEDIFF
			(
				MINUTE,			
				CONVERT(TIME, CASE WHEN d.dtOUT IS NULL 
									THEN CONVERT(DATETIME, '')
									ELSE d.dtOUT
								END),
				CONVERT(TIME, CASE WHEN g.SettingID IS NOT NULL
								THEN DATEADD
									(
										MINUTE, 
										CASE WHEN CAST(CASE WHEN d.isRamadan = 1 THEN j.RDepartFrom ELSE j.DepartFrom END AS TIME) > CAST(CASE WHEN d.isRamadan = 1 THEN j.RArrivalTo ELSE j.ArrivalTo END AS TIME)
											THEN DATEDIFF(MINUTE, CASE WHEN d.isRamadan = 1 THEN j.RArrivalTo ELSE j.ArrivalTo END, CASE WHEN d.isRamadan = 1 THEN j.RDepartFrom ELSE j.DepartFrom END)
											ELSE 1440 + DATEDIFF(MINUTE, CASE WHEN d.isRamadan = 1 THEN j.RArrivalTo ELSE j.ArrivalTo END, CASE WHEN d.isRamadan = 1 THEN j.RDepartFrom ELSE j.DepartFrom END)
										END, 
										e.dtIN
									)
								ELSE CASE WHEN d.isRamadan = 1 THEN j.RDepartFrom ELSE j.DepartFrom END
							END)
			) AS DepartureTimeDiff				
	FROM tas.AttendancePunctualityLog a WITH (NOLOCK)
		INNER JOIN tas.Master_Employee_JDE_View_V2 b WITH (NOLOCK) ON a.EmpNo = b.EmpNo
		LEFT JOIN tas.Master_BusinessUnit_JDE_view c WITH (NOLOCK) ON RTRIM(a.CostCenter) = RTRIM(c.BU)
		INNER JOIN tas.Tran_Timesheet d WITH (NOLOCK) ON a.EmpNo = d.EmpNo AND a.DT = d.DT AND d.IsLastRow = 1 --AND ISNULL(d.LeaveType, '') = ''		--Rev. #1.4
		OUTER APPLY
		(
			SELECT TOP 1 dtIN, dtOUT 
			FROM tas.Tran_Timesheet WITH (NOLOCK)
			WHERE DT = a.DT
				AND dtIN IS NOT NULL 
				AND dtOUT IS NOT NULL 
				AND EmpNo = a.EmpNo				
		) e
		CROSS APPLY
        (
			SELECT COUNT(DISTINCT CreatedDate) AS ReportOccurenceCount
			FROM tas.AttendancePunctualityLog WITH (NOLOCK) 	
			WHERE DT BETWEEN @startDate AND @endDate
				AND EmpNo = a.EmpNo
		) f
		LEFT JOIN tas.FlexiTimeSetting g WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(g.ShiftPatCode)
		LEFT JOIN tas.Master_ShiftPatternTitles h WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(h.ShiftPatCode)
		LEFT JOIN tas.Master_ShiftTimes i WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(i.ShiftPatCode) AND RTRIM(a.ShiftCode) = RTRIM(i.ShiftCode)	--Based on the scheduled shift
		LEFT JOIN tas.Master_ShiftTimes j ON RTRIM(a.ShiftPatCode) = RTRIM(j.ShiftPatCode) 
			AND 
				(
					CASE WHEN h.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
						THEN a.ShiftCode
						ELSE 
							CASE WHEN (ABS(DATEDIFF(MINUTE, CONVERT(TIME, d.dtIN), CONVERT(TIME, CASE WHEN d.isRamadan = 1 THEN i.RArrivalTo ELSE i.ArrivalTo END))) > d.Duration_Required / 2) OR (d.Duration_Worked_Cumulative >= (d.Duration_Required + (d.Duration_Required / 2)))
								THEN RTRIM(a.Actual_ShiftCode)
								ELSE RTRIM(a.ShiftCode)
							END
					END
				) = RTRIM(j.ShiftCode)
		/* Rev. 1.6
		LEFT JOIN tas.Master_ShiftTimes j WITH (NOLOCK) ON RTRIM(a.ShiftPatCode) = RTRIM(j.ShiftPatCode) 
			AND 
				(
					CASE WHEN h.IsDayShift = 1 OR RTRIM(a.ShiftCode) = 'O' 
						THEN a.ShiftCode
						ELSE 
							CASE 
								WHEN tas.fnCheckIfMutipleGateEntry(a.EmpNo, a.DT) = 1 AND d.IsLastRow = 1	--Rev. #1.3
								THEN 
									CASE 
										WHEN d.Duration_Worked >= d.Duration_Required AND d.Duration_Worked < (d.Duration_Required + (d.Duration_Required / 2)) THEN RTRIM(d.ShiftCode)  
										WHEN d.Duration_Worked < (d.Duration_Required / 2) THEN RTRIM(d.ShiftCode) 
										ELSE RTRIM(d.Actual_ShiftCode) 
									END  
								WHEN 
								(
									ABS(DATEDIFF(MINUTE, CONVERT(TIME, d.dtIN), CONVERT(TIME, CASE WHEN d.isRamadan = 1 THEN i.RArrivalTo ELSE i.ArrivalTo END))) > d.Duration_Required / 2) 
									OR (d.Duration_Worked_Cumulative >= (d.Duration_Required + (d.Duration_Required / 2))
								)
								THEN RTRIM(a.Actual_ShiftCode)
								ELSE RTRIM(a.ShiftCode)
							END
					END
				) = RTRIM(j.ShiftCode)
		*/
	WHERE 
		a.DT BETWEEN @startDate AND @endDate
		AND (RTRIM(a.CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)
		AND a.EmpNo IN
			(
				SELECT EmpNo 
				FROM tas.AttendancePunctualityLog WITH (NOLOCK) 
				WHERE DT BETWEEN @startDate AND @endDate
					AND (RTRIM(CostCenter) = RTRIM(@costCenter) OR @costCenter IS NULL)
					AND (EmpNo = @empNo OR @empNo IS NULL)
				GROUP BY EmpNo
				HAVING (COUNT(EmpNo) >= @CONST_OCCURENCE_LIMIT) 
			)
	ORDER BY a.CostCenter, a.EmpNo, a.DT 	

/*	Debug:

PARAMETERS:
	@startDate			DATETIME,
	@endDate			DATETIME,
	@costCenter			VARCHAR(12) = '',
	@empNo				INT = 0

	EXEC tas.Pr_EmpPunctualityByPeriod '06/01/2020', '06/15/2020', '3412', 0

*/

