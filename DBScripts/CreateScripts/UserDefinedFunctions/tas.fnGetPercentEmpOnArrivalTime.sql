/***************************************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetPercentEmpOnArrivalTime
*	Description: Get the percentage of employees who arrived base on the start and end time
*
*	Date:			Author:		Rev.#:		Comments:
*	04/12/2016		Ervin		1.0			Created
***************************************************************************************************************************************/

ALTER FUNCTION tas.fnGetPercentEmpOnArrivalTime
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

	DECLARE @count			INT,
			@totalEmp		INT,
			@percentCount	FLOAT 
	
	SELECT	@count			= 0,
			@totalEmp		= 0,
			@percentCount	= 0

	--Get the total number of employees
	SELECT @totalEmp = COUNT(a.EmpNo)
	FROM tas.Master_Employee_JDE a 
	WHERE a.DateResigned IS NULL 
		AND a.GradeCode > 0
		AND RTRIM(a.BusinessUnit) = RTRIM(@costCenter)

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

		IF ISNULL(@count, 0) > 0 AND @totalEmp > 0
			SET @percentCount = CAST(@count AS FLOAT) / CAST(@totalEmp AS FLOAT) * 100	
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

		IF ISNULL(@count, 0) > 0 AND @totalEmp > 0
			SET @percentCount = CAST(@count AS FLOAT) / CAST(@totalEmp AS FLOAT) * 100	
	END

	ELSE IF @comparisonType = 'A'	--Count Absences
	BEGIN 
    
		SELECT	@count = COUNT(a.EmpNo)
		FROM Vw_EmployeeFirstSwipe a
		WHERE RTRIM(a.BusinessUnit) = RTRIM(@costCenter)
			AND a.DT = @DT
			AND RTRIM(a.RemarkCode) = 'A'
			AND ISNULL(a.CorrectionCode, '') = ''

		IF ISNULL(@count, 0) > 0 AND @totalEmp > 0
			SET @percentCount = CAST(@count AS FLOAT) / CAST(@totalEmp AS FLOAT) * 100	
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

		IF ISNULL(@count, 0) > 0 AND @totalEmp > 0
			SET @percentCount = CAST(@count AS FLOAT) / CAST(@totalEmp AS FLOAT) * 100
    END 
    
	RETURN @percentCount

END

/*	Testing:

PARAMETERS:
	@startTime	TIME,
	@endTime	TIME,
	@DT			DATETIME,
	@costCenter	VARCHAR(12)
	@comparisonType		CHAR
	
	SELECT tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '07:40'), CONVERT(TIME, '07:50'), '11/30/2016', '7600', '')
	SELECT tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '08:00'), CONVERT(TIME, '08:00'), '11/28/2016', '7600', '<')
	SELECT tas.fnGetPercentEmpOnArrivalTime(CONVERT(TIME, '08:00'), CONVERT(TIME, '08:00'), '11/30/2016', '7600', '>')

*/
