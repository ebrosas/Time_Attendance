/*****************************************************************************************************************************************************************************
*	Revision History
*
*	Name: tas.Pr_GetGARMCOCalendar
*	Description: Retrieves the public holidays declared in GARMCO
*
*	Date			Author		Revision No.	Comments:
*	07/06/2016		Ervin		1.0				Created
*	05/01/2017		Ervin		1.1				Added DOW
******************************************************************************************************************************************************************************/

ALTER PROCEDURE tas.Pr_GetGARMCOCalendar
(   
	@year		INT = 0,
	@startDate	DATETIME = NULL,
	@endDate	DATETIME = NULL
)
AS

	--Validate parameters
	IF ISNULL(@year, 0) = 0
		SET @year = NULL 

	IF ISNULL(@startDate, '') = CONVERT(DATETIME, '')
		SET @startDate = NULL

	IF ISNULL(@endDate, '') = CONVERT(DATETIME, '')
		SET @endDate = NULL

	SELECT	tas.ConvertFromJulian(a.HOHDT) AS HolidayDate,
			DATENAME(DW, tas.ConvertFromJulian(a.HOHDT))   AS DOW,
			a.HODESC AS HolidayName,
			a.HOHLCD AS HolidayType 
	FROM tas.External_JDE_F55HOLID a  
	WHERE 
		(
			YEAR(tas.ConvertFromJulian(a.HOHDT)) = @year 
			OR
            @year IS NULL
		)
		AND
        (
			tas.ConvertFromJulian(a.HOHDT) BETWEEN @startDate AND @endDate
			OR
            (@startDate IS NULL AND @endDate IS NULL)
		)
	ORDER BY tas.ConvertFromJulian(a.HOHDT) DESC   	

GO 

/*	Debugging:

	EXEC tas.Pr_GetGARMCOCalendar 2016  
	EXEC tas.Pr_GetGARMCOCalendar 0, '01/01/2011', '31/12/2012'  

*/


