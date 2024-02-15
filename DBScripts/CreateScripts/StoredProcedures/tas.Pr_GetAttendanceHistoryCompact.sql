/**************************************************************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetAttendanceHistoryCompact
*	Description: Get the employee attendance history records
*
*	Date			Author		Rev. #		Comments:
*	19/08/2018		Ervin		1.0			Created
****************************************************************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetAttendanceHistoryCompact
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
			LTRIM(RTRIM(ISNULL(e.JMDL01, ''))) AS Position,

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
			--a.ShavedWorkDurationMinutes,
			--a.ShavedWorkDurationHours,
			a.OTDurationMinutes,
			a.OTDurationHours,
			a.NoPayHours,
			a.Remarks,
			--a.Duration_Required,
			--a.DayOffDuration,
			--a.RequiredToSwipeAtWorkplace,
			a.LastUpdateUser,
			a.LastUpdateTime	
	FROM tas.Vw_AttendanceHistoryCompact a WITH (NOLOCK)
		INNER JOIN tas.syJDE_F060116 b WITH (NOLOCK) ON a.EmpNo = CAST(b.YAAN8 AS INT) 
		LEFT JOIN tas.syJDE_F0101 c WITH (NOLOCK) ON b.YAAN8 = c.ABAN8
		LEFT JOIN tas.syJDE_F00092 d WITH (NOLOCK) ON b.YAAN8 = d.T3SBN1 AND LTRIM(RTRIM(d.T3TYDT)) = 'WH' AND LTRIM(RTRIM(d.T3SDB)) = 'E'
		LEFT JOIN tas.syJDE_F08001 e WITH (NOLOCK) on LTRIM(RTRIM(b.YAJBCD)) = LTRIM(RTRIM(e.JMJBCD))
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

	EXEC tas.Pr_GetAttendanceHistoryCompact '03/27/2016', '03/28/2016', '', 10001415
	EXEC tas.Pr_GetAttendanceHistoryCompact '03/20/2016', '03/21/2016', '', 10003034
	EXEC tas.Pr_GetAttendanceHistoryCompact '16/02/2016', '15/03/2016', '7600'
	EXEC tas.Pr_GetAttendanceHistoryCompact '03/16/2017', '04/15/2017', '', 10006038

*/
