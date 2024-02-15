/**************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetManHourBreakdown
*	Description: This functions gets all man-hour count breakdown based on specific period
*
*	Date:			Author:		Rev.#:		Comments:
*	30/01/2020		Ervin		1.0			Created
**************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetManHourBreakdownByDept
(
	@startDate		DATETIME,
	@endDate		DATETIME
)
RETURNS  @rtnTable TABLE  
(   
	CostCenter			VARCHAR(12),  
	CostCenterName		VARCHAR(50),  
	TotalManhour		INT   
) 
AS
BEGIN

	INSERT INTO @rtnTable 
	SELECT	ISNULL(a.BusinessUnit, 0), 
			RTRIM(b.BusinessUnitName),
			ROUND(CONVERT(FLOAT, SUM(a.Duration_Worked_Cumulative)) / 60, 0)
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
		LEFT JOIN tas.Master_BusinessUnit_JDE_V2 b WITH (NOLOCK) ON RTRIM(a.BusinessUnit) = RTRIM(b.BusinessUnit)
	WHERE a.DT BETWEEN @startDate AND @endDate
		AND a.IsLastRow = 1
		AND a.Duration_Worked_Cumulative > 0
		AND a.EmpNo > 0
	GROUP BY ISNULL(a.BusinessUnit, 0), b.BusinessUnitName

	RETURN 

END


/*	Debugging:
	
PARAMETERS:
	@startDate		DATETIME,
	@endDate		DATETIME

	SELECT * FROM tas.fnGetManHourBreakdownByDept('12/01/2019', '12/31/2019')
	SELECT * FROM tas.fnGetManHourBreakdownByDept('01/01/2020', '01/31/2020')

*/
