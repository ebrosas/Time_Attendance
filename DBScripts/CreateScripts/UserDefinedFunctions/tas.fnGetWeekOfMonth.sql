/***********************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetWeekOfMonth
*	Description: Get the week of the month
*
*	Date			Author		Rev. #		Comments:
*	25/06/2020		Ervin		1.0			Created
*************************************************************************************************************************************************/

CREATE FUNCTION tas.fnGetWeekOfMonth
(
	@dateInput	DATETIME
)
RETURNS TINYINT
AS
BEGIN      

	DECLARE @weekOfMonth INT

	SELECT @weekOfMonth = DATEPART(WEEK, @dateInput)  -   DATEPART(WEEK, DATEADD(MM, DATEDIFF(MM, 0, @dateInput), 0)) + 1 

	RETURN ISNULL(@weekOfMonth, 0)
END


/*	Debugging:

	SELECT tas.fnGetWeekOfMonth('05/30/2020')

*/
