/***************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetCountEmpOnArrivalTime
*	Description: Get the count of employees who arrived base on the start and end time
*
*	Date:			Author:		Rev.#:		Comments:
*	04/12/2016		Ervin		1.0			Created
***************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetCountEmpOnArrivalTime
(
	@startTime			TIME,
	@endTime			TIME,
	@DT					DATETIME,
	@costCenter			VARCHAR(12),
	@comparisonType		CHAR 
)
RETURNS FLOAT
AS
BEGIN

	DECLARE @count	FLOAT
	SET @count = 0

	IF @comparisonType = '<'	--Count arrivals less than the minimum Time-in 
	BEGIN 
    
		SELECT	@count = COUNT(a.EmpNo)
		FROM Vw_EmployeeFirstSwipe a
		WHERE RTRIM(a.BusinessUnit) = RTRIM(@costCenter)
			AND a.DT = @DT
			AND 
			(
				a.dtIN IS NOT NULL AND a.dtIN < @startTime
			)
	END
    
	ELSE IF @comparisonType = '>'	--Count arrivals greater than the maximum Time-in before being marked as late
	BEGIN 
    
		SELECT	@count = COUNT(a.EmpNo)
		FROM Vw_EmployeeFirstSwipe a
		WHERE RTRIM(a.BusinessUnit) = RTRIM(@costCenter)
			AND a.DT = @DT
			AND 
			(
				a.dtIN IS NOT NULL AND a.dtIN > @endTime
			)
	END

	ELSE IF @comparisonType = 'A'	--Count Absences
	BEGIN 
    
		SELECT	@count = COUNT(a.EmpNo)
		FROM Vw_EmployeeFirstSwipe a
		WHERE RTRIM(a.BusinessUnit) = RTRIM(@costCenter)
			AND a.DT = @DT
			AND RTRIM(a.RemarkCode) = 'A'
			AND ISNULL(a.CorrectionCode, '') = ''
	END

	ELSE
    BEGIN

		SELECT	@count = COUNT(a.EmpNo)
		FROM Vw_EmployeeFirstSwipe a
		WHERE RTRIM(a.BusinessUnit) = RTRIM(@costCenter)
			AND a.DT = @DT
			AND 
			(
				a.dtIN IS NOT NULL AND (a.dtIN >= @startTime AND a.dtIN < @endTime)
			)
    END 
    
	RETURN CAST(ISNULL(@count, 0) AS FLOAT)

END

/*	Testing:

PARAMETERS:
	@startTime	TIME,
	@endTime	TIME,
	@DT			DATETIME,
	@costCenter	VARCHAR(12)
	@comparisonType		CHAR
	
	SELECT tas.fnGetCountEmpOnArrivalTime(CONVERT(TIME, '07:40'), CONVERT(TIME, '07:50'), '11/30/2016', '7600', '')
	SELECT tas.fnGetCountEmpOnArrivalTime(CONVERT(TIME, '08:00'), CONVERT(TIME, '08:00'), '11/28/2016', '7600', '<')
	SELECT tas.fnGetCountEmpOnArrivalTime(CONVERT(TIME, '08:00'), CONVERT(TIME, '08:00'), '11/30/2016', '7600', '>')

*/
