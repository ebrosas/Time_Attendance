/***********************************************************************************************************************
*	Revision History
*
*	Name: tas.fnGetAllEmpLastSwipeOut
*	Description: This function is used to get the last time-out of an employee on specific date
*
*	Date			Author		Rev. #		Comments:
*	30/10/2023		Ervin		1.0			Created
***********************************************************************************************************************/

ALTER FUNCTION tas.fnGetAllEmpLastSwipeOut 
(
	@empNo				INT,
	@attendanceDate		DATETIME 
)
RETURNS DATETIME 
AS
BEGIN

    DECLARE	@result DATETIME

	--Get the last time out
	SELECT TOP 1 @result = SwipeTime 
	FROM tas.MainGateTodaySwipeLog a
	WHERE EmpNo = @empNo
		AND SwipeDate = @attendanceDate
		AND RTRIM(a.SwipeType) = 'OUT'
	ORDER BY SwipeTime DESC 	

	RETURN @result
END

/*	Debug:

	SELECT tas.fnGetAllEmpLastSwipeOut(10003632, '10/29/2023')

*/