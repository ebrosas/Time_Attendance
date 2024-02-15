/**************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetManHourBreakdown
*	Description: This functions gets all man-hour count breakdown based on specific period
*
*	Date:			Author:		Rev.#:		Comments:
*	30/01/2020		Ervin		1.0			Created
**************************************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetManHourBreakdown
(
	@startDate		DATETIME,
	@endDate		DATETIME
)
RETURNS  @rtnTable TABLE  
(   
	TotalHourEmployee		INT,  
	TotalHourContractor		INT,  
	TotalHourVisitor		INT,
	TotalManHourCount		INT   
) 
AS
BEGIN

	DECLARE @totalHourWorkEmployee		INT = 0,  
			@totalHourWorkContractor	INT = 0,  
			@totalHourWorkVisitor		INT = 0 

	--Get the manhour count for all employees
	SELECT @totalHourWorkEmployee = ROUND(CONVERT(FLOAT, SUM(a.Duration_Worked_Cumulative)) / 60, 0)
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
	WHERE 
		a.DT BETWEEN @startDate AND @endDate
		AND a.IsLastRow = 1
		AND a.Duration_Worked_Cumulative > 0
		AND a.EmpNo BETWEEN 1000000 AND 19999999

	--Get the manhour count for all contractors
	SELECT @totalHourWorkContractor = ROUND(CONVERT(FLOAT, SUM(a.Duration_Worked_Cumulative)) / 60, 0)
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
	WHERE 
		a.DT BETWEEN @startDate AND @endDate
		AND a.IsLastRow = 1
		AND a.Duration_Worked_Cumulative > 0
		AND a.EmpNo BETWEEN 50000 AND 99999

	--Get the manhour count for all visitors
	SELECT @totalHourWorkVisitor = ROUND(CONVERT(FLOAT, SUM(a.Duration_Worked_Cumulative)) / 60, 0)
	FROM tas.Tran_Timesheet a WITH (NOLOCK)
	WHERE 
		a.DT BETWEEN @startDate AND @endDate
		AND a.IsLastRow = 1
		AND a.Duration_Worked_Cumulative > 0
		AND a.EmpNo BETWEEN 10000 AND 19999

	INSERT INTO @rtnTable 
	SELECT	@totalHourWorkEmployee,  
			@totalHourWorkContractor,  
			@totalHourWorkVisitor,
			@totalHourWorkEmployee + @totalHourWorkContractor + @totalHourWorkVisitor			

	RETURN 

END


/*	Debugging:
	
PARAMETERS:
	@startDate		DATETIME,
	@endDate		DATETIME

	SELECT * FROM tas.fnGetManHourBreakdown('12/01/2019', '12/31/2019')
	SELECT * FROM tas.fnGetManHourBreakdown('01/01/2020', '01/31/2020')

*/
