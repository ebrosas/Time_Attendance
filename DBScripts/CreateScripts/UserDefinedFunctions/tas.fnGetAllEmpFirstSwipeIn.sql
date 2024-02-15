/***********************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetAllEmpFirstSwipeIn
*	Description: This function is used to get the first time-in of an employee on specific date
*
*	Date			Author		Rev. #		Comments:
*	17/04/2019		Ervin		1.0			Created
***********************************************************************************************************************/

ALTER FUNCTION tas.fnGetAllEmpFirstSwipeIn 
(
	@empNo				INT,
	@attendanceDate		DATETIME 
)
RETURNS DATETIME 
AS
BEGIN

    DECLARE	@result DATETIME

	--Get the first time-in
	--SELECT TOP 1 @result = SwipeTime 
	--FROM tas.fnGetAllEmployeeSwipe(@attendanceDate, @empNo) a
	--WHERE RTRIM(a.SwipeType) = 'IN'
	--ORDER BY SwipeTime ASC	

	--Get the first time-in
	SELECT TOP 1  @result = SwipeTime 
	FROM tas.MainGateTodaySwipeLog a
	WHERE EmpNo = @empNo
		AND SwipeDate = @attendanceDate 
		AND RTRIM(a.SwipeType) = 'IN'
	ORDER BY SwipeTime ASC	

	RETURN @result
END


/*	Debugging:

	SELECT tas.fnGetAllEmpFirstSwipeIn(10003323, '04/18/2019') 

*/