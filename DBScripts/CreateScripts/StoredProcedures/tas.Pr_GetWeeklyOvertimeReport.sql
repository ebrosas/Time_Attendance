/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetWeeklyOvertimeReport
*	Description: Get the data for the Weekly Overtime Report
*
*	Date			Author		Rev. #		Comments:
*	28/11/2016		Ervin		1.0			Created
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetWeeklyOvertimeReport
(   		
	@startDate				DATETIME,
	@endDate				DATETIME,
	@costCenterList			VARCHAR(300) = ''
)
AS

	--Validate parameters
	IF ISNULL(@costCenterList, '') = ''
		SET @costCenterList = NULL

	SELECT	a.BusinessUnit,
			b.BUname AS BusinessUnitName,
			SUM(DATEDIFF(MINUTE, a.OTStartTime, a.OTEndTime)) AS TotalOTMinutes
	FROM tas.Tran_Timesheet a
		LEFT JOIN tas.Master_BusinessUnit_JDE_view b ON RTRIM(a.BusinessUnit) = LTRIM(RTRIM(b.BU))
	WHERE 
		a.IsLastRow = 1
		AND a.OTStartTime IS NOT NULL	
		AND a.OTEndTime IS NOT NULL
		AND a.DT BETWEEN @startDate AND @endDate
		AND 
		(
			RTRIM(a.BusinessUnit) IN (SELECT GenericNo FROM tas.fnParseStringArrayToInt(@costCenterList, ','))
			OR
			@costCenterList IS NULL
		)
	GROUP BY a.BusinessUnit, b.BUname
	ORDER BY a.BusinessUnit

GO 

/*	Debug:

PARAMETERS:
	@costCenterList		VARCHAR(12) = '',
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL

	EXEC tas.Pr_GetWeeklyOvertimeReport '16/02/2016', '15/03/2016'
	EXEC tas.Pr_GetWeeklyOvertimeReport '16/02/2016', '15/03/2016', '2110,2112,3230'

*/