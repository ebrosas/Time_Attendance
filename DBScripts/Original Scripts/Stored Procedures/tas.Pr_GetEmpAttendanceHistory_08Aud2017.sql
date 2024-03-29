USE [tas2]
GO
/****** Object:  StoredProcedure [tas].[Pr_GetEmpAttendanceHistory]    Script Date: 08/08/2017 16:14:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*****************************************************************************************************************************************************************************
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
******************************************************************************************************************************************************************************/

ALTER PROCEDURE [tas].[Pr_GetEmpAttendanceHistory]
(   
	@startDate		DATETIME,
	@endDate		DATETIME,
	@costCenter		VARCHAR(12) = '',
	@empNo			INT = 0
)
AS

	--Validate parameters
	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	SELECT	a.AutoID,
			a.EmpNo,
			LTRIM(RTRIM(b.YAALPH)) AS EmpName,
			LTRIM(RTRIM(ISNULL(e.JMDL01, ''))) AS Position,

			CASE WHEN ISNULL(g.WorkingBusinessUnit, '') <> ''
				THEN LTRIM(RTRIM(g.WorkingBusinessUnit))
				ELSE
					CASE WHEN LTRIM(RTRIM(h.ABAT1)) = 'E' THEN LTRIM(RTRIM(b.YAHMCU))
						WHEN LTRIM(RTRIM(h.ABAT1)) = 'UG' THEN LTRIM(RTRIM(h.ABMCU)) 
					END
			END AS BusinessUnit,
			RTRIM(i.BUname) AS BusinessUnitName,
			--a.BusinessUnit,
			--f.BUname AS BusinessUnitName,

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
			a.Remarks,
			a.Duration_Required,
			a.DayOffDuration,
			a.RequiredToSwipeAtWorkplace,
			
			CASE WHEN ISNULL(a.IsLastRow, 0) = 0
				THEN a.dtIn
				ELSE
					CASE WHEN a.RequiredToSwipeAtWorkplace = 1 AND a.TimeInMG IS NULL AND a.dtIn IS NOT NULL	
						THEN a.dtIn
						ELSE a.TimeInMG
					END
			END AS TimeInMG,
			CASE WHEN ISNULL(a.IsLastRow, 0) = 0
				THEN a.dtIn
				ELSE a.TimeInWP
			END AS TimeInWP,
			CASE WHEN ISNULL(a.IsLastRow, 0) = 0
				THEN a.dtOut
				ELSE a.TimeOutWP
			END AS TimeOutWP,
			CASE WHEN ISNULL(a.IsLastRow, 0) = 0
				THEN a.dtOut
				ELSE 
					CASE WHEN a.RequiredToSwipeAtWorkplace = 1 AND a.TimeOutMG IS NULL AND a.dtOut IS NOT NULL	
						THEN a.dtOut
						ELSE a.TimeOutMG
					END 
			END AS TimeOutMG,
			a.LastUpdateUser,
			a.LastUpdateTime	
	FROM tas.Vw_EmployeeAttendanceHistory a
		INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = CAST(b.YAAN8 AS INT) 
		LEFT JOIN tas.syJDE_F0101 c ON b.YAAN8 = c.ABAN8
		LEFT JOIN tas.syJDE_F00092 d ON b.YAAN8 = d.T3SBN1 AND LTRIM(RTRIM(d.T3TYDT)) = 'WH' AND LTRIM(RTRIM(d.T3SDB)) = 'E'
		LEFT JOIN tas.syJDE_F08001 e on LTRIM(RTRIM(b.YAJBCD)) = LTRIM(RTRIM(e.JMJBCD))
		--LEFT JOIN tas.Master_BusinessUnit_JDE_view f ON RTRIM(a.BusinessUnit) = RTRIM(f.BU)
		LEFT JOIN tas.Master_EmployeeAdditional g ON CAST(b.YAAN8 AS INT) = g.EmpNo		
		LEFT JOIN tas.syJDE_F0101 h ON b.YAAN8 = h.ABAN8
		LEFT JOIN tas.Master_BusinessUnit_JDE_view i ON 
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

	EXEC tas.Pr_GetEmpAttendanceHistory '01/01/2012', '01/31/2012', '', 10001757
	EXEC tas.Pr_GetEmpAttendanceHistory '16/02/2016', '15/03/2016', '7600'
	EXEC tas.Pr_GetEmpAttendanceHistory '03/16/2017', '04/15/2017', '', 10006038

*/
