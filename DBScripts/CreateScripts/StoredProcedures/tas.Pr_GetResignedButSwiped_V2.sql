/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetResignedButSwiped_V2
*	Description: Get the list of employees with changes in the Shift Pattern
*
*	Date			Author		Revision No.	Comments:
*	28/06/2016		Ervin		1.0				Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetResignedButSwiped_V2
(   	
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12)	= ''
)
AS

	--Validate parameters
	IF ISNULL(@empNo, 0) = 0
		SET @empNo = NULL

	IF ISNULL(@costCenter, '') = ''
		SET @costCenter = NULL

	IF ISNULL(@startDate, '') = CONVERT(DATETIME, '')
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = CONVERT(DATETIME, '')
		SET @endDate = NULL

	SELECT	a.AutoID,
			a.RunDate,				
			a.EmpNo,
			LTRIM(RTRIM(b.YAALPH)) AS EmpName,
			LTRIM(RTRIM(ISNULL(d.JMDL01, ''))) AS Position,
			LTRIM(RTRIM(b.YAMCU)) AS BusinessUnit,
			c.BusinessUnitName,
			tas.ConvertFromJulian(b.YADT) AS DateResigned,
			SUBSTRING(a.Comment, CHARINDEX('RESIGNED', RTRIM(a.Comment)), (LEN(RTRIM(a.Comment)) - (CHARINDEX('RESIGNED', RTRIM(a.Comment))) + 1)) AS AttendanceRemarks
	FROM
		( 
			SELECT	AutoID,
					SUBSTRING(RTRIM(Comment), CHARINDEX('EmpNo', RTRIM(Comment)) + LEN('EmpNo = '), 9) AS EmpNo,
					RunDate,
					RTRIM(Comment) AS Comment
			FROM tas.System_EventLog 
			WHERE RTRIM(Comment) LIKE '%RESIGNED_BUT_SWIPED%' 
				AND LTRIM(RTRIM(Comment)) LIKE 'EmpNo = %'
		) a
		INNER JOIN tas.syJDE_F060116 b ON a.EmpNo = b.YAAN8
		LEFT JOIN tas.Master_BusinessUnit_JDE c ON LTRIM(RTRIM(b.YAMCU)) = RTRIM(c.BusinessUnit)
		LEFT JOIN tas.syJDE_F08001 d on LTRIM(RTRIM(b.YAJBCD)) = LTRIM(RTRIM(d.JMJBCD))
	WHERE 
		(a.EmpNo = @empNo OR @empNo IS NULL)
		AND (LTRIM(RTRIM(b.YAMCU)) = RTRIM(@costCenter) OR @costCenter IS NULL)
		AND 
		(
			CONVERT(DATETIME, CONVERT(VARCHAR, a.RunDate, 12)) BETWEEN @startDate AND @endDate
			OR
            (@startDate IS NULL AND @endDate IS NULL)
		)
	ORDER BY a.RunDate DESC, LTRIM(RTRIM(b.YAMCU)), a.EmpNo	

GO 

/*	Debugging:

PARAMETERS:
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@empNo			INT = 0,
	@costCenter		VARCHAR(12)	= ''

	EXEC tas.Pr_GetResignedButSwiped_V2 '', '', 10001281
	EXEC tas.Pr_GetResignedButSwiped_V2 '01/01/2016', '30/04/2016'
	EXEC tas.Pr_GetResignedButSwiped_V2 '', '', 0, '7600'

*/


