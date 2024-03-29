USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_GetEmpAttendanceHistory]    Script Date: 09/03/2021 11:00:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**************************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetEmpAttendanceHistory
*	Description: Get the employee attendance history records
*
*	Date			Author		Rev. #		Comments:
*	08/11/2016		Ervin		1.0			Created
*	17/11/2016		Ervin		1.1			Modified the Order By clause
*	09/01/2017		Ervin		1.2			Modified the WHERE Filter clause to allow viewing the attendance history of inactive employees
*	12/02/2017		Ervin		1.3			Modified the logic for getting the value of "TimeInMG" and "TimeOutMG" fields
*	22/02/2017		Ervin		1.4			Modified the logic in fetching the TimeMG, TimeOutMG, TimeInWP, TimeOutWP fields
*	11/04/2017		Ervin		1.5			Modified the logic in fetching the Cost Center information
*	17/05/2017		Ervin		1.6			Added "LastUpdateUser" and "LastUpdateTime" fields	
*	08/08/2017		Ervin		1.7			Refactored the logic in determining the time in/out from the Main gate and workplace readers by checking if multiple entries exist in the Timesheet
*	19/08/2018		Ervin		1.8			Added WIHT (NOLOCK) clause to enhance data retrieval performance
*	14/11/2018		Ervin		1.9			Set the "Remarks" field to "Day Off" if ShiftCode = O
*	15/01/2019		Ervin		2.0			Fixed the bug reported by HR wherein ExtraPay is not shown in the attendance sheet for employees who are on day-off during public holidays
*	27/03/2019		Ervin		2.1			Modified the logic in fetching the Remarks. Commented the changes applied in Rev. #1.9
*	02/07/2019		Ervin		2.2			Added "RelativeTypeName" and "DeathRemarks" fields
*	14/10/2019		Ervin		2.3			Added link to "F0005" in JDE to fetch the employee's job title
****************************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_GetEmpAttendanceHistory]
(   
	@startDate		DATETIME,
	@endDate		DATETIME,
	@costCenter		VARCHAR(12) = '',
	@empNo			INT = 0
)
AS

	--Tell SQL Engine not to return the row-count information
	SET NOCOUNT ON 

	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	SELECT	a.AutoID,
			a.EmpNo,
			LTRIM(RTRIM(b.YAALPH)) AS EmpName,

			--LTRIM(RTRIM(ISNULL(e.JMDL01, ''))) AS Position,
			LTRIM(RTRIM(e.DRDL01)) + RTRIM(e.DRDL02) AS Position,

			CASE WHEN ISNULL(g.WorkingBusinessUnit, '') <> ''
				THEN LTRIM(RTRIM(g.WorkingBusinessUnit))
				ELSE
					CASE WHEN LTRIM(RTRIM(h.ABAT1)) = 'E' THEN LTRIM(RTRIM(b.YAHMCU))
						WHEN LTRIM(RTRIM(h.ABAT1)) = 'UG' THEN LTRIM(RTRIM(h.ABMCU)) 
					END
			END AS BusinessUnit,
			RTRIM(i.BUname) AS BusinessUnitName,
			a.DT,
			a.dtIn,
			a.dtOut,
			a.ShiftPatCode,
			a.ShiftCode,
			a.Actual_ShiftCode,
			a.WorkDurationCumulative,
			a.WorkDurationMinutes,
			a.WorkDurationHours,
			a.ShavedWorkDurationMinutes,
			a.ShavedWorkDurationHours,
			a.OTDurationMinutes,
			a.OTDurationHours,
			a.NoPayHours,

			--CASE WHEN RTRIM(a.ShiftCode) = 'O' AND ISNULL(a.IsPublicHoliday, 0) = 0 THEN 'Day Off' ELSE RTRIM(a.Remarks) END AS Remarks,		--Rev. #1.9
			 RTRIM(a.Remarks) AS Remarks,	--Rev. #2.1

			a.Duration_Required,
			a.DayOffDuration,
			a.RequiredToSwipeAtWorkplace,
			
			--Start of Rev. #1.7
			CASE WHEN ISNULL(a.IsLastRow, 0) = 0
				THEN 
					CASE WHEN tas.fnCheckIfMutipleGateEntry(a.EmpNo, a.DT) = 1 THEN a.TimeInMG ELSE a.dtIn END 
				ELSE 
					CASE WHEN tas.fnCheckIfMutipleGateEntry(a.EmpNo, a.DT) = 1 
						THEN a.dtIn 
						ELSE 
							CASE WHEN a.RequiredToSwipeAtWorkplace = 1 AND a.TimeInMG IS NULL AND a.dtIn IS NOT NULL	
								THEN a.dtIn
								ELSE a.TimeInMG
							END
					END 
			END AS TimeInMG,

			CASE WHEN ISNULL(a.IsLastRow, 0) = 0
				THEN 
					CASE WHEN tas.fnCheckIfMutipleGateEntry(a.EmpNo, a.DT) = 1 THEN a.TimeInWP ELSE a.dtIn END 
				ELSE 
					CASE WHEN tas.fnCheckIfMutipleGateEntry(a.EmpNo, a.DT) = 1 THEN a.dtIn ELSE a.TimeInWP END 
			END AS TimeInWP,

			CASE WHEN ISNULL(a.IsLastRow, 0) = 0
				THEN 
					CASE WHEN tas.fnCheckIfMutipleGateEntry(a.EmpNo, a.DT) = 1 THEN a.TimeOutWP ELSE a.dtOut END 
				ELSE 
					CASE WHEN tas.fnCheckIfMutipleGateEntry(a.EmpNo, a.DT) = 1 THEN a.dtOut ELSE a.TimeOutWP END 
			END AS TimeOutWP,

			CASE WHEN ISNULL(a.IsLastRow, 0) = 0
				THEN 
					CASE WHEN tas.fnCheckIfMutipleGateEntry(a.EmpNo, a.DT) = 1 THEN a.TimeOutMG ELSE a.dtOut END 
				ELSE 
					CASE WHEN tas.fnCheckIfMutipleGateEntry(a.EmpNo, a.DT) = 1 
						THEN a.dtOut 
						ELSE 
							CASE WHEN a.RequiredToSwipeAtWorkplace = 1 AND a.TimeOutMG IS NULL AND a.dtOut IS NOT NULL	
								THEN a.dtOut
								ELSE a.TimeOutMG
							END 
					END 
			END AS TimeOutMG,
			--End of Rev. #1.7

			CASE WHEN RTRIM(a.ShiftCode) = 'O' THEN '' ELSE a.LastUpdateUser END AS LastUpdateUser,		--Rev. #1.9
			CASE WHEN RTRIM(a.ShiftCode) = 'O' THEN NULL ELSE a.LastUpdateTime END AS LastUpdateTime,	--Rev. #1.9
			a.CorrectionCode,
			a.RelativeTypeName,		--Rev. #2.2
			a.DeathRemarks			--Rev. #2.2
	FROM tas.Vw_EmployeeAttendanceHistory a WITH (NOLOCK)
		INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.EmpNo = CAST(b.YAAN8 AS INT) 
		LEFT JOIN tas.syJDE_F0101 c WITH (NOLOCK) ON b.YAAN8 = c.ABAN8
		LEFT JOIN tas.syJDE_F00092 d WITH (NOLOCK) ON b.YAAN8 = d.T3SBN1 AND LTRIM(RTRIM(d.T3TYDT)) = 'WH' AND LTRIM(RTRIM(d.T3SDB)) = 'E'
		LEFT JOIN tas.syJDE_F0005 e WITH (NOLOCK) ON LTRIM(RTRIM(b.YAJBCD)) = LTRIM(RTRIM(e.DRKY)) AND RTRIM(LTRIM(e.DRSY)) = '06' AND RTRIM(LTRIM(e.DRRT)) = 'G'	
		LEFT JOIN tas.Master_EmployeeAdditional g WITH (NOLOCK) ON CAST(b.YAAN8 AS INT) = g.EmpNo		
		LEFT JOIN tas.syJDE_F0101 h WITH (NOLOCK) ON b.YAAN8 = h.ABAN8
		LEFT JOIN tas.Master_BusinessUnit_JDE_view i WITH (NOLOCK) ON 
			CASE WHEN ISNULL(g.WorkingBusinessUnit, '') <> ''
				THEN LTRIM(RTRIM(g.WorkingBusinessUnit))
				ELSE
					CASE WHEN LTRIM(RTRIM(h.ABAT1)) = 'E' THEN LTRIM(RTRIM(b.YAHMCU))
						WHEN LTRIM(RTRIM(h.ABAT1)) = 'UG' THEN LTRIM(RTRIM(h.ABMCU)) 
					END
			END = RTRIM(i.BU)
	WHERE 
		--ISNUMERIC(CASE WHEN (b.YAPAST IN ('R', 'T', 'E', 'X') AND GETDATE() < tas.ConvertFromJulian(b.YADT)  OR UPPER(LTRIM(RTRIM(b.YAPAST))) = 'I') THEN '0' ELSE b.YAPAST END) = 1 AND	--Rev. #1.2 
		a.DT BETWEEN @startDate AND @endDate
		AND (RTRIM(a.BusinessUnit) = RTRIM(@costCenter) OR @costCenter IS NULL)
		AND (a.EmpNo = @empNo OR @empNo IS NULL)	
	ORDER BY a.EmpNo, a.DT, a.dtIN 


/*	Debugging:

PARAMETERS:
	@startDate		DATETIME,
	@endDate		DATETIME,
	@costCenter		VARCHAR(12) = '',
	@empNo			INT = 0

	EXEC tas.Pr_GetEmpAttendanceHistory '03/16/2016', '04/15/2016', '', 10003666

*/