/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetPunctualityReport
*	Description: Fetch data for the Punctuality Report
*
*	Date			Author		Revision No.	Comments:
*	04/12/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetPunctualityReport
(   
	@startDate		DATETIME,
	@endDate		DATETIME,
	@costCenter		VARCHAR(12),
	@showDayOff		BIT,
	@showCount		BIT = 1 
)
AS

	--IF ISNULL(@showCount, 0) = 0
	--	SET @showCount = 1

	IF @showCount = 1	
	BEGIN
    
		--Show count of employees
		SELECT	a.BusinessUnit,
				tas.fmtDate(a.DT)  + ' (' + tas.fmtDate_getDayOfWeek(a.DT, 1) + ')' AS AttendanceDate,
				tas.fnGetCountEmpOnArrivalTime(CONVERT(TIME, '07:00'), CONVERT(TIME, '07:00'), a.DT, RTRIM(a.BusinessUnit), '<') AS Total_0700_Below,
				tas.fnGetCountEmpOnArrivalTime(CONVERT(TIME, '07:00'), CONVERT(TIME, '07:10'), a.DT, RTRIM(a.BusinessUnit), '<') AS Total_0700_0710,
				tas.fnGetCountEmpOnArrivalTime(CONVERT(TIME, '07:10'), CONVERT(TIME, '07:20'), a.DT, RTRIM(a.BusinessUnit), '') AS Total_0710_0720,
				tas.fnGetCountEmpOnArrivalTime(CONVERT(TIME, '07:20'), CONVERT(TIME, '07:30'), a.DT, RTRIM(a.BusinessUnit), '') AS Total_0720_0730,
				tas.fnGetCountEmpOnArrivalTime(CONVERT(TIME, '07:30'), CONVERT(TIME, '07:40'), a.DT, RTRIM(a.BusinessUnit), '') AS Total_0730_0740,
				tas.fnGetCountEmpOnArrivalTime(CONVERT(TIME, '07:40'), CONVERT(TIME, '07:50'), a.DT, RTRIM(a.BusinessUnit), '') AS Total_0740_0750,
				tas.fnGetCountEmpOnArrivalTime(CONVERT(TIME, '07:50'), CONVERT(TIME, '08:00'), a.DT, RTRIM(a.BusinessUnit), '') AS Total_0750_0800,
				tas.fnGetCountEmpOnArrivalTime(CONVERT(TIME, '08:00'), CONVERT(TIME, '08:00'), a.DT, RTRIM(a.BusinessUnit), '>') AS Total_0800_Above,
				tas.fnGetCountEmpOnArrivalTime(NULL, NULL, a.DT, RTRIM(a.BusinessUnit), 'A') AS Total_Absent
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_ShiftPatternTitles b ON RTRIM(a.ShiftPatCode) = RTRIM(b.ShiftPatCode)
			INNER JOIN tas.Tran_ShiftPatternUpdates c ON a.EmpNo = c.EmpNo AND a.DT = c.DateX		
		WHERE 
			a.GradeCode >= 9
			AND a.DT BETWEEN @startDate AND @endDate 
			AND b.IsDayShift = 1
			AND RTRIM(a.BusinessUnit) = RTRIM(@costCenter)
			AND	--Show or hide day offs and holidays
			(
				(
					(
						a.DT NOT IN (SELECT HolidayDate FROM tas.Master_Calendar WHERE RTRIM(HolidayType) <> 'R' AND HolidayDate BETWEEN @startDate AND @endDate) 
						AND
						RTRIM(c.Effective_ShiftCode) <> 'O'
					)
					AND @showDayOff = 0
				)
				OR @showDayOff = 1
			)
		GROUP BY a.DT, a.BusinessUnit
		ORDER BY a.DT DESC, a.BusinessUnit
	END 

	ELSE
    BEGIN

		--Show percentage of employees
		SELECT	a.BusinessUnit,
				tas.fmtDate(a.DT)  + ' (' + tas.fmtDate_getDayOfWeek(a.DT, 1) + ')' AS AttendanceDate,
				tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:00'), CONVERT(TIME, '07:00'), a.DT, RTRIM(a.BusinessUnit), '<') AS Total_0700_Below,
				tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:00'), CONVERT(TIME, '07:10'), a.DT, RTRIM(a.BusinessUnit), '<') AS Total_0700_0710,
				tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:10'), CONVERT(TIME, '07:20'), a.DT, RTRIM(a.BusinessUnit), '') AS Total_0710_0720,
				tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:20'), CONVERT(TIME, '07:30'), a.DT, RTRIM(a.BusinessUnit), '') AS Total_0720_0730,
				tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:30'), CONVERT(TIME, '07:40'), a.DT, RTRIM(a.BusinessUnit), '') AS Total_0730_0740,
				tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:40'), CONVERT(TIME, '07:50'), a.DT, RTRIM(a.BusinessUnit), '') AS Total_0740_0750,
				tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:50'), CONVERT(TIME, '08:00'), a.DT, RTRIM(a.BusinessUnit), '') AS Total_0750_0800,
				tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '08:00'), CONVERT(TIME, '08:00'), a.DT, RTRIM(a.BusinessUnit), '>') AS Total_0800_Above,
				tas.fnGetPercentEmpOnArrivalTime(NULL, NULL, a.DT, RTRIM(a.BusinessUnit), 'A') AS Total_Absent

				--ROUND(tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:00'), CONVERT(TIME, '07:00'), a.DT, RTRIM(a.BusinessUnit), '<'), 2) AS Total_0700_Below,
				--ROUND(tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:00'), CONVERT(TIME, '07:10'), a.DT, RTRIM(a.BusinessUnit), '<'), 2) AS Total_0700_0710,
				--ROUND(tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:10'), CONVERT(TIME, '07:20'), a.DT, RTRIM(a.BusinessUnit), ''), 2) AS Total_0710_0720,
				--ROUND(tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:20'), CONVERT(TIME, '07:30'), a.DT, RTRIM(a.BusinessUnit), ''), 2) AS Total_0720_0730,
				--ROUND(tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:30'), CONVERT(TIME, '07:40'), a.DT, RTRIM(a.BusinessUnit), ''), 2) AS Total_0730_0740,
				--ROUND(tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:40'), CONVERT(TIME, '07:50'), a.DT, RTRIM(a.BusinessUnit), ''), 2) AS Total_0740_0750,
				--ROUND(tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:50'), CONVERT(TIME, '08:00'), a.DT, RTRIM(a.BusinessUnit), ''), 2) AS Total_0750_0800,
				--ROUND(tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '08:00'), CONVERT(TIME, '08:00'), a.DT, RTRIM(a.BusinessUnit), '>'), 2) AS Total_0800_Above,
				--ROUND(tas.fnGetPercentEmpOnArrivalTime(NULL, NULL, a.DT, RTRIM(a.BusinessUnit), 'A'), 2) AS Total_Absent
		FROM tas.Tran_Timesheet a
			INNER JOIN tas.Master_ShiftPatternTitles b ON RTRIM(a.ShiftPatCode) = RTRIM(b.ShiftPatCode)
			INNER JOIN tas.Tran_ShiftPatternUpdates c ON a.EmpNo = c.EmpNo AND a.DT = c.DateX		
		WHERE 
			a.GradeCode >= 9
			AND a.DT BETWEEN @startDate AND @endDate 
			AND b.IsDayShift = 1
			AND RTRIM(a.BusinessUnit) = RTRIM(@costCenter)
			AND	--Show or hide day offs and holidays
			(
				(
					(
						a.DT NOT IN (SELECT HolidayDate FROM tas.Master_Calendar WHERE RTRIM(HolidayType) <> 'R' AND HolidayDate BETWEEN @startDate AND @endDate) 
						AND
						RTRIM(c.Effective_ShiftCode) <> 'O'
					)
					AND @showDayOff = 0
				)
				OR @showDayOff = 1
			)
		GROUP BY a.DT, a.BusinessUnit
		ORDER BY a.DT DESC, a.BusinessUnit
    END 
GO 

/*	Testing:

PARAMETERS:
	@startDate		DATETIME,
	@endDate		DATETIME,
	@costCenter		VARCHAR(12),
	@showDayOff		BIT,
	@showCount		BIT = 1  

	EXEC tas.Pr_GetPunctualityReport '16/02/2016', '15/03/2016', '7600', 0, 0	--Show percentage
	EXEC tas.Pr_GetPunctualityReport '16/02/2016', '15/03/2016', '7600', 0, 1	--Show count

*/